﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using udesign;
using ulib;
using ulib.Base;
using ulib.Elements;

namespace udesign
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            m_glCtrl = new Controls.UIRenderBuffer_GL_Tao();
            m_glCtrl.Dock = DockStyle.Fill;
            this.splitContainer1.Panel2.Controls.Add(m_glCtrl);
        }

        public bool Init()
        {
            // connect the layout tree and the property grid
            m_uiPropertyGrid.PropertyValueChanged += () => { m_glCtrl.Refresh(); };
            m_uiPropertyGrid.ValidateNodeName += (node, newName) => { return !NodeNameUtil.HasNameCollisionWithSiblings(node, newName); };

            SceneEdEventNotifier.Instance.SelectNode += m_uiLayoutTree.OnSelectSceneNode;
            SceneEdEventNotifier.Instance.SelectNode += m_uiPropertyGrid.OnSelectSceneNode;

            SceneEdEventNotifier.Instance.RefreshScene += (opts) => 
            {
                if (opts.HasFlag(RefreshSceneOpt.Refresh_Layout))
                {
                    m_uiLayoutTree.PopulateLayout();
                }
                if (opts.HasFlag(RefreshSceneOpt.Refresh_Properties))
                {
                    m_uiPropertyGrid.GetGridCtrl().RefreshPropertyValues();
                }

                // Rendering 排在后面是为了反映前两者的变化的结果
                if (opts.HasFlag(RefreshSceneOpt.Refresh_Rendering))
                {
                    m_glCtrl.Refresh();
                }
            };

            // 这个操作需要发生在前面这些响应函数注册的操作后面，否则某些必要的消息（如场景被刷新）是无法被送达的
            if (!ResetScene(""))
                return false;

            return true;
        }

        private void menuItemGwenUnitTest_Click(object sender, EventArgs e)
        {
            GwenUnitTestForm tf = new GwenUnitTestForm();
            tf.Show();
        }

        private Gwen.Control.Button m_testButton;

        private Controls.UIRenderBuffer_GL_Tao m_glCtrl;

        private void MainForm_Load(object sender, EventArgs e)
        {
            m_testButton = new Gwen.Control.Button(m_glCtrl.GetCanvas());
            m_testButton.SetPosition(0, 780);
        }

        private ResForm m_resForm;

        private void m_menuSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Scene.Instance.CurrentFilePath))
            {
                PerformSaveAs();
            }
            else 
            {
                if (!Scene.Instance.Save())
                {
                    Session.Message("文件保存失败（细节请查看日志）。('{0}')", Scene.Instance.CurrentFilePath);
                }
            }
        }

        private void m_menuSaveAs_Click(object sender, EventArgs e)
        {
            PerformSaveAs();
        }

        private void m_menuOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();
            diag.InitialDirectory = Path.Combine(UDesignApp.Instance.RootPath, @"testdata\");
            diag.Filter = "ui layout files (*.ui_layout)|*.ui_layout|All files (*.*)|*.*";
            if (diag.ShowDialog(this) == DialogResult.OK)
            {
                string file = diag.FileName;
                if (File.Exists(file))
                {
                    // 这里重置前，应先提示用户保存
                    if (!ResetScene(file))
                        Session.Message("打开文件失败。");
                }
            }
        }

        private void m_menuNew_Click(object sender, EventArgs e)
        {
            if (Session.Confirm("新建文件将会清楚当前场景内所有数据并重置整个场景，继续操作吗？"))
            {
                ResetScene("");
            }
        }

        private bool ResetScene(string sceneName)
        {
            SceneEd.Instance.Selection.Clear();
            SceneEd.Instance.OperHistory.Clear();

            BootParams bp = new BootParams {
                DefaultReourceImage = Properties.Settings.Default.DefaultResFile,
                ReourceImages = ConfigTypical.Instance.ReourceImages,
                ScenePath = sceneName 
            };
            if (!Bootstrap.Instance.Init(bp))
                return false;

            // 不管是 Load 还是 Reset 成功，均需要刷新窗体的标题栏
            UpdateFormTitle();

            SceneEd.Instance.Select(Scene.Instance.Root);
            SceneEdEventNotifier.Instance.Emit_RefreshScene(RefreshSceneOpt.Refresh_All);
            return true;
        }

        private void m_menuResForm_Click(object sender, EventArgs e)
        {
            m_resForm = new ResForm();
            m_resForm.Show(this);

            string loc = ConfigUserPref.Instance.GetValue("forms.res_form", "location");
            string size = ConfigUserPref.Instance.GetValue("forms.res_form", "size");
            if (loc.Length != 0 && size.Length != 0)
            {
                Point pt = BaseUtil.StringToPoint(loc);
                Size sz = BaseUtil.StringToSize(size);
                m_resForm.SetDesktopBounds(pt.X, pt.Y, sz.Width, sz.Height);
            }
        }

        //void m_resForm_ApplyImage(string atlasFileName, string imageName)
        //{
        //    if (SceneEd.Instance.Selection.Count == 1)
        //    {
        //        ImageNode sel = SceneEd.Instance.Selection.First() as ImageNode;
        //        if (sel != null)
        //        {
        //            string newLoc = BaseUtil.ComposeResURL(atlasFileName, imageName);
        //            Session.Log("ImageNode '{0}' URL changed. (old: {1}, new: {2})", sel.Name, sel.Res, newLoc);
        //            sel.Res = newLoc;
        //            SceneEdEventNotifier.Instance.Emit_RefreshScene(RefreshSceneOpt.Refresh_Rendering | RefreshSceneOpt.Refresh_Properties);
        //        }
        //        else
        //        {
        //            Session.Message("现在暂不支持设置到非 ImageNode 节点.");
        //        }
        //    }
        //    else
        //    {
        //        Session.Message("请选中单个的 ImageNode 节点后再试 (现有 {0} 个节点被选中).", SceneEd.Instance.Selection.Count);
        //    }
        //}

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_resForm != null)
            {
                Point pt = PointToScreen(m_resForm.Location);
                ConfigUserPref.Instance.SetValue("forms.res_form", "location", BaseUtil.PointToString(pt));
                ConfigUserPref.Instance.SetValue("forms.res_form", "size", BaseUtil.SizeToString(m_resForm.Size));
            }
        }

        private void m_menuDelete_Click(object sender, EventArgs e)
        {
            SceneEd.Instance.DeleteSelected();
        }

        private void m_menuUndo_Click(object sender, EventArgs e)
        {
            SceneEd.Instance.Undo();
        }

        private void m_menuRedo_Click(object sender, EventArgs e)
        {
            SceneEd.Instance.Redo();
        }

        private void m_menuOpenTestLayout_Click(object sender, EventArgs e)
        {
            // 这里重置前，应先提示用户保存
            if (!ResetScene(@"testdata\test" + Constants.LayoutPostfix))
                Session.Message("打开文件失败。");
        }

        private void PerformSaveAs()
        {
            SaveFileDialog diag = new SaveFileDialog();
            diag.InitialDirectory = Path.Combine(UDesignApp.Instance.RootPath, @"testdata\");
            diag.Filter = "ui layout files (*.ui_layout)|*.ui_layout|All files (*.*)|*.*";
            if (diag.ShowDialog(this) == DialogResult.OK)
            {
                string file = diag.FileName;

                // perform saving
                if (!Scene.Instance.Save(file))
                {
                    Session.Message("文件保存失败（细节请查看日志）。('{0}')", file);
                }
                else
                {
                    UpdateFormTitle();
                }

            }
        }

        private void UpdateFormTitle()
        {
            Text = Properties.Settings.Default.AppName + " - " + Scene.Instance.CurrentFilePath;
        }
    }
}
