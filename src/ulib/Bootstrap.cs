﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ulib.Elements;

namespace ulib
{
    public class BootParams
    {
        public List<string> ReourceImages;
        public string ScenePath;
    }

    /// <summary>
    /// 
    /// Bootstrap 类是 ulib 库对外的入口点
    /// 
    /// ulib库包含了一些功能性的服务，这些服务定义在 Services 中，
    /// 出于效率和简单性考虑，它们并未被实现为（物理上的）严格隔离，
    /// 但在功能上，是尽量彼此正交而独立的。
    /// 
    /// Bootstrap 充当了启动脚本的作用，以最典型的方式启动这些服务。
    /// 
    /// Init() 函数被设计为可重复调用，效果相当于重置
    /// </summary>
    public class Bootstrap
    {
        public static Bootstrap Instance = new Bootstrap();

        public bool Init(BootParams bp)
        {
            if (Scene.Instance != null)
                Scene.Instance.Dispose();

            ResourceManager.Instance.Clear();
            foreach (string resFile in bp.ReourceImages)
            {
                if (!ResourceManager.Instance.LoadFile(resFile))
                    return false;
            }

            NodeNameUtil.ResetIDAllocLut();

            Scene.Instance = new Scene();
            if (!Scene.Instance.Init())
                return false;
            if (bp.ScenePath.Length != 0 && !Scene.Instance.Load(bp.ScenePath))
                return false;

            return true;
        }
    }
}