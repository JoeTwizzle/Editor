//using OpenTK.Graphics.OpenGL4;
//using RasterDraw.Core;
//using RasterDraw.Core.Helpers;
//using RasterDraw.Core.Rendering;
//using RasterDraw.Core.Scripting;
//using GLGraphics;
//using System;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using System.Text;

//namespace GameEditor
//{

//    public class CompTest : GameScript
//    {
//        [StructLayout(LayoutKind.Sequential)]
//        struct ShaderData
//        {
//            public float Near;
//            public float Far;
//            public float BlendFactor;
//            public float Offset;
//        }
//        //public Material Material;
//        GLProgram computeProg;
//        GLBuffer compBuffer;

//        DrawCallData rtQuad = new DrawCallData();
//        MeshFilter rtMF = new MeshFilter();

//        Texture2D skybox;
//        public override void DrawInit()
//        {
//            skybox = TextureLoader.LoadTexture2D(@"D:\Blender\HDRI\kloppenheim_02.jpg");
//            //targetTex.Init(1920, 1080);
//            InitMesh();
//            IRenderer.ActiveRenderer.OnPostRender += ActiveRenderer_OnPostRender;
//            GLShader compShader = new GLShader(ShaderType.ComputeShader);
//            using var sc = Utilities.GetResourceStream("RasterDraw.Shaders.ComputeTest.glsl");
//            compShader.SetSource(sc.ReadToEnd());
//            computeProg = new GLProgram();
//            computeProg.AddShader(compShader);
//            computeProg.LinkProgram();
//            compShader.Dispose();
//            compBuffer = new GLBuffer();
//            compBuffer.Init<ShaderData>(BufferType.ShaderStorageBuffer, 1);
//            compBuffer.UpdateData(new ShaderData());

//        }

//        void InitMesh()
//        {
//            var FSQuadProgram = new GLProgram();

//            GLShader quadFrag = new GLShader(ShaderType.FragmentShader);
//            using var sf = Utilities.GetResourceStream("RasterDraw.Shaders.SimpleFrag.glsl");
//            quadFrag.SetSource(sf.ReadToEnd());
//            FSQuadProgram.AddShader(quadFrag);

//            GLShader quadVert = new GLShader(ShaderType.VertexShader);
//            using var sv = Utilities.GetResourceStream("RasterDraw.Shaders.SimpleVert.glsl");
//            quadVert.SetSource(sv.ReadToEnd());
//            FSQuadProgram.AddShader(quadVert);

//            FSQuadProgram.LinkProgram();
//            quadVert.Dispose();
//            quadFrag.Dispose();
//            var q = Meshes.FullscreenQuad;
//            rtMF.Init(q, q.Vertices, q.Indices);
//            rtQuad.Layers = new RasterDraw.BitFlags64(~0u);
//            rtQuad.MeshFilter = rtMF;
//            rtQuad.Material = new Material(FSQuadProgram, RasterizerState.Fullscreen);
//        }

//        private void ActiveRenderer_OnPostRender(ICamera cam)
//        {

//            //if (targetTex.Width != cam.RenderTexture.ColorTexture.Width ||
//            //    targetTex.Height != cam.RenderTexture.ColorTexture.Height)
//            //{
//            //    targetTex.Free();
//            //    targetTex = new Texture2D();
//            //    targetTex.Init(cam.RenderTexture.ColorTexture.Width, cam.RenderTexture.ColorTexture.Height);
//            //}
//            //i += 0.006f;
//            //data.BlendFactor = ((MathF.Sin(i) + 1f) * 0.5f) * 0.5f + 0.25f;
//            //data.Offset += 0.0006f;
//            //data.Offset %= 1;
//            //data.Near = cam.Near;
//            //data.Far = cam.Far;
//            //compBuffer.UpdateData(data);
//            //
//            var targetTex = cam.RenderTexture.ColorTexture;
//            if (IsActiveAndEnabled)
//            {
//                skybox.Bind(0);
//                cam.RenderTexture.DepthTexture.Bind(1);
//                compBuffer.BindStorage();
//                computeProg.SetUniformMat4(0, (cam.Transform.WorldMatrix));
//                computeProg.SetUniformMat4(1, cam.PerspectiveMatrix.Inverted());
//                targetTex.BindImage(TextureAccess.ReadWrite, 0);
//                //
//                computeProg.Bind();
//                GL.DispatchCompute((int)MathF.Ceiling(targetTex.Width / 16.0f), (int)MathF.Ceiling(targetTex.Height / 16.0f), 1);
//            }


//            rtQuad.Material.SetTexture(targetTex, 0);
//            IRenderer.ActiveRenderer.RenderDrawCall(cam, rtQuad);
//        }
//    }
//}
