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
using System.Linq;

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
        List<string> gameScriptNames;
        List<Type> gameScripts;
        void InitInspector(GameObjectAttachment gameScript)
        {
            var all = AppDomain.CurrentDomain.GetAssemblies();
            gameScriptNames = new List<string>();
            gameScripts = new List<Type>();
            for (int i = 0; i < all.Length; i++)
            {
                var gs = Utilities.GetDerivedTypes(all[i], typeof(GameObjectAttachment)).ToList();
                gameScripts.AddRange(gs);
                for (int j = 0; j < gs.Count; j++)
                {
                    gameScriptNames.Add(gs[j].Name);
                }
            }

            var editor = GetEditor(gameScript);
            editor.SetTargetObj(gameScript);
            editor.Init();
            ActiveEditors.Add(editor);
        }

        public override void DrawUI()
        {
            if (ImGui.Begin(UIName, ref IsActive, ImGuiWindowFlags.NoCollapse))
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
                        DrawAddScriptUI();


                        if (removeGO)
                        {
                            selectedObjInfo?.Target?.GameLoop.Remove(selectedObjInfo.Target);
                        }
                    }
                }
            }
            ImGui.End();
        }
        string input = "";
        bool isOpen = false;
        bool shouldClose = false;
        void DrawAddScriptUI()
        {
            ImGui.InputText("##input", ref input, 128u);
            if (input.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase))
            {
                input = input.Remove(input.Length - 3);
            }
            string spaceLessInput = new string(input.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
            var filteredNames = gameScriptNames.Where(x => x.Contains(input, StringComparison.InvariantCultureIgnoreCase) || x.Contains(spaceLessInput, StringComparison.InvariantCultureIgnoreCase)).ToList();
            ImGui.SameLine();
            //Is text box highlighted?
            isOpen |= ImGui.IsItemActive();
            shouldClose = ImGui.IsWindowFocused() && isOpen;
            if (isOpen)
            {
                var tl = ImGui.GetItemRectMin();
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(ImGui.GetItemRectSize().X, 0));
                if (ImGui.Begin("##popup", ref isOpen, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoCollapse))
                {
                    shouldClose &= !ImGui.IsWindowFocused();
                    //Is suggestion field highlighted?
                    isOpen |= ImGui.IsWindowFocused();
                    for (int i = 0; i < filteredNames.Count; i++)
                    {
                        if (ImGui.Selectable(filteredNames[i]) || (ImGui.IsItemHovered() && ImGui.IsKeyPressed((int)ImGuiKey.Enter, false)))
                        {
                            input = filteredNames[i];
                            isOpen = false;
                        }

                    }
                    if (shouldClose)
                    {
                        isOpen = false;
                    }
                    ImGui.SetWindowPos(new System.Numerics.Vector2(tl.X, tl.Y - ImGui.GetWindowSize().Y));
                }
                ImGui.End();
            }
            if (ImGui.Button("Add Script"))
            {
                if (gameScriptNames.Contains(input))
                {
                    int i = gameScriptNames.IndexOf(input);
                    if (i == -1 || selectedObjInfo?.Target == null)
                    {
                        return;
                    }
                    var scr = (GameObjectAttachment)Activator.CreateInstance(gameScripts[i])!;
                    InitInspector(scr);
                    if (scr is GameScript a)
                    {
                        selectedObjInfo.Target.AddScript(a);
                    }
                    if (scr is ComponentData b)
                    {
                        selectedObjInfo.Target.AddComponent(b);
                    }
                }
            }
        }
    }
}
