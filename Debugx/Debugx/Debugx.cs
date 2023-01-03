#region AuthorInfo
////////////////////////////////////////////////////////////////////////////////////////////////////
// Author: WinhooFeng
// Time: 20220920
// Version: 2.0.1.0
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
// 10.新增DebugxTools类，提供一些可用的工具方法。
// 11.修复一些Bug。
////////////////////
// 1.1.1.0 20220903
// 1.新增功能，在Editor编辑器启动时，初始化调试成员配置到Debux，保证在编辑器非游玩时也能使用Debux.Log()。
// 2.DebugxMemberWindow改名为DebugxSettingWindow，调整窗口内容，优化代码。
// 3.Debugx.dll中修改Dictionary为List。为了DOTS等某些情况下，不支持Dictionary的情况。
// 4.LogOutput类，新增绘制Log到屏幕功能，在DebugxManager上设置是否绘制。
////////////////////
// 2.0.0.0 20220911
// 1.DebugxMemberConfig类改名为DebugxProjectSettings，增加更多成员字段；创建对应配置用类DebugxProjectSettingsAsset，用于生成.asset文件在编辑器中配置。
// 2.设置界面从EditorWindow改为SettingsProvider，在Editor->ProjectSetting->Debugx中设置。设置内容调整。
// 3.新增界面 PreferencesDebugx 在 Editor->Preferences->Debugx 目录下。可以让不同用户配置本地化的内容，比如一些成员在自己设备的项目中仅想看到自己打印的Log。
// 4.DebugxManager成员打印开关相关功能转移到新窗口DebugxConsole；DebugxManager在游戏运行时自动创建，不需要再默认创建并保存到场景中。
// 5.新增ColorDispenser类，用于在Member创建时分配一个颜色。
// 6.增加配置辅助功能，重置配置，快速设置全部成员开关，颜色重置和自动分配等。
// 7.编辑器配置界面，适应Dark和Light编辑器皮肤。
// 8.文件夹整理，类重命名，代码整理优化。
////////////////////
// 2.0.1.0 20220920
// 1.GUI界面更新，颜色调整。
// 2.移除DebugxEditorConfig类。
// FixBug
// 1.修复在安卓平台时Application.consoleLogPath获取为空导致无法输出Log文件的问题。
// 2.用户手册名称更新，去除中文。防止一些因中文路径导致打包失败。
// 3.替换掉 new() 的语法，防止低版本的C#报错。
// 4.修复DebugxProjectSettings自动创建流程相关的Bug。
// 5.修复ProjectSettings界面中数组越界Bug。
////////////////////
// 2.0.2.0 20221031
// 1.未注册成员进行打印功能，新增allowUnregisteredMember字段，用于配置是否允许没有注册者打印内容。
// FixBug
// 1.修复某些情况下DebugxProjectSettings初始化时无法通过Resources.Load加载，导致的各类问题。
// 2.DebugxProjectSettingsAsset配置资源加载和创建流程更新，尝试修复配置被重置为空的问题。
////////////////////
// 2.0.2.1 20221102 (更新中，未发布)
// 1.菜单栏新增CreateDebugxProjectSettingsAsset方法用于创建配置资源文件。
// 2.DebugxProjectSettingsProvider项目设置界面优化。
// 3.Log方法扩展，可以输入Signature签名来代替Key作为成员参数。
// 4.DebugxBurst更新，移除会导致DOTS项目编译报错的方法。
// FixBug
// 1.在没有DebugxProjectSettings.asset文件时，如果编辑器启动或代码重编译，会导致Resources.Load方法报错堆栈溢出的问题修复。
//   复现流程为在Editor启动方法中或代码编译时，新创建了DebugxProjectSettings.asset资源并保存后，直接调用Resources.Load方法加载此资源。
// 2.修复每次代码重编译时DebugxProjectSettingsAsset资源都被重新创建的默认资源覆盖的bug。
////////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using DebugxLog;
using System.Collections.Generic;

namespace DebugxLog
{
    /// <summary>
    /// Debugx设置资源接口
    /// </summary>
    public interface IDebugxProjectSettingsAsset
    {
        /// <summary>
        /// 复制数据
        /// </summary>
        /// <param name="settings"></param>
        void ApplyTo(DebugxProjectSettings settings);
    }

    /// <summary>
    /// 运行时成员信息
    /// </summary>
    public class DebugxMemberInfo
    {
        /// <summary>
        /// 是否开启
        /// </summary>
        public bool enableDefault;

        /// <summary>
        /// 调试成员密钥
        /// </summary>
        public int key;

        /// <summary>
        /// 使用者签名
        /// </summary>
        public string signature;

        /// <summary>
        /// 使用者签名是否打印在Log中
        /// </summary>
        public bool logSignature;

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
        /// 默认构造函数
        /// </summary>
        public DebugxMemberInfo() { }

        /// <summary>
        /// 快速构造简单成员信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="signature">使用者签名</param>
        public DebugxMemberInfo(int key, string signature)
        {
            this.key = key;
            enableDefault = true;
            this.signature = signature;
            this.logSignature = true;
            header = string.Empty;
            this.color = String.Empty;
            haveSignature = true;
            haveHeader = false;
        }
    }

    /// <summary>
    /// Debugx设置
    /// </summary>
    public class DebugxProjectSettings
    {
        /// <summary>
        /// Debugx项目设置Asset文件名称
        /// </summary>
        public const string fileName = "DebugxProjectSettings";
        private static DebugxProjectSettings instance;

        /// <summary>
        /// 单例
        /// </summary>
        public static DebugxProjectSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    LoadResources();
                }

                return instance;
            }
        }

        /// <summary>
        /// 管理者成员
        /// </summary>
        public DebugxMemberInfo AdminInfo
        {
            get
            {
                if (m_AdminInfo == null)
                {
                    m_AdminInfo = new DebugxMemberInfo(0, "Admin");
                }
                return m_AdminInfo;
            }
        }
        private DebugxMemberInfo m_AdminInfo;

        /// <summary>
        /// 成员信息列表
        /// </summary>
        public DebugxMemberInfo[] members;

        #region Static Data
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
        #endregion

        /// <summary>
        /// Log总开关
        /// </summary>
        public bool enableLogDefault = true;

        /// <summary>
        /// 成员Log总开关
        /// </summary>
        public bool enableLogMemberDefault = true;

        /// <summary>
        /// 允许没有注册成员进行打印
        /// </summary>
        public bool allowUnregisteredMember = true;

        /// <summary>
        /// 仅打印此Key的成员Log，0为关闭
        /// </summary>
        public int logThisKeyMemberOnlyDefault = 0;

        /// <summary>
        /// Key是否合法
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool KeyValid(int key)
        {
            return key > 0;
        }

        /// <summary>
        /// 加载配置资源
        /// </summary>
        private static void LoadResources()
        {
            //Resources.Load在某些生命周期时不可用，比如[InitializeOnLoadMethod]特性方法在启动Editor时调用会导致Resources.Load报错堆栈溢出
            try
            {
                IDebugxProjectSettingsAsset asset = (IDebugxProjectSettingsAsset)Resources.Load<ScriptableObject>(fileName);
                if (asset != null) ApplyBy(asset);
                else Debugx.LogAdmWarning("Failed to load the DebugxProjectSettings configuration resource file. 加载DebugxProjectSettings配置资源文件失败。");
            }
            catch
            {
                Debugx.LogAdmWarning("Failed to load the DebugxProjectSettings configuration resource file. 加载DebugxProjectSettings配置资源文件失败。");
            }
        }

        /// <summary>
        /// 从Asset读取数据保存到DebugxProjectSettings
        /// </summary>
        /// <param name="asset"></param>
        public static void ApplyBy(IDebugxProjectSettingsAsset asset)
        {
            if (asset == null) return;

            instance = new DebugxProjectSettings();
            asset.ApplyTo(instance);
        }

        #region Log Output

        /// <summary>
        /// 输出Log到本地文件
        /// </summary>
        public bool logOutput = true;

        /// <summary>
        /// 输出Log类型的堆栈跟踪
        /// </summary>
        public bool enableLogStackTrace = false;

        /// <summary>
        /// 输出Warning类型的堆栈跟踪
        /// </summary>
        public bool enableWarningStackTrace = false;

        /// <summary>
        /// 输出Error类型的堆栈跟踪
        /// </summary>
        public bool enableErrorStackTrace = true;

        /// <summary>
        /// 记录所有非Debugx打印的Log
        /// </summary>
        public bool recordAllNonDebugxLogs = false;

        /// <summary>
        /// 绘制Log到屏幕
        /// </summary>
        public bool drawLogToScreen = false;

        /// <summary>
        /// 限制绘制Log数量
        /// </summary>
        public bool restrictDrawLogCount = false;

        /// <summary>
        /// 绘制Log最大数量
        /// </summary>
        public int maxDrawLogs = 100;

        #endregion
    }
}

/// <summary>
/// Debugx核心工具类
/// </summary>
public class Debugx
{
    //Debugx，调试扩展工具类
    //在U3D项目中添加宏“DEBUG_X”开启功能方法

    private static DebugxProjectSettings Settings => DebugxProjectSettings.Instance;
    private static DebugxMemberInfo AdminInfo => Settings != null ? Settings.AdminInfo : adminInfoDefault;
    private static DebugxMemberInfo adminInfoDefault = new DebugxMemberInfo(0, "Admin");

    private static Func<bool> serverCheckDelegate;
    private readonly static StringBuilder logxSb = new StringBuilder();

    private readonly static Dictionary<int, bool> memberEnables = new Dictionary<int, bool>();

    /// <summary>
    /// Log总开关
    /// </summary>
    public static bool enableLog = true;

    /// <summary>
    /// 成员Log总开关
    /// </summary>
    public static bool enableLogMember = true;

    /// <summary>
    /// 允许没有注册成员进行打印
    /// </summary>
    public static bool allowUnregisteredMember = true;

    /// <summary>
    /// 仅打印此Key的Log
    /// 0为关闭，设置其他Key时，只有此Key对应的成员信息确实存在，才会只打印此Key的成员Log
    /// 必须关闭logMasterOnly后才能设置此值
    /// </summary>
    public static int logThisKeyMemberOnly = 0;

    /// <summary>
    /// OnAwake
    /// </summary>
    public static void OnAwake()
    {
        ResetToDefault();

        if (Settings != null && Settings.members != null)
        {
            for (int i = 0; i < Settings.members.Length; i++)
            {
                var info = Settings.members[i];
                memberEnables.Add(info.key, info.enableDefault);
            }
        }
    }

    /// <summary>
    /// OnDestroy
    /// </summary>
    public static void OnDestroy()
    {
        ResetToDefault();
    }

    /// <summary>
    /// 重置数据到Settings中Default
    /// </summary>
    public static void ResetToDefault()
    {
        if (Settings != null)
        {
            enableLog = Settings.enableLogDefault;
            enableLogMember = Settings.enableLogMemberDefault;
            allowUnregisteredMember = Settings.allowUnregisteredMember;
            logThisKeyMemberOnly = Settings.logThisKeyMemberOnlyDefault;
        }

        memberEnables.Clear();
    }

    /// <summary>
    /// 在游戏运行时设置成员开关
    /// </summary>
    /// <param name="key"></param>
    /// <param name="enable"></param>
    [Conditional("DEBUG_X")]
    public static void SetMemberEnable(int key, bool enable)
    {
        if (memberEnables == null || memberEnables.Count == 0) return;

        if (!memberEnables.ContainsKey(key))
        {
            Debugx.LogAdmWarning($"Debugx.SetMemberEnable: cant find memberInfo by key:{key}. 无法找到Key为{key}的成员信息。");
            return;
        }

        memberEnables[key] = enable;
    }

    /// <summary>
    /// 确认成员是否打开
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool MemberIsEnable(int key)
    {
        if (memberEnables != null && memberEnables.Count > 0)
        {
            if (!memberEnables.ContainsKey(key)) return false;
            return memberEnables[key];
        }

        if (Settings != null && Settings.members != null && Settings.members.Length > 0)
        {
            for (int i = 0; i < Settings.members.Length; i++)
            {
                var info = Settings.members[i];
                if (info.key == key)
                {
                    return info.enableDefault;
                }
            }
        }

        return false;
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
    /// 确认成员Key是否包含
    /// 在程序运行时和非运行时都可用
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool ContainsMemberKey(int key)
    {
        if (GetMemberInfo(key, out DebugxMemberInfo memberInfo))
        {
            return true;
        }

        return false;
    }

    private static bool GetMemberInfo(int key, out DebugxMemberInfo memberInfo)
    {
        memberInfo = null;
        if (Settings == null || Settings.members == null || Settings.members.Length == 0) return false;

        for (int i = 0; i < Settings.members.Length; i++)
        {
            if (Settings.members[i].key == key)
            {
                memberInfo = Settings.members[i];
                return true;
            }
        }

        return false;
    }

    private static bool GetMemberInfo(string signature, out DebugxMemberInfo memberInfo)
    {
        memberInfo = null;

        if (Settings == null)
        {
            LogAdmWarning($"Debugx.GetMemberInfo: The initial configuration is not performed.Settings is null. 未成功初始化配置，Settings为空。");
            return false;
        }

        if (Settings.members == null)
        {
            LogAdmWarning($"Debugx.GetMemberInfo: The initial configuration is not performed.Settings.members is null. 未初始化配置，Settings.members为空。");
            return false;
        }

        if (Settings.members.Length == 0)
        {
            LogAdmWarning($"Debugx.GetMemberInfo: There are no members available.Settings.members.Length is 0. 没有任何可用的成员，Settings.members.Length为0。");
            return false;
        }

        for (int i = 0; i < Settings.members.Length; i++)
        {
            if (Settings.members[i].signature == signature)
            {
                memberInfo = Settings.members[i];
                return true;
            }
        }

        return false;
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
        LogCreator(type, AdminInfo, message, showTime, showNetTag);
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
        Log(LogType.Log, DebugxProjectSettings.normalInfoKey, message, showTime, showNetTag);
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
        Log(LogType.Warning, DebugxProjectSettings.normalInfoKey, message, showTime, showNetTag);
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
        Log(LogType.Error, DebugxProjectSettings.normalInfoKey, message, showTime, showNetTag);
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
        Log(LogType.Log, DebugxProjectSettings.masterInfoKey, message, showTime, showNetTag);
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
        Log(LogType.Warning, DebugxProjectSettings.masterInfoKey, message, showTime, showNetTag);
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
        Log(LogType.Error, DebugxProjectSettings.masterInfoKey, message, showTime, showNetTag);
    }

    /// <summary>
    /// 成员打印Log
    /// </summary>
    /// <param name="key">成员密钥，DebugxMemberInfo中配置的key</param>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void Log(int key, object message, bool showTime = false, bool showNetTag = true)
    {
        Log(LogType.Log, key, message, showTime, showNetTag);
    }

    /// <summary>
    /// 成员打印Log
    /// </summary>
    /// <param name="signature">成员签名，DebugxMemberInfo中配置的Signature</param>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void Log(string signature, object message, bool showTime = false, bool showNetTag = true)
    {
        Log(LogType.Log, signature, message, showTime, showNetTag);
    }

    /// <summary>
    /// 成员打印LogWarning
    /// </summary>
    /// <param name="key">成员密钥，DebugxMemberInfo中配置的key</param>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogWarning(int key, object message, bool showTime = false, bool showNetTag = true)
    {
        Log(LogType.Warning, key, message, showTime, showNetTag);
    }

    /// <summary>
    /// 成员打印LogWarning
    /// </summary>
    /// <param name="signature">成员签名，DebugxMemberInfo中配置的Signature</param>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogWarning(string signature, object message, bool showTime = false, bool showNetTag = true)
    {
        Log(LogType.Warning, signature, message, showTime, showNetTag);
    }

    /// <summary>
    /// 成员打印LogError
    /// </summary>
    /// <param name="key">成员密钥，DebugxMemberInfo中配置的key</param>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogError(int key, object message, bool showTime = false, bool showNetTag = true)
    {
        Log(LogType.Error, key, message, showTime, showNetTag);
    }

    /// <summary>
    /// 成员打印LogError
    /// </summary>
    /// <param name="signature">成员签名，DebugxMemberInfo中配置的Signature</param>
    /// <param name="message">打印内容</param>
    /// <param name="showTime">显示时间</param>
    /// <param name="showNetTag">显示网络标记，Server或者Client。此功能依赖项目，需要项目通过SetServerCheck方法来设置</param>
    [Conditional("DEBUG_X")]
    public static void LogError(string signature, object message, bool showTime = false, bool showNetTag = true)
    {
        Log(LogType.Error, signature, message, showTime, showNetTag);
    }

    private static void Log(LogType type, int key, object message, bool showTime = false, bool showNetTag = true)
    {
        if (!enableLog || !enableLogMember) return;
        LogCreator(type, key, message, showTime, showNetTag);
    }

    private static void Log(LogType type, string signature, object message, bool showTime = false, bool showNetTag = true)
    {
        if (!enableLog || !enableLogMember) return;
        LogCreator(type, signature, message, showTime, showNetTag);
    }

    /// <summary>
    /// 确认是否开启了仅显示某个Key的成员
    /// true=允许通过 false=返回，不允许打印
    /// </summary>
    private static bool CheckLogThisKeyMemberOnly(int key)
    {
        return logThisKeyMemberOnly == 0 || !ContainsMemberKey(logThisKeyMemberOnly) || logThisKeyMemberOnly == key;
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
        if (GetMemberInfo(key, out DebugxMemberInfo memberInfo))
        {
            if (!MemberIsEnable(key)) return;//此成员未打开
        }
        else
        {
            LogAdmWarning($"Debugx.LogCreator: cant find memberInfo by key:{key}. 无法找到Key为{key}的成员信息。");
            if (!allowUnregisteredMember) return;
        }

        //设置了仅打印某个Key成员Log
        if (!CheckLogThisKeyMemberOnly(key)) return;

        LogCreator(type, memberInfo, message, showTime, showNetTag);
    }

    private static void LogCreator(LogType type, string signature, object message, bool showTime = false, bool showNetTag = true)
    {
        int key = 0;
        if (GetMemberInfo(signature, out DebugxMemberInfo memberInfo))
        {
            key = memberInfo.key;
            if (!MemberIsEnable(key)) return;//此成员未打开
        }
        else
        {
            LogAdmWarning($"Debugx.LogCreator: cant find memberInfo by signature:{signature}. 无法找到Signature为{signature}的成员信息。");
            if (!allowUnregisteredMember) return;
        }

        //设置了仅打印某个Key成员Log
        if (!CheckLogThisKeyMemberOnly(key)) return;

        LogCreator(type, memberInfo, message, showTime, showNetTag);
    }

    private static void LogCreator(LogType type, DebugxMemberInfo info, object message, bool showTime = false, bool showNetTag = true)
    {
        logxSb.Append(DebugxProjectSettings.debugxTag);

        if (showNetTag && serverCheckDelegate != null)
            logxSb.Append(serverCheckDelegate.Invoke() ? "Server: " : "Client: ");

        if (showTime)
        {
            logxSb.Append($" [{DateTime.Now.ToString("HH:mm:ss")}] ");
        }

        if (info != null)
        {
            if (info.LogSignature)
                logxSb.Append($"[Sig: {info.signature}]");

            if (!string.IsNullOrEmpty(info.color))
                logxSb.Append(info.haveHeader ? $" <color=#{info.color}>{info.header} : {message}</color>" : $" <color=#{info.color}>{message}</color>");
            else
                logxSb.Append(info.haveHeader ? $" {info.header} : {message}" : $" {message}");
        }
        else
        {
            logxSb.Append($" UnregisteredMember : {message}");
        }

        UnityEngine.Debug.unityLogger.Log(type, logxSb.ToString());
        logxSb.Length = 0;
    }
}