using ImGuiNET;
using OpenTK.Mathematics;
using RasterDraw.Core;
using RasterDraw.Core.Rendering;
using RasterDraw.Core.Scripting;
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace GameEditor.UI
{
    public class GameViewportWindow : EditorWindow
    {

        public GameViewportWindow()
        {
            UIName = "Game Viewport";
        }
        System.Numerics.Vector2 gameRegion;
        public override void DrawUI()
        {
            if (ImGui.Begin(UIName, ref IsActive, ImGuiWindowFlags.NoCollapse))
            {
                System.Numerics.Vector2 region = ImGui.GetContentRegionAvail();
                //Console.WriteLine("Game cam: " + ICamera.MainCamera.RenderTexture.ColorTexture.Handle);
                ImGui.Image(new IntPtr(ICamera.MainCamera.RenderTexture.ColorTexture.Handle), region, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
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
            ICamera.MainCamera.Viewport = new Viewport(new Box2i(Vector2i.Zero, new Vector2i((int)gameRegion.X, (int)gameRegion.Y)));
        }
    }
}
