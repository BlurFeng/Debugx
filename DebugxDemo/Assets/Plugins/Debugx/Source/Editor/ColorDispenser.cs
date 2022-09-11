using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DebugxLog
{
    public static class ColorDispenser
    {
        private static DebugxProjectSettingsAsset SettingsAsset => DebugxProjectSettingsAsset.Instance;

        //HSV中H色调的尺度，注意0和360的颜色是一样的
        private const int hDimension = 360;

        private const float s_Dark = 0.15f;
        private const float v_Dark = 0.95f;
        private const float s_Light = 0.75f;
        private const float v_Light = 0.17f;

        private static bool isDarkSkin;

        /// <summary>
        /// HSV颜色模型
        /// 值为0-1
        /// </summary>
        public struct ColorHSV
        {
            public float h;
            public float s;
            public float v;

            public ColorHSV(float h)
            {
                this.h = h;
                s = isDarkSkin ? s_Dark : s_Light;
                v = isDarkSkin ? v_Dark : v_Light;
            }
        }

        public static void OnInitializeOnLoadMethod()
        {
            DebugxProjectSettingsAsset.GetRandomColorForMember += GetRandomColorForMember;
            DebugxProjectSettingsAsset.GetNormalMemberColor += GetNormalMemberColor;
            DebugxProjectSettingsAsset.GetMasterMemberColor += GetMasterMemberColor;
            isDarkSkin = EditorGUIUtility.isProSkin;
        }

        public static void OnQuit()
        {
            DebugxProjectSettingsAsset.GetRandomColorForMember -= GetRandomColorForMember;
        }

        /// <summary>
        /// 为成员获取一个随机颜色
        /// </summary>
        /// <returns></returns>
        public static Color GetRandomColorForMember()
        {
            ColorHSV colorHSV = new(GetColorH() * 1f / hDimension);

            return Color.HSVToRGB(colorHSV.h, colorHSV.s, colorHSV.v);
        }

        public static Color GetNormalMemberColor()
        {
            return isDarkSkin ? Color.white : Color.black;
        }

        public static Color GetMasterMemberColor()
        {
            ColorHSV colorHSV = new(5f / hDimension);

            return Color.HSVToRGB(colorHSV.h, colorHSV.s, colorHSV.v);
        }

        //获取一个HSV颜色的H值，此值是和当前存在的Member的Color最不接近的值
        private static int GetColorH()
        {
            int useLength = SettingsAsset.CustomMemberAssetsLength;
            if (useLength == 0) return Random.Range(0, 361);
            else if(useLength == 1)
            {
                int num = GetColorH360(SettingsAsset.customMemberAssets[0].color);
                num += 180;
                if (num > hDimension) num -= hDimension;
                return num;
            }
            else if(useLength == 2)
            {
                int num1 = GetColorH360(SettingsAsset.customMemberAssets[0].color);
                int num2 = GetColorH360(SettingsAsset.customMemberAssets[1].color);
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
                    int num = right2 + Mathf.FloorToInt((hDimension - line1) * 0.5f);
                    if (num > hDimension) num -= hDimension;
                    return num;
                }
            }

            //定义HSV的H值为0-dimension，记录所有已经使用的Color对应的H。设定两个H之间为一个线段，找到长度最长的线段，并获取线段的中点作为新的H
            //S和V会根据编辑器皮肤Dark或者Light自动适应

            List<int> colorHArray = new List<int>(useLength);
            for (int i = 0; i < useLength; i++)
            {
                int num = GetColorH360(SettingsAsset.customMemberAssets[i].color);
                if (colorHArray.Count == 0 || !colorHArray.Contains(num)) colorHArray.Add(num);
            }

            colorHArray.Sort();
            int min = colorHArray[0], max = colorHArray[colorHArray.Count - 1];
            int left = -1;
            int line = hDimension - max + min;
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
                if (colorH > hDimension) colorH -= hDimension;
            }

            return colorH;
        }

        //获取Color对应的HSV的H值，并映射到0-dimension区间
        public static int GetColorH360(Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            return Mathf.FloorToInt(h * hDimension);
        }

        /// <summary>
        /// 为自定义成员自动重分配颜色
        /// </summary>
        public static void AutomaticallyReassignColors()
        {
            int length = SettingsAsset.CustomMemberAssetsLength;
            if (length == 0) return;

            int step = hDimension / SettingsAsset.CustomMemberAssetsLength;
            int h = 0;
            for (int i = 0; i < length; i++)
            {
                ColorHSV colorHSV = new(h * 1f / hDimension);
                SettingsAsset.customMemberAssets[i].color = Color.HSVToRGB(colorHSV.h, colorHSV.s, colorHSV.v);
                h += step;
            }
        }

        /// <summary>
        /// 重置所有成员颜色，根据当前编辑器皮肤
        /// </summary>
        public static void AdaptColorByEditorSkin()
        {
            bool change1 = ResetAllMemberColorByEditorSkin(SettingsAsset.defaultMemberAssets);
            bool change2 = ResetAllMemberColorByEditorSkin(SettingsAsset.customMemberAssets);

            if (change1 || change2)
            {
                SettingsAsset.ApplyTo(DebugxProjectSettings.Instance);
            }
        }

        private static bool ResetAllMemberColorByEditorSkin(DebugxMemberInfoAsset[] debugxMemberInfoAsset)
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