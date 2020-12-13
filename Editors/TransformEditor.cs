using OpenTK.Mathematics;
using RasterDraw.Core.NativeScripts;
using RasterDraw.Core.Scripting;
using RasterDraw.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using RasterDraw.Serialization;
using RasterDraw.Core.Helpers;

namespace GameEditor.Editors
{
    [CustomEditor(typeof(Transform))]
    class TransformEditor : Editor
    {

        ObjectInfo objInfo;
        Transform myTransform;

        public override void Init()
        {
            objInfo = new ObjectInfo(TargetObj);
            myTransform = (Transform)TargetObj;
            prevRotation = Quaternion.FromEulerAngles(0, 0, 0);
            Euler = (myTransform.Rotation * prevRotation.Inverted()).ToEulerAngles();
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
            object eulerDelta = (myTransform.Rotation * prevRotation.Inverted()).ToEulerAngles();

            //store external rotation delta in radians
            Euler = (Vector3)Euler + ((Vector3)eulerDelta);

            //convert to Degrees
            Euler = (Vector3)Euler * MathExtensions.Rad2Deg;
            
            //
            Vector3 euler = (Vector3)Euler;

            //store internal rotation delta in degrees
            if (EditorHelper.DrawMember(myTransform.UIDText, "Rotation", ref Euler, typeof(Vector3)))
            {
                //apply internal rotation delta in radians
                myTransform.Rotation *= Quaternion.FromEulerAngles(((Vector3)Euler - euler) * MathExtensions.Deg2Rad);
            }

            //store new rotation for the next frame
            prevRotation = myTransform.Rotation;

            //convert internal rotation delta in radians
            Euler = (Vector3)Euler * MathExtensions.Deg2Rad;

            object scale = myTransform.Scale;
            if (EditorHelper.DrawMember(myTransform.UIDText, "Scale", ref scale, typeof(Vector3)))
            {
                myTransform.Scale = (Vector3)scale;
            }
        }
    }
}
