using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DebugxLog
{
    public static class DebugxStaticData
    {

        public static string rootPath;

        public static string resourcesPath;

        #region Default Value
        //参数的默认值，用于恢复到默认参数功能
        //dll中的实际使用数据DebugxProjectSettings受到DebugxProjectSettingsAsset支配，其默认值不重要

        public static bool enableLogDefaultSet = true;
        public static bool enableLogMemberDefaultSet = true;
        public static int logThisKeyMemberOnlyDefaultSet = 0;

        public static bool logOutputSet = true;
        public static bool enableLogStackTraceSet = false;
        public static bool enableWarningStackTraceSet = false;
        public static bool enableErrorStackTraceSet = true;
        public static bool recordAllNonDebugxLogsSet = false;
        public static bool drawLogToScreenSet = false;
        public static bool restrictDrawLogCountSet = false;
        public static int maxDrawLogsSet = 100;

        #endregion

#if UNITY_EDITOR

        public static bool EnableAwakeTestLog
        {
            get => EditorPrefs.GetBool("DebugxStaticData.EnableAwakeTestLog", true);
            set => EditorPrefs.SetBool("DebugxStaticData.EnableAwakeTestLog", value);
        }

        public static bool EnableUpdateTestLog
        {
            get => EditorPrefs.GetBool("DebugxStaticData.EnableUpdateTestLog", false);
            set => EditorPrefs.SetBool("DebugxStaticData.EnableUpdateTestLog", value);
        }

        #region ProjectSettings
        public static bool FAMemberConfigSettingOpen
        {
            get => EditorPrefs.GetBool("DebugxStaticData.FAMemberConfigSettingOpen", true);
            set => EditorPrefs.SetBool("DebugxStaticData.FAMemberConfigSettingOpen", value);
        }
        #endregion

        #region Preferences

        /// <summary>
        /// 重置用户设置
        /// </summary>
        public static void ResetPreferences()
        {
            EnableLogDefaultPrefs = enableLogDefaultSet;
            EnableLogMemberDefaultPrefs = enableLogMemberDefaultSet;
            LogThisKeyMemberOnlyDefaultPrefs = logThisKeyMemberOnlyDefaultSet;

            LogOutputPrefs = logOutputSet;
            EnableLogStackTracePrefs = enableLogStackTraceSet;
            EnableWarningStackTracePrefs = enableWarningStackTraceSet;
            EnableErrorStackTracePrefs = enableErrorStackTraceSet;
            RecordAllNonDebugxLogsPrefs = recordAllNonDebugxLogsSet;
            DrawLogToScreenPrefs = drawLogToScreenSet;
            RestrictDrawLogCountPrefs = restrictDrawLogCountSet;
            MaxDrawLogsPrefs = maxDrawLogsSet;

            ResetPreferencesMembers();
        }

        public static void ResetPreferencesMembers()
        {
            DebugxStaticData.MemberEnableDefaultDicPrefs.Clear();
            EditorPrefs.DeleteKey("DebugxStaticData.MemberEnableDefaultDic");
        }

        public static bool EnableLogDefaultPrefs
        {
            get => EditorPrefs.GetBool("DebugxStaticData.EnableLogDefault", enableLogDefaultSet);
            set => EditorPrefs.SetBool("DebugxStaticData.EnableLogDefault", value);
        }

        public static bool EnableLogMemberDefaultPrefs
        {
            get => EditorPrefs.GetBool("DebugxStaticData.EnableLogMemberDefault", enableLogMemberDefaultSet);
            set => EditorPrefs.SetBool("DebugxStaticData.EnableLogMemberDefault", value);
        }

        public static int LogThisKeyMemberOnlyDefaultPrefs
        {
            get => EditorPrefs.GetInt("DebugxStaticData.LogThisKeyMemberOnlyDefault", logThisKeyMemberOnlyDefaultSet);
            set => EditorPrefs.SetInt("DebugxStaticData.LogThisKeyMemberOnlyDefault", value);
        }

        private static Dictionary<int, bool> memberEnableDefaultDicPrefs;
        public static Dictionary<int, bool> MemberEnableDefaultDicPrefs
        {
            get
            {
                if (memberEnableDefaultDicPrefs == null)
                {
                    memberEnableDefaultDicPrefs = new Dictionary<int, bool>();

                    string data = EditorPrefs.GetString("DebugxStaticData.MemberEnableDefaultDic");
                    if (!string.IsNullOrEmpty(data))
                    {
                        string[] datas = data.Split(';');
                        for (int i = 0; i < datas.Length; i++)
                        {
                            string[] item = datas[i].Split(',');
                            memberEnableDefaultDicPrefs.Add(int.Parse(item[0]), bool.Parse(item[1]));
                        }
                    }
                }

                return memberEnableDefaultDicPrefs;
            }
        }

        public static void SaveMemberEnableDefaultDicPrefs()
        {
            if (memberEnableDefaultDicPrefs != null)
            {
                StringBuilder sb = new StringBuilder();
                int counter = 0;
                foreach (var item in memberEnableDefaultDicPrefs)
                {
                    counter++;
                    sb.Append($"{item.Key},{item.Value}");
                    if (counter != memberEnableDefaultDicPrefs.Count) sb.Append(";");
                }
                EditorPrefs.SetString("DebugxStaticData.MemberEnableDefaultDic", sb.ToString());
            }
        }

        public static bool LogOutputPrefs
        {
            get => EditorPrefs.GetBool("DebugxStaticData.LogOutput", logOutputSet);
            set => EditorPrefs.SetBool("DebugxStaticData.LogOutput", value);
        }

        public static bool EnableLogStackTracePrefs
        {
            get => EditorPrefs.GetBool("DebugxStaticData.EnableLogStackTrace", enableLogStackTraceSet);
            set => EditorPrefs.SetBool("DebugxStaticData.EnableLogStackTrace", value);
        }

        public static bool EnableWarningStackTracePrefs
        {
            get => EditorPrefs.GetBool("DebugxStaticData.EnableWarningStackTrace", enableWarningStackTraceSet);
            set => EditorPrefs.SetBool("DebugxStaticData.EnableWarningStackTrace", value);
        }

        public static bool EnableErrorStackTracePrefs
        {
            get => EditorPrefs.GetBool("DebugxStaticData.EnableErrorStackTrace", enableErrorStackTraceSet);
            set => EditorPrefs.SetBool("DebugxStaticData.EnableErrorStackTrace", value);
        }

        public static bool RecordAllNonDebugxLogsPrefs
        {
            get => EditorPrefs.GetBool("DebugxStaticData.RecordAllNonDebugxLogs", recordAllNonDebugxLogsSet);
            set => EditorPrefs.SetBool("DebugxStaticData.RecordAllNonDebugxLogs", value);
        }

        public static bool DrawLogToScreenPrefs
        {
            get => EditorPrefs.GetBool("DebugxStaticData.DrawLogToScreen", drawLogToScreenSet);
            set => EditorPrefs.SetBool("DebugxStaticData.DrawLogToScreen", value);
        }

        public static bool RestrictDrawLogCountPrefs
        {
            get => EditorPrefs.GetBool("DebugxStaticData.RestrictDrawLogCount", restrictDrawLogCountSet);
            set => EditorPrefs.SetBool("DebugxStaticData.RestrictDrawLogCount", value);
        }

        public static int MaxDrawLogsPrefs
        {
            get => EditorPrefs.GetInt("DebugxStaticData.MaxDrawLogs", maxDrawLogsSet);
            set => EditorPrefs.SetInt("DebugxStaticData.MaxDrawLogs", value);
        }

        public static bool FAMemberEnableSettingOpen
        {
            get => EditorPrefs.GetBool("DebugxStaticData.FAMemberEnableSettingOpen", true);
            set => EditorPrefs.SetBool("DebugxStaticData.FAMemberEnableSettingOpen", value);
        }

        public static bool CanResetPreferences
        {
            get => EditorPrefs.GetBool("DebugxStaticData.CanResetPreferences", false);
            set => EditorPrefs.SetBool("DebugxStaticData.CanResetPreferences", value);
        }

        public static bool CanResetPreferencesMembers
        {
            get => EditorPrefs.GetBool("DebugxStaticData.CanResetPreferencesMembers", false);
            set => EditorPrefs.SetBool("DebugxStaticData.CanResetPreferencesMembers", value);
        }
        #endregion

        #region Text
        public const string ToolTip_CustomDebugxMemberAssets = "自定义调试成员信息列表";

        public const string ToolTip_EnableLogDefault = "Log总开关，启动时默认状态";
        public const string ToolTip_EnableLogMemberDefault = "成员Log总开关，启动时默认状态";
        public const string ToolTip_LogThisKeyMemberOnlyDefault = "仅打印此Key的成员Log，0为关闭。启动时默认状态";

        public const string ToolTip_LogOutput = "输出Log到本地（启动前设置，运行时设置无效）。编辑器时输出到项目的Logs文件夹下，实机运行时根据平台输出到不同目录下";
        public const string ToolTip_EnableLogStackTrace = "输出Log类型的堆栈跟踪";
        public const string ToolTip_EnableWarningStackTrace = "输出Warning类型的堆栈跟踪";
        public const string ToolTip_EnableErrorStackTrace = "输出Error类型的堆栈跟踪";
        public const string ToolTip_RecordAllNonDebugxLogs = "记录所有非Debugx打印的Log";
        public const string ToolTip_DrawLogToScreen = "绘制Log到屏幕";
        public const string ToolTip_RestrictDrawLogCount = "限制绘制Log数量";
        public const string ToolTip_MaxDrawLogs = "绘制Log最大数量";

        #endregion
#endif
    }
}