using System;
using System.Collections.Generic;
using System.Text;

namespace GameEditor
{
    public abstract class Editor
    {
        public void SetTargetObj(object target) { TargetObj = target; TargetObjType = target.GetType(); }
        public object TargetObj { get; private set; }
        public Type TargetObjType { get; private set; }
        public abstract void OnDrawUI();
        public abstract void Init();
    }
}
