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
        Compiler compiler = new Compiler();

        public BuildEditor()
        {
            UIName = "Build";
        }

        public override void DrawUI()
        {
            if (ImGui.Button("Build"))
            {
                //compiler.Compile();
            }
        }
    }
}
