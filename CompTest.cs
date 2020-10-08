using OpenTK.Graphics.OpenGL4;
using RasterDraw.Core;
using RasterDraw.Core.Helpers;
using RasterDraw.Core.Rendering;
using RasterDraw.Core.Scripting;
using RasterDraw.GLGraphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Editor
{
    public class CompTest : GameScript
    {
        [StructLayout(LayoutKind.Sequential)]
        struct ShaderData
        {
            public float BlendFactor;
            public float Offset;
        }
        public Material Material;
        GLProgram computeProg;
        GLBuffer compBuffer;

        DrawCallData rtQuad = new DrawCallData();
        MeshFilter rtMF = new MeshFilter();
        Texture2D targetTex = new Texture2D();
        Texture2D skybox = new Texture2D();
        public override void Init()
        {
            skybox.InitFromFile(@"D:\Blender\HDRI\kloppenheim_02.jpg");
            targetTex.Init(1920, 1080);
            InitMesh();
            IRenderer.ActiveRenderer.OnPostRender += ActiveRenderer_OnPostRender;
            GLShader compShader = new GLShader(ShaderType.ComputeShader);
            compShader.LoadFromResource("RasterDraw.Shaders.ComputeTest.glsl");
            compBuffer = new GLBuffer();
            compBuffer.Init<ShaderData>(BufferType.ShaderStorageBuffer, 1);
            compBuffer.UpdateData(new ShaderData());
            computeProg = new GLProgram();
            computeProg.AddShader(compShader);
            computeProg.LinkProgram();
            compShader.Free();
        }

        void InitMesh()
        {
            var FSQuadProgram = new GLProgram();
            GLShader quadFrag = new GLShader(ShaderType.FragmentShader);
            quadFrag.LoadFromStream(Utilities.GetResourceStream("RasterDraw.Shaders.SimpleFrag.glsl"));
            FSQuadProgram.AddShader(quadFrag);
            GLShader quadVert = new GLShader(ShaderType.VertexShader);
            quadVert.LoadFromStream(Utilities.GetResourceStream("RasterDraw.Shaders.SimpleVert.glsl"));
            FSQuadProgram.AddShader(quadVert);
            FSQuadProgram.LinkProgram();
            quadVert.Free();
            quadFrag.Free();
            var q = QuadMesh.FullscreenQuad;
            rtMF.Init(q, q.Vertices, q.Indices);
            rtQuad.Layers = new RasterDraw.BitFlags64(~0u);
            rtQuad.MeshFilter = rtMF;
            rtQuad.Material = new Material(FSQuadProgram, RasterizerState.Fullscreen);
        }

        float i = 0;
        ShaderData data;
        private void ActiveRenderer_OnPostRender(ICamera cam)
        {
            if (targetTex.Width != cam.RenderTexture.ColorTexture.Width ||
                targetTex.Height != cam.RenderTexture.ColorTexture.Height)
            {
                targetTex.Free();
                targetTex = new Texture2D();
                targetTex.Init(cam.RenderTexture.ColorTexture.Width, cam.RenderTexture.ColorTexture.Height);
            }
            i += 0.006f;
            data.BlendFactor = ((MathF.Sin(i) + 1f) * 0.5f) * 0.5f + 0.25f;
            data.Offset += 0.0006f;
            data.Offset %= 1;
            compBuffer.UpdateData(data);
            //
            skybox.Bind();
            compBuffer.BindStorage();
            computeProg.SetUniformMat4(0, (cam.Transform.WorldMatrix));
            computeProg.SetUniformMat4(1, cam.PerspectiveMatrix.Inverted());
            cam.RenderTexture.ColorTexture.BindImage(TextureAccess.ReadWrite, 0);
            targetTex.BindImage(TextureAccess.ReadWrite, 1);

            //
            computeProg.Bind();
            GL.DispatchCompute((int)MathF.Ceiling(targetTex.Width / 16.0f), (int)MathF.Ceiling(targetTex.Height / 16.0f), 1);
            rtQuad.Material.SetTexture(cam.RenderTexture.ColorTexture, 0);
            IRenderer.ActiveRenderer.RenderDrawCall(IRenderer.ActiveRenderer.VAOs[typeof(QuadVertex)], cam, rtQuad);
        }

        public override void Update()
        {

        }
    }
}
