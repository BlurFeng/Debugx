using DebugxLog;
using UnityEditor;

public static class DebugxTools
{
    [MenuItem("Tools/Debugx/Create DebugxProjectSettings Asset")]
    public static void CreateDebugxProjectSettingsAsset()
    {
        if (DebugxProjectSettingsAsset.Instance == null)
        {
            DebugxProjectSettingsAsset.CreateDebugxProjectSettingsAsset();
        }
        else if(EditorUtility.DisplayDialog("Create DebugxProjectSettings Asset", "The debugxProjectSettings asset already exists, whether to create a new and overwrite?\nDebugx��Ŀ������Դ�Ѿ����ڣ��Ƿ񴴽�һ���µĲ����ǣ�", "Yes", "Cancel"))
        {
            DebugxProjectSettingsAsset.CreateDebugxProjectSettingsAsset();
        }
    }
}
