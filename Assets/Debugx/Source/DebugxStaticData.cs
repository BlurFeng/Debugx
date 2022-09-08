using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class DebugxStaticData
{
    public static void OnEditorQuit()
    {
        SaveMemberEnableDefaultDic();
    }

    public static string rootPath;

    public static string resourcesPath;

    #region Preferences

    public static bool EnableLogDefault
    {
        get => EditorPrefs.GetBool("DebugxStaticData.EnableLogDefault", true);
        set => EditorPrefs.SetBool("DebugxStaticData.EnableLogDefault", value);
    }

    public static bool EnableLogMemberDefault
    {
        get => EditorPrefs.GetBool("DebugxStaticData.EnableLogMemberDefault", true);
        set => EditorPrefs.SetBool("DebugxStaticData.EnableLogMemberDefault", value);
    }

    public static int LogThisKeyMemberOnlyDefault
    {
        get => EditorPrefs.GetInt("DebugxStaticData.LogThisKeyMemberOnlyDefault", 0);
        set => EditorPrefs.SetInt("DebugxStaticData.LogThisKeyMemberOnlyDefault", value);
    }

    private static Dictionary<int, bool> memberEnableDefaultDic;
    public static Dictionary<int, bool> MemberEnableDefaultDic
    {
        get
        {
            if(memberEnableDefaultDic == null)
            {
                memberEnableDefaultDic = new Dictionary<int, bool>();

                string data = EditorPrefs.GetString("DebugxStaticData.MemberEnableDefaultDic");
                if (!string.IsNullOrEmpty(data))
                {
                    string[] datas = data.Split(';');
                    for (int i = 0; i < datas.Length; i++)
                    {
                        string[] item = datas[i].Split(',');
                        memberEnableDefaultDic.Add(int.Parse(item[0]), bool.Parse(item[1]));
                    }
                }
            }

            return memberEnableDefaultDic;
        }
    }

    public static void SaveMemberEnableDefaultDic()
    {
        if(memberEnableDefaultDic != null)
        {
            StringBuilder sb = new StringBuilder();
            int counter = 0;
            foreach (var item in memberEnableDefaultDic)
            {
                counter++;
                sb.Append($"{item.Key},{item.Value}");
                if (counter != memberEnableDefaultDic.Count) sb.Append(";");
            }
            EditorPrefs.SetString("DebugxStaticData.MemberEnableDefaultDic", sb.ToString());
        }
    }

    public static bool LogOutput
    {
        get => EditorPrefs.GetBool("DebugxStaticData.LogOutput", true);
        set => EditorPrefs.SetBool("DebugxStaticData.LogOutput", value);
    }

    public static bool EnableLogStackTrace
    {
        get => EditorPrefs.GetBool("DebugxStaticData.EnableLogStackTrace", false);
        set => EditorPrefs.SetBool("DebugxStaticData.EnableLogStackTrace", value);
    }

    public static bool EnableWarningStackTrace
    {
        get => EditorPrefs.GetBool("DebugxStaticData.EnableWarningStackTrace", false);
        set => EditorPrefs.SetBool("DebugxStaticData.EnableWarningStackTrace", value);
    }

    public static bool EnableErrorStackTrace
    {
        get => EditorPrefs.GetBool("DebugxStaticData.EnableErrorStackTrace", true);
        set => EditorPrefs.SetBool("DebugxStaticData.EnableErrorStackTrace", value);
    }

    public static bool RecordAllNonDebugxLogs
    {
        get => EditorPrefs.GetBool("DebugxStaticData.RecordAllNonDebugxLogs", false);
        set => EditorPrefs.SetBool("DebugxStaticData.RecordAllNonDebugxLogs", value);
    }

    public static bool DrawLogToScreen
    {
        get => EditorPrefs.GetBool("DebugxStaticData.DrawLogToScreen", false);
        set => EditorPrefs.SetBool("DebugxStaticData.DrawLogToScreen", value);
    }

    public static bool RestrictDrawLogCount
    {
        get => EditorPrefs.GetBool("DebugxStaticData.RestrictDrawLogCount", false);
        set => EditorPrefs.SetBool("DebugxStaticData.RestrictDrawLogCount", value);
    }

    public static int MaxDrawLogs
    {
        get => EditorPrefs.GetInt("DebugxStaticData.MaxDrawLogs", 100);
        set => EditorPrefs.SetInt("DebugxStaticData.MaxDrawLogs", value);
    }
    #endregion

#if UNITY_EDITOR
    [HideInInspector]
    public static bool enableAwakeTestLog = true;

    [HideInInspector]
    public static bool enableUpdateTestLog = true;
#endif
}
