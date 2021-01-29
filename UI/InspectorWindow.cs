using GameEditor.Editors;
using ImGuiNET;
using OpenTK.Mathematics;
using RasterDraw.Core;
using RasterDraw.Helpers;
using RasterDraw.Core.NativeScripts;
using RasterDraw.Core.Physics;
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
                for (int i = 0; i < selectedObjInfo.Target.ComponentData.Count; i++)
                {
                    InitInspector(selectedObjInfo.Target.ComponentData[i]);
                }
                for (int i = 0; i < selectedObjInfo.Target.Scripts.Count; i++)
                {
                    InitInspector(selectedObjInfo.Target.Scripts[i]);
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

            bool begin = ImGui.Begin(UIName, ref IsActive, ImGuiWindowFlags.NoCollapse);
            if (begin)
            {
                if (selectedObjInfo != null)
                {
                    if (selectedObjInfo.Target != null)
                    {
                        bool removeGO = ImGui.Button($"X##{selectedObjInfo?.Target?.UIDText}");

                        EditorHelper.DrawMember(selectedObjInfo!.Target!.UIDText, selectedObjInfo!.GetMemberInfoByName("Name"), selectedObjInfo.Target);
                        EditorHelper.DrawMember(selectedObjInfo!.Target!.UIDText, selectedObjInfo!.GetMemberInfoByName("IsActive"), selectedObjInfo.Target);
                        bool remove = false;
                        int r = -1;
                        for (int i = 0; i < ActiveEditors.Count; i++)
                        {
                            ImGui.Spacing();
                            ImGui.Separator();
                            ImGui.Spacing();
                            string inspectorID = $"{ActiveEditors[i].TargetObjType.Name}##{ActiveEditors[i].TargetObj.UIDText}";
                            float xSpace = ImGui.GetContentRegionAvail().X;
                            ImGui.Columns(2, inspectorID, false);
                            ImGui.SetColumnWidth(0, xSpace * 0.95f);
                            bool open = ImGui.CollapsingHeader(inspectorID, ImGuiTreeNodeFlags.DefaultOpen);
                            ImGui.SameLine();
                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                            remove = ImGui.Button($"x##{ActiveEditors[i].TargetObj.UIDText}");
                            ImGui.Columns(1);
                            if (open)
                            {
                                ActiveEditors[i].OnDrawUI();
                                if (remove)
                                {
                                    r = i;
                                    break;
                                }
                            }

                        }
                        if (remove)
                        {
                            if (ActiveEditors[r].TargetObj is ComponentData)
                            {
                                selectedObjInfo.Target.RemoveComponent((ComponentData)ActiveEditors[r].TargetObj);
                            }
                            if (ActiveEditors[r].TargetObj is GameScript)
                            {
                                selectedObjInfo.Target.RemoveScript((GameScript)ActiveEditors[r].TargetObj);
                            }
                            ActiveEditors.RemoveAt(r);
                        }
                        ImGui.Columns(1);
                        ImGui.Spacing();
                        ImGui.Separator();
                        ImGui.Spacing();
                        if (ImGui.Button("Add Script/Component"))
                        {
                            var scr = new RigidBody();
                            InitInspector(scr);

                            selectedObjInfo.Target.AddScript(scr);
                        }
                        if (removeGO)
                        {
                            selectedObjInfo?.Target?.GameLoop.Remove(selectedObjInfo.Target);
                        }
                    }
                }
            }
            ImGui.End();
        }
    }
}
