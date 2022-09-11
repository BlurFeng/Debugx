using System.IO;
using UnityEditor;
using UnityEngine;

namespace DebugxLog
{
    public class DebugxEditorLibrary
    {
        public static string Name { get; private set; } = "DebugxEditorLibrary";

        /// <summary>
        /// Debugx文件夹根节点
        /// </summary>
        public static string RootPath
        {
            get
            {
                if (DebugxStaticData.rootPath == null)
                {
                    DebugxStaticData.rootPath = GetAssetsPathBySelfFolder(Name).Replace(("/Source/Editor"), "");
                }

                return DebugxStaticData.rootPath;
            }
        }

        private static string editorConfigPath;
        /// <summary>
        /// 编辑器用配置存储文件夹
        /// </summary>
        public static string EditorConfigPath
        {
            get
            {
                editorConfigPath ??= RootPath + "/Source/Editor/Config";

                //确认文件夹是否存在，否则创建
                if (!Directory.Exists(editorConfigPath))
                    Directory.CreateDirectory(editorConfigPath);

                return editorConfigPath;
            }
        }

        
        /// <summary>
        /// 配置存储文件夹
        /// </summary>
        public static string ResourcesPath
        {
            get
            {
                DebugxStaticData.resourcesPath ??= RootPath + "/Resources";

                //确认文件夹是否存在，否则创建
                if (!Directory.Exists(DebugxStaticData.resourcesPath))
                    Directory.CreateDirectory(DebugxStaticData.resourcesPath);

                return DebugxStaticData.resourcesPath;
            }
        }

        //确认Debugx项目设置资源存在
        public static void OnInitializeOnLoadMethod() 
        {
            var resourcesPath = ResourcesPath;//确认文件夹路径
        }

        /// <summary>
        /// 获取自身插件文件夹下某个名称资源的路径
        /// </summary>
        /// <param name="name"></param>
        /// <param name="getDirectoryName">获取文件夹路径，false时获取到文件路径</param>
        /// <returns></returns>
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
        /// 获取默认配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">全路径</param>
        /// <param name="createOnNotFound">未找到时自动创建</param>
        /// <param name="createNew">未找到时自动创建</param>
        /// <returns></returns>
        public static T GetConfigDefault<T>(string path, bool createOnNotFound = true) where T : ScriptableObject
        {
            return GetConfigDefault<T>(path, out bool createNew, createOnNotFound);
        }

        /// <summary>
        /// 获取默认配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">全路径</param>
        /// <param name="createOnNotFound">未找到时自动创建</param>
        /// <param name="createNew">未找到时自动创建</param>
        /// <returns></returns>
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