﻿using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Fluffy
{
    public static class Widgets_Work
    {
        public static bool prioritiesDirty = false;

        public const float WorkBoxSize = 25f;

        private const int MidAptCutoff = 14;

        private const float PassionOpacity = 0.4f;

        private static readonly Texture2D WorkBoxBGTex_Bad = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Bad");

        private static readonly Texture2D WorkBoxBGTex_Mid = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Mid");

        private static readonly Texture2D WorkBoxBGTex_Excellent = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Excellent");

        private static readonly Texture2D WorkBoxCheckTex = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxCheck");

        private static Texture2D PassionWorkboxMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinorGray");

        private static Texture2D PassionWorkboxMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajorGray");

        private static Color ColorOfPriority(int prio)
        {
            switch (prio)
            {
                case 1:
                    return Color.green;
                case 2:
                    return new Color(1f, 0.9f, 0.6f);
                case 3:
                    return new Color(0.8f, 0.7f, 0.5f);
                case 4:
                    return new Color(0.6f, 0.6f, 0.6f);
                default:
                    return Color.grey;
            }
        }

        public static void DrawWorkBoxFor(Vector2 topLeft, Pawn p, WorkTypeDef wType)
        {
            if (p.story == null || p.story.WorkTypeIsDisabled(wType))
            {
                return;
            }
            Rect rect = new Rect(topLeft.x, topLeft.y, 25f, 25f);
            DrawWorkBoxBackground(rect, p, wType);
            if (Find.PlaySettings.useWorkPriorities)
            {
                int priority = p.workSettings.GetPriority(wType);
                string label;
                if (priority > 0)
                {
                    label = priority.ToString();
                }
                else
                {
                    label = string.Empty;
                }
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = ColorOfPriority(priority);
                Rect rect2 = rect.ContractedBy(-3f);
                Widgets.Label(rect2, label);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
                {
                    if (Event.current.button == 0)
                    {
                        int num = p.workSettings.GetPriority(wType) - 1;
                        if (num < 0)
                        {
                            num = 4;
                        }
                        p.workSettings.SetPriority(wType, num);
                        SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    }
                    if (Event.current.button == 1)
                    {
                        int num2 = p.workSettings.GetPriority(wType) + 1;
                        if (num2 > 4)
                        {
                            num2 = 0;
                        }
                        p.workSettings.SetPriority(wType, num2);
                        SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                    }
                    Event.current.Use();
                }
            }
            else
            {
                int priority2 = p.workSettings.GetPriority(wType);
                if (priority2 > 0)
                {
                    GUI.DrawTexture(rect, WorkBoxCheckTex);
                }
                if (Mouse.IsOver(rect))
                {
                    if (Input.GetMouseButton(0))
                    {
                        // Log.Message(p.workSettings.GetPriority(wType).ToString());
                        // Priority of 'active' is 1 when manual is diabled, even if set to 3
                        if (p.workSettings.GetPriority(wType) < 1)
                        { 
                            p.workSettings.SetPriority(wType, 3);
                            SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
                        }
                    } 
                    else if (Input.GetMouseButton(1))
                    {
                        if (p.workSettings.GetPriority(wType) > 0)
                        {
                            p.workSettings.SetPriority(wType, 0);
                            SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                        }
                    }
                }
            }
        }

        public static TipSignal TipForPawnWorker(Pawn p, WorkTypeDef wDef)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(wDef.gerundLabel);
            if (p.story.WorkTypeIsDisabled(wDef))
            {
                stringBuilder.Append("CannotDoThisWork".Translate(p.NameStringShort));
            }
            else
            {
                string text = string.Empty;
                if (wDef.relevantSkills.Count == 0)
                {
                    text = "NoneBrackets".Translate();
                }
                else
                {
                    foreach (SkillDef current in wDef.relevantSkills)
                    {
                        text = text + current.skillLabel + ", ";
                    }
                    text = text.Substring(0, text.Length - 2);
                }
                stringBuilder.AppendLine("RelevantSkills".Translate(text, p.skills.AverageOfRelevantSkillsFor(wDef).ToString(), 20));
                stringBuilder.AppendLine();
                stringBuilder.Append(wDef.description);
            }
            return stringBuilder.ToString();
        }

        private static void DrawWorkBoxBackground(Rect rect, Pawn p, WorkTypeDef workDef)
        {
            float num = p.skills.AverageOfRelevantSkillsFor(workDef);
            Texture2D image;
            Texture2D image2;
            float a;
            if (num <= 14f)
            {
                image = WorkBoxBGTex_Bad;
                image2 = WorkBoxBGTex_Mid;
                a = num / 14f;
            }
            else
            {
                image = WorkBoxBGTex_Mid;
                image2 = WorkBoxBGTex_Excellent;
                a = (num - 14f) / 6f;
            }
            GUI.DrawTexture(rect, image);
            GUI.color = new Color(1f, 1f, 1f, a);
            GUI.DrawTexture(rect, image2);
            Passion passion = p.skills.MaxPassionOfRelevantSkillsFor(workDef);
            if (passion > Passion.None)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.4f);
                Rect position = rect;
                position.xMin = rect.center.x;
                position.yMin = rect.center.y;
                if (passion == Passion.Minor)
                {
                    GUI.DrawTexture(position, PassionWorkboxMinorIcon);
                }
                else if (passion == Passion.Major)
                {
                    GUI.DrawTexture(position, PassionWorkboxMajorIcon);
                }
            }
            GUI.color = Color.white;
        }
    }
}