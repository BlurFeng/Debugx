using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DebugxU3D
{
    public class DebugxSettingWindow : EditorWindow
    {
        private static EditorWindow window;

        private DebugxMemberConfig Config => DebugxSettingWindowConfig.Current.AutoSave ? DebugxSettingWindowConfig.Current.debugxMemberConfigSet : configCopy;
        private DebugxMemberConfig configCopy;//配置复制，在非自动保存时缓存修改内容

        private readonly List<FadeArea> memberInfosFadeAreaPool = new();
        private Vector2 scrollViewPos;
        private FadeArea faTemp;

        private bool OnGUIInit;
        private DebugxMemberConfig debugxMemberConfigOld;


        [MenuItem("Tools/Debugx/DebugxSettingWindow", false, 1)]
        public static void ShowWindow()
        {
            window = EditorWindow.GetWindow(typeof(DebugxSettingWindow));
            window.minSize = new Vector2(460f, 500f);
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Debugx Setting Window");
            debugxMemberConfigOld = DebugxSettingWindowConfig.Current.debugxMemberConfigSet;

            DebugxSettingWindowConfig.OnAutoSaveChange += OnAutoSaveChange;
        }

        private void OnDisable()
        {
            //确认是否需要保存
            SaveCheck(DebugxSettingWindowConfig.Current.debugxMemberConfigSet);

            DebugxSettingWindowConfig.OnAutoSaveChange -= OnAutoSaveChange;

            //标脏当前配置，并保存。主要为了保证FadeAreaOpenCached能被保存下来
            //否则每次FadeAreaOpenCached更新后，重启项目后又恢复旧的数据了。只有直接鼠标点击修改Config文件上的FadeAreaOpenCached才有被确实的标脏和保存
            EditorUtility.SetDirty(DebugxSettingWindowConfig.Current.debugxMemberConfigSet);
            AssetDatabase.SaveAssetIfDirty(DebugxSettingWindowConfig.Current.debugxMemberConfigSet);
        }

        private void OnGUI()
        {
            if(!OnGUIInit)
            {
                //一些初始化内容调用到GUI类，必须在OnGUI内调用
                OnGUIInit = true;
                ResetWindowData();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debugx设置窗口", EditorStyle.Get.TitleStyle_1);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("编辑器设置", EditorStyle.Get.TitleStyle_2);
            EditorGUI.BeginChangeCheck();
            var configOldInitOnEditorLoad = DebugxSettingWindowConfig.Current.debugxMemberConfigSet;
            DebugxSettingWindowConfig.Current.debugxMemberConfigInitOnEditorLoad =
                EditorGUILayout.ObjectField(
                    new GUIContent("ConfigInitOnEditorLoad", "在编辑器启动时加载的调试成员配置。"),
                    DebugxSettingWindowConfig.Current.debugxMemberConfigInitOnEditorLoad,
                    typeof(DebugxMemberConfig), false) as DebugxMemberConfig;
            if (EditorGUI.EndChangeCheck() || DebugxSettingWindowConfig.Current.debugxMemberConfigInitOnEditorLoad == null)
            {
                if(DebugxSettingWindowConfig.Current.debugxMemberConfigInitOnEditorLoad == null)
                    DebugxSettingWindowConfig.Current.debugxMemberConfigInitOnEditorLoad = configOldInitOnEditorLoad;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("编辑调试成员配置", EditorStyle.Get.TitleStyle_2);
            //调试成员配置文件
            EditorGUI.BeginChangeCheck();
            var configOld = DebugxSettingWindowConfig.Current.debugxMemberConfigSet;
            DebugxSettingWindowConfig.Current.debugxMemberConfigSet = 
                EditorGUILayout.ObjectField(
                    new GUIContent("Config", "默认调试成员配置文件"), 
                    DebugxSettingWindowConfig.Current.debugxMemberConfigSet, 
                    typeof(DebugxMemberConfig), false) as DebugxMemberConfig;
            if (EditorGUI.EndChangeCheck() || DebugxSettingWindowConfig.Current.debugxMemberConfigSet == null)
            {
                if(debugxMemberConfigOld != DebugxSettingWindowConfig.Current.debugxMemberConfigSet)
                {
                    if(debugxMemberConfigOld != null)
                    {
                        EditorUtility.SetDirty(debugxMemberConfigOld);
                        AssetDatabase.SaveAssetIfDirty(debugxMemberConfigOld);
                    }
                    debugxMemberConfigOld = DebugxSettingWindowConfig.Current.debugxMemberConfigSet;
                }

                //不允许为空
                if(DebugxSettingWindowConfig.Current.debugxMemberConfigSet == null)
                    DebugxSettingWindowConfig.Current.debugxMemberConfigSet = configOld;
                //更换了调试成员配置资源文件
                else
                {
                    SaveCheck(configOld);
                    configCopy = ScriptableObject.Instantiate(DebugxSettingWindowConfig.Current.debugxMemberConfigSet);
                }
            }

            DebugxSettingWindowConfig.Current.AutoSave = EditorGUILayout.Toggle(new GUIContent("AutoSave", "自动保存"), DebugxSettingWindowConfig.Current.AutoSave);

            //保存和回退按钮
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!isDirty);
            if (GUILayoutx.ButtonGreen("Save")) Save(DebugxSettingWindowConfig.Current.debugxMemberConfigSet);
            if (GUILayoutx.ButtonRed("Revert")) 
                if (EditorUtility.DisplayDialog("回退修改", "确定要回退修改的内容吗？", "确定", "取消")) 
                    Revert(DebugxSettingWindowConfig.Current.debugxMemberConfigSet);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            //确认是否修改任何参数
            bool anyDataChange = false, anyDataChangeTemp = false;
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("默认Log配置", EditorStyle.Get.TitleStyle_2);
            EditorGUILayout.Space();

            faTemp = memberInfosFadeAreaPool[0];
            faTemp.Begin();
            anyDataChangeTemp = EditorGUI.EndChangeCheck();
            anyDataChange = anyDataChange ? anyDataChange : anyDataChangeTemp;//将开关FadeArea的操作排除
            faTemp.Header("普通Log");
            EditorGUI.BeginChangeCheck();
            DebugxSettingWindowConfig.Current.debugxMemberConfigSet.normalInfo.fadeAreaOpenCached = faTemp.BeginFade();
            if(DebugxSettingWindowConfig.Current.debugxMemberConfigSet.normalInfo.fadeAreaOpenCached)
                DrawMemberInfo(ref Config.normalInfo, true, true);
            faTemp.End();

            faTemp = memberInfosFadeAreaPool[1];
            faTemp.Begin();
            anyDataChangeTemp = EditorGUI.EndChangeCheck();
            anyDataChange = anyDataChange ? anyDataChange : anyDataChangeTemp;//将开关FadeArea的操作排除
            faTemp.Header("高级Log");
            EditorGUI.BeginChangeCheck();
            DebugxSettingWindowConfig.Current.debugxMemberConfigSet.masterInfo.fadeAreaOpenCached = faTemp.BeginFade();
            if (DebugxSettingWindowConfig.Current.debugxMemberConfigSet.masterInfo.fadeAreaOpenCached)
                DrawMemberInfo(ref Config.masterInfo, true, true);
            faTemp.End();

            EditorGUILayout.Space(); EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("成员信息列表", EditorStyle.Get.TitleStyle_2);
            if(GUILayout.Button("添加一个成员"))
            {
                //创建新成员
                DebugxMemberInfo mInfo = new DebugxMemberInfo();
                GetMemberKey(out int newKey);
                mInfo.signature = $"Menber{newKey}";
                mInfo.logSignature = true;
                mInfo.key = newKey;
                mInfo.color = Color.white;
                mInfo.enableCached = true;
                mInfo.fadeAreaOpenCached = true;

                //添加到数组末尾
                List<DebugxMemberInfo> memberInfos = Config.debugxMemberInfos != null ? new List<DebugxMemberInfo>(Config.debugxMemberInfos) : new List<DebugxMemberInfo>();
                memberInfos.Add(mInfo);
                Config.debugxMemberInfos = memberInfos.ToArray();

                OnAddMemberInfo(mInfo);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos);
            if (Config && Config.debugxMemberInfos != null)
            {
                int removeIndex = -1;

                for (int i = 0; i < Config.debugxMemberInfos.Length; i++)
                {
                    faTemp = memberInfosFadeAreaPool[i + 2];
                    DebugxMemberInfo mInfo = Config.debugxMemberInfos[i];

                    faTemp.Begin();
                    GUILayout.BeginHorizontal();
                    anyDataChange = anyDataChange ? anyDataChange : EditorGUI.EndChangeCheck();//将开关FadeArea的操作排除
                    faTemp.Header(string.IsNullOrEmpty(mInfo.signature) ? $"Member {mInfo.key}": mInfo.signature, 320);
                    EditorGUI.BeginChangeCheck();
                    if (GUILayout.Button("删除成员", GUILayout.MinWidth(100)))
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
                    if(DebugxSettingWindowConfig.Current.debugxMemberConfigSet.debugxMemberInfos != null && i < DebugxSettingWindowConfig.Current.debugxMemberConfigSet.debugxMemberInfos.Length)
                        DebugxSettingWindowConfig.Current.debugxMemberConfigSet.debugxMemberInfos[i].fadeAreaOpenCached = mInfo.fadeAreaOpenCached;//FadeArea的开关状态直接改变不需要保存确认

                    Config.debugxMemberInfos[i] = mInfo;

                    faTemp.End();
                }

                //移除
                if(removeIndex >= 0)
                {
                    OnRemoveMemberInfo(removeIndex, Config.debugxMemberInfos[removeIndex]);
                    List<DebugxMemberInfo> mInfos = new List<DebugxMemberInfo>(Config.debugxMemberInfos);
                    mInfos.RemoveAt(removeIndex);
                    Config.debugxMemberInfos = mInfos.ToArray();
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            anyDataChange = anyDataChange ? anyDataChange : EditorGUI.EndChangeCheck();
            if (anyDataChange)
            {
                
                if (DebugxSettingWindowConfig.Current.AutoSave)
                {
                    DebugxManager.Instance.OnDebugxMemberConfigChange(Config);
                }
                else
                {
                    isDirty = true;//非自动保存时，标脏
                }
            }
        }

        public void DrawMemberInfo(ref DebugxMemberInfo mInfo, bool lockSignature = false, bool lockKey = false)
        {
            DebugxMemberInfo mInfoOld = mInfo;

            //签名
            EditorGUI.BeginDisabledGroup(lockSignature);
            mInfo.signature = EditorGUILayout.TextField(new GUIContent("Signature", "成员签名"), mInfo.signature);
            EditorGUI.EndDisabledGroup();

            //是否打印签名
            mInfo.logSignature = EditorGUILayout.Toggle(new GUIContent("LogSignature", "是否打印签名"), mInfo.logSignature);

            //打印密钥
            EditorGUI.BeginDisabledGroup(lockKey);
            EditorGUI.BeginChangeCheck();
            mInfo.key = EditorGUILayout.IntField(new GUIContent("Key", "成员信息密钥，在效用Debugx.Logx()方法时使用"), mInfo.key);
            if (EditorGUI.EndChangeCheck())
            {
                RemoveKey(mInfoOld.key);

                //确认Key是否重复，重复时自动从最小可用Key开始使用
                if (!Debugx.KeyValid(mInfo.key) || CheckMemberKeyRepetition(mInfo.key))
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

            //头部信息
            mInfo.header = EditorGUILayout.TextField(new GUIContent("Header", "头部信息，在答应log时打印在头部"), mInfo.header);

            //打印颜色
            mInfo.color = EditorGUILayout.ColorField(new GUIContent("Color", "Log颜色"), mInfo.color);
        }

        //重置窗口数据
        private void ResetWindowData()
        {
            memberInfosFadeAreaPool.Clear();
            memberInfoKeyDic.Clear();

            if (DebugxSettingWindowConfig.Current.debugxMemberConfigSet == null)
                DebugxSettingWindowConfig.Current.debugxMemberConfigSet = DebugxEditorLibrary.DebugxMemberConfigDefault;
            configCopy = ScriptableObject.Instantiate(DebugxSettingWindowConfig.Current.debugxMemberConfigSet);

            if (DebugxSettingWindowConfig.Current.debugxMemberConfigInitOnEditorLoad == null)
                DebugxSettingWindowConfig.Current.debugxMemberConfigInitOnEditorLoad = DebugxEditorLibrary.DebugxMemberConfigDefault;

            //此方法内调用到了GUI.skin.button，GUI类必须在OnGUI才能调用，不能在OnEnable
            //确认FadeArea是否足够
            OnAddMemberInfo(Config.normalInfo);
            OnAddMemberInfo(Config.masterInfo);

            if (Config.debugxMemberInfos != null)
            {
                for (int i = 0; i < Config.debugxMemberInfos.Length; i++)
                {
                    OnAddMemberInfo(Config.debugxMemberInfos[i]);
                }
            }
        }

        //当添加一个成员信息时
        private void OnAddMemberInfo(DebugxMemberInfo info)
        {
            memberInfosFadeAreaPool.Add(new FadeArea(window, info.fadeAreaOpenCached, EditorStyle.Get.AreaStyle_1, EditorStyle.Get.LabelStyle_FadeAreaHeader));
            AddKey(info.key);
        }

        //当移除一个成员信息时
        private void OnRemoveMemberInfo(int index, DebugxMemberInfo info)
        {
            memberInfosFadeAreaPool.RemoveAt(index + 2);//前面两个时Normal和Master用的
            RemoveKey(info.key);
        }

        #region MemberInfoKey
        //Key=DebugxMemberInfo.key vaule=使用的成员信息，要求只能有一个
        private Dictionary<int, int> memberInfoKeyDic = new Dictionary<int, int>();

        private void AddKey(int key)
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

        private void RemoveKey(int key)
        {
            if (!memberInfoKeyDic.ContainsKey(key)) return;

            memberInfoKeyDic[key]--;
        }

        //确认成员信息的Key是否重复，重复时返回true
        private bool CheckMemberKeyRepetition(int key)
        {
            if (!memberInfoKeyDic.ContainsKey(key)) return false;

            return memberInfoKeyDic[key] >= 1;
        }

        //获取一个不重复的Key
        private bool GetMemberKey(out int key)
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

        private bool isDirty;

        //当自动保存开关变化时
        private void OnAutoSaveChange(bool enable)
        {
            if (enable)
            {
                SaveCheck(DebugxSettingWindowConfig.Current.debugxMemberConfigSet);
            }
            else
            {
                configCopy = ScriptableObject.Instantiate(DebugxSettingWindowConfig.Current.debugxMemberConfigSet);
            }
        }

        //确认是否需要存储
        private void SaveCheck(DebugxMemberConfig configAsset)
        {
            if (isDirty)
            {
                if (EditorUtility.DisplayDialog("保存修改内容", "有内容被修改未保存，是否保存？", "保存", "回退"))
                {
                    Save(configAsset);
                }
                else
                {
                    Revert(configAsset);
                }
            }
        }

        private void Save(DebugxMemberConfig configAsset)
        {
            if (!isDirty) return;
            isDirty = false;

            configAsset.normalInfo = configCopy.normalInfo;
            configAsset.masterInfo = configCopy.masterInfo;
            configAsset.debugxMemberInfos = configCopy.debugxMemberInfos;
            configCopy = ScriptableObject.Instantiate(configAsset);

            EditorUtility.SetDirty(configAsset);
            AssetDatabase.SaveAssetIfDirty(configAsset);

            DebugxManager.Instance.OnDebugxMemberConfigChange(configAsset);
        }

        private void Revert(DebugxMemberConfig configAsset)
        {
            if (!isDirty) return;
            isDirty = false;

            ResetWindowData();
        }

        #endregion
    }
}