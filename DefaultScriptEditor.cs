using ImGuiNET;
using OpenTK.Mathematics;
using RasterDraw.Core.Scripting;
using RasterDraw.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameEditor
{
    public class DefaultScriptEditor : Editor
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
            for (int i = 0; i < objInfo.members.Length; i++)
            {
                var member = objInfo.members[i];
                if (member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)
                {
                    EditorHelper.DrawMember(((GameScript)objInfo.Target).UIDText, member, TargetObj);
                }
            }
        }
    }
}
