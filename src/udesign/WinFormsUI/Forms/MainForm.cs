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
            m_uiPropertyGrid.PropertyValueChanged += () =>
            {
                m_uiLayoutTree.PopulateLayout(); 
                m_glCtrl.Refresh(); 
            };
            m_uiPropertyGrid.ValidateNodeName += (node, newName) => { return !NodeNameUtil.HasNameCollisionWithSiblings(node, newName); };


            SceneEd.Instance.HasModifierKeyDown = (keyCode) => { return (ModifierKeys & keyCode) != 0; };


            SceneEdEventNotifier.Instance.SelectNode += m_uiLayoutTree.OnSelectSceneNode;
            SceneEdEventNotifier.Instance.SelectNode += m_uiPropertyGrid.OnSelectSceneNode;
            SceneEdEventNotifier.Instance.RefreshScene += OnSceneRefreshed;

            // 这个操作需要发生在前面这些响应函数注册的操作后面，否则某些必要的消息（如场景被刷新）是无法被送达的
            if (!ResetScene(""))
                return false;

            // 下面的逻辑仅在欢迎界面被执行
            AddWelcomeString("1. 这是一个欢迎页面");
            AddWelcomeString("2. 本页面包含一个根节点和若干 Label (均可鼠标选中和移动)");
            AddWelcomeString("3. sample_layouts 目录中有一些范例文件，可通过 “File | Open” 打开");

            Scene.Instance.Root.LogicalSize = new Size(1000, 1000); // 测试方便

            return true;
        }

        private void OnSceneRefreshed(RefreshSceneOpt opts)
        {
            if (opts.HasFlag(RefreshSceneOpt.Refresh_MainMenu))
            {
                m_menuCut.Enabled = SceneEd.Instance.HasSelection;
                m_menuCopy.Enabled = SceneEd.Instance.HasSelection;
                m_menuPaste.Enabled = SceneEd.Instance.Clipboard.IsInUse;
                m_menuDelete.Enabled = SceneEd.Instance.HasSelection;
            }

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
        }

        Point m_welcomePos = new Point(50, 30);
        private void AddWelcomeString(string text)
        {
            ulib.Controls.Label welcomeText = new ulib.Controls.Label();
            welcomeText.Text = text;
            welcomeText.TextColor = Color.White;
            welcomeText.Position = m_welcomePos;
            m_welcomePos = m_welcomePos + new Size(0, 30);
            welcomeText.RequestedSizeRefreshing = true;
            Scene.Instance.Root.Attach(welcomeText);
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
            m_testButton.SetPosition(0, 0);
            m_testButton.SetSize(1, 1);
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
            diag.InitialDirectory = Path.Combine(UDesignApp.Instance.RootPath, Properties.Settings.Default.InitialOpenFileDir);
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
            SceneEd.Instance.Selection.ClearSelection();
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_resForm != null)
            {
                Point pt = PointToScreen(m_resForm.Location);
                ConfigUserPref.Instance.SetValue("forms.res_form", "location", BaseUtil.PointToString(pt));
                ConfigUserPref.Instance.SetValue("forms.res_form", "size", BaseUtil.SizeToString(m_resForm.Size));
            }
        }

        public static Control FindFocusedControl(Control control)
        {
            var container = control as ContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as ContainerControl;
            }
            return control;
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

        private void m_menuCut_Click(object sender, EventArgs e)
        {
            SceneEd.Instance.Cut();
        }

        private void m_menuCopy_Click(object sender, EventArgs e)
        {
            SceneEd.Instance.Copy();
        }

        private void m_menuPaste_Click(object sender, EventArgs e)
        {
            SceneEd.Instance.Paste();
        }
    }
}