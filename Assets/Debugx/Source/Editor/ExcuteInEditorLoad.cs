using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace DebugxU3D
{
    [InitializeOnLoad]
    public class ExcuteInEditorLoad
    {

        static ExcuteInEditorLoad()
        {
            //创建默认调试成员配置文件
            var configDefault = DebugxEditorLibrary.DebugxMemberConfigDefault;
            
            //设置调试成员编辑窗口默认配置
            bool debugxMemberWindowConfigidDirty = false;
            if (DebugxSettingWindowConfig.Current.debugxMemberConfigSet == null)
            {
                DebugxSettingWindowConfig.Current.debugxMemberConfigSet = configDefault;
                debugxMemberWindowConfigidDirty = true;
            }
            if(DebugxSettingWindowConfig.Current.debugxMemberConfigInitOnEditorLoad == null)
            {
                DebugxSettingWindowConfig.Current.debugxMemberConfigInitOnEditorLoad = configDefault;
                debugxMemberWindowConfigidDirty = true;
            }
            if(debugxMemberWindowConfigidDirty)
            {
                EditorUtility.SetDirty(DebugxSettingWindowConfig.Current);
                AssetDatabase.SaveAssetIfDirty(DebugxSettingWindowConfig.Current);
            }

            //初始化编辑器非运行时用调试成员配置
            Debugx.Init(DebugxSettingWindowConfig.Current.debugxMemberConfigInitOnEditorLoad);
            Debugx.LogAdm( $"Debugx init in editor load. DebugxMemberConfigInitOnEditorLoad : {DebugxSettingWindowConfig.Current.debugxMemberConfigInitOnEditorLoad.name}");
        }
    }
}

