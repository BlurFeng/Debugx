using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DebugxLog
{
    public class DebugxSettingsProviderConfig : ScriptableObject
    {
        private static DebugxSettingsProviderConfig m_DebugxSettingsProviderConfig;
        /// <summary>
        /// 调试成员编辑窗口配置文件
        /// </summary>
        public static DebugxSettingsProviderConfig Get
        {
            get
            {
                if (m_DebugxSettingsProviderConfig == null)
                    m_DebugxSettingsProviderConfig = DebugxEditorLibrary.GetConfigDefault<DebugxSettingsProviderConfig>(DebugxEditorLibrary.EditorConfigPath + "/DebugxSettingsProviderConfig.asset");

                return m_DebugxSettingsProviderConfig;
            }
        }

        public static Action<bool> OnAutoSaveChange;
        private bool m_AutoSave = true;
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

        public bool fa_MemberConfigSettingOpen;
    }
}