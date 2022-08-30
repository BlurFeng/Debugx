#region AuthorInfo
////////////////////////////////////////////////////////////////////////////////////////////////////
// Author: WinhooFeng
// Time: 20220829
// Version: 1.1.0.0
// Description:
// The debug log is managed according to its members.use macro "DEBUG_X" open the functional.
// 此插件用于以成员的方式管理调试日志。使用宏"DEBUG_X"来开启功能。
////////////////////////////////////////////////////////////////////////////////////////////////////
// Update log:
// 1.0.0.0
// 创建插件。成员数据的配置功能，打印Log功能。
////////////////////
// 1.1.0.0
// 新增类LogOutput，用于到处Log数据到本地txt文件。
// 新增AdminInfo成员用于管理者打印，此成员不受开关影响。
// 默认成员配置文件中增加Winhoo成员。
// 修复在Window中移除一个成员时，移除的对应FadeArea不正确的问题。
// 创建新成员时，设置默认signature且设置logSignature=true。
// 通过 Tools/Debugx/CreateDebugxManager 创建Manager时，配置当前debugxMemberConfig为DebugxEditorLibrary.DebugxMemberConfigDefault。
// 增加logThisKeyMemberOnly参数，用于设置仅打印某个Key的成员Log。LogAdm不受影响，LogMasterOnly为设置logThisKeyMemberOnly为MasterKey的快速开关。
// DebugxManager的Inspector界面更新。
// 新增ActionHandler类，用于创建Action事件。
// 新增DebugxTools类，提供一些可用的工具方法
// 修复一些Bug。
////////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using UnityEngine;
using DebugxLog.Tools;
using System.Diagnostics;

namespace DebugxU3D
{
    /// <summary>
    /// Debugx调试功能管理器
    /// 一般添加到启动用GameObject上
    /// </summary>
    public class DebugxManager  : MonoBehaviour
    {
        //在U3D项目中添加宏“DEBUG_X”开启功能

        private static DebugxManager instance;
        private static GameObject GameObject;

        public static DebugxManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<DebugxManager>();

                    if (instance == null)
                    {
                        GameObject = new GameObject { name = typeof(DebugxManager).Name };
                        instance = GameObject.AddComponent<DebugxManager>();
                    }
                }
                return instance;
            }
        }

        //成员在DebugxManagerIns类中公开到Inspector面板

        [HideInInspector]
        public DebugxMemberConfig debugxMemberConfig;

        [HideInInspector]
        public bool enableLog = true;

        [HideInInspector]
        public bool enableLogNormal = true;

        [HideInInspector]
        public bool enableLogMaster = true;

        [HideInInspector]
        public bool enableLogMember = true;

        [HideInInspector]
        public bool logMasterOnly = false;
        private bool logMasterOnlyChangeToTrue;

        [HideInInspector]
        public int logThisKeyMemberOnly;

        [HideInInspector]
        public bool enbaleLogOutput = true;

        [HideInInspector]
        public bool enableLogStackTrace = false;

        [HideInInspector]
        public bool enableWarningStackTrace = false;

        [HideInInspector]
        public bool enableErrorStackTrace = true;

        [HideInInspector]
        public bool revordAllNonDebugxLogs = false;

#if UNITY_EDITOR
        [HideInInspector]
        public bool enableAwakeTestLog = true;

        [HideInInspector]
        public bool enableUpdateTestLog = false;
#endif

        public DebugxManager()
        {
#if UNITY_EDITOR
            Debugx.OnDebugxMemberConfigValidate.Bind(OnDebugxMemberConfigChange);
#endif
        }

        private void Awake()
        {
#if !DEBUG_X
            return;
#else
            DontDestroyOnLoad(this);

            RefreshDebugxDataCommon();

            //初始化Log输出
            if (enbaleLogOutput)
            {
#if UNITY_EDITOR
                //编辑器时重写Log输出路径到项目Logs文件夹下
                string directoryPathCover = Application.dataPath;
                directoryPathCover = directoryPathCover.Replace("Assets", "Logs");
                Debugx.LogAdm($"DirectoryPathCover: {directoryPathCover}");
                LogOutput.directoryPathCover = directoryPathCover;
#endif
                //默认输出路径一般为C:\Users\Winhoo\AppData\Local\Unity\Editor
                LogOutput.Init();
            }

            Debugx.LogAdm("DebugxManager: Awake");

            if (debugxMemberConfig != null)
                Debugx.Init(debugxMemberConfig);
            else
            {
                Debugx.CheckConfigValid();
                Debugx.LogMst($"DebugxManager: debugxMemberConfig is null.The default configuration will be used. 没有设置调试成员配置，将使用默认配置。");
            }



#if UNITY_EDITOR

            if (enableAwakeTestLog)
            {
                //测试打印，可注释。有对应key的成员信息时才会被打印。
                Debugx.LogNom("LogNom Print Test");
                Debugx.LogMst("LogMst Print Test");
                Debugx.Log(1, "Key 1 Print Test");
                Debugx.Log(2, "Key 2 Print Test");
                Debugx.Log(999, "Key 999 Print Test");

                UnityEngine.Debug.Log("UnityEngine.Debug.Log in Awake.test.");
            }
#endif
#endif
        }

        private void Update()
        {
#if !DEBUG_X
            return;
#else
#if UNITY_EDITOR
            if (enableUpdateTestLog)
            {
                //测试打印，可注释。有对应key的成员信息时才会被打印。
                Debugx.Log(1, "MemberKey 1 Update");
                Debugx.LogNom("LogNom Update");
                Debugx.LogMst("LogMst Update");
            }
#endif
#endif
        }

        private void OnDestroy()
        {
#if !DEBUG_X
            return;
#else
            LogOutput.OnDestroy();
#endif
        }

        /// <summary>
        /// 设置成员开关
        /// 运行时可通过此方法设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="enable"></param>
        [Conditional("DEBUG_X")]
        public void SetMemberEnable(int key, bool enable)
        {
            Debugx.SetMemberEnable(key, enable);
        }

        /// <summary>
        /// 更新Debugx类中，通用的数据。
        /// 在Editor时还会更新其他的编辑用数据
        /// </summary>
        private void RefreshDebugxDataCommon()
        {
            Debugx.enableLog = enableLog;
            Debugx.EnableLogNormal = enableLogNormal;
            Debugx.EnableLogMaster = enableLogMaster;
            Debugx.enableLogMember = enableLogMember;

            Debugx.LogMasterOnly = logMasterOnly;
            if (!logMasterOnlyChangeToTrue && logMasterOnly)
            {
                logMasterOnlyChangeToTrue = true;
                logThisKeyMemberOnly = Debugx.LogThisKeyMemberOnly;
            }
            else if (logMasterOnlyChangeToTrue && !logMasterOnly)
            {
                logMasterOnlyChangeToTrue = false;
                logThisKeyMemberOnly = Debugx.LogThisKeyMemberOnly;
            }

            //logMasterOnly==false时才可以设置logThisKeyMemberOnly
            if (!logMasterOnly)
            {
                if (!Debugx.ContainsMemberKey(logThisKeyMemberOnly)) logThisKeyMemberOnly = 0;
                Debugx.LogThisKeyMemberOnly = logThisKeyMemberOnly;
                bool logMasterOnlyTemp = logThisKeyMemberOnly == Debugx.masterInfoKey;
                if (logMasterOnlyTemp != logMasterOnly)
                {
                    logMasterOnlyChangeToTrue = true;
                    logMasterOnly = logMasterOnlyTemp;
                }
            }

            LogOutput.enableLogStackTrace = enableLogStackTrace;
            LogOutput.enableWarningStackTrace = enableWarningStackTrace;
            LogOutput.enableErrorStackTrace = enableErrorStackTrace;
            LogOutput.revordAllNonDebugxLogs = revordAllNonDebugxLogs;
        }

#if UNITY_EDITOR

        public void OnDestroyEditor()
        {
            Debugx.OnDebugxMemberConfigValidate.Unbind(OnDebugxMemberConfigChange);
        }

        /// <summary>
        /// 当通过菜单创建时
        /// </summary>
        /// <param name="config"></param>
        public void Create(DebugxMemberConfig config)
        {
            //用于创建方法调用
            debugxMemberConfig = config;
            OnValidate();
        }

        /// <summary>
        /// 当保存调试成员配置文件时
        /// </summary>
        public void OnDebugxMemberConfigChange(DebugxMemberConfig debugxMemberConfig)
        {
            OnValidate();
        }

        #region Inspector data
        //public的对象才能被serializedObject.FindProperty()获取到

        private DebugxMemberConfig debugxMemberConfigCached;

        [HideInInspector]
        public bool[] debugxMemberEnables;
        [HideInInspector]
        public string[] debugxMemberSignatures;
        [HideInInspector]
        public int[] debugxMemberKeys;

        private void OnValidate()
        {
#if !DEBUG_X
            return;
#endif
            //编辑器未启动游戏时，初始化数据，保证可用
            if (!Application.isPlaying)
            {
                if (debugxMemberConfig != null)
                    Debugx.Init(debugxMemberConfig);
                else
                {
                    Debugx.ClearDebugxMemberConfig();
                }
            }

            //更换了配置文件，或者当前配置文件成员数量变化。更新所有相关数据
            if (debugxMemberConfig == null)
            {
                debugxMemberConfigCached = null;
                debugxMemberEnables = null;
                debugxMemberSignatures = null;
                debugxMemberKeys = null;
                return;
            }
            else
            {
                if (debugxMemberConfig.debugxMemberInfos != null)
                {
                    bool resetInspectorDataCached = false;

                    //没更换配置文件
                    if (debugxMemberConfigCached == debugxMemberConfig)
                    {
                        //改变了Inspector上的数据，覆盖到配置文件数据
                        for (int i = 0; i < debugxMemberEnables.Length; i++)
                        {
                            if (i >= debugxMemberConfig.debugxMemberInfos.Length) break;

                            var info = debugxMemberConfig.debugxMemberInfos[i];
                            bool enable = debugxMemberEnables[i];
                            info.enableCached = enable;

                            if (Application.isPlaying)
                                Debugx.SetMemberEnable(info.key, enable);

                            debugxMemberConfig.debugxMemberInfos[i] = info;
                        }
                    }
                    //更换了配置文件
                    else
                    {
                        resetInspectorDataCached = true;
                        debugxMemberConfigCached = debugxMemberConfig;
                    }


                    //更新检视面板显示数据和配置数据一致
                    if (!resetInspectorDataCached)
                        resetInspectorDataCached = debugxMemberEnables.Length != debugxMemberConfig.debugxMemberInfos.Length;
                    if(resetInspectorDataCached)
                    {
                        int length = debugxMemberConfig.debugxMemberInfos.Length;
                        debugxMemberEnables = new bool[length];
                        debugxMemberSignatures = new string[length];
                        debugxMemberKeys = new int[length];
                    }
                    
                    for (int i = 0; i < debugxMemberConfig.debugxMemberInfos.Length; i++)
                    {
                        var info = debugxMemberConfig.debugxMemberInfos[i];
                        debugxMemberEnables[i] = info.enableCached;
                        debugxMemberSignatures[i] = info.signature;
                        debugxMemberKeys[i] = info.key;
                    }
                }
                else
                {
                    debugxMemberEnables = null;
                    debugxMemberSignatures = null;
                    debugxMemberKeys = null;
                }
            }

            RefreshDebugxDataCommon();
        }
#endregion
#endif
    }
}