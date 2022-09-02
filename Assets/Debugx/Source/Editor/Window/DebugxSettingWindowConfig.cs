using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugxU3D
{
    public class DebugxSettingWindowConfig : ScriptableObject
    {
        private static DebugxSettingWindowConfig m_DebugxMemberWindowConfig;
        /// <summary>
        /// 调试成员编辑窗口配置文件
        /// </summary>
        public static DebugxSettingWindowConfig Current
        {
            get
            {
#if UNITY_EDITOR
                if (m_DebugxMemberWindowConfig == null)
                    m_DebugxMemberWindowConfig = DebugxEditorLibrary.GetConfigDefault<DebugxSettingWindowConfig>(DebugxEditorLibrary.EditorConfigPath + "/DebugxMemberWindowConfig.asset");
#endif
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

        public DebugxMemberConfig debugxMemberConfigSet;

        public DebugxMemberConfig debugxMemberConfigInitOnEditorLoad;
    }
}