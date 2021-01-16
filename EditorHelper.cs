using ImGuiNET;
using OpenTK.Mathematics;
using RasterDraw.Helpers;
using RasterDraw.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace GameEditor
{
    public static class EditorHelper
    {
        public static bool DrawMember<T>(string uid, string name, ref T val, Type type) where T : unmanaged
        {
            if (type == null)
            {
                return false;
            }
            object v = (val);
            bool changed = DrawMemberInternal(uid, name, ref v!, type);
            val = (T)v!;
            return changed;
        }
        public static bool DrawMember(string uid, string name, ref object val, Type type)
        {
            if (type == null)
            {
                return false;
            }
            return DrawMemberInternal(uid, name, ref val!, type);
        }

        public static void DrawMember(string uid, MemberInfo member, object TargetObj)
        {
            if (member == null || TargetObj == null || !(member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field))
            {
                throw new ArgumentNullException();
            }
            var val = member.GetValue(TargetObj)!;
            var type = member.GetTypeInfo();
            if (DrawMemberInternal(uid, member.Name, ref val, type))
            {
                member.SetValue(TargetObj, val);
            }
        }

        static bool DrawMemberInternal(string uid, string name, ref object? val, Type type)
        {
            float xSpace = ImGui.GetContentRegionAvail().X;
            string identifier = $"##{uid}{name}";
            ImGui.Columns(2, identifier, false);
            ImGui.SetColumnWidth(0, xSpace * 0.4f);
            ImGui.Text(name);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            bool changed = false;
            if (type == typeof(float))
            {
                var value = (float)val!;
                if (ImGui.DragFloat(identifier, ref value, 0.05f))
                {
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(Vector2))
            {
                var value = (Vector2)val!;
                var vec = new System.Numerics.Vector2(value.X, value.Y);
                if (ImGui.DragFloat2(identifier, ref vec, 0.05f))
                {
                    value = new Vector2(vec.X, vec.Y);
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(Vector3))
            {
                var value = (Vector3)val!;
                var vec = new System.Numerics.Vector3(value.X, value.Y, value.Z);
                if (ImGui.DragFloat3(identifier, ref vec, 0.05f))
                {
                    value = new Vector3(vec.X, vec.Y, vec.Z);
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(Vector4))
            {
                var value = (Vector4)val!;
                var vec = new System.Numerics.Vector4(value.X, value.Y, value.Z, value.W);
                if (ImGui.DragFloat4(identifier, ref vec, 0.05f))
                {
                    value = new Vector4(vec.X, vec.Y, vec.Z, vec.W);
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(System.Numerics.Vector2))
            {
                var value = (System.Numerics.Vector2)val!;
                if (ImGui.DragFloat2(identifier, ref value, 0.05f))
                {
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(System.Numerics.Vector3))
            {
                var value = (System.Numerics.Vector3)val!;
                if (ImGui.DragFloat3(identifier, ref value, 0.05f))
                {
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(System.Numerics.Vector4))
            {
                var value = (System.Numerics.Vector4)val!;
                if (ImGui.DragFloat4(identifier, ref value, 0.05f))
                {
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(int))
            {
                var value = (int)val!;
                if (ImGui.DragInt(identifier, ref value, 0.05f))
                {
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(Vector2i))
            {
                var value = (Vector2i)val!;
                if (ImGui.DragInt2(identifier, ref value.X, 0.05f))
                {
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(Vector3i))
            {
                var value = (Vector3i)val!;
                if (ImGui.DragInt3(identifier, ref value.X, 0.05f))
                {
                    val = value;
                    changed = true;
                }
            }

            else if (type == typeof(Vector4i))
            {
                var value = (Vector4i)val!;
                if (ImGui.DragInt4(identifier, ref value.X, 0.05f))
                {
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(Quaternion))
            {
                var value = (Quaternion)val!;
                var vec = new System.Numerics.Vector4(value.X, value.Y, value.Z, value.W);
                if (ImGui.DragFloat4(identifier, ref vec, 0.05f))
                {
                    value = new Quaternion(vec.X, vec.Y, vec.Z, vec.W);
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(bool))
            {
                var value = (bool)val!;
                if (ImGui.Checkbox(identifier, ref value))
                {
                    val = value;
                    changed = true;
                }
            }
            else if (type == typeof(string))
            {
                var value = (string)val!;
                if (ImGui.InputText(identifier, ref value, 255))
                {
                    val = value;
                    changed = true;
                }
            }
            else if (typeof(IList).IsAssignableFrom(type) && type.IsGenericType)
            {
                var value = (IList)val!;
                if (type.GetGenericArguments()[0].IsValueType)
                {
                    if (ImGui.TreeNodeEx(identifier, ImGuiTreeNodeFlags.SpanAvailWidth))
                    {
                        if (value != null)
                        {
                            for (int i = 0; i < value.Count; i++)
                            {
                                ImGui.Text(value[i]?.ToString());
                            }
                        }
                        ImGui.TreePop();
                    }
                }
            }
            ImGui.Columns(1);
            return changed;
        }
    }
}