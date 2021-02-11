using GameEditor.Editors;
using GameEditor.UI;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using RasterDraw.Core;
using RasterDraw.Core.GUI;
using RasterDraw.Helpers;
using RasterDraw.Core.NativeScripts;
using RasterDraw.Core.Scripting;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RasterDraw.Rendering;
using RasterDraw.Rendering.PostFX;

namespace GameEditor
{
    public class EditorManager : GameScript
    {
        List<EditorWindow> EditorWindows;
        ImGuiController GuiController;
        bool showMetrics;
        bool showExample;
        Vector2i Size;

        public Camera cam { get; private set; }
        public GameLoop GameGameLoop;

        public EditorManager(Vector2i Size)
        {
            this.Size = Size;
            EditorWindows = new List<EditorWindow>();
            GuiController = new ImGuiController(Size.X, Size.Y);
            LoadEditors();
            evp = GetWindow<EditorViewportWindow>()!;
            gvp = GetWindow<GameViewportWindow>()!;
            gvp.IsActive = true;
            evp.IsActive = true;
            GetWindow<HierarchyWindow>()!.IsActive = true;
            GetWindow<InspectorWindow>()!.IsActive = true;
        }
        GameViewportWindow gvp;
        EditorViewportWindow evp;
        void LoadEditors()
        {
            var types = Utilities.GetDerivedTypes(Assembly.GetExecutingAssembly(), typeof(EditorWindow));
            foreach (var type in types)
            {
                var editor = (EditorWindow)Activator.CreateInstance(type)!;
                editor.Manager = this;
                EditorWindows.Add(editor);
            }
        }

        public T? GetWindow<T>() where T : EditorWindow
        {
            return (T?)EditorWindows.FirstOrDefault(x => x.GetType() == typeof(T));
        }

        GameObject EditorCam;
        public override void Init()
        {
            //Add Editor Camera
            cam = new Camera(new Viewport(new Box2i(0, 0, Size.X, Size.Y)));
            EditorCam = new GameObject("Editor Camera");
            cam.MSAA = 1;
            cam.MultiSample = false;
            EditorCam.AddScript(cam);
            EditorCam.AddScript(new FlyCamController());

            GameObject.GameLoop.Add(EditorCam);
        }

        public void Resize(Vector2i Size)
        {
            this.Size = Size;
            gvp.InvalidateRegion();
            evp.InvalidateRegion();
            GuiController.WindowResized(Size.X, Size.Y);
        }

        public void Render(bool discard = false)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            GuiController.Render(discard);
        }

        public void CharPressed(char c)
        {
            GuiController.PressChar(c);
        }

        public void MouseScroll(Vector2 amount)
        {
            GuiController.MouseScroll(amount);
        }
        bool initDraw;
        public void DrawUI()
        {
            if (!initDraw)
            {
                var stack = new PostProcessStack();
                stack.AddEffect(new BloomEffect());
                stack.AddEffect(new ToneMapACES());
                EditorCam.AddScript(stack);
                initDraw = true;
            }

            GuiController.Update(GameObject.GameLoop.NativeWindow, GameObject.GameLoop.RenderDeltaTime);
            uint dockSpaceID = 0;
            ImGui.SetNextWindowPos(new System.Numerics.Vector2());
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(Size.X, Size.Y));
            if (ImGui.Begin("Main Window", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar))
            {
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.Button(BaseGame.Play ? "Pause" : "Play"))
                    {
                        BaseGame.Play = !BaseGame.Play;
                    }
                    if (ImGui.MenuItem("File"))
                    {
                        if (ImGui.MenuItem("Import"))
                        {
                            
                        }
                    }
                    if (ImGui.MenuItem("Edit"))
                    {

                    }
                    if (ImGui.BeginMenu("View"))
                    {
                        for (int i = 0; i < EditorWindows.Count; i++)
                        {
                            if (ImGui.MenuItem(EditorWindows[i].UIName))
                            {
                                EditorWindows[i].IsActive = true;
                            }
                        }
                        ImGui.EndMenu();
                    }
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

            for (int i = 0; i < EditorWindows.Count; i++)
            {
                if (EditorWindows[i].IsActive)
                {
                    EditorWindows[i].DrawUI();
                }
            }

            if (showMetrics)
            {
                ImGui.ShowMetricsWindow();
            }

            if (showExample)
            {
                ImGui.ShowDemoWindow();
            }
        }
    }
}
