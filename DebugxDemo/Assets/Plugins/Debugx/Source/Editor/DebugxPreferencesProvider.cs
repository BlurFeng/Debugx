﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace DebugxLog
{
    static class DebugxPreferencesProvider
    {
        private static SettingsProvider settingsProvider;
        private static DebugxProjectSettings Settings => DebugxProjectSettings.Instance;
        private static DebugxProjectSettingsAsset SettingsAsset => DebugxProjectSettingsAsset.Instance;
        private static Dictionary<int, bool> MemberEnableDefaultDic => DebugxStaticData.MemberEnableDefaultDicPrefs;
        private static FadeArea faMemberEnableSetting;
        private static bool isInitGUI;
        private static ReorderableList membersList;

        [SettingsProvider]
        public static SettingsProvider DebugxPreferencesProviderCreate()
        {
            if(settingsProvider == null)
            {
                isInitGUI = false;

                settingsProvider = new SettingsProvider("Preferences/Debugx", SettingsScope.User)
                {
                    label = "Debugx",
                    activateHandler = Enable,
                    guiHandler = Draw,
                    deactivateHandler = Disable,
                };
            }

            return settingsProvider;
        }

        private static void Enable(string searchContext, VisualElement rootElement)
        {
            DebugxProjectSettingsAsset.CheckDebugxProjectSettingsAsset();
        }

        private static void Disable()
        {
        }

        private static void Draw(string searchContext)
        {
            if (SettingsAsset == null) return;

            if(!isInitGUI)
            {
                isInitGUI = true;

                faMemberEnableSetting = new FadeArea(settingsProvider, DebugxStaticData.FAMemberEnableSettingOpen, GUIStylex.Get.AreaStyle_1, GUIStylex.Get.LabelStyle_FadeAreaHeader, 0.8f);
                membersList = new ReorderableList(Settings.members, typeof(DebugxMemberInfo), false, true, false, false)
                {
                    drawHeaderCallback = DrawMembersHeader,
                    drawElementCallback = DrawMembersElement,
                    elementHeight = 20f,
                };
            }

            string defineSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            if (!defineSymbols.Contains("DEBUG_X"))
            {
                EditorGUILayout.HelpBox("当前项目的Standalone平台未配置宏\"DEBUG_X\",Debugx不会进行工作。", MessageType.Warning);
            }

            EditorGUILayout.HelpBox("此处为用户设置，在 UNITY_EDITOR 编辑器时，一些参数会优先使用 Preferences 用户设置。用户设置不会影响项目的其他人。", MessageType.Info);

            EditorGUI.BeginDisabledGroup(!DebugxStaticData.CanResetPreferences);
            if (GUILayout.Button("Reset to Default"))
            {
                if (EditorUtility.DisplayDialog("Reset to Default", "确认要重置到默认设置吗？", "Ok", "Cancel"))
                {
                    ResetPreferences();
                    Apply();
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Toggle", GUIStylex.Get.TitleStyle_2);

            EditorGUI.BeginChangeCheck();

            DebugxStaticData.EnableLogDefaultPrefs = GUILayoutx.Toggle("EnableLog Default", DebugxStaticData.ToolTip_EnableLogDefault, DebugxStaticData.EnableLogDefaultPrefs);
            DebugxStaticData.EnableLogMemberDefaultPrefs = GUILayoutx.Toggle("EnableLogMember Default", DebugxStaticData.ToolTip_EnableLogMemberDefault, DebugxStaticData.EnableLogMemberDefaultPrefs);
            DebugxStaticData.AllowUnregisteredMember = GUILayoutx.Toggle("AllowUnregisteredMember", DebugxStaticData.ToolTip_AllowUnregisteredMember, DebugxStaticData.AllowUnregisteredMember);
            DebugxStaticData.LogThisKeyMemberOnlyDefaultPrefs = GUILayoutx.IntField("LogThisKeyMemberOnly Default", DebugxStaticData.ToolTip_LogThisKeyMemberOnlyDefault, DebugxStaticData.LogThisKeyMemberOnlyDefaultPrefs);

            EditorGUILayout.Space();

            faMemberEnableSetting.Begin();
            faMemberEnableSetting.Header("Members Enable");
            DebugxStaticData.FAMemberEnableSettingOpen = faMemberEnableSetting.BeginFade();
            if (DebugxStaticData.FAMemberEnableSettingOpen)
            {
                if (Settings.members != null && Settings.members.Length > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Enable All")) SetMemberEnableDefaultDicAll(true);
                    if (GUILayout.Button("Disable All")) SetMemberEnableDefaultDicAll(false);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    membersList.DoLayoutList();
                }
                else EditorGUILayout.LabelField("没有配置任何成员");
                
            }
            faMemberEnableSetting.End();


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Log Output", GUIStylex.Get.TitleStyle_2);
            DebugxStaticData.LogOutputPrefs = GUILayoutx.Toggle("EnbaleLogOutput", DebugxStaticData.ToolTip_LogOutput, DebugxStaticData.LogOutputPrefs);
            EditorGUI.BeginDisabledGroup(!DebugxStaticData.LogOutputPrefs);
            DebugxStaticData.EnableLogStackTracePrefs = GUILayoutx.Toggle("EnableLogStackTrace", DebugxStaticData.ToolTip_EnableLogStackTrace, DebugxStaticData.EnableLogStackTracePrefs);
            DebugxStaticData.EnableWarningStackTracePrefs = GUILayoutx.Toggle("EnableWarningStackTrace", DebugxStaticData.ToolTip_EnableWarningStackTrace, DebugxStaticData.EnableWarningStackTracePrefs);
            DebugxStaticData.EnableErrorStackTracePrefs = GUILayoutx.Toggle("EnableErrorStackTrace", DebugxStaticData.ToolTip_EnableErrorStackTrace, DebugxStaticData.EnableErrorStackTracePrefs);
            DebugxStaticData.RecordAllNonDebugxLogsPrefs = GUILayoutx.Toggle("RecordAllNonDebugxLogs", DebugxStaticData.ToolTip_RecordAllNonDebugxLogs, DebugxStaticData.RecordAllNonDebugxLogsPrefs);
            DebugxStaticData.DrawLogToScreenPrefs = GUILayoutx.Toggle("DrawLogToScreen", DebugxStaticData.ToolTip_DrawLogToScreen, DebugxStaticData.DrawLogToScreenPrefs);
            EditorGUI.BeginDisabledGroup(!DebugxStaticData.DrawLogToScreenPrefs);
            DebugxStaticData.RestrictDrawLogCountPrefs = GUILayoutx.Toggle("RestrictDrawLogCount", DebugxStaticData.ToolTip_RestrictDrawLogCount, DebugxStaticData.RestrictDrawLogCountPrefs);
            DebugxStaticData.MaxDrawLogsPrefs = GUILayoutx.IntField("MaxDrawLogs", DebugxStaticData.ToolTip_MaxDrawLogs, DebugxStaticData.MaxDrawLogsPrefs);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();


            //还是调用DebugxProjectSettingsAsset的保存，里面会判断在UNITY_EDITOR时优先使用Prefs
            if (EditorGUI.EndChangeCheck())
            {
                DebugxStaticData.CanResetPreferences = true;
                Apply();
            }

            EditorGUILayout.Space(16f);
        }

        /// <summary>
        /// 重置用户设置
        /// </summary>
        public static void ResetPreferences()
        {
            if (!DebugxStaticData.CanResetPreferences) return;
            DebugxStaticData.ResetPreferences();
            DebugxStaticData.CanResetPreferences = false;
        }

        private static void DrawMembersHeader(Rect rect)
        {
            var buttonRect = rect;
            buttonRect.x += rect.width - 140f;
            buttonRect.width = 140f;
            buttonRect.y += 1f;
            buttonRect.height -= 2f;

            var titleRect = rect;
            titleRect.width -= buttonRect.width;
            GUI.Label(titleRect, new GUIContent("Members Enable", "此处仅能配置成员的默认开关，详细成员配置在 Project Settings 中设置。在重置到默认状态时，成员开关将恢复到成员配置中的 Enable Default 的值。"));

            EditorGUI.BeginDisabledGroup(!DebugxStaticData.CanResetPreferencesMembers);
            if (GUI.Button(buttonRect, new GUIContent("Reset", "重置到和目前成员配置中的 Enable Default 值一致。")))
            {
                GUI.FocusControl("");//移除焦点
                DebugxStaticData.ResetPreferencesMembers();
                DebugxStaticData.CanResetPreferencesMembers = false;
                Apply();
            }
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawMembersElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= Settings.members.Length) return;

            const float idWidth = 90f;
            const float enableWidth = 30f;
            const float interval = 5f;

            var info = Settings.members[index];
            bool enable = MemberEnableDefaultDic.ContainsKey(info.key) ? MemberEnableDefaultDic[info.key] : Debugx.MemberIsEnable(info.key);
            EditorGUI.BeginChangeCheck();

            Rect idRect = rect;
            idRect.width = idWidth;
            GUI.Label(idRect, $"[{info.key}]");

            Rect signatureRect = rect;
            signatureRect.x += idWidth + interval;
            signatureRect.width = rect.width - idWidth - interval - enableWidth - interval;
            GUI.Label(signatureRect, $"{(string.IsNullOrEmpty(info.signature) ? "Member" : info.signature)}");

            Rect enableRect = rect;
            enableRect.x = signatureRect.x + signatureRect.width + interval;
            enableRect.width = enableWidth;

            bool newEnable = EditorGUI.Toggle(enableRect, enable);
            if (newEnable != enable)
            {
                SetMemberEnableDefaultDic(info.key, newEnable);

                DebugxStaticData.SaveMemberEnableDefaultDicPrefs();
                DebugxStaticData.CanResetPreferencesMembers = true;
                Apply();
            }
        }

        public static void SetMemberEnableDefaultDic(int key, bool enable)
        {
            if (MemberEnableDefaultDic.ContainsKey(key))
                MemberEnableDefaultDic[key] = enable;
            else
                MemberEnableDefaultDic.Add(key, enable);
        }

        public static void SetMemberEnableDefaultDicAll(bool enable)
        {
            for (int i = 0; i < Settings.members.Length; i++)
            {
                SetMemberEnableDefaultDic(Settings.members[i].key, enable);
            }

            DebugxStaticData.SaveMemberEnableDefaultDicPrefs();
            DebugxStaticData.CanResetPreferencesMembers = true;
        }

        public static void Apply()
        {
            //ApplyTo中会判断如果在Editor就使用用户偏好设置，而不是使用DebugxProjectSettingsAsset配置
            DebugxProjectSettingsAsset.Instance.ApplyTo(DebugxProjectSettings.Instance);
        }
    }
}