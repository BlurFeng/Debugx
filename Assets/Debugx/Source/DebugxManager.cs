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

        //成员在DebugxManagerIns类中公开到Inspector面板

        [HideInInspector]
        public DebugxProjectSettings Settings => DebugxProjectSettings.Instance;

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