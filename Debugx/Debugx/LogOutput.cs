using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DebugxLog.Tools
{
    /// <summary>
    /// 输出Log到本地txt文件工具类
    /// </summary>
    public class LogOutput
    {
        /// <summary>
        /// 输出Log类型的堆栈跟踪
        /// </summary>
        public static bool enableLogStackTrace = false;

        /// <summary>
        /// 输出Warning类型的堆栈跟踪
        /// </summary>
        public static bool enableWarningStackTrace = false;

        /// <summary>
        /// 输出错误类型的堆栈跟踪
        /// </summary>
        public static bool enableErrorStackTrace = true;

        /// <summary>
        /// 记录所有非Debugx打印的Log
        /// </summary>
        public static bool recordAllNonDebugxLogs = false;

        private static bool init;
        private const string fileName = "DebugxLog";
        private const string fileNameFull = "DebugxLog.log";
        private const string fileType = ".log";
        /// <summary>
        /// Log输出文件夹重写
        /// </summary>
        public static string directoryPathCover;
        private static string directoryPath;
        /// <summary>
        /// 输出文件夹路径
        /// </summary>
        private static string DirectoryPath
        {
            get
            {
                if (!string.IsNullOrEmpty(directoryPathCover) && Directory.Exists(directoryPathCover))
                    return directoryPathCover;
                return directoryPath;
            }
        }
        private static string savePath;
        private static System.Object locker = new System.Object();
        private static readonly StringBuilder logBuilder = new StringBuilder();

        //用于裁剪color代码的正则表达式
        private static Regex regex_messageCut = new Regex(@"<color=#([\S.]{6})>|</color>|\[Debugx\]");
        private static Regex regex_RecordMessageTag = new Regex(@"\[Debugx\]");

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            if (init) return;
            init = true;

            System.IO.DirectoryInfo dir = new DirectoryInfo(Application.consoleLogPath);
            directoryPath = dir.Parent.FullName;
            savePath = string.Format("{0}/{1}", DirectoryPath, fileNameFull);

            //#if UNITY_EDITOR
            //            System.IO.DirectoryInfo dir = new DirectoryInfo(Application.dataPath);
            //            savePath = string.Format("{0}/{1}", dir.Parent.FullName, fileName);
            //#elif UNITY_STANDALONE_WIN
                          //输出到C盘用户文件夹下
            //#elif UNITY_ANDROID
            //            savePath = string.Format("{0}/Log/{1}", Application.persistentDataPath, fileName);
            //#elif UNITY_IPHONE
            //            savePath = string.Format("{0}/{1}", Application.persistentDataPath, fileName);
            //#elif UNITY_STANDALONE_WIN
            //            savePath = string.Format("{0}/{1}", Application.dataPath, fileName);
            //#endif
            
            FileInfo fileInfo = new FileInfo(savePath);

            //创建文件夹
            directoryPath = fileInfo.DirectoryName;
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            try
            {
                //创建一个新文件，向其中写入指定的字符串，然后关闭文件。 如果目标文件已存在，则覆盖该文件。
                System.IO.File.WriteAllText(savePath, string.Empty, Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                Debugx.LogMstError("error can not open." + ex.StackTrace.ToString() + ex.Message);
            }

            Application.logMessageReceived += LogCallBack;
        }

        /// <summary>
        /// 当销毁时，应用程序结束时
        /// </summary>
        public static void OnDestroy()
        {
            if (!init) return;

            FileInfo fileInfo = new FileInfo(savePath);
            if(fileInfo != null)
            {
                if(fileInfo.Length == 0)
                {
                    fileInfo.Delete();
                }
                else
                {
                    //将打印的文件重命名
                    string filePath = $"{directoryPath}\\{fileName}-{DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss")}{fileType}";
                    Debugx.LogAdmWarning($"logOutput over, file path : {filePath}");
                    File.Move(savePath, filePath);
                }
            }

            Application.logMessageReceived -= LogCallBack;
        }

        private static void LogCallBack(string message, string stackTrace, LogType type)
        {
            if (!recordAllNonDebugxLogs && !regex_RecordMessageTag.IsMatch(message)) return;

            lock (locker)
            {
                //裁剪Color代码
                message = regex_messageCut.Replace(message, "");

                string strTime = System.DateTime.Now.ToString("MM-dd HH:mm:ss");
                string log = $"[{strTime}][{Time.frameCount}]{message}";
                logBuilder.Append(log);

                //是否记录对战跟踪
                if(type == LogType.Log && enableLogStackTrace 
                    ||type == LogType.Warning && enableWarningStackTrace
                    || type == LogType.Error && enableErrorStackTrace)
                {
                    logBuilder.Append(stackTrace);
                    logBuilder.Append("\n");
                }

                if (logBuilder.Length > 0)
                {
                    //创建一个 StreamWriter，它将 UTF-8 编码文本追加到现有文件或新文件（如果指定文件不存在）。
                    using (StreamWriter sw = File.AppendText(savePath))
                    {
                        sw.WriteLine(logBuilder.ToString());
                    }

                    logBuilder.Length = 0;
                }

                HandleDrawLogs(message, stackTrace, type);
            }
        }

        #region Draw Logs 绘制Logs

        private struct DrawLogInfo
        {
            public string message;
            public string stackTrace;
            public LogType type;
        }

        /// <summary>
        /// 将Log绘制到屏幕
        /// </summary>
        public static bool drawLogToScreen = false;

        /// <summary>  
        /// 限制绘制Log数量
        /// </summary>  
        public static bool restrictDrawLogCount = false;

        /// <summary>  
        /// 绘制Log最大数量
        /// </summary>  
        public static int maxDrawLogs = 1000;


        private static readonly List<DrawLogInfo> drawLogs = new List<DrawLogInfo>();
        private static Vector2 scrollPosition;
        private static bool collapse;//折叠或打开整个界面
        private static bool collapseRepetition;//折叠重复信息

        //窗口设置
        private const int margin = 10;
        private static readonly Rect titleBarRect = new Rect(0, 0, 1000, 20);
        private static Rect windowRect;

        /// <summary>
        /// 绘制GUI
        /// </summary>
        public static void DrawGUI()
        {
            if (!drawLogToScreen)
            {
                return;
            }

            windowRect = collapse? new Rect(10,10, Screen.width * 0.4f - (margin * 5), 40f) : new Rect(10, 10, Screen.width * 0.4f - (margin * 5), Screen.height * 0.3f - (margin * 6));
            windowRect = GUILayout.Window(19940223, windowRect, DrawConsoleWindow, "Debugx Logs");
        }

        private static Color GetLogColor(LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                    return Color.red;
                case LogType.Assert:
                    return Color.white;
                case LogType.Warning:
                    return Color.yellow;
                case LogType.Log:
                    return Color.white;
                case LogType.Exception:
                    return Color.red;
            }

            return Color.white;
        }

        /// <summary>  
        /// Displays a window that lists the recorded logs.  
        /// </summary>  
        /// <param name="windowID">Window ID.</param>  
        private static void DrawConsoleWindow(int windowID)
        {
            DrawToolbar();
            if(!collapse)
                DrawLogsList();

            // Allow the window to be dragged by its title bar.  
            GUI.DragWindow(titleBarRect);
        }

        /// <summary>  
        /// Displays a scrollable list of logs.  
        /// </summary>  
        private static void DrawLogsList()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

            // Iterate through the recorded logs.  
            for (var i = 0; i < drawLogs.Count; i++)
            {
                var log = drawLogs[i];
                //  Destroy(logs[i - 1]);
                // Combine identical messages if collapse option is chosen.  
                if (collapseRepetition && i > 0)
                {
                    var previousMessage = drawLogs[i - 1].message;

                    if (log.message == previousMessage)
                    {
                        continue;
                    }
                }

                GUI.contentColor = GetLogColor(log.type);
                GUILayout.Label(log.message);
            }

            GUILayout.EndScrollView();

            // Ensure GUI colour is reset before drawing other components.  
            GUI.contentColor = Color.white;
        }

        /// <summary>  
        /// Displays options for filtering and changing the logs list.  
        /// </summary>  
        private static void DrawToolbar()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(collapse ? "Open" : "Collapse"))
            {
                collapse = !collapse;
            }
            if (GUILayout.Button("Clear"))
            {
                drawLogs.Clear();
            }

            collapseRepetition = GUILayout.Toggle(collapseRepetition, "Collapse", GUILayout.ExpandWidth(false));  

            GUILayout.EndHorizontal();
        }

        /// <summary>  
        /// Records a log from the log callback.  
        /// </summary>  
        /// <param name="message">Message.</param>  
        /// <param name="stackTrace">Trace of where the message came from.</param>  
        /// <param name="type">Type of message (error, exception, warning, assert).</param>  
        private static void HandleDrawLogs(string message, string stackTrace, LogType type)
        {
            drawLogs.Add(new DrawLogInfo
            {
                message = message,
                stackTrace = stackTrace,
                type = type,
            });

            TrimExcessLogs();
        }

        /// <summary>  
        /// Removes old logs that exceed the maximum number allowed.  
        /// </summary>  
        private static void TrimExcessLogs()
        {
            if (!restrictDrawLogCount)
            {
                return;
            }

            var amountToRemove = Mathf.Max(drawLogs.Count - maxDrawLogs, 0);

            if (amountToRemove == 0)
            {
                return;
            }

            drawLogs.RemoveRange(0, amountToRemove);
        }

        #endregion
    }
}
