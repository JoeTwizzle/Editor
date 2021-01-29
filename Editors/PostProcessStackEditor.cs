using RasterDraw.Core.NativeScripts;
using RasterDraw.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using System.Threading.Tasks;

namespace GameEditor.Editors
{
    [CustomEditor(typeof(PostProcessStack))]
    class PostProcessStackEditor : Editor
    {
        PostProcessStack stack;
        ObjectInfo[] effectInfos;
        public override void Init()
        {
            stack = (PostProcessStack)TargetObj;
            effectInfos = new ObjectInfo[stack.postProcessEffects.Count];
            for (int i = 0; i < stack.postProcessEffects.Count; i++)
            {
                effectInfos[i] = new ObjectInfo(stack.postProcessEffects[i]);
            }
        }

        public override void OnDrawUI()
        {
            bool enabled = stack.Enabled;
            if (EditorHelper.DrawMember(stack.UIDText, "Enabled", ref enabled, typeof(bool)))
            {
                stack.Enabled = enabled;
            }
            for (int i = 0; i < effectInfos.Length; i++)
            {
                ImGui.Spacing();
                ImGui.Separator();
                var uiNameInfo = effectInfos[i].GetMemberInfoByName("UIName")!;
                ImGui.Text((string)(uiNameInfo.GetValue(effectInfos[i].Target)!));
                for (int j = 0; j < effectInfos[i].variableMembers.Length; j++)
                {
                    var member = effectInfos[i].variableMembers[j];
                    if (member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)
                    {
                        if (member != uiNameInfo)
                        {
                            string name = stack.UIDText + (string)uiNameInfo.GetValue(effectInfos[i].Target)!;
                            EditorHelper.DrawMember(name, member, effectInfos[i].Target);
                        }
                    }
                }

            }
        }
    }
}
