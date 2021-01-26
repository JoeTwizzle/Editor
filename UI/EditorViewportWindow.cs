using ImGuiNET;
using OpenTK.Mathematics;
using RasterDraw.Core;
using RasterDraw.Core.Rendering;
using RasterDraw.Core.Scripting;
using RasterDraw.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEditor.UI
{
    class EditorViewportWindow : EditorWindow
    {
        public EditorViewportWindow()
        {
            UIName = "Editor Viewport";
        }
        System.Numerics.Vector2 gameRegion;
        public override void DrawUI()
        {
            if (ImGui.Begin(UIName, ref IsActive, ImGuiWindowFlags.NoCollapse))
            {
                System.Numerics.Vector2 region = ImGui.GetContentRegionAvail();
                //Console.WriteLine("Editor cam: " + Manager.cam.RenderTexture.ColorTexture.Handle);
                ImGui.Image(new IntPtr(Manager.cam.RenderTexture.ColorTexture.Handle), region, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
                if (gameRegion != region)
                {
                    gameRegion = region;
                    OnResizeGame();
                }
            }
            ImGui.End();
        }

        public void InvalidateRegion()
        {
            gameRegion = new System.Numerics.Vector2(-1, -1);
        }

        void OnResizeGame()
        {
            Manager.cam.Viewport = new Viewport(new Box2i(Vector2i.Zero, new Vector2i((int)gameRegion.X, (int)gameRegion.Y)));
        }

        void SelectionChanged(GameObject selected)
        {
            Manager.GetWindow<InspectorWindow>()!.SetSelectedObj(selected);
        }
    }
}
