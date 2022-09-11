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
            ColorDispenser.OnInitializeOnLoadMethod();
        }

        static bool Quit()
        {
            ColorDispenser.OnQuit();
            EditorApplication.wantsToQuit -= Quit;
            return true;
        }
    }
}

