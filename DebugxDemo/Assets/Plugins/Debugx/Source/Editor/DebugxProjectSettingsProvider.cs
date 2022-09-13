using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;

namespace DebugxLog
{
    static class DebugxProjectSettingsProvider
    {
        private static SettingsProvider settingsProvider;
        private static DebugxProjectSettingsAsset SettingsAsset => DebugxProjectSettingsAsset.Instance;
        private static readonly List<FadeArea> memberInfosFadeAreaPool = new();
        private static FadeArea faMemberConfigSetting;
        private static bool isInitGUI;
        private static bool assetIsDirty;

        [SettingsProvider]
        public static SettingsProvider DebugxProjectSettingsProviderCreate()
        {
            if(settingsProvider == null)
            {
                isInitGUI = false;

                settingsProvider = new SettingsProvider("Project/Debugx", SettingsScope.Project)
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
            DebugxStaticDataEditor.OnAutoSaveChange.Bind(OnAutoSaveChange);
        }

        private static void Disable()
        {
            Save();

            DebugxStaticDataEditor.OnAutoSaveChange.Unbind(OnAutoSaveChange);
        }

        private static void Draw(string searchContext)
        {
            if (!isInitGUI)
            {
                //一些初始化内容调用到GUI类，必须在OnGUI内调用
                isInitGUI = true;
                ResetWindowData();
            }

            string defineSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            if (!defineSymbols.Contains("DEBUG_X"))
            {
                EditorGUILayout.HelpBox("当前项目的Standalone平台未配置宏\"DEBUG_X\",Debugx不会进行工作。", MessageType.Warning);
            }

            EditorGUILayout.HelpBox("此处为项目设置，项目设置会影响所有人的项目和打包后的包体软件。\n如果你仅想对自己的项目做一些本地化的设置，请在 Preferences/Debugx 用户设置中配置。", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            DebugxStaticDataEditor.AutoSave = GUILayoutx.Toggle("AutoSave Asset", "自动保存配置资源，自动保存时在修改内容时会有卡顿。", DebugxStaticDataEditor.AutoSave);
            EditorGUI.BeginDisabledGroup(!assetIsDirty);
            if (GUILayoutx.ButtonGreen("Save Asset")) Save();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(false/*!EditorConfig.canResetProjectSettings*/);
            if (GUILayoutx.ButtonRed("Reset to Default"))
            {
                if (EditorUtility.DisplayDialog("Reset To Default", "确认要重置到默认设置吗？\n重置并不会清空Member成员配置，仅将成员配置的部分字段重置。", "Ok", "Cancel"))
                {
                    Undo.RecordObject(SettingsAsset, "ResetToDefault");
                    ResetProjectSettings();
                    Apply();
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Config Settings", GUIStylex.Get.TitleStyle_3);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("", DebugxProjectSettingsAsset.Instance, typeof(DebugxProjectSettingsAsset), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            //确认是否修改任何参数
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Toggle", GUIStylex.Get.TitleStyle_2);

            SettingsAsset.enableLogDefault = Toggle("EnableLog Default", DebugxStaticData.ToolTip_EnableLogDefault, SettingsAsset.enableLogDefault);
            SettingsAsset.enableLogMemberDefault = Toggle("EnableLogMember Default", DebugxStaticData.ToolTip_EnableLogMemberDefault, SettingsAsset.enableLogMemberDefault);
            SettingsAsset.logThisKeyMemberOnlyDefault = IntField("LogThisKeyMemberOnly Default", DebugxStaticData.ToolTip_LogThisKeyMemberOnlyDefault, SettingsAsset.logThisKeyMemberOnlyDefault);

            //成员配置修改
            DrawMemberConfigSetting();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Log Output", GUIStylex.Get.TitleStyle_2);
            SettingsAsset.logOutput = Toggle("EnbaleLogOutput", DebugxStaticData.ToolTip_LogOutput, SettingsAsset.logOutput);
            EditorGUI.BeginDisabledGroup(!SettingsAsset.logOutput);
            SettingsAsset.enableLogStackTrace = Toggle("EnableLogStackTrace", DebugxStaticData.ToolTip_EnableLogStackTrace, SettingsAsset.enableLogStackTrace);
            SettingsAsset.enableWarningStackTrace = Toggle("EnableWarningStackTrace", DebugxStaticData.ToolTip_EnableWarningStackTrace, SettingsAsset.enableWarningStackTrace);
            SettingsAsset.enableErrorStackTrace = Toggle("EnableErrorStackTrace", DebugxStaticData.ToolTip_EnableErrorStackTrace, SettingsAsset.enableErrorStackTrace);
            SettingsAsset.recordAllNonDebugxLogs = Toggle("RecordAllNonDebugxLogs", DebugxStaticData.ToolTip_RecordAllNonDebugxLogs, SettingsAsset.recordAllNonDebugxLogs);
            SettingsAsset.drawLogToScreen = Toggle("DrawLogToScreen", DebugxStaticData.ToolTip_DrawLogToScreen, SettingsAsset.drawLogToScreen);
            EditorGUI.BeginDisabledGroup(!SettingsAsset.drawLogToScreen);
            SettingsAsset.restrictDrawLogCount = Toggle("RestrictDrawLogCount", DebugxStaticData.ToolTip_RestrictDrawLogCount, SettingsAsset.restrictDrawLogCount);
            SettingsAsset.maxDrawLogs = IntField("MaxDrawLogs", DebugxStaticData.ToolTip_MaxDrawLogs, SettingsAsset.maxDrawLogs);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                Apply();
            }

            EditorGUILayout.Space(16f);
        }

        private static void DrawMemberConfigSetting()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Member Settings", GUIStylex.Get.TitleStyle_2);

            faMemberConfigSetting.Begin();
            faMemberConfigSetting.Header("Members");

            DebugxStaticDataEditor.FAMemberConfigSettingOpen = faMemberConfigSetting.BeginFade();
            if (DebugxStaticDataEditor.FAMemberConfigSettingOpen)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Reset Members Part Config", "重置成员设置，仅会重置部分成员的配置到默认值。（重置内容：EnableDefault,LogSignature,fadeAreaOpen）")))
                {
                    if (EditorUtility.DisplayDialog("Reset Members Part Config", "确认要重置所有成员的部分设置吗？", "Ok", "Cancel"))
                    {
                        Undo.RecordObject(SettingsAsset, "ResetMembersPartConfig");
                        ResetProjectSettingsMembers();
                    }
                }
                if (GUILayout.Button(new GUIContent("Adapt Color By Editor Skin", "颜色根据编辑器皮肤自动适应。在Dark暗皮肤时Log颜色会变亮，在Light亮皮肤时Log颜色会变暗。")))
                {
                    if (EditorUtility.DisplayDialog("Adapt Color By Editor Skin", "确认要执行颜色根据编辑器皮肤自动适应吗？", "Ok", "Cancel"))
                    {
                        Undo.RecordObject(SettingsAsset, "AdaptColorByEditorSkin");
                        ColorDispenser.AdaptColorByEditorSkin();
                    }
                }
                EditorGUILayout.EndHorizontal();

                //默认成员
                if (SettingsAsset.defaultMemberAssets != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Default", GUIStylex.Get.TitleStyle_3);
                    if (GUILayoutx.ButtonYellow("Reset Default Members", "重置默认成员，这会重置默认成员的所有数据。"))
                    {
                        if (EditorUtility.DisplayDialog("Reset Default Members", "确认要重置所有默认成员吗？", "Ok", "Cancel"))
                        {
                            Undo.RecordObject(SettingsAsset, "ResetDefaultMembers");
                            SettingsAsset.CreateDefaultMembers();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    FadeArea faTemp;
                    for (int i = 0; i < SettingsAsset.defaultMemberAssets.Length; i++)
                    {
                        DebugxMemberInfoAsset mInfo = SettingsAsset.defaultMemberAssets[i];

                        faTemp = memberInfosFadeAreaPool[i];
                        faTemp.Begin();
                        faTemp.Header(string.IsNullOrEmpty(mInfo.signature) ? $"Member {mInfo.key}" : mInfo.signature);
                        SettingsAsset.defaultMemberAssets[i].fadeAreaOpenCached = faTemp.BeginFade();
                        if (SettingsAsset.defaultMemberAssets[i].fadeAreaOpenCached)
                            DrawMemberInfo(ref SettingsAsset.defaultMemberAssets[i], true, true);
                        faTemp.End();
                    }
                }

                //自定义成员
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Custom Members", GUIStylex.Get.TitleStyle_3);
                if (GUILayoutx.ButtonYellow("Automatically Reassign Colors", "自动重分配所有自定义成员的颜色，颜色将根据成员数量平均分配。"))
                {
                    if (EditorUtility.DisplayDialog("Automatically Reassign Colors", "确认要重分配所有自定义成员的颜色吗？", "Ok", "Cancel"))
                    {
                        Undo.RecordObject(SettingsAsset, "AutomaticallyReassignColors");
                        ColorDispenser.AutomaticallyReassignColors();
                    }
                }
                if (GUILayout.Button("Add Member"))
                {
                    Undo.RecordObject(SettingsAsset, "AddMember");

                    //创建新成员
                    GetMemberKey(out int newKey);
                    DebugxMemberInfoAsset mInfo = new DebugxMemberInfoAsset(newKey);

                    //添加到数组末尾
                    List<DebugxMemberInfoAsset> memberInfos = SettingsAsset.customMemberAssets != null ? new List<DebugxMemberInfoAsset>(SettingsAsset.customMemberAssets) : new List<DebugxMemberInfoAsset>();
                    memberInfos.Add(mInfo);
                    SettingsAsset.customMemberAssets = memberInfos.ToArray();

                    OnAddMemberInfo(mInfo);
                }
                EditorGUILayout.EndHorizontal();

                if (SettingsAsset && SettingsAsset.customMemberAssets != null)
                {
                    int removeIndex = -1;

                    FadeArea faTemp;
                    for (int i = 0; i < SettingsAsset.customMemberAssets.Length; i++)
                    {
                        int index = i + SettingsAsset.DefaultMemberAssetsLength;
                        if (index >= memberInfosFadeAreaPool.Count)
                        {
                            //Undo回退可能导致memberInfosFadeAreaPool数量不正确
                            ResetWindowData();
                            break;
                        }
                        faTemp = memberInfosFadeAreaPool[index];
                        DebugxMemberInfoAsset mInfo = SettingsAsset.customMemberAssets[i];

                        faTemp.Begin();
                        GUILayout.BeginHorizontal();
                        faTemp.Header(string.IsNullOrEmpty(mInfo.signature) ? $"Member {mInfo.key}" : mInfo.signature, 320);
                        if (GUILayout.Button("Delete Member"))
                        {
                            removeIndex = i;
                        }
                        GUILayout.EndHorizontal();

                        mInfo.fadeAreaOpenCached = faTemp.BeginFade();
                        if (mInfo.fadeAreaOpenCached)
                        {
                            DrawMemberInfo(ref mInfo);
                        }

                        //更新数据
                        if (SettingsAsset.customMemberAssets != null && i < SettingsAsset.customMemberAssets.Length)
                            SettingsAsset.customMemberAssets[i].fadeAreaOpenCached = mInfo.fadeAreaOpenCached;//FadeArea的开关状态直接改变不需要保存确认

                        SettingsAsset.customMemberAssets[i] = mInfo;

                        faTemp.End();
                    }

                    //移除
                    if (removeIndex >= 0)
                    {
                        Undo.RecordObject(SettingsAsset, "DeleteMember");
                        OnRemoveMemberInfo(removeIndex, SettingsAsset.customMemberAssets[removeIndex]);
                        List<DebugxMemberInfoAsset> mInfos = new(SettingsAsset.customMemberAssets);
                        mInfos.RemoveAt(removeIndex);
                        SettingsAsset.customMemberAssets = mInfos.ToArray();
                    }
                }
            }

            faMemberConfigSetting.End();
        }

        public static void DrawMemberInfo(ref DebugxMemberInfoAsset mInfo, bool lockSignature = false, bool lockKey = false)
        {
            DebugxMemberInfoAsset mInfoOld = mInfo;

            mInfo.enableDefault = EditorGUILayout.Toggle(new GUIContent("Enable Default", "是否开启，在运行时也可通过DebugxConsole动态改变开关。"), mInfo.enableDefault);

            //签名
            EditorGUI.BeginDisabledGroup(lockSignature);
            mInfo.signature = EditorGUILayout.DelayedTextField(new GUIContent("Signature", "成员签名"), mInfo.signature);
            EditorGUI.EndDisabledGroup();
            mInfo.logSignature = EditorGUILayout.Toggle(new GUIContent("LogSignature", "是否打印签名"), mInfo.logSignature);

            //打印密钥
            EditorGUI.BeginDisabledGroup(lockKey);
            EditorGUI.BeginChangeCheck();
            mInfo.key = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Key", "成员信息密钥，在效用Debugx.Logx()方法时使用"), mInfo.key), int.MinValue, int.MaxValue);
            if (EditorGUI.EndChangeCheck())
            {
                RemoveKey(mInfoOld.key);

                //确认Key是否重复，重复时自动从最小可用Key开始使用
                if (!DebugxProjectSettings.KeyValid(mInfo.key) || CheckMemberKeyRepetition(mInfo.key))
                {
                    if (GetMemberKey(out int newKey))
                    {
                        mInfo.key = newKey;
                    }
                    else
                    {
                        mInfo.key = mInfoOld.key;
                    }
                }

                AddKey(mInfo.key);
            }
            EditorGUI.EndDisabledGroup();

            mInfo.header = EditorGUILayout.DelayedTextField(new GUIContent("Header", "头部信息，在答应log时打印在头部"), mInfo.header);

            EditorGUILayout.BeginHorizontal();
            mInfo.color = EditorGUILayout.ColorField(new GUIContent("Color", "Log颜色"), mInfo.color);
            if (GUILayout.Button(new GUIContent("Adapt Color", "颜色根据编辑器皮肤自动适应。在Dark暗皮肤时Log颜色会变亮，在Light亮皮肤时Log颜色会变暗。"), GUILayout.Width(100)))
            {
                Undo.RecordObject(SettingsAsset, "AdaptColor Single");
                mInfo.color = ColorDispenser.GetMemberColorByEditorSkin(mInfo.color);
            }
            EditorGUILayout.EndHorizontal();
        }

        //重置窗口数据
        private static void ResetWindowData()
        {
            //此方法内调用到了GUI.skin.button，GUI类必须在OnGUI才能调用，不能在OnEnable

            faMemberConfigSetting = new FadeArea(settingsProvider, DebugxStaticDataEditor.FAMemberConfigSettingOpen, GUIStylex.Get.AreaStyle_1, GUIStylex.Get.LabelStyle_FadeAreaHeader, 0.8f);

            memberInfosFadeAreaPool.Clear();
            memberInfoKeyDic.Clear();

            if (SettingsAsset.defaultMemberAssets != null)
            {
                for (int i = 0; i < SettingsAsset.defaultMemberAssets.Length; i++)
                {
                    OnAddMemberInfo(SettingsAsset.defaultMemberAssets[i]);
                }
            }

            if (SettingsAsset.customMemberAssets != null)
            {
                for (int i = 0; i < SettingsAsset.customMemberAssets.Length; i++)
                {
                    OnAddMemberInfo(SettingsAsset.customMemberAssets[i]);
                }
            }
        }

        //当添加一个成员信息时
        private static void OnAddMemberInfo(DebugxMemberInfoAsset info)
        {
            memberInfosFadeAreaPool.Add(new FadeArea(settingsProvider, info.fadeAreaOpenCached, GUIStylex.Get.AreaStyle_1, GUIStylex.Get.LabelStyle_FadeAreaHeader));
            AddKey(info.key);
        }

        //当移除一个成员信息时
        private static void OnRemoveMemberInfo(int index, DebugxMemberInfoAsset info)
        {
            memberInfosFadeAreaPool.RemoveAt(index + 2);//前面两个时Normal和Master用的
            RemoveKey(info.key);
        }

        private static void ResetProjectSettings()
        {
            //if (!EditorConfig.canResetProjectSettings) return;

            SettingsAsset.enableLogDefault = DebugxStaticData.enableLogDefaultSet;
            SettingsAsset.enableLogMemberDefault = DebugxStaticData.enableLogMemberDefaultSet;
            SettingsAsset.logThisKeyMemberOnlyDefault = DebugxStaticData.logThisKeyMemberOnlyDefaultSet;

            SettingsAsset.logOutput = DebugxStaticData.logOutputSet;
            SettingsAsset.enableLogStackTrace = DebugxStaticData.enableLogStackTraceSet;
            SettingsAsset.enableWarningStackTrace = DebugxStaticData.enableWarningStackTraceSet;
            SettingsAsset.enableErrorStackTrace = DebugxStaticData.enableErrorStackTraceSet;
            SettingsAsset.recordAllNonDebugxLogs = DebugxStaticData.recordAllNonDebugxLogsSet;
            SettingsAsset.drawLogToScreen = DebugxStaticData.drawLogToScreenSet;
            SettingsAsset.restrictDrawLogCount = DebugxStaticData.restrictDrawLogCountSet;
            SettingsAsset.maxDrawLogs = DebugxStaticData.maxDrawLogsSet;

            ResetProjectSettingsMembers();

            //EditorConfig.canResetProjectSettings = false;
        }

        private static void ResetProjectSettingsMembers()
        {
            for (int i = 0; i < SettingsAsset.defaultMemberAssets.Length; i++)
            {
                SettingsAsset.defaultMemberAssets[i].ResetToDefaultPart();
            }

            for (int i = 0; i < SettingsAsset.customMemberAssets.Length; i++)
            {
                SettingsAsset.customMemberAssets[i].ResetToDefaultPart();
            }
        }

        public static void Apply()
        {
            SettingsAsset.ApplyTo(DebugxProjectSettings.Instance);

            if(DebugxStaticDataEditor.AutoSave)
            {
                Save(true);
            }
            else
            {
                assetIsDirty = true;
            }

            //EditorConfig.canResetProjectSettings = true;
        }

        private static void Save(bool force = false)
        {
            if (!force && !assetIsDirty) return;
            assetIsDirty = false;

            EditorUtility.SetDirty(SettingsAsset);
            AssetDatabase.SaveAssetIfDirty(SettingsAsset);
        }

        private static void OnAutoSaveChange(bool enable)
        {
            //变为自动保存时，自动进行一次存储
            if(enable)
            {
                Save();
            }
        }

        #region MemberInfoKey
        //Key=DebugxMemberInfo.key vaule=使用的成员信息，要求只能有一个
        private static Dictionary<int, int> memberInfoKeyDic = new Dictionary<int, int>();

        private static void AddKey(int key)
        {
            if (memberInfoKeyDic.ContainsKey(key))
            {
                memberInfoKeyDic[key]++;

                if (memberInfoKeyDic[key] > 1)
                {
                    //重复了
                    Debugx.LogNomError($"Key：{key} 重复了。");
                }
            }
            //记录已经被使用过的Key
            else
            {
                memberInfoKeyDic.Add(key, 1);
            }
        }

        private static void RemoveKey(int key)
        {
            if (!memberInfoKeyDic.ContainsKey(key)) return;

            memberInfoKeyDic[key]--;
        }

        //确认成员信息的Key是否重复，重复时返回true
        private static bool CheckMemberKeyRepetition(int key)
        {
            if (!memberInfoKeyDic.ContainsKey(key)) return false;

            return memberInfoKeyDic[key] >= 1;
        }

        //获取一个不重复的Key
        private static bool GetMemberKey(out int key)
        {
            key = 1;
            while (CheckMemberKeyRepetition(key))
            {
                if (key > int.MaxValue)
                {
                    return false;
                }
                key++;
            }

            return true;
        }
        #endregion

        #region GUILayout for Undo

        public static bool Toggle(string label, string tooltip, bool value, float width = 250f)
        {
            bool valueCached = value;
            bool valueNew = GUILayoutx.Toggle(label, tooltip, valueCached, width);
            if(valueNew != valueCached)
            {
                Undo.RecordObject(SettingsAsset, "DebugxSettingsProvider Toggle Set");
            }

            return valueNew;
        }

        public static int IntField(string label, string tooltip, int value, float width = 250f)
        {
            int valueCached = value;
            int valueNew = GUILayoutx.IntField(label, tooltip, valueCached, width);
            if (valueNew != valueCached)
            {
                Undo.RecordObject(SettingsAsset, "DebugxSettingsProvider Int Set");
            }

            return valueNew;
        }

        #endregion
    }
}