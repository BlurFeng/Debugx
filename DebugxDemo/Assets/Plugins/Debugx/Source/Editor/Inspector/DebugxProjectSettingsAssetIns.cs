using UnityEditor;

namespace DebugxLog
{
    [CanEditMultipleObjects, CustomEditor(typeof(DebugxProjectSettingsAsset))]
    public class DebugxProjectSettingsAssetIns : Editor
    {
        public override void OnInspectorGUI()
        {
            //禁止直接修改.asset，这会导致不必要的问题。
            EditorGUILayout.HelpBox("禁止直接修改配置资源文件，请通过 Editor->ProjectSettings->Debugx 界面修改", MessageType.Info);
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }
    }
}