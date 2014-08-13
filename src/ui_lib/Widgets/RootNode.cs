﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ui_lib.Base;
using ui_lib.Elements;

namespace ui_lib.Widgets
{
    public class RootNode : Node
    {
        public bool IsFullscren 
        { 
            get 
            { 
                return m_isFullscreen; 
            } 
            set 
            { 
                m_isFullscreen = value;
                OnFullscreenChanged(); 
            }
        }

        public RootNode()
        {
            base.m_parent = null;
            base.Name = RootNodeConstants.Default_Name;

            OnFullscreenChanged();
        }

        protected void OnFullscreenChanged()
        {
            if (IsFullscren)
            {
                Position = Constants.ZeroPoint;

                System.Drawing.Size size;
                bool ret = ResolutionLut.Table.TryGetValue(RootNodeConstants.Default_Resolution, out size);
                System.Diagnostics.Debug.Assert(ret);
                Size = size;
            }
            else
            {
                Position = RootNodeConstants.Default_Position;
                Size = RootNodeConstants.Default_Size;
            }
        }

        protected bool m_isFullscreen = RootNodeConstants.Default_IsFullscreen;
    }
}