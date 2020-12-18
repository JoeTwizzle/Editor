using GameEditor.Editors;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using RasterDraw.Core;
using RasterDraw.Core.GUI;
using RasterDraw.Core.Helpers;
using RasterDraw.Core.NativeScripts;
using RasterDraw.Core.Rendering;
using RasterDraw.Core.Scripting;
using RasterDraw.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace GameEditor
{
    public unsafe class EditorManager : GameScript
    {
        Vector2i Size;
        ImGuiController GuiController;
        public GameLoop LÖÖPS;
        public EditorManager(Vector2i Size)
        {
            this.Size = Size;
            GuiController = new ImGuiController(Size.X, Size.Y);
        }
        public void Resize(Vector2i Size)
        {
            this.Size = Size;
            GuiController.WindowResized(Size.X, Size.Y);
            OnResizeGame();
        }

        public void Update(GameWindow gw, float dt)
        {
            GuiController.Update(gw, dt);
            DrawUI(LÖÖPS.masterScene);
        }

        public void Render()
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            GuiController.Render();
        }

        void OnResizeGame()
        {
            RenderTexture.DefaultWidth = (int)gameRegion.X;
            RenderTexture.DefaultHeight = (int)gameRegion.Y;
            ICamera.MainCamera.Viewport = new Viewport(new Box2i(Vector2i.Zero, new Vector2i((int)gameRegion.X, (int)gameRegion.Y)));
        }

        public void CharPressed(char c)
        {
            GuiController.PressChar(c);
        }

        public void MouseScroll(Vector2 amount)
        {
            GuiController.MouseScroll(amount);
        }

        ObjectInfo? selectedObjInfo;
        GameObject? selectedObj;
        GameObject? prevSelectedObj;
        public GameObject? SelectedObj
        {
            get => selectedObj;
            set
            {
                prevSelectedObj = selectedObj;
                selectedObj = value;
                if (SelectedObj != null)
                {
                    selectedObjInfo = new ObjectInfo(SelectedObj);
                }
                else
                {
                    selectedObjInfo = null;
                }
            }
        }

        Transform scr;


        unsafe void Traverse(GameObject gameObject)
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
            bool open = ImGui.TreeNodeEx(gameObject.UIDText, (!ImGui.IsItemHovered() && leaf ? ImGuiTreeNodeFlags.Leaf : 0) | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow, gameObject.Name);


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
                SelectedObj = gameObject;
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

        System.Numerics.Vector2 gameRegion;
        bool showMetrics;
        bool showExample;
        public unsafe void DrawUI(Scene scene)
        {
            uint dockSpaceID = 0;
            ImGui.SetNextWindowPos(new System.Numerics.Vector2());
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(Size.X, Size.Y));
            if (ImGui.Begin("Main Window", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar))
            {
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.MenuItem("Show Metrics"))
                    {
                        showMetrics = !showMetrics;
                    }
                    if (ImGui.MenuItem("Show Example"))
                    {
                        showExample = !showExample;
                    }
                }

                ImGui.EndMenuBar();
                dockSpaceID = ImGui.GetID("Main Dockspace");
                ImGui.DockSpace(dockSpaceID);
            }
            ImGui.End();

            if (ImGui.Begin("Hierarchy", ImGuiWindowFlags.NoCollapse))
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
                if (ImGui.TreeNodeEx(scene.UIDText, ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.FramePadding, "Root Scene"))
                {
                    var gos = scene.ReadOnlyGameObjects;
                    for (int i = 0; i < gos.Count; i++)
                    {
                        if (gos[i].Transform.Parent == null)
                        {
                            Traverse(gos[i]);
                        }
                    }
                    ImGui.TreePop();
                }
                ImGui.PopStyleColor();
            }
            ImGui.End();

            if (ImGui.Begin("ViewPort", ImGuiWindowFlags.NoCollapse))
            {
                System.Numerics.Vector2 region = ImGui.GetContentRegionAvail();
                if (gameRegion != region)
                {
                    gameRegion = region;
                    OnResizeGame();
                }
                ImGui.Image(new IntPtr(ICamera.MainCamera.RenderTexture.ColorTexture.Handle), region, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
            }
            ImGui.End();

            if (ImGui.Begin("Inspector", ImGuiWindowFlags.NoCollapse))
            {
                if (SelectedObj != null)
                {
                    if (SelectedObj != prevSelectedObj)
                    {
                        prevSelectedObj = selectedObj;
                        ActiveEditors.Clear();
                        for (int i = 0; i < SelectedObj.Scripts.Count; i++)
                        {
                            InitInspector(SelectedObj.Scripts[i]);
                        }
                    }
                    EditorHelper.DrawMember(SelectedObj.UIDText, selectedObjInfo!.GetMemberInfoByName("Name"), SelectedObj);
                    EditorHelper.DrawMember(SelectedObj.UIDText, selectedObjInfo!.GetMemberInfoByName("IsActive"), SelectedObj);
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
                }
            }
            ImGui.End();

            if (showMetrics)
            {
                ImGui.ShowMetricsWindow();
            }

            if (showExample)
            {
                ImGui.ShowDemoWindow();
            }
        }

        List<Editor> ActiveEditors = new List<Editor>();
        IEnumerable<(Type, CustomEditorAttribute)> CustomEditors;

        public void LoadEditors()
        {
            CustomEditors = Utilities.GetTypesWithAnAttribute<CustomEditorAttribute>(Assembly.GetExecutingAssembly());
        }

        private void InitInspector(GameScript gameScript)
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

            editor.SetTargetObj(gameScript);
            editor.Init();
            ActiveEditors.Add(editor);
        }
    }
}
