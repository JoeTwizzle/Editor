using GameEditor.Editors;
using ImGuiNET;
using OpenTK.Mathematics;
using RasterDraw.Core;
using RasterDraw.Core.Helpers;
using RasterDraw.Core.NativeScripts;
using RasterDraw.Core.Physics;
using RasterDraw.Core.Rendering;
using RasterDraw.Core.Scripting;
using RasterDraw.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace GameEditor.UI
{
    public class InspectorWindow : EditorWindow
    {
        ObjectInfo<GameObject>? selectedObjInfo;
        List<Editor> ActiveEditors;
        IEnumerable<(Type, CustomEditorAttribute)> CustomEditors;

        public InspectorWindow()
        {
            UIName = "Inspector";
            ActiveEditors = new List<Editor>();
            CustomEditors = Utilities.GetTypesWithAnAttribute<CustomEditorAttribute>(Assembly.GetExecutingAssembly());
        }

        public void SetSelectedObj(GameObject selected)
        {
            if (selectedObjInfo?.Target == selected)
            {
                return;
            }
            selectedObjInfo = new ObjectInfo<GameObject>(selected);
            ActiveEditors.Clear();
            if (selectedObjInfo.Target != null)
            {
                for (int i = 0; i < selectedObjInfo.Target.Scripts.Count; i++)
                {
                    InitInspector(selectedObjInfo.Target.Scripts[i]);
                }
                for (int i = 0; i < selectedObjInfo.Target.ComponentData.Count; i++)
                {
                    InitInspector(selectedObjInfo.Target.ComponentData[i]);
                }
            }
        }

        Editor GetEditor(GameObjectAttachment gameScript)
        {
            Editor editor = null!;  //HAHA LMAO   
            var type = gameScript.GetType();
            bool hasCustomEditor = false;
            foreach (var e in CustomEditors)
            {
                if (type == e.Item2.TargetType)
                {
                    editor = (Editor)Activator.CreateInstance(e.Item1)!;
                    hasCustomEditor = true;
                    break;
                }
            }

            if (!hasCustomEditor)
            {
                editor = new DefaultEditor();
            }
            return editor;
        }

        void InitInspector(GameObjectAttachment gameScript)
        {
            var editor = GetEditor(gameScript);
            editor.SetTargetObj(gameScript);
            editor.Init();
            ActiveEditors.Add(editor);
        }

        public override void DrawUI()
        {
            if (ImGui.Begin(UIName, ref IsActive, ImGuiWindowFlags.NoCollapse))
            {
                if (selectedObjInfo?.Target != null)
                {
                    EditorHelper.DrawMember(selectedObjInfo.Target.UIDText, selectedObjInfo!.GetMemberInfoByName("Name"), selectedObjInfo.Target);
                    EditorHelper.DrawMember(selectedObjInfo.Target.UIDText, selectedObjInfo!.GetMemberInfoByName("IsActive"), selectedObjInfo.Target);
                    for (int i = 0; i < ActiveEditors.Count; i++)
                    {
                        ImGui.Spacing();
                        ImGui.Separator();
                        ImGui.Spacing();
                        if (ImGui.CollapsingHeader(ActiveEditors[i].TargetObjType.Name, ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ActiveEditors[i].OnDrawUI();
                        }
                    }
                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();
                    if (ImGui.Button("Add Script/Component"))
                    {
                        var scr = new RigidBody();
                        InitInspector(scr);

                        selectedObjInfo.Target.AddScript(scr);
                    }
                }
            }
            ImGui.End();
        }
    }
}
