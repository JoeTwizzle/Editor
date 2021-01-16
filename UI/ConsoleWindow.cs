using ImGuiNET;
using OpenTK.Mathematics;
using RasterDraw.Core;
using RasterDraw.Core.Physics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameEditor.UI
{
    public class ConsoleWindow : EditorWindow
    {
        enum MSGType
        {
            Message,
            Warning,
            Error
        }

        struct MSGInfo
        {
            public MSGType MSGType;
            public string MSG;

            public MSGInfo(MSGType mSGType, string mSG)
            {
                MSGType = mSGType;
                MSG = mSG;
            }
        }

        List<string> Errors = new List<string>();
        List<string> Warnings = new List<string>();
        List<string> Messages = new List<string>();
        List<MSGInfo> Text = new List<MSGInfo>();

        bool showMsg = true;
        bool showWarnings = true;
        bool showErrors = true;

        public ConsoleWindow()
        {
            UIName = "Console";
        }

        public void Clear()
        {
            Messages.Clear();
            Warnings.Clear();
            Errors.Clear();
            Text.Clear();
        }

        public void LogMessage(string msg)
        {
            Messages.Add(msg);
            Text.Add(new MSGInfo(MSGType.Message, msg));
        }

        public void LogWarning(string msg)
        {
            Warnings.Add(msg);
            Text.Add(new MSGInfo(MSGType.Warning, msg));
        }

        public void LogError(string msg)
        {
            Errors.Add(msg);
            Text.Add(new MSGInfo(MSGType.Error, msg));
        }

        void DrawButtons()
        {
            var style = ImGui.GetStyle();
            var prevBorder = style.FrameBorderSize;
            style.FrameBorderSize = 1;
            //Draw Msg button
            if (showMsg)
            {
                ImGui.PushStyleColor(ImGuiCol.Border, Color4.MediumSlateBlue.ToNumerics());
            }
            var BtnStr = $"{Messages.Count} " + (Messages.Count == 1 ? "Message" : "Messages");
            bool pressed = ImGui.Button(BtnStr);
            if (showMsg)
            {
                ImGui.PopStyleColor();
            }
            if (pressed)
            {
                showMsg = !showMsg;
            }
            ImGui.SameLine();

            //Draw warnings btn
            if (showWarnings)
            {
                ImGui.PushStyleColor(ImGuiCol.Border, Color4.MediumSlateBlue.ToNumerics());
            }
            BtnStr = $"{Warnings.Count} " + (Warnings.Count == 1 ? "Warning" : "Warnings");
            pressed = ImGui.Button(BtnStr);
            if (showWarnings)
            {
                ImGui.PopStyleColor();
            }
            if (pressed)
            {
                showWarnings = !showWarnings;
            }
            ImGui.SameLine();
            //Draw errors btn
            BtnStr = $"{Errors.Count} " + (Errors.Count == 1 ? "Error" : "Errors");
            if (showErrors)
            {
                ImGui.PushStyleColor(ImGuiCol.Border, Color4.MediumSlateBlue.ToNumerics());
            }
            pressed = ImGui.Button(BtnStr);
            if (showErrors)
            {
                ImGui.PopStyleColor();
            }
            if (pressed)
            {
                showErrors = !showErrors;
            }
            ImGui.SameLine();

            BtnStr = "Clear";
            pressed = ImGui.Button(BtnStr);
            if (pressed)
            {
                Clear();
            }
            style.FrameBorderSize = prevBorder;
        }


        public override void DrawUI()
        {
            if (ImGui.Begin(UIName, ref IsActive, ImGuiWindowFlags.NoCollapse))
            {

                DrawButtons();
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                for (int i = 0; i < Text.Count; i++)
                {
                    if (Text[i].MSGType == MSGType.Message && showMsg)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, Color4.WhiteSmoke.ToNumerics());
                        ImGui.TextUnformatted(Text[i].MSG);
                        ImGui.PopStyleColor();
                    }
                    else if (Text[i].MSGType == MSGType.Warning && showWarnings)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, Color4.LightYellow.ToNumerics());
                        ImGui.TextUnformatted(Text[i].MSG);
                        ImGui.PopStyleColor();
                    }
                    else if (Text[i].MSGType == MSGType.Error && showErrors)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, Color4.DarkRed.ToNumerics());

                        ImGui.TextUnformatted(Text[i].MSG);
                        ImGui.PopStyleColor();
                    }
                }
            }
            ImGui.End();
        }
    }
}
