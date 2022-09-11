using DebugxLog.Tools;
using DebugxLog;
using System.Collections.Generic;
using System.Text;
using UnityEditor;

public static class DebugxStaticDataEditor
{
    #region ProjectSettings
    public static bool FAMemberConfigSettingOpen
    {
        get => EditorPrefs.GetBool("DebugxStaticData.FAMemberConfigSettingOpen", true);
        set => EditorPrefs.SetBool("DebugxStaticData.FAMemberConfigSettingOpen", value);
    }

    public static ActionHandler<bool> OnAutoSaveChange = new ActionHandler<bool>();
    public static byte autoSave;//0=未设置 1=自动保存 2=不自动保存
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
