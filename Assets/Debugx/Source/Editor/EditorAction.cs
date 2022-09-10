using UnityEditor;

namespace DebugxLog
{
    [InitializeOnLoad]
    public class EditorAction
    {
        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            DebugxEditorLibrary.ExcuteInEditorLoad();
            
            EditorApplication.wantsToQuit += Quit;
            DebugxProjectSettingsAsset.GetRandomColorForMember += ColorDispenser.GetRandomColorForMember;
        }

        static bool Quit()
        {
            EditorApplication.wantsToQuit -= Quit;
            DebugxProjectSettingsAsset.GetRandomColorForMember -= ColorDispenser.GetRandomColorForMember;
            return true;
        }
    }
}

