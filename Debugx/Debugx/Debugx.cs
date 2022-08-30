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

#define DEBUG_X

using DebugxLog.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

/// <summary>
/// Debugx调试配置
/// 推荐使用编辑器工具编辑，也可以直接编辑.asset文件
/// </summary>
[CreateAssetMenu(fileName = "NewDebugxMemberConfig", menuName = "Debugx/DebugxMemberConfig", order = 1)]
public class DebugxMemberConfig : ScriptableObject
{
    /// <summary>
    /// 普通Log配置
    /// </summary>
    [Tooltip("普通Log配置")]
    public DebugxMemberInfo normalInfo;

    /// <summary>
    /// 高级Log配置
    /// </summary>
    [Tooltip("高级Log配置")]
    public DebugxMemberInfo masterInfo;

    /// <summary>
    /// 调试成员信息列表
    /// </summary>
    [Tooltip("调试成员信息列表")]
    public DebugxMemberInfo[] debugxMemberInfos;

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public DebugxMemberConfig()
    {
        //普通Log成员信息
        normalInfo.signature = Debugx.normalInfoSignature;
        normalInfo.logSignature = true;
        normalInfo.key = Debugx.normalInfoKey;
        normalInfo.color = Debugx.NormalInfoColor;
        normalInfo.enableCached = true;
        normalInfo.fadeAreaOpenCached = true;

        //高级Log成员信息
        masterInfo.signature = Debugx.masterInfoSignature;
        masterInfo.logSignature = true;
        masterInfo.key = Debugx.masterInfoKey;
        masterInfo.color = Debugx.MasterInfoColor;
        masterInfo.enableCached = true;
        masterInfo.fadeAreaOpenCached = true;
    }

    /// <summary>
    /// 默认Log成员信息限制
    /// </summary>
    /// <param name="config"></param>
    public static void DefaultDebugxMemberInfoLimit(DebugxMemberConfig config)
    {
        if (config == null) return;

        //普通log参数限制
        config.normalInfo.signature = Debugx.normalInfoSignature;
        config.normalInfo.key = Debugx.normalInfoKey;

        //高级log参数限制
        config.masterInfo.signature = Debugx.masterInfoSignature;
        config.masterInfo.key = Debugx.masterInfoKey;
    }

    private void OnValidate()
    {
        DefaultDebugxMemberInfoLimit(this);

        //防止直接修改Config中的Key为重复的值
        if (debugxMemberInfos != null && debugxMemberInfos.Length > 0)
        {
            List<int> keys = new List<int>();
            for (int i = 0; i < debugxMemberInfos.Length; i++)
            {
                var mInfo = debugxMemberInfos[i];
                if (Debugx.KeyValid(mInfo.key) && !keys.Contains(mInfo.key))
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

                debugxMemberInfos[i] = mInfo;
            }
        }

        Debugx.OnDebugxMemberConfigValidate.Invoke(this);
    }
}

/// <summary>
/// Debugx调试成员信息
/// 用于配置的数据
/// </summary>
[Serializable]
public struct DebugxMemberInfo
{
    /// <summary>
    /// 使用者签名
    /// </summary>
    [Tooltip("使用者签名")]
    public string signature;

    /// <summary>
    /// 使用者签名是否打印在Log中
    /// </summary>
    [Tooltip("使用者签名是否打印在Log中")]
    public bool logSignature;

    /// <summary>
    /// 此成员信息密钥,不要重复
    /// </summary>
    [Tooltip("此成员信息密钥,不要重复")]
    public int key;

    /// <summary>
    /// 头部信息，在打印Log会打印在头部
    /// </summary>
    [Tooltip("头部信息，在打印Log会打印在头部")]
    public string header;

    /// <summary>
    /// 打印Log颜色
    /// </summary>
    [Tooltip("打印Log颜色")]
    public Color color;

    /// <summary>
    /// 是否开启，缓存数据
    /// </summary>
    [HideInInspector]
    public bool enableCached;

    /// <summary>
    /// 编辑器窗口，隐藏区域是否开启
    /// </summary>
    [HideInInspector]
    public bool fadeAreaOpenCached;
}

/// <summary>
/// 运行时成员信息
/// </summary>
public struct DebugxMemberInfo_Internal
{
    /// <summary>
    /// 使用者签名
    /// </summary>
    public string signature;

    /// <summary>
    /// 使用者签名是否打印在Log中
    /// </summary>
    private readonly bool logSignature;

    /// <summary>
    /// 头部信息，在打印Log会打印在头部
    /// </summary>
    public string header;

    /// <summary>
    /// 打印Log颜色的RGB十六进制数
    /// </summary>
    public string color;

    /// <summary>
    /// 是否有签名
    /// </summary>
    public bool haveSignature;

    /// <summary>
    /// 是否有头部信息
    /// </summary>
    public bool haveHeader;

    /// <summary>
    /// 打印签名
    /// </summary>
    public bool LogSignature => logSignature && haveSignature;

    /// <summary>
    /// 通过DebugxMemberInfo配置数据构造
    /// </summary>
    /// <param name="mInfo"></param>
    public DebugxMemberInfo_Internal(DebugxMemberInfo mInfo)
    {
        signature = mInfo.signature;
        logSignature = mInfo.logSignature;
        header = mInfo.header;
        color = ColorUtility.ToHtmlStringRGB(mInfo.color);

        haveSignature = !string.IsNullOrEmpty(signature);
        haveHeader = !string.IsNullOrEmpty(header);
    }

    /// <summary>
    /// 快速构造简单成员信息
    /// </summary>
    /// <param name="signature">使用者签名</param>
    /// <param name="color">颜色</param>
    public DebugxMemberInfo_Internal(string signature, Color color)
    {
        this.signature = signature;
        this.logSignature = true;
        header = string.Empty;
        this.color = ColorUtility.ToHtmlStringRGB(color);
        haveSignature = true;
        haveHeader = false;
    }
}

/// <summary>
/// Debug工具类
/// </summary>
public class Debugx
{
    #region Action

    /// <summary>
    /// 当DebugxMemberConfig的数据变化时
    /// </summary>
    public static ActionHandler<DebugxMemberConfig> OnDebugxMemberConfigValidate = new ActionHandler<DebugxMemberConfig>();

    #endregion

    //Debugx，调试扩展工具类
    //在U3D项目中添加宏“DEBUG_X”开启功能方法

    /// <summary>
    /// Log总开关
    /// </summary>
    public static bool enableLog = true;

    /// <summary>
    /// 成员Log总开关
    /// </summary>
    public static bool enableLogMember = true;

    /// <summary>
    /// 普通Log开关
    /// </summary>
    public static bool EnableLogNormal
    {
        get => m_EnableLogNormal;
        set 
        {
            if (m_EnableLogNormal != value)
            {
                m_EnableLogNormal = value;
                SetMemberEnable(normalInfoKey, value);
            }
        }
    }
    private static bool m_EnableLogNormal = true;


    /// <summary>
    /// 最高优先级Log开关
    /// </summary>
    public static bool EnableLogMaster
    {
        get => m_EnableLogMaster;
        set 
        { 
            if (m_EnableLogMaster != value)
            {
                m_EnableLogMaster = value;
                SetMemberEnable(masterInfoKey, value);
            }
        }
    }
    private static bool m_EnableLogMaster = true;


    /// <summary>
    /// 仅显示最高优先级Log
    /// </summary>
    public static bool LogMasterOnly
    {
        get => m_logThisKeyMemberOnly == masterInfoKey;
        set
        {
            if(value)
            {
                m_logThisKeyMemberOnly = masterInfoKey;
            }
            else
            {
                m_logThisKeyMemberOnly = 0;
            }
        }
    }

    /// <summary>
    /// 仅打印此Key的Log
    /// 0为关闭，设置其他Key时，只有此Key对应的成员信息确实存在，才会只打印此Key的成员Log
    /// 必须关闭logMasterOnly后才能设置此值
    /// </summary>
    public static int LogThisKeyMemberOnly
    {
        get => m_logThisKeyMemberOnly;
        set
        {
            if (!LogMasterOnly && m_logThisKeyMemberOnly != value)
            {
                m_logThisKeyMemberOnly = value;
                if (m_logThisKeyMemberOnly == masterInfoKey)
                    LogMasterOnly = true;
            }
        }
    }
    private static int m_logThisKeyMemberOnly = 0;

    private readonly static Dictionary<int, DebugxMemberInfo_Internal> debugxMemberInfoDic = new Dictionary<int, DebugxMemberInfo_Internal>();
    private readonly static Dictionary<int, bool> debugxMemberEnableDic = new Dictionary<int, bool>();
    private static Func<bool> serverCheckDelegate;
    private readonly static StringBuilder logxSb = new StringBuilder();

    private static DebugxMemberConfig debugxMemberConfigSet;//当前调试成员配置

    /// <summary>
    /// 当前调试成员配置有效
    /// </summary>
    public static bool DebugxMemberConfigSetValid => debugxMemberConfigSet != null;

    private static bool initDefaultDebugxMemberInfo;
    private static DebugxMemberInfo_Internal normalMemberInfoDefault;
    private static DebugxMemberInfo_Internal masterMemberInfoDefault;
    private static DebugxMemberInfo_Internal adminInfo;//管理者成员

    /// <summary>
    /// Debugx打印的内容标识
    /// 不允许带有任何正则表达式的特殊含义符号
    /// 修改时需要同事修改LogOutput的正则表达式
    /// </summary>
    public const string debugxTag = "[Debugx]";

    /// <summary>
    /// 默认成员，普通成员签名
    /// </summary>
    public const string normalInfoSignature = "Normal";
    /// <summary>
    /// 默认成员，普通成员密钥
    /// </summary>
    public const int normalInfoKey = -1;
    /// <summary>
    /// 默认成员，普通成员颜色
    /// </summary>
    public static Color NormalInfoColor => Color.white;
    /// <summary>
    /// 默认成员，高级成员签名
    /// </summary>
    public const string masterInfoSignature = "Master";
    /// <summary>
    /// 默认成员，高级成员密钥
    /// </summary>
    public const int masterInfoKey = -2;
    /// <summary>
    /// 默认成员，高级成员颜色
    /// </summary>
    public static Color MasterInfoColor => new Color(1f, 0.627f, 0.627f, 1f);




    /// <summary>
    /// 设置DebugxInfo信息，在项目初始化时设置
    /// </summary>
    /// <param name="config"></param>
    [Conditional("DEBUG_X")]
    public static void Init(DebugxMemberConfig config)
    {
        //管理者成员
        adminInfo = new DebugxMemberInfo_Internal("Admin", new Color(0.7843f, 0.941f, 1f, 1f));

        ClearDebugxMemberConfig();
        if (config == null) return;
        debugxMemberConfigSet = config;

        //添加普通和高级成员信息
        debugxMemberInfoDic.Add(normalInfoKey, new DebugxMemberInfo_Internal(config.normalInfo));
        debugxMemberEnableDic.Add(normalInfoKey, config.normalInfo.enableCached);
        debugxMemberInfoDic.Add(masterInfoKey, new DebugxMemberInfo_Internal(config.masterInfo));
        debugxMemberEnableDic.Add(masterInfoKey, config.masterInfo.enableCached);

        //添加自动以成员信息
        if(config.debugxMemberInfos != null && config.debugxMemberInfos.Length > 0)
        {
            for (int i = 0; i < config.debugxMemberInfos.Length; i++)
            {
                var info = config.debugxMemberInfos[i];
                debugxMemberInfoDic.Add(info.key, new DebugxMemberInfo_Internal(info));
                debugxMemberEnableDic.Add(info.key, info.enableCached);
            }
        }
    }

    /// <summary>
    /// 清空调试成员配置
    /// </summary>
    [Conditional("DEBUG_X")]
    public static void ClearDebugxMemberConfig()
    {
        debugxMemberConfigSet = null;
        debugxMemberInfoDic.Clear();
        debugxMemberEnableDic.Clear();
    }

    /// <summary>
    /// 确认成员配置是否可用，否则使用默认配置
    /// 主要为了保证在没有InitDebugxMemberInfo()时，LogNom和LogMst的可用性
    /// </summary>
    public static bool CheckConfigValid()
    {
        if (!DebugxMemberConfigSetValid)
        {
            if (!initDefaultDebugxMemberInfo)
            {
                initDefaultDebugxMemberInfo = true;
                normalMemberInfoDefault = new DebugxMemberInfo_Internal(normalInfoSignature, NormalInfoColor);
                masterMemberInfoDefault = new DebugxMemberInfo_Internal(masterInfoSignature, MasterInfoColor);
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// 设置确认是否是服务器的方法
    /// 由项目调用设置，那么Logx系列方法的showNetTag参数设置为true后，才能打印网络标记
    /// </summary>
    /// <param name="serverCheckFunc"></param>
    [Conditional("DEBUG_X")]
    public static void SetServerCheck(Func<bool> serverCheckFunc)
    {
        if (serverCheckFunc == null) return;

        serverCheckDelegate = serverCheckFunc;
    }

    /// <summary>
    /// 设置成员开关
    /// </summary>
    /// <param name="key"></param>
    /// <param name="enable"></param>
    [Conditional("DEBUG_X")]
    public static void SetMemberEnable(int key, bool enable)
    {
        if (debugxMemberEnableDic == null) return;
        if (!debugxMemberEnableDic.ContainsKey(key))
        {
            LogAdmWarning($"Debugx.SetMemberEnable: cant find memberInfo by key:{key}. 无法找到Key为{key}的成员信息。");
            return;
        }
        debugxMemberEnableDic[key] = enable;
    }

    /// <summary>
    /// Key是否合法
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool KeyValid(int key)
    {
        return key != masterInfoKey && key != normalInfoKey && key != 0;
    }

    /// <summary>
    /// 确认成员Key是否包含
    /// 在程序运行时和非运行时都可用
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool ContainsMemberKey(int key)
    {
        if (debugxMemberInfoDic != null)
            return debugxMemberInfoDic.ContainsKey(key);
        else if (debugxMemberConfigSet != null && debugxMemberConfigSet.debugxMemberInfos != null)
        {
            for (int i = 0; i < debugxMemberConfigSet.debugxMemberInfos.Length; i++)
            {
                if (debugxMemberConfigSet.debugxMemberInfos[i].key == key)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 普通打印Log
    /// </summary>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogNom(object message, bool showTime = false, bool showNetTag = true)
    {
        LogNom(LogType.Log, message, showTime, showNetTag);
    }

    /// <summary>
    /// 普通打印LogWarning
    /// </summary>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogNomWarning(object message, bool showTime = false, bool showNetTag = true)
    {
        LogNom(LogType.Warning, message, showTime, showNetTag);
    }

    /// <summary>
    /// 普通打印LogError
    /// </summary>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogNomError(object message, bool showTime = false, bool showNetTag = true)
    {
        LogNom(LogType.Error, message, showTime, showNetTag);
    }

    private static void LogNom(LogType type, object message, bool showTime = false, bool showNetTag = true)
    {
        if (!enableLog || !EnableLogNormal) return;
        if (CheckConfigValid())
            LogCreator(type, normalInfoKey, message, showTime, showNetTag);
        else if(CheckLogThisKeyMemberOnly(normalInfoKey))
            LogCreator(type, normalMemberInfoDefault, message, showTime, showNetTag);
    }

    /// <summary>
    /// 高级打印Log
    /// </summary>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogMst(object message, bool showTime = false, bool showNetTag = true)
    {
        LogMst(LogType.Log, message, showTime, showNetTag);
    }

    /// <summary>
    /// 高级打印LogWarning
    /// </summary>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogMstWarning(object message, bool showTime = false, bool showNetTag = true)
    {
        LogMst(LogType.Warning, message, showTime, showNetTag);
    }

    /// <summary>
    /// 高级打印LogError
    /// </summary>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogMstError(object message, bool showTime = false, bool showNetTag = true)
    {
        LogMst(LogType.Error, message, showTime, showNetTag);
    }

    private static void LogMst(LogType type, object message, bool showTime = false, bool showNetTag = true)
    {
        if (!enableLog || !EnableLogMaster) return;
        if (CheckConfigValid())
            LogCreator(type, masterInfoKey, message, showTime, showNetTag);
        else if(CheckLogThisKeyMemberOnly(normalInfoKey))
            LogCreator(type, masterMemberInfoDefault, message, showTime, showNetTag);
    }

    /// <summary>
    /// 管理打印Log
    /// 插件开发者使用，所有人理论上都不可使用此方法。
    /// </summary>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogAdm(object message, bool showTime = false, bool showNetTag = true)
    {
        LogAdm(LogType.Log, message, showTime, showNetTag);
    }

    /// <summary>
    /// 管理打印LogWarning
    /// 插件开发者使用，所有人理论上都不可使用此方法。
    /// </summary>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogAdmWarning(object message, bool showTime = false, bool showNetTag = true)
    {
        LogAdm(LogType.Warning, message, showTime, showNetTag);
    }

    /// <summary>
    /// 管理打印LogError
    /// 插件开发者使用，所有人理论上都不可使用此方法。
    /// </summary>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogAdmError(object message, bool showTime = false, bool showNetTag = true)
    {
        LogAdm(LogType.Error, message, showTime, showNetTag);
    }

    private static void LogAdm(LogType type, object message, bool showTime = false, bool showNetTag = true)
    {
        LogCreator(type, adminInfo, message, showTime, showNetTag);
    }

    /// <summary>
    /// 成员打印Log
    /// </summary>
    /// <param name="key">DebugxMemberInfo中配置的key</param>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void Log(int key, object message, bool showTime = false, bool showNetTag = true)
    {
        Log(LogType.Log, key, message, showTime, showNetTag);
    }

    /// <summary>
    /// 成员打印LogWarning
    /// </summary>
    /// <param name="key">DebugxMemberInfo中配置的key</param>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogWarning(int key, object message, bool showTime = false, bool showNetTag = true)
    {
        Log(LogType.Warning, key, message, showTime, showNetTag);
    }

    /// <summary>
    /// 成员打印LogError
    /// </summary>
    /// <param name="key">DebugxMemberInfo中配置的key</param>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogError(int key, object message, bool showTime = false, bool showNetTag = true)
    {
        Log(LogType.Error, key, message, showTime, showNetTag);
    }

    private static void Log(LogType type, int key, object message, bool showTime = false, bool showNetTag = true)
    {
        if (!enableLog || !enableLogMember) return;
        LogCreator(type, key, message, showTime, showNetTag);
    }

    /// <summary>
    /// 确认是否开启了仅显示某个Key的成员
    /// true=允许通过 false=返回，不允许打印
    /// </summary>
    private static bool CheckLogThisKeyMemberOnly(int key)
    {
        return LogThisKeyMemberOnly == 0 || !debugxMemberInfoDic.ContainsKey(LogThisKeyMemberOnly) || LogThisKeyMemberOnly == key;
    }

    /// <summary>
    /// Log扩展打印方法
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="key">DebugxMemberInfo中配置的key</param>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    private static void LogCreator(LogType type, int key, object message, bool showTime = false, bool showNetTag = true)
    {
        if(debugxMemberInfoDic == null)
        {
            LogAdmWarning($"Debugx.LogCreator: The initial configuration is not performed. 未初始化配置。");
            return;
        }
        if (!debugxMemberInfoDic.ContainsKey(key))
        {
            LogAdmWarning($"Debugx.LogCreator: cant find memberInfo by key:{key}. 无法找到Key为{key}的成员信息。");
            return;
        }

        //设置了仅打印某个Key成员Log
        if (!CheckLogThisKeyMemberOnly(key)) return;

        if (debugxMemberEnableDic == null || !debugxMemberEnableDic.ContainsKey(key) || !debugxMemberEnableDic[key]) return;//此成员未打开

        DebugxMemberInfo_Internal info = debugxMemberInfoDic[key];
        LogCreator(type, info, message, showTime, showNetTag);
    }

    private static void LogCreator(LogType type, DebugxMemberInfo_Internal info, object message, bool showTime = false, bool showNetTag = true)
    {
        logxSb.Append(debugxTag);

        if (showNetTag && serverCheckDelegate != null)
            logxSb.Append(serverCheckDelegate.Invoke() ? "Server: " : "Client: ");

        if (showTime)
        {
            logxSb.Append($" [{DateTime.Now.ToString("HH:mm:ss")}] ");
        }

        if (info.LogSignature)
            logxSb.Append($"[Sig: {info.signature}]");

        logxSb.Append(info.haveHeader ? $" <color=#{info.color}>{info.header} : {message}</color>" : $" <color=#{info.color}>{message}</color>");

        UnityEngine.Debug.unityLogger.Log(type, logxSb.ToString());
        logxSb.Length = 0;
    }
}