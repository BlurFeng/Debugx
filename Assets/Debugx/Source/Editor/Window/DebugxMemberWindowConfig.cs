using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DebugxU3D
{
    public class DebugxMemberWindowConfig : ScriptableObject
    {
        private static DebugxMemberWindowConfig m_DebugxMemberWindowConfig;
        /// <summary>
        /// 调试成员编辑窗口配置文件
        /// </summary>
        public static DebugxMemberWindowConfig Current
        {
            get
            {
                if (m_DebugxMemberWindowConfig == null)
                    m_DebugxMemberWindowConfig = DebugxEditorLibrary.GetConfigDefault<DebugxMemberWindowConfig>(DebugxEditorLibrary.EditorConfigPath + "/DebugxMemberWindowConfig.asset");
                return m_DebugxMemberWindowConfig;
            }
        }

        public static Action<bool> OnAutoSaveChange;
        private bool m_AutoSave = false;
        /// <summary>
        /// 自动保存
        /// </summary>
        public bool AutoSave
        {
            get { return m_AutoSave; }
            set
            {
                if (value != m_AutoSave)
                {
                    m_AutoSave = value;
                    OnAutoSaveChange?.Invoke(m_AutoSave);
                }
            }
        }

        public DebugxMemberConfig debugxMemberConfigDefault;
    }
}