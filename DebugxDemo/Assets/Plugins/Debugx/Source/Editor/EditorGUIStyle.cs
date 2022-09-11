using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DebugxLog
{
    public class GUIStyle
    {
        private static int editorSkinCached = -1;

        private static GUIStyle style = null;
        /// <summary>
        /// 获取编辑风格示例对象
        /// </summary>
        public static GUIStyle Get 
        { 
            get
            { 
                if (style == null || editorSkinCached != (EditorGUIUtility.isProSkin ? 1 : 2))
                {
                    style = new GUIStyle();
                    editorSkinCached = EditorGUIUtility.isProSkin ? 1 : 2;
                }

                return style; 
            } 
        }

        /// <summary>
        /// 标题风格1级
        /// </summary>
        public UnityEngine.GUIStyle TitleStyle_1 { get; private set; }

        /// <summary>
        /// 标题风格2级
        /// </summary>
        public UnityEngine.GUIStyle TitleStyle_2 { get; private set; }

        /// <summary>
        /// 标题风格3级
        /// </summary>
        public UnityEngine.GUIStyle TitleStyle_3 { get; private set; }

        /// <summary>
        /// 隐藏空间标题
        /// </summary>
        public UnityEngine.GUIStyle AreaStyle_1 { get; private set; }

        /// <summary>
        /// 隐藏空间标题
        /// </summary>
        public UnityEngine.GUIStyle LabelStyle_FadeAreaHeader { get; private set; }

        public Color colorDark { get; private set; } = new(0.1f, 0.1f, 0.1f);
        public Color colorLightGray { get; private set; } = new (0.8f, 0.8f, 0.8f);

        private Texture2D m_DarkGreyTex;
        public Texture2D backgroundTex
        {
            get
            {
                if (m_DarkGreyTex == null)
                {
                    bool isDark = EditorGUIUtility.isProSkin;

                    m_DarkGreyTex = new Texture2D(32, 32);
                    Color color = isDark ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.85f, 0.85f, 0.85f);
                    Color colorLine1 = isDark ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.65f, 0.65f, 0.65f);
                    Color colorLine2 = isDark ? new Color(0.19f, 0.19f, 0.19f) : new Color(0.69f, 0.69f, 0.69f);
                    Color colorNone = new Color(1f, 1f, 1f, 0f);
                    int width = m_DarkGreyTex.width;
                    int height = m_DarkGreyTex.height;
                    for (int w = 0; w < width; w++)
                    {
                        for (int h = 0; h < height; h++)
                        {
                            if (w == 0 && (h <= 0 || h >= height - 1)
                                || w == width - 1 && (h <= 0 || h >= height - 1))
                            {
                                m_DarkGreyTex.SetPixel(w, h, colorNone);
                            }
                            else if ((w == 0 || w == 1 || w == width - 1 || w == width - 2) && (h == 0 || h == 1 || h == height - 2 || h == height - 1))
                            {
                                m_DarkGreyTex.SetPixel(w, h, colorLine2);
                            }
                            else if (w == 0 || h == 0 || w == width - 1 || h == height - 1)
                            {
                                m_DarkGreyTex.SetPixel(w, h, colorLine1);
                            }
                            else
                                m_DarkGreyTex.SetPixel(w, h, color);
                        }
                    }
                    m_DarkGreyTex.Apply();
                }

                return m_DarkGreyTex;
            }
        }

        public GUIStyle()
        {
            bool isDark = EditorGUIUtility.isProSkin;

            //一级标题风格
            TitleStyle_1 = new UnityEngine.GUIStyle
            {
                fontSize = 16,
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = isDark ? colorLightGray : colorDark
                }
            };

            //二级标题风格
            TitleStyle_2 = new UnityEngine.GUIStyle
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = isDark ? colorLightGray : colorDark
                }
            };

            //三级标题风格
            TitleStyle_3 = new UnityEngine.GUIStyle
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = isDark ? colorLightGray : colorDark
                }
            };

            LabelStyle_FadeAreaHeader = new UnityEngine.GUIStyle(GUI.skin.label);
            LabelStyle_FadeAreaHeader.fontStyle = FontStyle.Bold;

            AreaStyle_1 = new UnityEngine.GUIStyle(GUI.skin.button);
            AreaStyle_1.normal.background = backgroundTex;
        }
    }
}