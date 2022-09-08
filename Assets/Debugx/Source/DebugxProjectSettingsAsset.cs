using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using DebugxLog.Tools;

namespace DebugxLog
{
    /// <summary>
    /// Debugx调试成员信息
    /// 用于配置的数据
    /// </summary>
    [Serializable]
    public struct DebugxMemberInfoAsset
    {
        [Tooltip("是否开启")]
        public bool enableDefault;

        [Tooltip("使用者签名")]
        public string signature;

        [Tooltip("使用者签名是否打印在Log中")]
        public bool logSignature;

        [Tooltip("此成员信息密钥,不要重复")]
        public int key;

        [Tooltip("头部信息，在打印Log会打印在头部")]
        public string header;

        [Tooltip("打印Log颜色")]
        public Color color;

        public DebugxMemberInfo CreateDebugxMemberInfo()
        {
            DebugxMemberInfo info = new()
            {
                key = key,
                enableDefault = enableDefault,
                signature = signature,
                logSignature = logSignature,
                header = header,
                color = ColorUtility.ToHtmlStringRGB(color),

                haveSignature = !string.IsNullOrEmpty(signature),
                haveHeader = !string.IsNullOrEmpty(header),
            };

#if UNITY_EDITOR
            if (DebugxStaticData.MemberEnableDefaultDic.ContainsKey(key))
            {
                info.enableDefault = DebugxStaticData.MemberEnableDefaultDic[key];
            }
#endif

            return info;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器窗口，隐藏区域是否开启
        /// </summary>
        [HideInInspector]
        public bool fadeAreaOpenCached;
#endif
    }

    /// <summary>
    /// Debugx调试配置
    /// 推荐使用编辑器工具编辑，也可以直接编辑.asset文件
    /// </summary>
    public class DebugxProjectSettingsAsset : ScriptableObject, IDebugxProjectSettingsAsset
    {
        private static DebugxProjectSettingsAsset instance;
        public static DebugxProjectSettingsAsset Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<DebugxProjectSettingsAsset>(DebugxProjectSettings.fileName);
#if UNITY_EDITOR
                    if (instance == null)
                    {
                        instance = ScriptableObject.CreateInstance(typeof(DebugxProjectSettingsAsset)) as DebugxProjectSettingsAsset;
                        instance.customDebugxMemberAssets = new DebugxMemberInfoAsset[1]
                        {
                            new DebugxMemberInfoAsset(){ signature = "WinhooFeng", logSignature = true, key = 1, color = new Color(0.7843f, 0.941f, 1f, 1f), enableDefault = true }
                        };

                        UnityEditor.AssetDatabase.CreateAsset(instance, $"{DebugxStaticData.resourcesPath}/{DebugxProjectSettings.fileName}.asset");

                        instance.ApplyTo(DebugxProjectSettings.Instance);
                    }
#endif
                }

                return instance;
            }
        }

        public static ActionHandler OnApplyTo = new ActionHandler();

        [Tooltip("Log总开关")]
        public bool enableLogDefault = true;

        [Tooltip("成员Log总开关")]
        public bool enableLogMemberDefault = true;

        [Tooltip("仅打印此Key的成员Log，0为关闭")]
        public int logThisKeyMemberOnlyDefault = 0;

        [Tooltip("普通成员配置")]
        public DebugxMemberInfoAsset normalMember;

        [Tooltip("高级成员配置")]
        public DebugxMemberInfoAsset masterMember;

        [Tooltip("自定义调试成员信息列表")]
        public DebugxMemberInfoAsset[] customDebugxMemberAssets;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public DebugxProjectSettingsAsset()
        {
            //普通Log成员信息
            normalMember.signature = DebugxProjectSettings.normalInfoSignature;
            normalMember.logSignature = true;
            normalMember.key = DebugxProjectSettings.normalInfoKey;
            normalMember.color = DebugxProjectSettings.NormalInfoColor;
            normalMember.enableDefault = true;
            normalMember.fadeAreaOpenCached = true;

            //高级Log成员信息
            masterMember.signature = DebugxProjectSettings.masterInfoSignature;
            masterMember.logSignature = true;
            masterMember.key = DebugxProjectSettings.masterInfoKey;
            masterMember.color = DebugxProjectSettings.MasterInfoColor;
            masterMember.enableDefault = true;
            masterMember.fadeAreaOpenCached = true;
        }

        [Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            //普通log参数限制
            normalMember.signature = DebugxProjectSettings.normalInfoSignature;
            normalMember.key = DebugxProjectSettings.normalInfoKey;

            //高级log参数限制
            masterMember.signature = DebugxProjectSettings.masterInfoSignature;
            masterMember.key = DebugxProjectSettings.masterInfoKey;

            //防止直接修改.asset文件中Key为重复的值
            if (customDebugxMemberAssets != null && customDebugxMemberAssets.Length > 0)
            {
                List<int> keys = new List<int>();
                for (int i = 0; i < customDebugxMemberAssets.Length; i++)
                {
                    var mInfo = customDebugxMemberAssets[i];
                    if (DebugxProjectSettings.KeyValid(mInfo.key) && !keys.Contains(mInfo.key))
                    {
                        keys.Add(mInfo.key);
                    }
                    else
                    {
                        //Key重复了
                        int newKey = 1;
                        while (keys.Contains(newKey))
                        {
                            if (newKey >= int.MaxValue)
                                break;
                            newKey++;
                        }
                        mInfo.key = newKey;
                        keys.Add(mInfo.key);
                    }

                    customDebugxMemberAssets[i] = mInfo;
                }
            }

            ApplyTo(DebugxProjectSettings.Instance);
        }

        #region Log Output

        /// <summary>
        /// 输出Log到本地文件
        /// </summary>
        [Tooltip("普通Log配置")]
        public bool logOutput = true;

        /// <summary>
        /// 输出Log类型的堆栈跟踪
        /// </summary>
        [Tooltip("输出Log类型的堆栈跟踪")]
        public bool enableLogStackTrace = false;

        /// <summary>
        /// 输出Warning类型的堆栈跟踪
        /// </summary>
        [Tooltip("输出Warning类型的堆栈跟踪")]
        public bool enableWarningStackTrace = false;

        /// <summary>
        /// 输出Error类型的堆栈跟踪
        /// </summary>
        [Tooltip("输出Error类型的堆栈跟踪")]
        public bool enableErrorStackTrace = true;

        /// <summary>
        /// 记录所有非Debugx打印的Log
        /// </summary>
        [Tooltip("记录所有非Debugx打印的Log")]
        public bool recordAllNonDebugxLogs = false;

        /// <summary>
        /// 绘制Log到屏幕
        /// </summary>
        [Tooltip("绘制Log到屏幕")]
        public bool drawLogToScreen = false;

        /// <summary>
        /// 限制绘制Log数量
        /// </summary>
        [Tooltip("限制绘制Log数量")]
        public bool restrictDrawLogCount = false;

        /// <summary>
        /// 绘制Log最大数量
        /// </summary>
        [Tooltip("绘制Log最大数量")]
        public int maxDrawLogs = 100;

        #endregion

        /// <summary>
        /// 复制数据到自身
        /// </summary>
        /// <param name="settingsAsset"></param>
        public void Copy(DebugxProjectSettingsAsset settingsAsset)
        {
            enableLogDefault = settingsAsset.enableLogDefault;
            enableLogMemberDefault = settingsAsset.enableLogMemberDefault;
            logThisKeyMemberOnlyDefault = settingsAsset.logThisKeyMemberOnlyDefault;

            normalMember = settingsAsset.normalMember;
            masterMember = settingsAsset.masterMember;
            customDebugxMemberAssets = settingsAsset.customDebugxMemberAssets;

            logOutput = settingsAsset.logOutput;
            enableLogStackTrace = settingsAsset.enableLogStackTrace;
            enableWarningStackTrace = settingsAsset.enableWarningStackTrace;
            enableErrorStackTrace = settingsAsset.enableErrorStackTrace;
            recordAllNonDebugxLogs = settingsAsset.recordAllNonDebugxLogs;
            drawLogToScreen = settingsAsset.drawLogToScreen;
            restrictDrawLogCount = settingsAsset.restrictDrawLogCount;
            maxDrawLogs = settingsAsset.maxDrawLogs;
        }

        //在DebugxProjectSettings初始化时通过接口调用，以及在DebugxProjectSettingsAsset资源变化时调用
        public void ApplyTo(DebugxProjectSettings settings)
        {
#if UNITY_EDITOR
            settings.enableLogDefault = DebugxStaticData.EnableLogDefault;
            settings.enableLogMemberDefault = DebugxStaticData.EnableLogMemberDefault;
            settings.logThisKeyMemberOnlyDefault = DebugxStaticData.LogThisKeyMemberOnlyDefault;
#else
            settings.enableLogDefault = enableLogDefault;
            settings.enableLogMemberDefault = enableLogMemberDefault;
            settings.logThisKeyMemberOnlyDefault = logThisKeyMemberOnlyDefault;
#endif
            int customLength = customDebugxMemberAssets != null ? customDebugxMemberAssets.Length : 0;
            settings.members = new DebugxMemberInfo[customLength + 2];
            //添加普通和高级成员信息
            settings.members[0] = normalMember.CreateDebugxMemberInfo();
            settings.members[1] = masterMember.CreateDebugxMemberInfo();

            //添加自定义成员信息
            if (customDebugxMemberAssets != null && customDebugxMemberAssets.Length > 0)
            {
                for (int i = 0; i < customDebugxMemberAssets.Length; i++)
                {
                    var info = customDebugxMemberAssets[i];
                    settings.members[i + 2] = info.CreateDebugxMemberInfo();
                }
            }

#if UNITY_EDITOR
            //Log输出设置
            settings.logOutput = DebugxStaticData.LogOutput;
            settings.enableLogStackTrace = DebugxStaticData.EnableLogStackTrace;
            settings.enableWarningStackTrace = DebugxStaticData.EnableWarningStackTrace;
            settings.enableErrorStackTrace = DebugxStaticData.EnableErrorStackTrace;
            settings.recordAllNonDebugxLogs = DebugxStaticData.RecordAllNonDebugxLogs;
            settings.drawLogToScreen = DebugxStaticData.DrawLogToScreen;
            settings.restrictDrawLogCount = DebugxStaticData.RestrictDrawLogCount;
            settings.maxDrawLogs = DebugxStaticData.MaxDrawLogs;
#else
            //Log输出设置
            settings.logOutput = logOutput;
            settings.enableLogStackTrace = enableLogStackTrace;
            settings.enableWarningStackTrace = enableWarningStackTrace;
            settings.enableErrorStackTrace = enableErrorStackTrace;
            settings.recordAllNonDebugxLogs = recordAllNonDebugxLogs;
            settings.drawLogToScreen = drawLogToScreen;
            settings.restrictDrawLogCount = restrictDrawLogCount;
            settings.maxDrawLogs = maxDrawLogs;
#endif

            Debugx.ResetToDefault();

            OnApplyTo.Invoke();
        }
    }
}