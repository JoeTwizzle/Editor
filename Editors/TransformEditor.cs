using OpenTK.Mathematics;
using RasterDraw.Core.NativeScripts;
using RasterDraw.Serialization;
using RasterDraw.Helpers;
using System;

namespace GameEditor.Editors
{
    [CustomEditor(typeof(Transform))]
    class TransformEditor : Editor
    {
        Transform myTransform;

        public override void Init()
        {
            myTransform = (Transform)TargetObj;
            prevRotation = Quaternion.FromEulerAngles(0, 0, 0);
            Euler = myTransform.Rotation.ToEulerAngles() * MathExtensions.Rad2Deg;
        }

        object Euler;
        Quaternion prevRotation;
        public override void OnDrawUI()
        {
            DrawMembers();
        }

        void DrawMembers()
        {
            object pos = myTransform.Position;
            if (EditorHelper.DrawMember(myTransform.UIDText, "Position", ref pos, typeof(Vector3)))
            {
                myTransform.Position = (Vector3)pos;
            }

            //calculate external rotation delta in Radians
            object eulerDelta = (myTransform.Rotation * prevRotation.Inverted()).ToEulerAngles() * MathExtensions.Rad2Deg;

            //store external rotation delta in radians
            Euler = (Vector3)Euler + ((Vector3)eulerDelta);

            //store internal rotation delta in degrees
            if (EditorHelper.DrawMember(myTransform.UIDText, "Rotation", ref Euler, typeof(Vector3)))
            {
                myTransform.Rotation = Quaternion.FromEulerAngles((Vector3)Euler * MathExtensions.Deg2Rad);
            }

            //store new rotation for the next frame
            prevRotation = myTransform.Rotation;

            object scale = myTransform.Scale;
            if (EditorHelper.DrawMember(myTransform.UIDText, "Scale", ref scale, typeof(Vector3)))
            {
                myTransform.Scale = (Vector3)scale;
            }
        }
    }
}
