using System;
using System.Collections.Generic;
using System.Text;

namespace GameEditor.UI
{
    public abstract class EditorWindow
    {
        public EditorManager Manager;
        public bool IsActive;
        public string UIName { get; protected set; } = "";
        public abstract void DrawUI();
    }
}
