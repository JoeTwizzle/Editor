using OpenTK.Graphics.OpenGL4;
using RasterDraw.Core;
using RasterDraw.Core.Helpers;
using RasterDraw.Core.Rendering;
using RasterDraw.Core.Scripting;
using RasterDraw.GLGraphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Editor
{
    public class CompTest : GameScript
    {
        public Material Material;
        GLProgram computeProg;
        GLBuffer compBuffer;
        
        DrawCallData rtQuad = new DrawCallData();
        MeshFilter rtMF = new MeshFilter();
        Texture2D targetTex = new Texture2D();

        public override void Init()
        {
            targetTex.Init(1920, 1080);
            InitMesh();
            IRenderer.ActiveRenderer.OnPostRender += ActiveRenderer_OnPostRender;
            GLShader compShader = new GLShader(ShaderType.ComputeShader);
            compShader.LoadFromResource("RasterDraw.Shaders.ComputeTest.glsl");
            compBuffer = new GLBuffer();
            compBuffer.Init<float>(BufferType.ShaderStorageBuffer, 1);
            compBuffer.UpdateData(1f);
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

        private void ActiveRenderer_OnPostRender(ICamera cam)
        {
            i += 0.0006f;
            i %= 1;
            compBuffer.UpdateData(i);
            compBuffer.BindStorage();
            computeProg.Bind();
            targetTex.BindImage(TextureAccess.ReadWrite,1);
            GL.DispatchCompute(targetTex.Width / 16, targetTex.Height / 16, 1);
            rtQuad.Material.SetTexture(targetTex, 0);
            IRenderer.ActiveRenderer.RenderDrawCall(IRenderer.ActiveRenderer.VAOs[typeof(QuadVertex)], cam, rtQuad);
        }

        float i = 0;
        public override void Update()
        {

        }
    }
}
