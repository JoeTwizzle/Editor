using ImGuiNET;
using OpenTK.Mathematics;
using RasterDraw.Core;
using RasterDraw.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEditor.UI
{
    public class ViewportWindow : EditorWindow
    {

        public ViewportWindow()
        {
            UIName = "Viewport";
        }

        System.Numerics.Vector2 gameRegion;
        public override void DrawUI()
        {
            if (ImGui.Begin(UIName, ref IsActive, ImGuiWindowFlags.NoCollapse))
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
        }
        void OnResizeGame()
        {
            RenderTexture.DefaultWidth = (int)gameRegion.X;
            RenderTexture.DefaultHeight = (int)gameRegion.Y;
            ICamera.MainCamera.Viewport = new Viewport(new Box2i(Vector2i.Zero, new Vector2i((int)gameRegion.X, (int)gameRegion.Y)));
        }
    }
}
