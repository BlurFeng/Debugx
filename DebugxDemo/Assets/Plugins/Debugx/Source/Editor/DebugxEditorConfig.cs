using UnityEngine;

namespace DebugxLog
{
    public class DebugxEditorConfig : ScriptableObject
    {
        private static DebugxEditorConfig debugxEditorConfig;
        /// <summary>
        /// 调试成员编辑窗口配置文件
        /// </summary>
        public static DebugxEditorConfig Get
        {
            get
            {
                if (debugxEditorConfig == null)
                    debugxEditorConfig = DebugxEditorLibrary.GetConfigDefault<DebugxEditorConfig>(DebugxEditorLibrary.EditorConfigPath + "/DebugxEditorConfig.asset");

                return debugxEditorConfig;
            }
        }

        //public bool canResetProjectSettings;
    }
}