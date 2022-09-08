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

            EditorApplication.wantsToQuit -= Quit;
            EditorApplication.wantsToQuit += Quit;
        }

        static bool Quit()
        {
            DebugxStaticData.OnEditorQuit();

            return true;
        }
    }
}

