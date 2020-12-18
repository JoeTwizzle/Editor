﻿using RasterDraw.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEditor.Editors
{
    public abstract class Editor
    {
        public void SetTargetObj(GameScript target) { TargetObj = target; TargetObjType = target.GetType(); }
        public GameScript TargetObj { get; private set; }
        public Type TargetObjType { get; private set; }
        public abstract void OnDrawUI();
        public abstract void Init();
    }
}