using UnityEditor;
using UnityEngine;

namespace DebugxLog
{
    public class GUIStylex
    {
        private static int editorSkinCached = -1;
        public static bool isDarkSkin => editorSkinCached == 1;
        private static GUIStylex style = null;
        public static GUIStylex Get
        {
            get
            {
                if (style == null || editorSkinCached != (EditorGUIUtility.isProSkin ? 1 : 2))
                {
                    editorSkinCached = EditorGUIUtility.isProSkin ? 1 : 2;
                    style = new GUIStylex();
                }

                return style;
            }
        }

        /// <summary>
        /// 标题风格1级。
        /// </summary>
        public GUIStyle TitleStyle_1 { get; private set; }

        /// <summary>
        /// 标题风格2级。
        /// </summary>
        public GUIStyle TitleStyle_2 { get; private set; }

        /// <summary>
        /// 标题风格3级。
        /// </summary>
        public GUIStyle TitleStyle_3 { get; private set; }

        /// <summary>
        /// 隐藏空间标题。
        /// </summary>
        public GUIStyle AreaStyle_1 { get; private set; }

        /// <summary>
        /// 隐藏空间标题。
        /// </summary>
        public GUIStyle LabelStyle_FadeAreaHeader { get; private set; }

        private Texture2D backgroundTex;
        private Texture2D BackgroundTex
        {
            get
            {
                if (backgroundTex == null)
                {
                    bool isDark = EditorGUIUtility.isProSkin;

                    backgroundTex = new Texture2D(32, 32);
                    Color color = isDark ? new(0.2509f, 0.2509f, 0.2509f) : new(0.8117f, 0.8117f, 0.8117f);
                    Color colorLine = isDark ? new(0.1372f, 0.1372f, 0.1372f) : new(0.6627f, 0.6627f, 0.6627f);
                    Color colorSideRound = isDark ? new(0.1764f, 0.1764f, 0.1764f) : new(0.7137f, 0.7137f, 0.7137f);
                    Color colorNone = new(1f, 1f, 1f, 0f);
                    int width = backgroundTex.width;
                    int height = backgroundTex.height;
                    for (int w = 0; w < width; w++)
                    {
                        for (int h = 0; h < height; h++)
                        {
                            if (w == 0 && (h <= 0 || h >= height - 1)
                                || w == width - 1 && (h <= 0 || h >= height - 1))
                            {
                                backgroundTex.SetPixel(w, h, colorNone);
                            }
                            else if ((w == 0 || w == 1 || w == width - 1 || w == width - 2) && (h == 0 || h == 1 || h == height - 2 || h == height - 1))
                            {
                                backgroundTex.SetPixel(w, h, colorSideRound);
                            }
                            else if (w == 0 || h == 0 || w == width - 1 || h == height - 1)
                            {
                                backgroundTex.SetPixel(w, h, colorLine);
                            }
                            else
                                backgroundTex.SetPixel(w, h, color);
                        }
                    }
                    backgroundTex.Apply();
                }

                return backgroundTex;
            }
        }

        public GUIStylex()
        {
            Color colorDark = new(0.1f, 0.1f, 0.1f);
            Color colorLightGray = new(0.8784f, 0.8784f, 0.8784f);

            // 一级标题风格。
            TitleStyle_1 = new GUIStyle
            {
                fontSize = 16,
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = isDarkSkin ? colorLightGray : colorDark
                }
            };

            // 二级标题风格。
            TitleStyle_2 = new GUIStyle
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = isDarkSkin ? colorLightGray : colorDark
                }
            };

            // 三级标题风格。
            TitleStyle_3 = new GUIStyle
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = isDarkSkin ? colorLightGray : colorDark
                }
            };

            LabelStyle_FadeAreaHeader = new GUIStyle(GUI.skin.label);
            LabelStyle_FadeAreaHeader.fontStyle = FontStyle.Bold;

            AreaStyle_1 = new GUIStyle(GUI.skin.button);
            AreaStyle_1.normal.background = BackgroundTex;
        }
    }
}