using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using RasterDraw;
using RasterDraw.Core;
using RasterDraw.Core.NativeScripts;
using RasterDraw.Core.Rendering;
using RasterDraw.Core.Scripting;
using RasterDraw.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEditor
{
    public class EditorCamera : GameScript, ICamera
    {
        public bool MultiSample { get; set; }
        public bool KeepAspect { get; set; }
        private BitFlags64 layers;
        public Matrix4 ViewMatrix { get { return Matrix4.LookAt(Transform.LocalPosition, Transform.LocalPosition + Transform.LocalForward, Transform.LocalUp); } }
        public Matrix4 PerspectiveMatrix { get { return ComputePerspective(); } }
        public Matrix4 ProjectionMatrix { get { return ViewMatrix * PerspectiveMatrix; } }

        private int ssaa = 1;
        public int SSAA
        {
            get => ssaa; set
            {
                ssaa = Math.Max(value, 1);
            }
        }

        private float fov = 90;
        public float AspectRatio { get; set; } = 1;

        private Viewport viewport;
        private float near;
        private float far;

        Matrix4 ComputePerspective()
        {
            //return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), AspectRatio, Near, Far);
            //float f = 1.0f / MathF.Tan(MathHelper.DegreesToRadians(FOV) / 2.0f);
            try
            {
                var result = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), RenderTexture.Width / (float)RenderTexture.Height, Near, Far);
                result.Row2.X = 0;
                result.Row2.Y = 0;
                result.Row2.Z = (Near / (Far - Near));
                result.Row3.Z = (Far * Near) / (Far - Near);
                return result;
            }
            catch
            {
                return Matrix4.Identity;
            }
        }

        public RenderTexture RenderTexture { get; private set; }
        public Transform Transform { get; set; }
        public BitFlags64 Layers { get => layers; set => layers = value; }
        public Viewport Viewport
        {
            get => viewport;
            set
            {
                viewport = value;
                RenderTexture.Dispose();
                RenderTexture = new RenderTexture();
                RenderTexture.Init(Viewport.Resolution.X * SSAA, Viewport.Resolution.Y * SSAA);
                RenderTexture.FrameBuffer.SetLabel("EDITOR CAMERA FBO");
            }
        }

        public float Near
        {
            get => near; set
            {
                if (value <= 0)
                {
                    return;
                }
                near = value;
            }
        }

        public float Far
        {
            get => far; set
            {
                if (value <= near)
                {
                    return;
                }
                far = value;
            }
        }

        public float FOV
        {
            get => fov; set
            {
                fov = MathHelper.Clamp(value, 0.1f, 179.9f);
            }
        }

        public EditorCamera(Viewport viewport)
        {
            RenderTexture = new RenderTexture();
            RenderTexture.Init(Viewport.Resolution.X * SSAA, Viewport.Resolution.Y * SSAA);
            Viewport = viewport;
            Layers = new BitFlags64(~0uL);
            near = 0.06f;
            far = 100000;
            FOV = 90f;
            Transform = Transform.Create();
            IRenderCore.CurrentRenderCore.Cameras.Add(this);
        }
        float angleX, angleY;

        public override void Update()
        {
            float speedRot = 3f;
            float speed = 3f;

            if (Input.Keyboard.IsKeyDown(Keys.Escape) && !Input.Keyboard.WasKeyDown(Keys.Escape))
            {
                GameObject.GameLoop.NativeWindow.CursorGrabbed = !GameObject.GameLoop.NativeWindow.CursorGrabbed;
                if (!GameObject.GameLoop.NativeWindow.CursorGrabbed)
                {
                    GameObject.GameLoop.NativeWindow.CursorVisible = true;
                }
            }
            if (GameObject.GameLoop.NativeWindow.CursorGrabbed)
            {
                angleX -= Input.Mouse.Delta.X * 0.005f;
                angleY += Input.Mouse.Delta.Y * 0.005f;
                angleY = MathHelper.Clamp(angleY, -MathHelper.PiOver2 + 0.001f, MathHelper.PiOver2 - 0.001f);
            }
            var fwd = Transform.LocalForward;
            fwd = new Vector3(fwd.X, 0, fwd.Z).Normalized();
            if (Input.Keyboard.IsKeyDown(Keys.W))
            {
                Transform.LocalPosition += fwd * speed * GameObject.GameLoop.DeltaTime;
            }
            if (Input.Keyboard.IsKeyDown(Keys.S))
            {
                Transform.LocalPosition -= fwd * speed * GameObject.GameLoop.DeltaTime;
            }
            if (Input.Keyboard.IsKeyDown(Keys.A))
            {
                Transform.LocalPosition += Transform.LocalRight * speed * GameObject.GameLoop.DeltaTime;
            }
            if (Input.Keyboard.IsKeyDown(Keys.D))
            {
                Transform.LocalPosition -= Transform.LocalRight * speed * GameObject.GameLoop.DeltaTime;
            }
            if (Input.Keyboard.IsKeyDown(Keys.Space))
            {
                Transform.LocalPosition += new Vector3(0, GameObject.GameLoop.DeltaTime, 0) * speed;
            }
            if (Input.Keyboard.IsKeyDown(Keys.LeftShift))
            {
                Transform.LocalPosition -= new Vector3(0, GameObject.GameLoop.DeltaTime, 0) * speed;
            }

            if (Input.Keyboard.IsKeyDown(Keys.Left))
            {
                angleX += (float)GameObject.GameLoop.DeltaTime * speedRot;
            }
            if (Input.Keyboard.IsKeyDown(Keys.Right))
            {
                angleX -= GameObject.GameLoop.DeltaTime * speedRot;
            }
            if (Input.Keyboard.IsKeyDown(Keys.Down))
            {
                angleY += GameObject.GameLoop.DeltaTime * speedRot;
            }
            if (Input.Keyboard.IsKeyDown(Keys.Up))
            {
                angleY -= GameObject.GameLoop.DeltaTime * speedRot;
            }
            if (Input.Keyboard.IsKeyDown(Keys.Enter))
            {
                GC.Collect();
            }
            Transform.LocalRotation = Quaternion.FromAxisAngle(Vector3.UnitY, angleX) * Quaternion.FromAxisAngle(Vector3.UnitX, angleY);
        }
    }
}
