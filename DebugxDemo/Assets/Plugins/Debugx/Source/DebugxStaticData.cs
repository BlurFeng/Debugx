﻿using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DebugxLog
{
    public static class DebugxStaticData
    {

        public static bool IsChineseSimplified = Application.systemLanguage == SystemLanguage.ChineseSimplified;
        
        public static string RootPath;

        public static string ResourcesPath;

        #region Tools

        public static bool PlayerPrefsGetBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 2) == 1;
        }

        public static void PlayerPrefsSetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 2);
        }

        #endregion

        public static bool EnableAwakeTestLog
        {
            get => PlayerPrefsGetBool("DebugxStaticData.EnableAwakeTestLog", true);
            set => PlayerPrefsSetBool("DebugxStaticData.EnableAwakeTestLog", value);
        }

        public static bool EnableUpdateTestLog
        {
            get => PlayerPrefsGetBool("DebugxStaticData.EnableUpdateTestLog", false);
            set => PlayerPrefsSetBool("DebugxStaticData.EnableUpdateTestLog", value);
        }

        #region Text
        public const string ToolTip_CustomDebugxMemberAssets = "自定义调试成员信息列表";

        public const string ToolTip_EnableLogDefault = "Log总开关，启动时默认状态";
        public const string ToolTip_EnableLogMemberDefault = "成员Log总开关，启动时默认状态";
        public const string ToolTip_AllowUnregisteredMember = "允许没有注册成员进行打印";
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

        #region Default Value
        // The default value of the parameter, used to restore to the default parameter function.
        // The actual usage data in the DLL, namely DebugxProjectSettings, is controlled by DebugxProjectSettingsAsset. Its default value is not significant.
        // 参数的默认值，用于恢复到默认参数功能。
        // dll中的实际使用数据DebugxProjectSettings受到DebugxProjectSettingsAsset支配，其默认值不重要。

        public const bool EnableLogDefaultSet = true;
        public const bool EnableLogMemberDefaultSet = true;
        public const bool AllowUnregisteredMemberSet = true;
        public const int LogThisKeyMemberOnlyDefaultSet = 0;

        public const bool LogOutputSet = true;
        public const bool EnableLogStackTraceSet = false;
        public const bool EnableWarningStackTraceSet = false;
        public const bool EnableErrorStackTraceSet = true;
        public const bool RecordAllNonDebugxLogsSet = false;
        public const bool DrawLogToScreenSet = false;
        public const bool RestrictDrawLogCountSet = false;
        public const int MaxDrawLogsSet = 100;

        #endregion

        #region Preferences

        /// <summary>
        /// Reset user settings.
        /// 重置用户设置。
        /// </summary>
        public static void ResetPreferences()
        {
            EnableLogDefaultPrefs = DebugxStaticData.EnableLogDefaultSet;
            EnableLogMemberDefaultPrefs = DebugxStaticData.EnableLogMemberDefaultSet;
            AllowUnregisteredMember = DebugxStaticData.AllowUnregisteredMemberSet;
            LogThisKeyMemberOnlyDefaultPrefs = DebugxStaticData.LogThisKeyMemberOnlyDefaultSet;

            LogOutputPrefs = DebugxStaticData.LogOutputSet;
            EnableLogStackTracePrefs = DebugxStaticData.EnableLogStackTraceSet;
            EnableWarningStackTracePrefs = DebugxStaticData.EnableWarningStackTraceSet;
            EnableErrorStackTracePrefs = DebugxStaticData.EnableErrorStackTraceSet;
            RecordAllNonDebugxLogsPrefs = DebugxStaticData.RecordAllNonDebugxLogsSet;
            DrawLogToScreenPrefs = DebugxStaticData.DrawLogToScreenSet;
            RestrictDrawLogCountPrefs = DebugxStaticData.RestrictDrawLogCountSet;
            MaxDrawLogsPrefs = DebugxStaticData.MaxDrawLogsSet;

            ResetPreferencesMembers();
        }

        public static void ResetPreferencesMembers()
        {
            MemberEnableDefaultDicPrefs.Clear();
            PlayerPrefs.DeleteKey("DebugxStaticData.MemberEnableDefaultDic");
        }

        public static bool EnableLogDefaultPrefs
        {
            get => PlayerPrefsGetBool("DebugxStaticData.EnableLogDefault", DebugxStaticData.EnableLogDefaultSet);
            set => PlayerPrefsSetBool("DebugxStaticData.EnableLogDefault", value);
        }

        public static bool EnableLogMemberDefaultPrefs
        {
            get => PlayerPrefsGetBool("DebugxStaticData.EnableLogMemberDefault", DebugxStaticData.EnableLogMemberDefaultSet);
            set => PlayerPrefsSetBool("DebugxStaticData.EnableLogMemberDefault", value);
        }

        public static bool AllowUnregisteredMember
        {
            get => PlayerPrefsGetBool("DebugxStaticData.AllowUnregisteredMember", DebugxStaticData.AllowUnregisteredMemberSet);
            set => PlayerPrefsSetBool("DebugxStaticData.AllowUnregisteredMember", value);
        }

        public static int LogThisKeyMemberOnlyDefaultPrefs
        {
            get => PlayerPrefs.GetInt("DebugxStaticData.LogThisKeyMemberOnlyDefault", DebugxStaticData.LogThisKeyMemberOnlyDefaultSet);
            set => PlayerPrefs.SetInt("DebugxStaticData.LogThisKeyMemberOnlyDefault", value);
        }

        private static Dictionary<int, bool> memberEnableDefaultDicPrefs;
        public static Dictionary<int, bool> MemberEnableDefaultDicPrefs
        {
            get
            {
                if (memberEnableDefaultDicPrefs == null)
                {
                    memberEnableDefaultDicPrefs = new Dictionary<int, bool>();

                    string data = PlayerPrefs.GetString("DebugxStaticData.MemberEnableDefaultDic");
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
                PlayerPrefs.SetString("DebugxStaticData.MemberEnableDefaultDic", sb.ToString());
            }
        }

        public static bool LogOutputPrefs
        {
            get => PlayerPrefsGetBool("DebugxStaticData.LogOutput", DebugxStaticData.LogOutputSet);
            set => PlayerPrefsSetBool("DebugxStaticData.LogOutput", value);
        }

        public static bool EnableLogStackTracePrefs
        {
            get => PlayerPrefsGetBool("DebugxStaticData.EnableLogStackTrace", DebugxStaticData.EnableLogStackTraceSet);
            set => PlayerPrefsSetBool("DebugxStaticData.EnableLogStackTrace", value);
        }

        public static bool EnableWarningStackTracePrefs
        {
            get => PlayerPrefsGetBool("DebugxStaticData.EnableWarningStackTrace", DebugxStaticData.EnableWarningStackTraceSet);
            set => PlayerPrefsSetBool("DebugxStaticData.EnableWarningStackTrace", value);
        }

        public static bool EnableErrorStackTracePrefs
        {
            get => PlayerPrefsGetBool("DebugxStaticData.EnableErrorStackTrace", DebugxStaticData.EnableErrorStackTraceSet);
            set => PlayerPrefsSetBool("DebugxStaticData.EnableErrorStackTrace", value);
        }

        public static bool RecordAllNonDebugxLogsPrefs
        {
            get => PlayerPrefsGetBool("DebugxStaticData.RecordAllNonDebugxLogs", DebugxStaticData.RecordAllNonDebugxLogsSet);
            set => PlayerPrefsSetBool("DebugxStaticData.RecordAllNonDebugxLogs", value);
        }

        public static bool DrawLogToScreenPrefs
        {
            get => PlayerPrefsGetBool("DebugxStaticData.DrawLogToScreen", DebugxStaticData.DrawLogToScreenSet);
            set => PlayerPrefsSetBool("DebugxStaticData.DrawLogToScreen", value);
        }

        public static bool RestrictDrawLogCountPrefs
        {
            get => PlayerPrefsGetBool("DebugxStaticData.RestrictDrawLogCount", DebugxStaticData.RestrictDrawLogCountSet);
            set => PlayerPrefsSetBool("DebugxStaticData.RestrictDrawLogCount", value);
        }

        public static int MaxDrawLogsPrefs
        {
            get => PlayerPrefs.GetInt("DebugxStaticData.MaxDrawLogs", DebugxStaticData.MaxDrawLogsSet);
            set => PlayerPrefs.SetInt("DebugxStaticData.MaxDrawLogs", value);
        }

        public static bool FAMemberEnableSettingOpen
        {
            get => PlayerPrefsGetBool("DebugxStaticData.FAMemberEnableSettingOpen", true);
            set => PlayerPrefsSetBool("DebugxStaticData.FAMemberEnableSettingOpen", value);
        }

        public static bool CanResetPreferences
        {
            get => PlayerPrefsGetBool("DebugxStaticData.CanResetPreferences", false);
            set => PlayerPrefsSetBool("DebugxStaticData.CanResetPreferences", value);
        }

        public static bool CanResetPreferencesMembers
        {
            get => PlayerPrefsGetBool("DebugxStaticData.CanResetPreferencesMembers", false);
            set => PlayerPrefsSetBool("DebugxStaticData.CanResetPreferencesMembers", value);
        }
        #endregion
    }
}