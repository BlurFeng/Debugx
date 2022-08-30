using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DebugxU3D
{
    public class DebugxEditorLibrary
    {
        public static string Name { get; private set; } = "DebugxEditorLibrary";

        private static string m_RootPath;
        /// <summary>
        /// Debugx文件夹根节点
        /// </summary>
        public static string RootPath
        {
            get
            {
                if (m_RootPath == null)
                {
                    m_RootPath = GetAssetsPathBySelfFolder(Name).Replace(("/Source/Editor"), "");
                }

                return m_RootPath;
            }
        }

        private static string m_EditorConfigPath;
        /// <summary>
        /// 编辑器用配置存储文件夹
        /// </summary>
        public static string EditorConfigPath
        {
            get
            {
                if (m_EditorConfigPath == null)
                {
                    m_EditorConfigPath = RootPath + "/Source/Editor/Config";
                }

                //确认文件夹是否存在，否则创建
                if (!Directory.Exists(m_EditorConfigPath))
                    Directory.CreateDirectory(m_EditorConfigPath);

                return m_EditorConfigPath;
            }
        }

        private static string m_onfigPath;
        /// <summary>
        /// 配置存储文件夹
        /// </summary>
        public static string ConfigPath
        {
            get
            {
                if (m_onfigPath == null)
                {
                    m_onfigPath = RootPath + "/Config";
                }

                //确认文件夹是否存在，否则创建
                if (!Directory.Exists(m_onfigPath))
                    Directory.CreateDirectory(m_onfigPath);

                return m_onfigPath;
            }
        }

        private static DebugxMemberConfig m_DebugxMemberConfigDefault;
        /// <summary>
        /// 默认调试成员配置文件
        /// </summary>
        public static DebugxMemberConfig DebugxMemberConfigDefault
        {
            get
            {
                if (m_DebugxMemberConfigDefault == null)
                {
                    m_DebugxMemberConfigDefault = GetConfigDefault<DebugxMemberConfig>($"{ConfigPath}/DebugxMemberConfigDefault.asset", out bool createNew);
                    if(createNew)
                    {
                        m_DebugxMemberConfigDefault.debugxMemberInfos = new DebugxMemberInfo[1]
                        {
                            new DebugxMemberInfo()
                            { signature = "WinhooFeng", logSignature = true, key = 1, color = new Color(0.7843f, 0.941f, 1f, 1f), enableCached = true }
                        };

                        EditorUtility.SetDirty(m_DebugxMemberConfigDefault);
                        AssetDatabase.SaveAssetIfDirty(m_DebugxMemberConfigDefault);
                        //AssetDatabase.SaveAssets();
                    }
                }

                return m_DebugxMemberConfigDefault;
            }
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
                AssetDatabase.SaveAssets();
            }

            return config;
        }
    }
}