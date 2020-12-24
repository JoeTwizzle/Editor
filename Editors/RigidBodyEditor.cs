using RasterDraw.Core.Physics;
using System;
using System.Collections.Generic;
using ImGuiNET;
using BepuPhysics.Collidables;
using System.Numerics;

namespace GameEditor.Editors
{
    [CustomEditor(typeof(RigidBody))]
    class RigidBodyEditor : Editor
    {
        RigidBody rigidBody;
        public override void Init()
        {
            rigidBody = (RigidBody)TargetObj;
            switch (rigidBody.Shape)
            {
                case Sphere s:
                    ShapeIndex = 0;
                    r = s.Radius;
                    break;
                case Box s:
                    ShapeIndex = 1;
                    l = s.Length;
                    w = s.Width;
                    h = s.Height;
                    break;
                case Capsule s:
                    ShapeIndex = 2;
                    r = s.Radius;
                    l = s.Length;
                    break;
                case Cylinder s:
                    ShapeIndex = 3;
                    r = s.Radius;
                    l = s.Length;
                    break;
                case Triangle s:
                    ShapeIndex = 4;
                    a = s.A;
                    b = s.B;
                    c = s.C;
                    break;
                case ConvexHull s:
                    ShapeIndex = 5;
                    break;
                default:
                    break;
            }
            RigidBodyTypes = Enum.GetNames(typeof(RigidBodyType));
            RigidBodyTypeIndex = (int)rigidBody.RigidBodyType;
            Shapes = new string[] { "Sphere", "Box", "Capsule", "Cylinder", "Triangle", "Convex Hull" };
            DetectionTypes = new string[] { "Discrete", "Passive", "Continuous" };
            DetectionIndex = (int)rigidBody.DetectionSettings.Mode;
        }

        string[] RigidBodyTypes;
        int RigidBodyTypeIndex;
        string[] Shapes;
        int ShapeIndex;
        string[] DetectionTypes;
        int DetectionIndex;
        bool shapeChanged;
        public override void OnDrawUI()
        {

            shapeChanged = false;
            float xSpace = ImGui.GetContentRegionAvail().X;
            ImGui.Columns(2, rigidBody.UIDText, false);
            ImGui.SetColumnWidth(0, xSpace * 0.4f);
            ImGui.Text("RigidBody Type");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (ImGui.Combo($"##{rigidBody.UIDText}RigidBody Type", ref RigidBodyTypeIndex, RigidBodyTypes, RigidBodyTypes.Length))
            {
                rigidBody.RigidBodyType = (RigidBodyType)Enum.Parse(typeof(RigidBodyType), RigidBodyTypes[RigidBodyTypeIndex]);
            }
            ImGui.NextColumn();
            ImGui.Text("Detection Mode");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (ImGui.Combo($"##{rigidBody.UIDText}Detection Mode", ref DetectionIndex, DetectionTypes, DetectionTypes.Length))
            {
                if (DetectionIndex == 0)
                {
                    rigidBody.DetectionSettings = ContinuousDetectionSettings.Discrete;
                }
                else if (DetectionIndex == 1)
                {
                    rigidBody.DetectionSettings = ContinuousDetectionSettings.Passive;
                }
                else if (DetectionIndex == 2)
                {
                    rigidBody.DetectionSettings = ContinuousDetectionSettings.Continuous(1e-3f, 0.35e-2f);
                }
            }
            ImGui.NextColumn();
            ImGui.Text("Shape");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            shapeChanged |= ImGui.Combo($"##{rigidBody.UIDText} Shape", ref ShapeIndex, Shapes, Shapes.Length);
            ImGui.Columns(1);
            CreateShape();
        }

        float r = 1, w = 1, h = 1, l = 1;
        Vector3 a = new Vector3(1, 0, 0), b = new Vector3(0, 1, 0), c = new Vector3(0, 0, 1);
        void CreateShape()
        {
            if (ShapeIndex == 0)
            {
                var ass = (Sphere)rigidBody.Shape;
                r = ass.Radius;
                shapeChanged |= EditorHelper.DrawMember(rigidBody.UIDText, "Radius", ref r, typeof(float));
                r = MathF.Max(0.001f, r);
                if (shapeChanged)
                {
                    rigidBody.Shape = new Sphere(r);
                }
            }
            if (ShapeIndex == 1)
            {
                shapeChanged |= EditorHelper.DrawMember(rigidBody.UIDText, "Width", ref w, typeof(float));
                w = MathF.Max(0.001f, w);

                shapeChanged |= EditorHelper.DrawMember(rigidBody.UIDText, "Height", ref h, typeof(float));
                h = MathF.Max(0.001f, h);

                shapeChanged |= EditorHelper.DrawMember(rigidBody.UIDText, "Length", ref l, typeof(float));
                l = MathF.Max(0.001f, l);

                if (shapeChanged)
                {
                    rigidBody.Shape = new Box(w, h, l);
                }
            }
            if (ShapeIndex == 2)
            {
                shapeChanged |= EditorHelper.DrawMember(rigidBody.UIDText, "Radius", ref r, typeof(float));
                r = MathF.Max(0.001f, r);
                shapeChanged |= EditorHelper.DrawMember(rigidBody.UIDText, "Length", ref l, typeof(float));
                l = MathF.Max(0.001f, l);
                if (shapeChanged)
                {
                    rigidBody.Shape = new Capsule(r, l);
                }
            }
            if (ShapeIndex == 3)
            {
                shapeChanged |= EditorHelper.DrawMember(rigidBody.UIDText, "Radius", ref r, typeof(float));
                r = MathF.Max(0.001f, r);
                shapeChanged |= EditorHelper.DrawMember(rigidBody.UIDText, "Length", ref l, typeof(float));
                l = MathF.Max(0.001f, l);
                if (shapeChanged)
                {
                    rigidBody.Shape = new Cylinder(r, l);
                }
            }
            if (ShapeIndex == 4)
            {
                shapeChanged |= EditorHelper.DrawMember(rigidBody.UIDText, "Point A", ref a, typeof(Vector3));
                shapeChanged |= EditorHelper.DrawMember(rigidBody.UIDText, "Point B", ref b, typeof(Vector3));
                shapeChanged |= EditorHelper.DrawMember(rigidBody.UIDText, "Point C", ref c, typeof(Vector3));
                if (shapeChanged)
                {
                    rigidBody.Shape = new Triangle(a, b, c);
                }
            }
        }
    }
}
