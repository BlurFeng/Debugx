﻿using DebugxLog.Tools;
using UnityEditor;

namespace DebugxLog.Editor
{
    public static class DebugxStaticDataEditor
    {
        #region ProjectSettings
        public static bool FAMemberConfigSettingOpen
        {
            get => EditorPrefs.GetBool("DebugxStaticData.FAMemberConfigSettingOpen", true);
            set => EditorPrefs.SetBool("DebugxStaticData.FAMemberConfigSettingOpen", value);
        }

        public static ActionHandler<bool> OnAutoSaveChange = new ActionHandler<bool>();
        // 0 = Not set 1 = Automatic saving 2 = Do not automatically save.
        // 0=未设置 1=自动保存 2=不自动保存。
        public static byte autoSave;
        public static bool AutoSave
        {
            get
            {
                if (autoSave == 0)
                {
                    autoSave = (byte)(EditorPrefs.GetBool("DebugxStaticData.AutoSave", true) ? 1 : 2);
                }

                return autoSave == 1;
            }
            set
            {
                if (value != (autoSave == 1))
                {
                    EditorPrefs.SetBool("DebugxStaticData.AutoSave", value);
                    autoSave = (byte)(value ? 1 : 2);

                    OnAutoSaveChange.Invoke(AutoSave);
                }
            }
        }
        #endregion
    }
}