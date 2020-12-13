﻿using RasterDraw.Core.Scripting;
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
            if (!type.IsSubclassOf(typeof(GameScript)))
            {
                throw new ArgumentException("The provided type must derive from GameScript.");
            }
            TargetType = type;
        }
    }
}
