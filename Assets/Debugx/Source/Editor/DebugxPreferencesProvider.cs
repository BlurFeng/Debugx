using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.Search;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UIElements;

namespace DebugxLog
{
    static class DebugxPreferencesProvider
    {
        private static SettingsProvider settingsProvider;

        private static DebugxProjectSettings Settings => DebugxProjectSettings.Instance;
        private static Dictionary<int, bool> MemberEnableDefaultDic => DebugxStaticData.MemberEnableDefaultDic;

        [SettingsProvider]
        public static SettingsProvider DebugxDebugxSettingsProvider()
        {
            settingsProvider = new SettingsProvider("Preferences/Debugx", SettingsScope.User)
            {
                label = "Debugx",
                activateHandler = Enable,
                guiHandler = Draw,
                deactivateHandler = Disable,
            };

            return settingsProvider;
        }

        private static void Enable(string searchContext, VisualElement rootElement)
        {

        }

        private static void Disable()
        {

        }

        private static void Draw(string searchContext)
        {
            EditorGUILayout.HelpBox("此处为用户设置，在 UNITY_EDITOR 编辑器时，一些参数会优先使用 Preferences 用户设置。用户设置不会影响项目的其他人。", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Default", EditorStyle.Get.TitleStyle_2);

            EditorGUI.BeginChangeCheck();

            DebugxStaticData.EnableLogDefault = GUILayoutx.Toggle("EnableLog Default", "Log总开关，默认状态", DebugxStaticData.EnableLogDefault);
            DebugxStaticData.EnableLogMemberDefault = GUILayoutx.Toggle("EnableLogMember Default", "成员Log总开关，默认状态", DebugxStaticData.EnableLogMemberDefault);
            DebugxStaticData.LogThisKeyMemberOnlyDefault = GUILayoutx.IntField("LogThisKeyMemberOnly", "仅打印此Key的成员Log，0为关闭", DebugxStaticData.LogThisKeyMemberOnlyDefault);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Members", EditorStyle.Get.TitleStyle_3);
            if (Settings.members != null && Settings.members.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < Settings.members.Length; i++)
                {
                    var info = Settings.members[i];
                    EditorGUILayout.BeginHorizontal();
                    bool enable = MemberEnableDefaultDic.ContainsKey(info.key) ? MemberEnableDefaultDic[info.key] : Debugx.MemberIsEnable(info.key);
                    EditorGUI.BeginChangeCheck();
                    enable = GUILayoutx.Toggle($" [{info.key}] {(string.IsNullOrEmpty(info.signature) ? "Member" : info.signature)}", "", enable);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (MemberEnableDefaultDic.ContainsKey(info.key))
                            MemberEnableDefaultDic[info.key] = enable;
                        else
                            MemberEnableDefaultDic.Add(info.key, enable);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (EditorGUI.EndChangeCheck())
                    DebugxStaticData.SaveMemberEnableDefaultDic();
            }
            else
            {
                EditorGUILayout.LabelField("没有配置任何成员");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Log Output", EditorStyle.Get.TitleStyle_2);
            DebugxStaticData.LogOutput = GUILayoutx.Toggle("EnbaleLogOutput", "输出Log到本地（启动前设置，运行时设置无效）。编辑器时输出到项目的Logs文件夹下，实机运行时根据平台输出到不同目录下", DebugxStaticData.LogOutput);
            EditorGUI.BeginDisabledGroup(!DebugxStaticData.LogOutput);
            DebugxStaticData.EnableLogStackTrace = GUILayoutx.Toggle("EnableLogStackTrace", "输出Log类型的堆栈跟踪", DebugxStaticData.EnableLogStackTrace);
            DebugxStaticData.EnableWarningStackTrace = GUILayoutx.Toggle("EnableWarningStackTrace", "输出Warning类型的堆栈跟踪", DebugxStaticData.EnableWarningStackTrace);
            DebugxStaticData.EnableErrorStackTrace = GUILayoutx.Toggle("EnableErrorStackTrace", "输出Error类型的堆栈跟踪", DebugxStaticData.EnableErrorStackTrace);
            DebugxStaticData.RecordAllNonDebugxLogs = GUILayoutx.Toggle("RecordAllNonDebugxLogs", "记录所有非Debugx打印的Log", DebugxStaticData.RecordAllNonDebugxLogs);
            DebugxStaticData.DrawLogToScreen = GUILayoutx.Toggle("DrawLogToScreen", "绘制Log到屏幕", DebugxStaticData.DrawLogToScreen);
            EditorGUI.BeginDisabledGroup(!DebugxStaticData.DrawLogToScreen);
            DebugxStaticData.RestrictDrawLogCount = GUILayoutx.Toggle("RestrictDrawLogCount", "限制绘制Log数量", DebugxStaticData.RestrictDrawLogCount);
            DebugxStaticData.MaxDrawLogs = GUILayoutx.IntField("MaxDrawLogs", "绘制Log最大数量", DebugxStaticData.MaxDrawLogs);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            //还是调用DebugxProjectSettingsAsset的保存，里面会判断在UNITY_EDITOR时优先使用Prefs
            if (EditorGUI.EndChangeCheck())
            {
                DebugxProjectSettingsAsset.Instance.ApplyTo(DebugxProjectSettings.Instance);
            }
        }
    }
}