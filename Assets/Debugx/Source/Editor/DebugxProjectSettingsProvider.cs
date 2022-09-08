using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.Search;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UIElements;

namespace DebugxLog
{
    static class DebugxProjectSettingsProvider
    {
        private static SettingsProvider settingsProvider;

        private static DebugxSettingsProviderConfig ProviderConfig => DebugxSettingsProviderConfig.Get;
        private static DebugxProjectSettingsAsset SettingsAsset => ProviderConfig.AutoSave ? DebugxProjectSettingsAsset.Instance : settingsCopy;
        private static DebugxProjectSettingsAsset SettingsAssetSource => DebugxProjectSettingsAsset.Instance;
        private static DebugxProjectSettingsAsset settingsCopy;//配置复制，在非自动保存时缓存修改内容

        private static readonly List<FadeArea> memberInfosFadeAreaPool = new();
        private static FadeArea fa_MemberConfigSetting;

        private static bool OnGUIInit;

        private static bool anyDataChange = false;

        [SettingsProvider]
        public static SettingsProvider DebugxDebugxSettingsProvider()
        {
            settingsProvider = new SettingsProvider("Project/Debugx", SettingsScope.Project)
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
            DebugxSettingsProviderConfig.OnAutoSaveChange += OnAutoSaveChange;
        }

        private static void Disable()
        {
            OnGUIInit = false;

            //确认是否需要保存
            SaveCheck();

            //标脏当前配置，并保存。主要为了保证FadeAreaOpenCached能被保存下来
            //否则每次FadeAreaOpenCached更新后，重启项目后又恢复旧的数据了。只有直接鼠标点击修改Config文件上的FadeAreaOpenCached才有被确实的标脏和保存
            EditorUtility.SetDirty(ProviderConfig);
            AssetDatabase.SaveAssetIfDirty(ProviderConfig);

            DebugxSettingsProviderConfig.OnAutoSaveChange -= OnAutoSaveChange;
        }

        private static void Draw(string searchContext)
        {
            if (!OnGUIInit)
            {
                //一些初始化内容调用到GUI类，必须在OnGUI内调用
                OnGUIInit = true;
                ResetWindowData();
            }

            EditorGUILayout.HelpBox("此处为项目设置，项目设置会影响所有人的项目和打包后的包体软件。\n如果你仅想对自己的项目做一些本地化的设置，请在 Preferences/Debugx 用户设置中配置。", MessageType.Info);

            anyDataChange = false;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Config Settings", EditorStyle.Get.TitleStyle_3);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("", DebugxProjectSettingsAsset.Instance, typeof(DebugxProjectSettingsAsset), false);
            EditorGUI.EndDisabledGroup();

            //Setting.AutoSave = EditorGUILayout.Toggle(new GUIContent("AutoSave", "自动保存"), Setting.AutoSave);

            ////保存和回退按钮
            //EditorGUILayout.BeginHorizontal();
            //EditorGUI.BeginDisabledGroup(!isDirty);
            //if (GUILayoutx.ButtonGreen("Save")) Save();
            //if (GUILayoutx.ButtonRed("Revert"))
            //    if (EditorUtility.DisplayDialog("回退修改", "确定要回退修改的内容吗？", "确定", "取消"))
            //        Revert();
            //EditorGUI.EndDisabledGroup();
            //EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            //确认是否修改任何参数
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Default", EditorStyle.Get.TitleStyle_2);

            SettingsAsset.enableLogDefault = Toggle("EnableLog Default", "Log总开关，默认状态", SettingsAsset.enableLogDefault);
            SettingsAsset.enableLogMemberDefault = Toggle("EnableLogMember Default", "成员Log总开关，默认状态", SettingsAsset.enableLogMemberDefault);
            SettingsAsset.logThisKeyMemberOnlyDefault = IntField("LogThisKeyMemberOnly", "仅打印此Key的成员Log，0为关闭", SettingsAsset.logThisKeyMemberOnlyDefault);

            //成员配置修改
            DrawMemberConfigSetting();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Log Output", EditorStyle.Get.TitleStyle_2);
            SettingsAsset.logOutput = Toggle("EnbaleLogOutput", "输出Log到本地（启动前设置，运行时设置无效）。编辑器时输出到项目的Logs文件夹下，实机运行时根据平台输出到不同目录下", SettingsAsset.logOutput);
            EditorGUI.BeginDisabledGroup(!SettingsAsset.logOutput);
            SettingsAsset.enableLogStackTrace = Toggle("EnableLogStackTrace", "输出Log类型的堆栈跟踪", SettingsAsset.enableLogStackTrace);
            SettingsAsset.enableWarningStackTrace = Toggle("EnableWarningStackTrace", "输出Warning类型的堆栈跟踪", SettingsAsset.enableWarningStackTrace);
            SettingsAsset.enableErrorStackTrace = Toggle("EnableErrorStackTrace", "输出Error类型的堆栈跟踪", SettingsAsset.enableErrorStackTrace);
            SettingsAsset.recordAllNonDebugxLogs = Toggle("RecordAllNonDebugxLogs", "记录所有非Debugx打印的Log", SettingsAsset.recordAllNonDebugxLogs);
            SettingsAsset.drawLogToScreen = Toggle("DrawLogToScreen", "绘制Log到屏幕", SettingsAsset.drawLogToScreen);
            EditorGUI.BeginDisabledGroup(!SettingsAsset.drawLogToScreen);
            SettingsAsset.restrictDrawLogCount = Toggle("RestrictDrawLogCount", "限制绘制Log数量", SettingsAsset.restrictDrawLogCount);
            SettingsAsset.maxDrawLogs = IntField("MaxDrawLogs", "绘制Log最大数量", SettingsAsset.maxDrawLogs);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            anyDataChange = anyDataChange ? anyDataChange : EditorGUI.EndChangeCheck();
            if (anyDataChange)
            {
                if (ProviderConfig.AutoSave)
                {
                    //DebugxManager.Instance.OnDebugxConfigChange(Config);
                    SettingsAsset.ApplyTo(DebugxProjectSettings.Instance);
                }
                else
                {
                    isDirty = true;//非自动保存时，标脏
                }
            }
        }

        private static void DrawMemberConfigSetting()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Member", EditorStyle.Get.TitleStyle_2);

            fa_MemberConfigSetting.Begin();
            GUIUtilityx.EndChangeCheck(ref anyDataChange);//将开关FadeArea的操作排除isDirty判断
            fa_MemberConfigSetting.Header("成员配置");
            EditorGUI.BeginChangeCheck();

            ProviderConfig.fa_MemberConfigSettingOpen = fa_MemberConfigSetting.BeginFade();
            if (ProviderConfig.fa_MemberConfigSettingOpen)
            {
                EditorGUILayout.LabelField("默认成员配置", EditorStyle.Get.TitleStyle_3);

                FadeArea faTemp = memberInfosFadeAreaPool[0];
                faTemp.Begin();
                GUIUtilityx.EndChangeCheck(ref anyDataChange);//将开关FadeArea的操作排除isDirty判断
                faTemp.Header("普通成员");
                EditorGUI.BeginChangeCheck();
                SettingsAssetSource.normalMember.fadeAreaOpenCached = faTemp.BeginFade();
                if (SettingsAssetSource.normalMember.fadeAreaOpenCached)
                    DrawMemberInfo(ref SettingsAsset.normalMember, true, true);
                faTemp.End();

                faTemp = memberInfosFadeAreaPool[1];
                faTemp.Begin();
                GUIUtilityx.EndChangeCheck(ref anyDataChange);//将开关FadeArea的操作排除isDirty判断
                faTemp.Header("高级成员");
                EditorGUI.BeginChangeCheck();
                SettingsAssetSource.masterMember.fadeAreaOpenCached = faTemp.BeginFade();
                if (SettingsAssetSource.masterMember.fadeAreaOpenCached)
                    DrawMemberInfo(ref SettingsAsset.masterMember, true, true);
                faTemp.End();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("自定义成员配置", EditorStyle.Get.TitleStyle_3);
                if (GUILayout.Button("添加一个成员"))
                {
                    //创建新成员
                    DebugxMemberInfoAsset mInfo = new DebugxMemberInfoAsset();
                    GetMemberKey(out int newKey);
                    mInfo.enableDefault = true;
                    mInfo.signature = $"Menber{newKey}";
                    mInfo.logSignature = true;
                    mInfo.key = newKey;
                    mInfo.color = Color.white;
                    mInfo.fadeAreaOpenCached = true;

                    //添加到数组末尾
                    List<DebugxMemberInfoAsset> memberInfos = SettingsAsset.customDebugxMemberAssets != null ? new List<DebugxMemberInfoAsset>(SettingsAsset.customDebugxMemberAssets) : new List<DebugxMemberInfoAsset>();
                    memberInfos.Add(mInfo);
                    SettingsAsset.customDebugxMemberAssets = memberInfos.ToArray();

                    OnAddMemberInfo(mInfo);
                }
                EditorGUILayout.EndHorizontal();

                if (SettingsAsset && SettingsAsset.customDebugxMemberAssets != null)
                {
                    int removeIndex = -1;

                    for (int i = 0; i < SettingsAsset.customDebugxMemberAssets.Length; i++)
                    {
                        faTemp = memberInfosFadeAreaPool[i + 2];
                        DebugxMemberInfoAsset mInfo = SettingsAsset.customDebugxMemberAssets[i];

                        faTemp.Begin();
                        GUILayout.BeginHorizontal();
                        GUIUtilityx.EndChangeCheck(ref anyDataChange);//将开关FadeArea的操作排除isDirty判断
                        faTemp.Header(string.IsNullOrEmpty(mInfo.signature) ? $"Member {mInfo.key}" : mInfo.signature, 320);
                        EditorGUI.BeginChangeCheck();
                        if (GUILayout.Button("删除成员"))
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
                        if (SettingsAssetSource.customDebugxMemberAssets != null && i < SettingsAssetSource.customDebugxMemberAssets.Length)
                            SettingsAssetSource.customDebugxMemberAssets[i].fadeAreaOpenCached = mInfo.fadeAreaOpenCached;//FadeArea的开关状态直接改变不需要保存确认

                        SettingsAsset.customDebugxMemberAssets[i] = mInfo;

                        faTemp.End();
                    }

                    //移除
                    if (removeIndex >= 0)
                    {
                        OnRemoveMemberInfo(removeIndex, SettingsAsset.customDebugxMemberAssets[removeIndex]);
                        List<DebugxMemberInfoAsset> mInfos = new List<DebugxMemberInfoAsset>(SettingsAsset.customDebugxMemberAssets);
                        mInfos.RemoveAt(removeIndex);
                        SettingsAsset.customDebugxMemberAssets = mInfos.ToArray();
                    }
                }
            }

            fa_MemberConfigSetting.End();
        }

        public static void DrawMemberInfo(ref DebugxMemberInfoAsset mInfo, bool lockSignature = false, bool lockKey = false)
        {
            DebugxMemberInfoAsset mInfoOld = mInfo;

            mInfo.enableDefault = EditorGUILayout.Toggle(new GUIContent("Enable Default", "是否开启，在运行时也可通过DebugxConsole动态改变开关。"), mInfo.enableDefault);

            //签名
            EditorGUI.BeginDisabledGroup(lockSignature);
            mInfo.signature = EditorGUILayout.TextField(new GUIContent("Signature", "成员签名"), mInfo.signature);
            EditorGUI.EndDisabledGroup();
            mInfo.logSignature = EditorGUILayout.Toggle(new GUIContent("LogSignature", "是否打印签名"), mInfo.logSignature);

            //打印密钥
            EditorGUI.BeginDisabledGroup(lockKey);
            EditorGUI.BeginChangeCheck();
            mInfo.key = EditorGUILayout.IntField(new GUIContent("Key", "成员信息密钥，在效用Debugx.Logx()方法时使用"), mInfo.key);
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

            mInfo.header = EditorGUILayout.TextField(new GUIContent("Header", "头部信息，在答应log时打印在头部"), mInfo.header);
            mInfo.color = EditorGUILayout.ColorField(new GUIContent("Color", "Log颜色"), mInfo.color);
        }

        //重置窗口数据
        private static void ResetWindowData()
        {
            fa_MemberConfigSetting = new FadeArea(settingsProvider, ProviderConfig.fa_MemberConfigSettingOpen, EditorStyle.Get.AreaStyle_1, EditorStyle.Get.LabelStyle_FadeAreaHeader, 0.8f);

            memberInfosFadeAreaPool.Clear();
            memberInfoKeyDic.Clear();

            settingsCopy = ScriptableObject.Instantiate(SettingsAssetSource);

            //此方法内调用到了GUI.skin.button，GUI类必须在OnGUI才能调用，不能在OnEnable
            //确认FadeArea是否足够
            OnAddMemberInfo(SettingsAsset.normalMember);
            OnAddMemberInfo(SettingsAsset.masterMember);

            if (SettingsAsset.customDebugxMemberAssets != null)
            {
                for (int i = 0; i < SettingsAsset.customDebugxMemberAssets.Length; i++)
                {
                    OnAddMemberInfo(SettingsAsset.customDebugxMemberAssets[i]);
                }
            }
        }

        //当添加一个成员信息时
        private static void OnAddMemberInfo(DebugxMemberInfoAsset info)
        {
            memberInfosFadeAreaPool.Add(new FadeArea(settingsProvider, info.fadeAreaOpenCached, EditorStyle.Get.AreaStyle_1, EditorStyle.Get.LabelStyle_FadeAreaHeader));
            AddKey(info.key);
        }

        //当移除一个成员信息时
        private static void OnRemoveMemberInfo(int index, DebugxMemberInfoAsset info)
        {
            memberInfosFadeAreaPool.RemoveAt(index + 2);//前面两个时Normal和Master用的
            RemoveKey(info.key);
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

        #region Save Revert

        private static bool isDirty;

        //当自动保存开关变化时
        private static void OnAutoSaveChange(bool enable)
        {
            if (enable)
            {
                SaveCheck();
            }
            else
            {
                settingsCopy = ScriptableObject.Instantiate(SettingsAssetSource);
            }
        }

        //确认是否需要存储
        private static void SaveCheck()
        {
            if (isDirty)
            {
                if (EditorUtility.DisplayDialog("保存修改内容", "有内容被修改未保存，是否保存？", "保存", "回退"))
                {
                    Save();
                }
                else
                {
                    Revert();
                }
            }
        }

        private static void Save()
        {
            if (!isDirty) return;
            isDirty = false;

            DebugxProjectSettingsAsset.Instance.Copy(settingsCopy);
            settingsCopy = ScriptableObject.Instantiate(DebugxProjectSettingsAsset.Instance);
            EditorUtility.SetDirty(DebugxProjectSettingsAsset.Instance);
            AssetDatabase.SaveAssetIfDirty(DebugxProjectSettingsAsset.Instance);
            SettingsAsset.ApplyTo(DebugxProjectSettings.Instance);
        }

        private static void Revert()
        {
            if (!isDirty) return;
            isDirty = false;

            ResetWindowData();
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