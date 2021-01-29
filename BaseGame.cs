using ImGuiNET;
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
using RasterDraw.Rendering.PostFX;

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


            if (!IsMultiThreaded)
            {
                if (Play)
                {
                    OnUpdateFrame(new FrameEventArgs(p1.TimeOfDay.TotalSeconds - p2.TimeOfDay.TotalSeconds));
                }
                OnRenderFrame(new FrameEventArgs(p1.TimeOfDay.TotalSeconds - p2.TimeOfDay.TotalSeconds));
            }
            resize = true;
        }
        protected override void OnRefresh()
        {
            resize = true;
            base.OnRefresh();
        }
        GameLoop GameLoop;
        GameLoop EditorLoop;
        PhysicsEngine PhysicsEngine;
        public static RenderTexture DisplayRT;
        GLProgram FullscreenQuadProgram;
        bool editMode;

        void Init()
        {
            Console.WriteLine(GL.GetString(StringName.Renderer));
            Console.WriteLine(GL.GetString(StringName.Version));
            AssetLoader.Init();
            RenderCore.Create();
            GameLoop = new GameLoop(this);
            EditorLoop = new GameLoop(this);
            DisplayRT = new RenderTexture();
            FullscreenQuadProgram = ShaderUtils.CreateFromResource("RasterDraw.Shaders.SimpleVert.glsl", "RasterDraw.Shaders.SimpleFrag.glsl");
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
            ICamera.MainCamera = c;
            GameObject CameraObj = new GameObject("Main Camera");
            CameraObj.AddScript(c);
            var stack = new PostProcessStack();
            stack.AddEffect(new BloomEffect());
            stack.AddEffect(new ToneMapACES());
            CameraObj.AddScript(stack);
            GameLoop.Add(CameraObj);

            //Spinny thingy
            zelda.Root.AddScript(new TurnTable());
            var t = Sphere.Root.GetComponent<Transform>()!;
            t.Position = new Vector3(0, 4, 0);
            t.Scale = new Vector3(0.1f);

            Input.Keyboard = KeyboardState;
            Input.Mouse = MouseState;

            EditMode = true;
        }

        bool resize = false;
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
                resize = true;
                EditMode = !EditMode;
            }         

            if (Play)
            {
                GameLoop.Update((float)args.Time);
                PhysicsEngine.Simulate((float)args.Time);
            }
            EditorLoop.Update((float)args.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (resize)
            {
                ModeChanged();
                if (EditMode)
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
                resize = false;
            }

            GLObjectCleaner.Update((float)args.Time);

            GameLoop.Draw((float)args.Time);
            EditorLoop.Draw((float)args.Time);
            IRenderCore.CurrentRenderCore.DrawObjects();
            GameLoop.PostDraw();
            var rt = ICamera.MainCamera.RenderTexture;
            if (DisplayRT.Width != rt.Width || DisplayRT.Height != rt.Height)
            {
                DisplayRT.Dispose();
                DisplayRT.Init(rt.Width, rt.Height, 1, rt.ColorTexture.TextureFormat, rt.DepthTexture.TextureFormat);
            }
            rt.FrameBuffer.Blit(DisplayRT.FrameBuffer, new Box2i(0, 0, rt.Width, rt.Height), new Box2i(0, 0, DisplayRT.Width, DisplayRT.Height));
            EditorLoop.PostDraw();
            IRenderCore.CurrentRenderCore.ClearDrawCalls();

            RenderTexture.BindDefault();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            if (EditMode)
            {
                EditorManager.DrawUI();
                EditorManager.Render();
            }
            else
            {
                DisplayRT.ColorTexture.Bind();
                FullscreenQuadProgram.Bind();
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

        public bool EditMode
        {
            get => editMode; set
            {
                editMode = value;
            }
        }

        private void ModeChanged()
        {
            for (int i = 0; i < IRenderCore.CurrentRenderCore.Cameras.Count; i++)
            {
                IRenderCore.CurrentRenderCore.Cameras[i].MultiSample = !editMode;
            }
        }

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
    }
}