using ImGuiNET;
using OpenTK.Mathematics;
using RasterDraw.Core;
using RasterDraw.Core.Physics;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using GameEditor.Builds;

namespace GameEditor.UI
{
    public class BuildEditor : EditorWindow
    {
        public BuildEditor()
        {
            UIName = "Build";
        }

        public override void DrawUI()
        {
            if (ImGui.Begin(UIName, ref IsActive, ImGuiWindowFlags.NoCollapse))
            {
                if (ImGui.Button("Create New"))
                {
                    Compiler.CreateNewProject();
                }
                if (ImGui.Button("Build"))
                {
                    Compiler.BuildProject();
                }
                if (ImGui.Button("Load"))
                {
                    Compiler.LoadProject();
                }
            }
            ImGui.End();
        }
    }
}
