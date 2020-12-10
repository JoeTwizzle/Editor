using OpenTK.Graphics.OpenGL4;
using RasterDraw.Core;
using RasterDraw.Core.Rendering;
using RasterDraw.Core.Scripting;
using GLGraphics;
using System;
using System.Collections.Generic;
using System.Text;
using RasterDraw.Core.Helpers;

namespace GameEditor
{
    class SkyboxFX : GameScript
    {
        Texture2D Skybox;
        PostProcessEffect SkyboxEffect;
        public override void Init()
        {
            Skybox = new Texture2D();
            new TextureData(@"D:\Blender\HDRI\kloppenheim_02.jpg").InitializeTexture(Skybox);

            GLShader vertShader = new GLShader(ShaderType.VertexShader);
            using var sv = Utilities.GetResourceStream("RasterDraw.Shaders.PostProcessing.SkyboxVert.glsl");
            vertShader.SetSource(sv.ReadToEnd());

            GLShader fragShader = new GLShader(ShaderType.FragmentShader);
            using var sf = Utilities.GetResourceStream("RasterDraw.Shaders.PostProcessing.SkyboxVert.glsl");
            fragShader.SetSource(sf.ReadToEnd());

            var FXProgram = new GLProgram();
            FXProgram.AddShader(vertShader);
            FXProgram.AddShader(fragShader);
            FXProgram.LinkProgram();
            vertShader.Dispose();
            fragShader.Dispose();

            Material material = new Material(FXProgram, RasterizerState.Fullscreen);
            SkyboxEffect = new PostProcessEffect(material, RenderFX);
        }

        private void RenderFX(ICamera cam)
        {
            SkyboxEffect.Program.SetUniformMat4(0, (cam.Transform.WorldMatrix));
            SkyboxEffect.Program.SetUniformMat4(1, cam.PerspectiveMatrix.Inverted());
            cam.RenderTexture.ColorTexture.Bind(0);
            cam.RenderTexture.DepthTexture.Bind(1);
            Skybox.Bind(2);
            SkyboxEffect.Present(cam);
        }
    }
}
