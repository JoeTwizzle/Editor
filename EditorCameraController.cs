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
    public class EditorCameraController : GameScript
    {
        public override void Init()
        {
            Transform = GameObject.Transform;
        }
        float angleX, angleY;
        Transform Transform;
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
