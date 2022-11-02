using UnityEditor;

namespace DebugxLog
{
    [InitializeOnLoad]
    public class EditorAction
    {
        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            DebugxEditorLibrary.OnInitializeOnLoadMethod();
            DebugxProjectSettingsAsset.OnInitializeOnLoadMethod();
            ColorDispenser.OnInitializeOnLoadMethod();

            EditorApplication.wantsToQuit += Quit;
        }

        static bool Quit()
        {
            ColorDispenser.OnQuit();

            EditorApplication.wantsToQuit -= Quit;
            return true;
        }
    }
}

