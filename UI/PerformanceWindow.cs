using System;
using System.Collections.Generic;
using RasterDraw.Core.Scripting;
using ImGuiNET;
using System.Threading.Tasks;
using RasterDraw.Rendering;

namespace GameEditor.UI
{
    public class PerformanceWindow : EditorWindow
    {
        public PerformanceWindow()
        {
            UIName = "Performance";
        }
        float[] vals = new float[1000];
        int i = 0;
        public override void DrawUI()
        {
            vals[i] = Manager.GameObject.GameLoop.DeltaTime * 1000;
            i++;
            i %= vals.Length;
            if (ImGui.Begin(UIName, ref IsActive, ImGuiWindowFlags.NoCollapse))
            {
                ImGui.Text("Polygons: " + IRenderCore.CurrentRenderCore.GetPolyCount());
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                var region = ImGui.GetContentRegionAvail();
                ImGui.PlotLines("Frame time", ref vals[0], vals.Length, 0, "", 0, 40, region);
            }
            ImGui.End();
        }
    }
}
