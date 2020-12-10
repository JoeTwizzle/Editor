using System;
using System.Collections.Generic;
using System.Text;

namespace GameEditor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomEditorAttribute : Attribute
    {
        public readonly Type TargetType;
        public CustomEditorAttribute(Type type)
        {
            TargetType = type;
        }
    }
}
