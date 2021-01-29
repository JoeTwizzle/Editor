using ImGuiNET;
using OpenTK.Mathematics;
using RasterDraw.Core;
using RasterDraw.Core.NativeScripts;
using RasterDraw.Rendering;
using RasterDraw.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEditor.UI
{
    public unsafe class HierarchyWindow : EditorWindow
    {
        GameObject selectedObj;
        Transform scr;

        public HierarchyWindow()
        {
            UIName = "Hierarchy";
        }

        void Traverse(GameObject gameObject)
        {
            bool leaf = gameObject.Transform.Children.Count <= 0;
            if (gameObject == selectedObj)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.25882352941f, 0.5294117647f, 0.96078431372f, 1));
            }
            var x = ImGui.GetCursorPosX();
            ImGui.Dummy(new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFontSize()));
            ImGui.SameLine();
            ImGui.SetCursorPosX(x);
            bool open = ImGui.TreeNodeEx(gameObject.UIDText, (!ImGui.IsMouseClicked(ImGuiMouseButton.Left) && leaf ? ImGuiTreeNodeFlags.Leaf : 0) | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow, gameObject.Name);


            if (ImGui.BeginDragDropSource())
            {
                if (ImGui.SetDragDropPayload("Transform", IntPtr.Zero, 0))
                {
                    scr = gameObject.Transform;
                }
                ImGui.EndDragDropSource();
            }

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("Transform");
                if (payload.NativePtr != null && scr != null)
                {
                    scr.Parent = gameObject.Transform;
                }
                ImGui.EndDragDropTarget();
            }

            if (gameObject == selectedObj)
            {
                ImGui.PopStyleColor();
            }

            if (ImGui.IsItemClicked())
            {
                if (selectedObj != gameObject)
                {
                    SelectionChanged(gameObject);
                }
                selectedObj = gameObject;
            }

            if (open)
            {

                for (int i = 0; i < gameObject.Transform.Children.Count; i++)
                {
                    Traverse(gameObject.Transform.Children[i].GameObject);
                }
                ImGui.TreePop();
            }

        }

        void SelectionChanged(GameObject selected)
        {
            Manager.GetWindow<InspectorWindow>()!.SetSelectedObj(selected);
        }

        public override void DrawUI()
        {
            if (ImGui.Begin(UIName, ref IsActive, ImGuiWindowFlags.NoCollapse))
            {
                ImGui.Dummy(ImGui.GetContentRegionAvail());
                ImGui.SetCursorPosY(0);

                ImGui.PushStyleColor(ImGuiCol.DragDropTarget, new System.Numerics.Vector4(0.219f, 0.223f, 0.623f, 1));
                if (ImGui.BeginDragDropTarget())
                {
                    var payload = ImGui.AcceptDragDropPayload("Transform");
                    if (payload.NativePtr != null && scr != null)
                    {
                        scr.Parent = null;
                    }
                    ImGui.EndDragDropTarget();
                }
                //for (int i = 0; i < EditorManager.Instance.LÖÖPS.Scenes.Count; i++)
                {
                    Scene scene = Manager.GameGameLoop.MasterScene;
                    if (ImGui.TreeNodeEx(scene.UIDText, ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.FramePadding, "Root Scene"))
                    {
                        var gos = scene.RootObjects;
                        for (int j = 0; j < gos.Count; j++)
                        {
                            if (gos[j].Transform.Parent == null)
                            {
                                Traverse(gos[j]);
                            }
                        }
                        ImGui.TreePop();
                    }
                }
                ImGui.PopStyleColor();
            }
            ImGui.End();
        }
    }
}
