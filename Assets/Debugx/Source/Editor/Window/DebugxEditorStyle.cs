using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugxU3D
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
        /// 标题风格2级
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
                    textColor = new Color(1f, 1f, 1f)
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
                    textColor = new Color(1f, 1f, 1f)
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
                    textColor = new Color(1f, 1f, 1f)
                }
            };

            LabelStyle_FadeAreaHeader = GUI.skin.label;
            LabelStyle_FadeAreaHeader.fontStyle = FontStyle.Bold;
            AreaStyle_1 = GUI.skin.button;
        }
    }
}