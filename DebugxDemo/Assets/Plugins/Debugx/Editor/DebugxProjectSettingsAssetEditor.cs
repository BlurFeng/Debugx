using UnityEditor;
using UnityEngine;

namespace DebugxLog
{
    public static class DebugxProjectSettingsAssetEditor
    {
        public static void OnInitializeOnLoadMethod()
        {
            //项目启动时确认配置资源是否存在
            CheckDebugxProjectSettingsAsset();
        }
        
        /// <summary>
        /// 确认配置资源是否存在
        /// </summary>
        public static void CheckDebugxProjectSettingsAsset()
        {
            //在代码编译时走到这里，无法通过instance = Resources.Load<DebugxProjectSettingsAsset>(DebugxProjectSettings.fileName);方式获取到实际存在的文件
            //这会导致每次代码重编译时都创建了新的DebugxProjectSettingsAsset并覆盖了旧的

            bool haveAsset = false;
            string[] assetGUIDs = AssetDatabase.FindAssets(DebugxProjectSettings.fileName);
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                if (path.Contains("/Debugx/Resources/"))
                {
                    var ps = AssetDatabase.LoadAssetAtPath<DebugxProjectSettingsAsset>(path);
                    haveAsset = true;
                }
            }

            if (!haveAsset) CreateDebugxProjectSettingsAsset();
        }
        
        /// <summary>
        /// 创建配置资源
        /// </summary>
        public static void CreateDebugxProjectSettingsAsset()
        {
            //在编辑器启动或代码编译时，创建Asset后直接Resources.Load此资源会导致一个堆栈溢出的Bug。所以我们使用DebugxProjectSettings.ApplyBy接口进行保存，避开会调用到Resources.Load的流程
            //调用Resources.Load加载已经存在的资源则不会有此问题，应该是Editor启动时创建的Asset资源还未成功进行保存

            string path = $"{DebugxStaticData.ResourcesPath}/{DebugxProjectSettings.fileName}.asset";

            if (DebugxProjectSettingsAsset.Instance) AssetDatabase.DeleteAsset(path);//移除旧资源

            DebugxProjectSettingsAsset.Instance = ScriptableObject.CreateInstance(typeof(DebugxProjectSettingsAsset)) as DebugxProjectSettingsAsset;

            //确认文件夹是否存在，否则创建
            if (!System.IO.Directory.Exists(DebugxStaticData.ResourcesPath))
                System.IO.Directory.CreateDirectory(DebugxStaticData.ResourcesPath);

            AssetDatabase.CreateAsset(DebugxProjectSettingsAsset.Instance, path);

            DebugxProjectSettingsAsset.Instance.ResetMembers();

            EditorUtility.SetDirty(DebugxProjectSettingsAsset.Instance);
            AssetDatabase.SaveAssetIfDirty(DebugxProjectSettingsAsset.Instance);
            DebugxProjectSettings.ApplyBy(DebugxProjectSettingsAsset.Instance);
        }
    }
}
