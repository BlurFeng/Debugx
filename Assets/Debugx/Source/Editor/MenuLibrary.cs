using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DebugxU3D
{
    public class MenuLibrary
    {
        [MenuItem("Tools/Debugx/CreateDebugxManager", false, 2)]
        public static void CreateDebugxManager()
        {
            DebugxManager.Instance.Create(DebugxEditorLibrary.DebugxMemberConfigDefault);
        }
    }
}