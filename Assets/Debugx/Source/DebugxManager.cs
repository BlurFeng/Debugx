#region AuthorInfo
////////////////////////////////////////////////////////////////////////////////////////////////////
// Author: WinhooFeng
// Time: 20220903
// Version: 1.1.0.0
// Description:
// The debug log is managed according to its members.use macro "DEBUG_X" open the functional.
// 此插件用于以成员的方式管理调试日志。使用宏"DEBUG_X"来开启功能。
// 版本号使用规范 大版本前后不兼容.新功能.小功能或功能更新.bug修复
////////////////////////////////////////////////////////////////////////////////////////////////////
// Update log:
// 1.0.0.0 20220829
// 1.创建插件。成员数据的配置功能，打印Log功能。
////////////////////
// 1.1.0.0 20220830
// 1.新增类LogOutput，用于到处Log数据到本地txt文件。
// 2.新增AdminInfo成员用于管理者打印，此成员不受开关影响。
// 3.默认成员配置文件中增加Winhoo成员。
// 4.修复在Window中移除一个成员时，移除的对应FadeArea不正确的问题。
// 5.创建新成员时，设置默认signature且设置logSignature=true。
// 6.通过 Tools/Debugx/CreateDebugxManager 创建Manager时，配置当前debugxMemberConfig为DebugxEditorLibrary.DebugxMemberConfigDefault。
// 7.增加logThisKeyMemberOnly参数，用于设置仅打印某个Key的成员Log。LogAdm不受影响，LogMasterOnly为设置logThisKeyMemberOnly为MasterKey的快速开关。
// 8.DebugxManager的Inspector界面更新。
// 9.新增ActionHandler类，用于创建Action事件。
// 10.新增DebugxTools类，提供一些可用的工具方法
// 11.修复一些Bug。
////////////////////
// 1.1.1.0 20220903
// 1.新增功能，在Editor编辑器启动时，初始化调试成员配置到Debux，保证在编辑器非游玩时也能使用Debux.Log()
// 2.DebugxMemberWindow改名为DebugxSettingWindow，调整窗口内容，优化代码。
// 3.Debugx.dll中修改Dictionary为List。为了DOTS等某些情况下，不支持Dictionary的情况。
// 4.LogOutput类，新增绘制Log到屏幕功能，在DebugxManager上设置是否绘制。
////////////////////
// 2.0.0.0 20220908
// 1.DebugxMemberConfig类改名为DebugxProjectSettings，增加更多成员字段；创建对应配置用类DebugxProjectSettingsAsset，用于生成.asset文件在编辑器中配置。
// 2.设置界面从EditorWindow改为SettingsProvider，在Editor->ProjectSetting->Debugx中设置。设置内容调整。
// 3.新增界面 PreferencesDebugx 在 Editor->Preferences->Debugx 目录下。可以让不同用户配置本地化的内容，比如一些成员在自己设备的项目中仅想看到自己打印的Log。
// 4.DebugxManager成员打印开关相关功能转移到新窗口DebugxConsole；DebugxManager在游戏运行时自动创建，不需要再默认创建并保存到场景中。
////////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using UnityEngine;
using DebugxLog.Tools;
using System.Diagnostics;

namespace DebugxLog
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

        public static bool Have => instance != null;

        //成员在DebugxManagerIns类中公开到Inspector面板

        [HideInInspector]
        public DebugxProjectSettings Settings => DebugxProjectSettings.Instance;

        [HideInInspector]
        public bool enableLog = true;

        [HideInInspector]
        public bool enableLogMember = true;

        [HideInInspector]
        public bool logMasterOnly = false;
        private bool logMasterOnlyChangeToTrue;

        [HideInInspector]
        public int logThisKeyMemberOnly;

        public DebugxManager()
        {
#if UNITY_EDITOR
            //Debugx.OnDebugxConfigValidate.Bind(OnDebugxConfigChange);
#endif
        }

#if DEBUG_X
        [RuntimeInitializeOnLoadMethod]
        private static void CheckInstance()
        {
            if (instance == null && Application.isPlaying)
            {
                DebugxManager.Instance.Create();
            }
        }
#endif

        public void Create() { }

        private void Awake()
        {
            Debugx.LogAdm("DebugxManager --- Awake");

            DontDestroyOnLoad(this);

            Debugx.OnAwake();

#if UNITY_EDITOR
            //编辑器时重写Log输出路径到项目Logs文件夹下
            string directoryPathCover = Application.dataPath;
            directoryPathCover = directoryPathCover.Replace("Assets", "Logs");
            LogOutput.directoryPathCover = directoryPathCover;
#endif
            LogOutput.RecordStart();
            if (DebugxProjectSettings.Instance.logOutput)
                Debugx.LogAdm($"DebugxManager --- Log output to {LogOutput.DirectoryPath}");
#if UNITY_EDITOR

            if (DebugxStaticData.enableAwakeTestLog)
            {
                //测试打印，可注释。有对应key的成员信息时才会被打印。
                Debugx.LogNom("LogNom Print Test");
                Debugx.LogMst("LogMst Print Test");
                Debugx.Log(1, "Key 1 Print Test");
                Debugx.Log(2, "Key 2 Print Test");
                Debugx.Log(999, "Key 999 Print Test");
            }
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (DebugxStaticData.enableUpdateTestLog)
            {
                //测试打印，可注释。有对应key的成员信息时才会被打印。
                Debugx.Log(1, "MemberKey 1 Update");
                Debugx.LogNom("LogNom Update");
                Debugx.LogMst("LogMst Update");
            }
#endif
        }

        private void OnDestroy()
        {
            Debugx.OnDestroy();
            LogOutput.RecordOver();
        }

        private void OnGUI()
        {
            LogOutput.DrawGUI();

            if(Settings == null)
            {
                GUI.Label(new Rect(10,10,200,200),"debugxConfig is null");
            }
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
    }
}