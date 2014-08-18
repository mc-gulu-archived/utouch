﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ui_lib;
using ui_lib.Elements;

namespace ui_designer_shell
{
    public class Action_Move : Action
    {
        public Action_Move(List<Node> sel)
        {
            m_selection = sel.ToArray();
            m_initialPositions = new Point[m_selection.Length];
            for (int i = 0; i < m_selection.Length; ++i)
            {
                m_initialPositions[i] = m_selection[i].Position;
            }
        }

        public static int Clamp(int x, int min, int max)
        {
            return x < min ? min : (x > max ? max : x);
        }

        public static Point Clamp(Point pt, Rectangle rect)
        {
            return new Point(
                Clamp(pt.X, rect.Left, rect.Right),
                Clamp(pt.Y, rect.Top, rect.Bottom));
        }

        public void UpdatePosition(Point offset)
        {
            for (int i = 0; i < m_selection.Length; ++i)
            {
                Point loc = m_initialPositions[i];
                loc.X += offset.X;
                loc.Y += offset.Y;

                Node p = m_selection[i].Parent;
                if (p != null)
                    loc = Clamp(loc, new Rectangle(0, 0,
                        p.Size.Width - m_selection[i].Size.Width,
                        p.Size.Height - m_selection[i].Size.Height));

                m_selection[i].Position = loc;
            }
        }

        public void EndUpdatePosition(Point offset)
        {
            UpdatePosition(offset);

            m_finalPositions = new Point[m_selection.Length];
            for (int i = 0; i < m_selection.Length; ++i)
            {
                m_finalPositions[i] = m_selection[i].Position;
            }
        }

        public Node[] m_selection;
        public Point[] m_initialPositions;
        public Point[] m_finalPositions;

        public override void Undo()
        {
            for (int i = 0; i < m_selection.Length; ++i)
            {
                m_selection[i].Position = m_initialPositions[i];
            }
        }

        public override void Redo()
        {
            for (int i = 0; i < m_selection.Length; ++i)
            {
                m_selection[i].Position = m_finalPositions[i];
            }
        }
    }
}