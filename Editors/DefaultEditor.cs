using ImGuiNET;
using OpenTK.Mathematics;
using RasterDraw.Core.Scripting;
using RasterDraw.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameEditor.Editors
{
    public class DefaultEditor : Editor
    {
        ObjectInfo objInfo;
        public override void Init()
        {
            objInfo = new ObjectInfo(TargetObj);
        }
        public override void OnDrawUI()
        {
            DrawMembers();
        }

        void DrawMembers()
        {
            for (int i = 0; i < objInfo.variableMembers.Length; i++)
            {
                var member = objInfo.variableMembers[i];
                if (member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)
                {
                    EditorHelper.DrawMember(((GameScript)objInfo.Target).UIDText, member, TargetObj);
                }
            }
        }
    }
}
