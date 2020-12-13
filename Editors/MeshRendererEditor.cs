using ImGuiNET;
using RasterDraw.Core.NativeScripts;
using RasterDraw.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace GameEditor.Editors
{
    [CustomEditor(typeof(MeshRenderer))]
    class MeshRendererEditor : Editor
    {
        MeshRenderer Target;
        ObjectInfo TargetInfo;
        ObjectInfo MaterialInfo;
        public override void Init()
        {
            Target = (MeshRenderer)TargetObj;
            TargetInfo = new ObjectInfo(Target);
            MaterialInfo = new ObjectInfo(Target.Material);
        }

        public override void OnDrawUI()
        {
            //ImGui.BeginCombo("",,ImGuiComboFlags.);
            for (int i = 0; i < TargetInfo.variableMembers.Length; i++)
            {
                var member = TargetInfo.variableMembers[i];
                if (member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)
                {
                    EditorHelper.DrawMember(Target.UIDText, member, TargetObj);
                }
            }
        }
    }
}
