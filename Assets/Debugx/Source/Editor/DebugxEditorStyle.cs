using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugxLog
{
    public class EditorStyle
    {
        private static EditorStyle style = null;
        /// <summary>
        /// 获取编辑风格示例对象
        /// </summary>
        public static EditorStyle Get { get { if (style == null) style = new EditorStyle();  return style; } }

        /// <summary>
        /// 标题风格1级
        /// </summary>
        public GUIStyle TitleStyle_1 { get; private set; }

        /// <summary>
        /// 标题风格2级
        /// </summary>
        public GUIStyle TitleStyle_2 { get; private set; }

        /// <summary>
        /// 标题风格3级
        /// </summary>
        public GUIStyle TitleStyle_3 { get; private set; }

        /// <summary>
        /// 隐藏空间标题
        /// </summary>
        public GUIStyle AreaStyle_1 { get; private set; }

        /// <summary>
        /// 隐藏空间标题
        /// </summary>
        public GUIStyle LabelStyle_FadeAreaHeader { get; private set; }

        public Color colorLightGray { get; private set; } = new (0.8f, 0.8f, 0.8f);

        private Texture2D m_DarkGreyTex;
        public Texture2D DarkGreyTex
        {
            get
            {
                if (m_DarkGreyTex == null)
                {
                    m_DarkGreyTex = new Texture2D(32, 32);
                    Color color = new Color(0.25f, 0.25f, 0.25f);
                    Color colorLine1 = new Color(0.15f, 0.15f, 0.15f);
                    Color colorLine2 = new Color(0.19f, 0.19f, 0.19f);
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

        public EditorStyle()
        {
            //一级标题风格
            TitleStyle_1 = new GUIStyle
            {
                fontSize = 16,
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = colorLightGray
                }
            };

            //二级标题风格
            TitleStyle_2 = new GUIStyle
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = colorLightGray
                }
            };

            //三级标题风格
            TitleStyle_3 = new GUIStyle
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = colorLightGray
                }
            };

            LabelStyle_FadeAreaHeader = GUI.skin.label;
            LabelStyle_FadeAreaHeader.fontStyle = FontStyle.Bold;

            AreaStyle_1 = new GUIStyle(GUI.skin.button);
            AreaStyle_1.normal.background = DarkGreyTex;
        }
    }
}