﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
 
using ulib.Elements;

namespace ulib.Controls
{
    public class Button : Control
    {
        public Button()
        {
            ImageResource ir = ResourceManager.Instance.GetDefaultResource("anniu2changtai.png");
            if (ir != null)
            {
                Size = ir.Size;
            }
            Res_Normal = ResourceManager.Instance.ComposeDefaultResURL("anniu2changtai.png");
            Res_Pressed = "";
                                 
            m_textNode = new TextNode();
            m_textNode.Text = "Button";
            m_textNode.TextColor = Color.White;
            m_textNode.Dock = DockType.Center;
            m_textNode.AlignH = AlignHori.Center;
            m_textNode.AlignV = AlignVert.Middle;
            Attach(m_textNode);
        }

        [Category("Button")]
        public string Res_Normal 
        {
            get { return m_resNormal; }
            set
            {
                m_resNormal = value;

                if (!GState.IsInLoadingProcess)
                {
                    ResizeToResSize();
                }
            }
        }

        [Category("Button")]
        public string Res_Pressed { get; set; }

        [Category("Button")]
        public string ButtonText { get { return m_textNode.Text; } set { m_textNode.Text = value; } }

        [Browsable(false)]
        [JsonIgnore]
        public bool Pressed { get { return m_isPressed; } }

        [Browsable(false)]
        [JsonIgnore]
        public string Res_Current { get { return m_isPressed ? Res_Pressed : Res_Normal; } }

        public override System.Drawing.Size GetExpectedResourceSize()
        {
            return ResourceManager.Instance.GetResourceSize(Res_Normal);
        }

        private bool m_isPressed = false;
        private TextNode m_textNode;
        private string m_resNormal;
    }
}
