using UnityEditor;
using UnityEngine;

namespace DebugxU3D
{
    [CanEditMultipleObjects, CustomEditor(typeof(DebugxManager))]
    public class DebugxManagerIns : Editor
    {
        //不要在OnEnable中获取SerializedProperty并缓存，可能导致报错
        //比如在调用Editor.CreateEditor(xxxObj.GetComponent<xxxControl>())方法时

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // 更新显示
            serializedObject.Update();

            SerializedProperty debugxMemberConfig = serializedObject.FindProperty("debugxMemberConfig");
            SerializedProperty enableLog = serializedObject.FindProperty("enableLog");
            SerializedProperty enableLogNormal = serializedObject.FindProperty("enableLogNormal");
            SerializedProperty enableLogMaster = serializedObject.FindProperty("enableLogMaster");
            SerializedProperty logMasterOnly = serializedObject.FindProperty("logMasterOnly");
            SerializedProperty logThisKeyMemberOnly = serializedObject.FindProperty("logThisKeyMemberOnly");

            SerializedProperty enbaleLogOutput = serializedObject.FindProperty("enbaleLogOutput");
            SerializedProperty enableLogStackTrace = serializedObject.FindProperty("enableLogStackTrace");
            SerializedProperty enableWarningStackTrace = serializedObject.FindProperty("enableWarningStackTrace");
            SerializedProperty enableErrorStackTrace = serializedObject.FindProperty("enableErrorStackTrace");
            SerializedProperty revordAllNonDebugxLogs = serializedObject.FindProperty("revordAllNonDebugxLogs");

            SerializedProperty enableAwakeTestLog = serializedObject.FindProperty("enableAwakeTestLog");
            SerializedProperty enableUpdateTestLog = serializedObject.FindProperty("enableUpdateTestLog");

            SerializedProperty debugxMemberEnables = serializedObject.FindProperty("debugxMemberEnables");
            SerializedProperty debugxMemberSignatures = serializedObject.FindProperty("debugxMemberSignatures");
            SerializedProperty debugxMemberKeys = serializedObject.FindProperty("debugxMemberKeys");


            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("通用配置", EditorStyle.Get.TitleStyle_3);
            EditorGUILayout.PropertyField(debugxMemberConfig, new GUIContent("DebugxMemberConfig", "调试成员配置"));
            EditorGUILayout.PropertyField(enableLog, new GUIContent("EnableLog", "Log总开关"));
            EditorGUILayout.PropertyField(enableLogNormal, new GUIContent("EnableLogNormal", "普通Log开关"));
            EditorGUILayout.PropertyField(enableLogMaster, new GUIContent("EnableLogMaster", "最高优先级Log开关"));
            EditorGUILayout.PropertyField(logMasterOnly, new GUIContent("LogMasterOnly", "仅显示最高优先级Log开关"));
            EditorGUI.BeginDisabledGroup(DebugxManager.Instance.logMasterOnly);
            EditorGUILayout.PropertyField(logThisKeyMemberOnly, new GUIContent("LogThisKeyMemberOnly", "仅打印此Key的成员Log。0为关闭，设置时Key必须是存在于配置的成员。必须关闭LogMasterOnly后才能设置此值。"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.LabelField("工具配置", EditorStyle.Get.TitleStyle_3);
            EditorGUILayout.PropertyField(enbaleLogOutput, new GUIContent("EnbaleLogOutput", "输出Log到本地（启动前设置，运行时设置无效）。编辑器时输出到项目的Logs文件夹下，实机运行时根据平台输出到不同目录下。"));
            EditorGUILayout.PropertyField(enableLogStackTrace, new GUIContent("EnableLogStackTrace", "输出Log类型的堆栈跟踪"));
            EditorGUILayout.PropertyField(enableWarningStackTrace, new GUIContent("EnableWarningStackTrace", "输出Warning类型的堆栈跟踪"));
            EditorGUILayout.PropertyField(enableErrorStackTrace, new GUIContent("EnableErrorStackTrace", "输出错误类型的堆栈跟踪"));
            EditorGUILayout.PropertyField(revordAllNonDebugxLogs, new GUIContent("RevordAllNonDebugxLogs", "记录所有非Debugx打印的Log"));

            EditorGUILayout.LabelField("编辑器时配置", EditorStyle.Get.TitleStyle_3);
            EditorGUILayout.PropertyField(enableAwakeTestLog, new GUIContent("EnableAwakeTestLog", "打开Awake中测试用的Log打印"));
            EditorGUILayout.PropertyField(enableUpdateTestLog, new GUIContent("EnableUpdateTestLog", "打开Update中测试用的Log打印"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("调试成员开关列表", EditorStyle.Get.TitleStyle_3);
            if (debugxMemberEnables != null && debugxMemberEnables.arraySize > 0)
            {
                for (int i = 0; i < debugxMemberEnables.arraySize; i++)
                {
                    SerializedProperty enable = debugxMemberEnables.GetArrayElementAtIndex(i);
                    SerializedProperty signature = debugxMemberSignatures.GetArrayElementAtIndex(i);
                    SerializedProperty key = debugxMemberKeys.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    string memberSignature = string.IsNullOrEmpty(signature.stringValue) ? "Member" : signature.stringValue;
                    EditorGUILayout.LabelField($"{memberSignature} Key: {key.intValue}");
                    EditorGUILayout.PropertyField(enable, new GUIContent("Enable"));
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("没有配置任何成员");
            }

            if(EditorGUI.EndChangeCheck())
            {
                //应用属性修改
                serializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(DebugxManager.Instance.debugxMemberConfig);
                AssetDatabase.SaveAssetIfDirty(DebugxManager.Instance.debugxMemberConfig);
            }
        }
    }
}