using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static DebugxLog.ColorDispenser;

namespace DebugxLog
{
    public static class ColorDispenser
    {
        private const float s_Dark = 0.15f;
        private const float v_Dark = 0.95f;
        private const float s_Light = 0.75f;
        private const float v_Light = 0.17f;

        public struct ColorHSV
        {
            public float h;
            public float s;
            public float v;

            public ColorHSV(float h)
            {
                bool isDark = EditorGUIUtility.isProSkin;

                this.h = h;
                s = isDark ? s_Dark : s_Light;
                v = isDark ? v_Dark : v_Light;
            }
        }

        public static DebugxEditorConfig EditorConfig => DebugxEditorConfig.Get;

        /// <summary>
        /// 为成员获取一个随机颜色
        /// </summary>
        /// <returns></returns>
        public static Color GetRandomColorForMember()
        {
            ColorHSV colorHSV = new(GetColorH() / 360f);

            return Color.HSVToRGB(colorHSV.h, colorHSV.s, colorHSV.v);
        }

        //获取一个HSV颜色的H值，此值是和当前存在的Member的Color最不接近的值
        private static int GetColorH()
        {
            int useLength = DebugxProjectSettingsAsset.Instance.CustomMemberAssetsLength;
            if (useLength == 0) return Random.Range(0, 361);
            else if(useLength == 1)
            {
                int num = GetColorH360(DebugxProjectSettingsAsset.Instance.customMemberAssets[0].color);
                num += 180;
                if (num > 360) num -= 360;
                return num;
            }
            else if(useLength == 2)
            {
                int num1 = GetColorH360(DebugxProjectSettingsAsset.Instance.customMemberAssets[0].color);
                int num2 = GetColorH360(DebugxProjectSettingsAsset.Instance.customMemberAssets[1].color);
                bool compare = num1 < num2;
                int left2 = compare ? num1 : num2;
                int right2 = compare ? num2 : num1;
                int line1 = right2 - left2;
                if (line1 > 180)
                {
                    return left2 + Mathf.FloorToInt(line1 * 0.5f);
                }
                else
                {
                    int num = right2 + Mathf.FloorToInt((360 - line1) * 0.5f);
                    if (num > 360) num -= 360;
                    return num;
                }
            }

            //定义HSV的H值为0-360，记录所有已经使用的Color对应的H。设定两个H之间为一个线段，找到长度最长的线段，并获取线段的中点作为新的H
            //S和V会根据编辑器皮肤Dark或者Light自动适应

            List<int> colorHArray = new List<int>(useLength);
            for (int i = 0; i < useLength; i++)
            {
                int num = GetColorH360(DebugxProjectSettingsAsset.Instance.customMemberAssets[i].color);
                if (colorHArray.Count == 0 || !colorHArray.Contains(num)) colorHArray.Add(num);
            }

            colorHArray.Sort();
            int min = colorHArray[0], max = colorHArray[colorHArray.Count - 1];
            int left = -1;
            int line = 360 - max + min;
            for (int i = 0; i < colorHArray.Count - 1; i++)
            {
                int lineTemp = colorHArray[i + 1] - colorHArray[i];
                if (lineTemp > line)
                {
                    left = colorHArray[i];
                    line = lineTemp;
                }
            }

            int colorH;
            //其他线段
            if ( left >= 0)
            {
                colorH = left + (int)(line * 0.5f);
            }
            //头尾相接的线段
            else
            {
                colorH = max + (int)(line * 0.5f);
                if (colorH > 360) colorH -= 360;
            }

            return colorH;
        }

        //获取Color对应的HSV的H值，并映射到0-360区间
        public static int GetColorH360(Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            return Mathf.FloorToInt(h * 360f);
        }
        
        /// <summary>
        /// 重置所有成员颜色，根据当前编辑器皮肤
        /// </summary>
        public static void ResetAllMemberColorByEditorSkin()
        {
            bool change1 = ResetAllMemberColorByEditorSkin2(DebugxProjectSettingsAsset.Instance.defaultMemberAssets);
            bool change2 = ResetAllMemberColorByEditorSkin2(DebugxProjectSettingsAsset.Instance.customMemberAssets);

            if (change1 || change2)
            {
                DebugxProjectSettingsAsset.Instance.ApplyTo(DebugxProjectSettings.Instance);
            }
        }

        private static bool ResetAllMemberColorByEditorSkin2(DebugxMemberInfoAsset[] debugxMemberInfoAsset)
        {
            int length = debugxMemberInfoAsset != null ? debugxMemberInfoAsset.Length : 0;
            if (length == 0) return false;

            bool isDark = EditorGUIUtility.isProSkin;

            bool change = false;

            for (int i = 0; i < length; i++)
            {
                var mInfo = debugxMemberInfoAsset[i];

                Color color = mInfo.color;
                Color.RGBToHSV(color, out float h, out float s, out float v);

                if (s == (isDark ? s_Dark : s_Light) && v == (isDark ? v_Dark : v_Light)) continue;
                change = true;

                var colorHSV = new ColorHSV(h);
                mInfo.color = Color.HSVToRGB(colorHSV.h, colorHSV.s, colorHSV.v);
                debugxMemberInfoAsset[i] = mInfo;
            }

            return change;
        }
    }
}