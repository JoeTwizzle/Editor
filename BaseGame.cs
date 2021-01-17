﻿using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using RasterDraw.Assets;
using RasterDraw.Core;
using RasterDraw.Core.GUI;
using RasterDraw.Helpers;
using RasterDraw.Core.NativeScripts;
using RasterDraw.Core.Rendering;
using RasterDraw.Core.Scripting;
using GLGraphics;
using GLGraphics.Helpers;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using RasterDraw.Core.Physics;
using System.Reflection;
using RasterDraw.Rendering;

namespace GameEditor
{
    public class BaseGame : GameWindow
    {
        public BaseGame() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {

        }
        public BaseGame(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {

        }

        DateTime p1;
        DateTime p2;
        EditorManager EditorManager;
        public static bool Play = false;
        protected override void OnResize(ResizeEventArgs e)
        {
            p2 = p1;
            p1 = DateTime.UtcNow;
            base.OnResize(e);
           
            if (Play)
            {
                OnUpdateFrame(new FrameEventArgs(p1.TimeOfDay.TotalSeconds - p2.TimeOfDay.TotalSeconds));
            }

            if (!IsMultiThreaded)
            {
                OnRenderFrame(new FrameEventArgs(p1.TimeOfDay.TotalSeconds - p2.TimeOfDay.TotalSeconds));
            }

            if (!ShowGame)
            {
                EditorManager.Resize(e.Size);
            }
            else
            {
                RenderTexture.DefaultWidth = ClientSize.X;
                RenderTexture.DefaultHeight = ClientSize.Y;
                for (int i = 0; i < IRenderCore.CurrentRenderCore.Cameras.Count; i++)
                {
                    var cam = IRenderCore.CurrentRenderCore.Cameras[i];
                    if (!cam.KeepAspect)
                    {
                        cam.Viewport = new Viewport(new Box2i(Vector2i.Zero, e.Size));
                    }
                }
            }
        }



        GameLoop GameLoop;
        GameLoop EditorLoop;
        PhysicsEngine PhysicsEngine;

        bool ShowGame;

        void Init()
        {
            Console.WriteLine(GL.GetString(StringName.Renderer));
            Console.WriteLine(GL.GetString(StringName.Version));

            AssetLoader.Init();
            RenderCore.Create();
            GameLoop = new GameLoop(this);
            EditorLoop = new GameLoop(this);
            PhysicsEngine = new PhysicsEngine();
            EditorManager = new EditorManager(ClientSize);
            EditorManager.GameGameLoop = GameLoop;
            GameObject editorManager = new GameObject("Editor Manager");
            editorManager.AddScript(EditorManager);
            EditorManager.Enabled = false;
            EditorLoop.Add(editorManager);
        }

        protected override void OnLoad()
        {
            //Initialize opengl debug callback
            _debugProcCallbackHandle = GCHandle.Alloc(_debugProcCallback);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
            
            //Init game contexts
            Init();

            //Load test assets
            var sponza = AssetLoader.LoadAssets(@"D:\Blender\Models\sponza-gltf-pbr\sponza.glb", 1f);
            var zelda = AssetLoader.LoadAssets(@"D:\Blender\Models\JourneyPBR\Export\untitled.glb", 0.01f);
            var car = AssetLoader.LoadAssets(@"D:\Blender\Models\Audi R8\Models\Audi R8.fbx", 0.1f);
            var Sphere = AssetLoader.LoadAssets(@"D:\Blender\Models\Sphere.fbx", 1f);
            //var mc = AssetLoader.LoadAssets(@"D:\Blender\Models\beta map split.obj", 1f);

            //mc.Root.Transform.Position -= Vector3.UnitY * 128;
            //GameLoop.Instantiate(mc, null);
            GameLoop.Instantiate(Sphere, null);
            GameLoop.Instantiate(car, null);
            GameLoop.Instantiate(zelda, null);
            GameLoop.Instantiate(sponza, null);

            //Add RigidBodys
            var rb1 = new RigidBody();
            rb1.DetectionSettings = RigidBody.ContinuousDetectionSettings;
            rb1.Shape = new BepuPhysics.Collidables.Sphere(Sphere.Root.Transform.Scale.X);
            Sphere.Root.AddScript(rb1);

            var rb2 = new RigidBody();
            rb2.RigidBodyType = RigidBodyType.Kinematic;
            rb2.Shape = new BepuPhysics.Collidables.Box(1000, 0.1f, 1000);
            sponza.Root.AddScript(rb2);
           
            //Add Game Camera
            var c = new Camera(new Viewport(ClientRectangle));
            GameObject CameraObj = new GameObject("Main Camera");
            CameraObj.AddScript(c);
            GameLoop.Add(CameraObj);

            //Spinny thingy
            zelda.Root.AddScript(new TurnTable());
            var t = Sphere.Root.GetComponent<Transform>()!;
            t.Position = new Vector3(0, 4, 0);
            t.Scale = new Vector3(0.1f);

            Input.Keyboard = KeyboardState;
            Input.Mouse = MouseState;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            p2 = p1;
            p1 = DateTime.UtcNow;
            Input.Keyboard = KeyboardState;
            Input.Mouse = MouseState;
            if (KeyboardState.IsKeyDown(Keys.V) && !KeyboardState.WasKeyDown(Keys.V))
            {
                if (VSync == VSyncMode.Adaptive)
                {
                    VSync = VSyncMode.Off;
                }
                else
                {
                    VSync = VSyncMode.Adaptive;
                }
            }


            if (KeyboardState.IsKeyDown(Keys.F10) && !KeyboardState.WasKeyDown(Keys.F10))
            {
                ShowGame = !ShowGame;
                if (!ShowGame)
                {
                    EditorManager.Resize(ClientSize);
                }
                else
                {
                    RenderTexture.DefaultWidth = ClientSize.X;
                    RenderTexture.DefaultHeight = ClientSize.Y;
                    for (int i = 0; i < IRenderCore.CurrentRenderCore.Cameras.Count; i++)
                    {
                        var cam = IRenderCore.CurrentRenderCore.Cameras[i];
                        if (!cam.KeepAspect)
                        {
                            cam.Viewport = new Viewport(new Box2i(Vector2i.Zero, ClientSize));
                        }
                    }
                }
            }

            if (Play)
            {
                GameLoop.Update((float)args.Time);
                PhysicsEngine.Simulate((float)args.Time);
            }
            EditorLoop.Update((float)args.Time);
            if (!ShowGame)
            {
                EditorManager.Update(this, (float)args.Time);
            }

        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Input.Keyboard = KeyboardState;
            Input.Mouse = MouseState;

            GLObjectCleaner.Update((float)args.Time);

            GameLoop.Draw((float)args.Time);
            IRenderCore.CurrentRenderCore.DrawObjects();
            IRenderCore.CurrentRenderCore.ClearDrawCalls();

            RenderTexture.BindDefault();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            if (!ShowGame)
            {
                EditorLoop.Draw((float)args.Time);
            }
            else
            {
                ICamera.MainCamera.RenderTexture.ColorTexture.Bind();
                IRenderCore.CurrentRenderCore.FullscreenPass();
            }
            SwapBuffers();
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);


            EditorManager.CharPressed((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            EditorManager.MouseScroll(e.Offset);
        }

        static DebugProc _debugProcCallback = LogDebug;
        static GCHandle _debugProcCallbackHandle;

        static void LogDebug(DebugSource debugSource, DebugType debugType, int Id, DebugSeverity debugSeverity, int length, IntPtr message, IntPtr userParams)
        {
            switch (debugSeverity)
            {
                case DebugSeverity.DontCare:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case DebugSeverity.DebugSeverityNotification:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case DebugSeverity.DebugSeverityHigh:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case DebugSeverity.DebugSeverityMedium:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case DebugSeverity.DebugSeverityLow:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    break;
            }
            Console.WriteLine($"{debugSource} {debugSeverity} {debugType} | {Marshal.PtrToStringAnsi(message, length)} ID:{Id}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        //    protected override void OnLoad()
        //    {

        //        v = new Vertex[]
        //        {
        //            new Vertex(new Vector3( -1.0f,-1.0f,-1.0f)), // triangle 1 : begin
        //            new Vertex(new Vector3( -1.0f,-1.0f, 1.0f)),
        //            new Vertex(new Vector3( -1.0f, 1.0f, 1.0f)), // triangle 1 : end
        //            new Vertex(new Vector3( 1.0f, 1.0f,-1.0f)), // triangle 2 : begin
        //            new Vertex(new Vector3( -1.0f,-1.0f,-1.0f)),
        //            new Vertex(new Vector3( -1.0f, 1.0f,-1.0f)), // triangle 2 : end
        //            new Vertex(new Vector3( 1.0f,-1.0f, 1.0f)),
        //            new Vertex(new Vector3( -1.0f,-1.0f,-1.0f)),
        //            new Vertex(new Vector3( 1.0f,-1.0f,-1.0f)),
        //            new Vertex(new Vector3( 1.0f, 1.0f,-1.0f)),
        //            new Vertex(new Vector3( 1.0f,-1.0f,-1.0f)),
        //            new Vertex(new Vector3( -1.0f,-1.0f,-1.0f)),
        //            new Vertex(new Vector3( -1.0f,-1.0f,-1.0f)),
        //            new Vertex(new Vector3( -1.0f, 1.0f, 1.0f)),
        //            new Vertex(new Vector3( -1.0f, 1.0f,-1.0f)),
        //            new Vertex(new Vector3( 1.0f,-1.0f, 1.0f)),
        //            new Vertex(new Vector3( -1.0f,-1.0f, 1.0f)),
        //            new Vertex(new Vector3( -1.0f,-1.0f,-1.0f)),
        //            new Vertex(new Vector3( -1.0f, 1.0f, 1.0f)),
        //            new Vertex(new Vector3( -1.0f,-1.0f, 1.0f)),
        //            new Vertex(new Vector3( 1.0f,-1.0f, 1.0f)),
        //            new Vertex(new Vector3( 1.0f, 1.0f, 1.0f)),
        //            new Vertex(new Vector3( 1.0f,-1.0f,-1.0f)),
        //            new Vertex(new Vector3( 1.0f, 1.0f,-1.0f)),
        //            new Vertex(new Vector3( 1.0f,-1.0f,-1.0f)),
        //            new Vertex(new Vector3( 1.0f, 1.0f, 1.0f)),
        //            new Vertex(new Vector3( 1.0f,-1.0f, 1.0f)),
        //            new Vertex(new Vector3( 1.0f, 1.0f, 1.0f)),
        //            new Vertex(new Vector3( 1.0f, 1.0f,-1.0f)),
        //            new Vertex(new Vector3( -1.0f, 1.0f,-1.0f)),
        //            new Vertex(new Vector3( 1.0f, 1.0f, 1.0f)),
        //            new Vertex(new Vector3( -1.0f, 1.0f,-1.0f)),
        //            new Vertex(new Vector3( -1.0f, 1.0f, 1.0f)),
        //            new Vertex(new Vector3( 1.0f, 1.0f, 1.0f)),
        //            new Vertex(new Vector3( -1.0f, 1.0f, 1.0f)),
        //            new Vertex(new Vector3( 1.0f,-1.0f, 1.0f ))};
        //        GL.ClearColor(Color4.Black);
        //        unsafe
        //        { 
        //            //GLFW.CreateWindow(640, 480, "Help me!!!", null, WindowPtr);
        //            //GLFW.SetWindowAttrib(WindowPtr, WindowAttributeSetter.Decorated, false);
        //        }

        //        program = new ShaderProgram();

        //        Shader vShader = new Shader(ShaderType.VertexShader);
        //        vShader.SetSource(
        //        @"#version 450
        //        in vec3 Position;

        //        uniform mat4 u_MVP;

        //        void main()
        //        {
        //            gl_Position = u_MVP * vec4(Position, 1.0);
        //        }");

        //        Shader fShader = new Shader(ShaderType.FragmentShader);
        //        fShader.SetSource(
        //        @"#version 450
        //        out vec4 color;

        //        void main()
        //        {
        //           color = vec4(1,1,1,1);
        //        }");

        //        program.AddShader(vShader);
        //        checkError();
        //        program.AddShader(fShader);
        //        VAO = new VertexArray();
        //        VAO.Bind();
        //        buffer = new GLBuffer();
        //        buffer.Init(BufferType.ArrayBuffer, v);
        //        VAO.BindAttribute(program.ProgramInfo.GetAttribLocation("Position"), buffer, 3, VertexAttribPointerType.Float, buffer.DataSize, 0, false);
        //        Matrix4 m = Matrix4.CreateOrthographic(10, 10, 0.01f, 100);
        //        GL.ProgramUniformMatrix4(program.Handle, program.ProgramInfo.GetUniformLocation("MVP"), false, ref m);
        //        Texture2D texture = new Texture2D();
        //        texture.InitFromFile(@"C:\Users\Jaouad-PC\Desktop\unknown.png");
        //    }
        //    protected override void OnUpdateFrame(FrameEventArgs args)
        //    {
        //        GLObjectCleaner.Update((float)args.Time);
        //    }
        //    protected override void OnRenderFrame(FrameEventArgs args)
        //    {

        //        Graphics.Clear();
        //        checkError();
        //        GL.DrawArrays(PrimitiveType.Triangles, 0, v.Length);
        //        Graphics.Print();
        //    }
        //}
        //[StructLayout(LayoutKind.Sequential)]
        //struct Vertex
        //{
        //    Vector3 Position;

        //    public Vertex(Vector3 position)
        //    {
        //        Position = position;
        //    }
        //}
    }
}