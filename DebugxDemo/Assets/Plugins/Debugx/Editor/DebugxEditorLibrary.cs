using System.IO;
using UnityEditor;
using UnityEngine;

namespace DebugxLog.Editor
{
    public class DebugxEditorLibrary
    {
        public static string Name { get; private set; } = "DebugxEditorLibrary";

        /// <summary>
        /// Root directory of the Debugx folder.
        /// Debugx文件夹根节点。
        /// </summary>
        public static string RootPath
        {
            get
            {
                if (DebugxStaticData.RootPath == null)
                {
                    DebugxStaticData.RootPath = GetAssetsPathBySelfFolder(Name).Replace(("/Source/Editor"), "");
                }

                return DebugxStaticData.RootPath;
            }
        }

        /// <summary>
        /// Configuration storage folder.
        /// 配置存储文件夹。
        /// </summary>
        public static string ResourcesPath
        {
            get
            {
                DebugxStaticData.ResourcesPath ??= RootPath + "/Resources";

                //确认文件夹是否存在，否则创建
                if (!Directory.Exists(DebugxStaticData.ResourcesPath))
                    Directory.CreateDirectory(DebugxStaticData.ResourcesPath);

                return DebugxStaticData.ResourcesPath;
            }
        }

        /// <summary>
        /// Confirm that the Debugx project settings asset exists.
        /// 确认Debugx项目设置资源存在。
        /// </summary>
        public static void OnInitializeOnLoadMethod()
        {
            // Confirm the folder path. 确认文件夹路径。
            var resourcesPath = ResourcesPath;
        }

        /// <summary>
        /// Get the path of a resource with a given name under the plugin’s own folder.
        /// 获取自身插件文件夹下某个名称资源的路径。
        /// </summary>
        /// <param name="name">The name of the resource. 资源名称。</param>
        /// <param name="getDirectoryName">If true, get the folder path; if false, get the file path. 获取文件夹路径，false时获取到文件路径。</param>
        /// <returns>The requested path as a string. 返回路径字符串。</returns>
        public static string GetAssetsPathBySelfFolder(string name, bool getDirectoryName = true)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            string[] assetGUIDs = AssetDatabase.FindAssets(name);
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                if (path.Contains("/Debugx/"))
                {
                    if (getDirectoryName)
                        return Path.GetDirectoryName(path).Replace('\\', '/');
                    else
                        return path;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the default configuration.
        /// 获取默认配置。
        /// </summary>
        /// <typeparam name="T">Type parameter. 类型参数。</typeparam>
        /// <param name="path">Full path. 全路径。</param>
        /// <param name="createOnNotFound">Automatically create if not found. 未找到时自动创建。</param>
        /// <param name="createNew">Automatically create new if not found. 未找到时自动创建新的实例。</param>
        /// <returns>Returns the configuration object. 返回配置对象。</returns>
        public static T GetConfigDefault<T>(string path, bool createOnNotFound = true) where T : ScriptableObject
        {
            return GetConfigDefault<T>(path, out bool createNew, createOnNotFound);
        }
        
        /// <summary>
        /// Get the default configuration.
        /// 获取默认配置。
        /// </summary>
        /// <typeparam name="T">Type parameter. 类型参数。</typeparam>
        /// <param name="path">Full path. 全路径。</param>
        /// <param name="createOnNotFound">Automatically create if not found. 未找到时自动创建。</param>
        /// <param name="createNew">Automatically create new if not found. 未找到时自动创建。</param>
        /// <returns>Returns the configuration object. 返回配置对象。</returns>
        public static T GetConfigDefault<T>(string path, out bool createNew, bool createOnNotFound = true) where T : ScriptableObject
        {
            createNew = false;
            T config = AssetDatabase.LoadAssetAtPath<T>(path);
            if (!config && createOnNotFound)
            {
                createNew = true;
                config = ScriptableObject.CreateInstance(typeof(T)) as T;
                AssetDatabase.CreateAsset(config, path);
            }

            return config;
        }
    }
}