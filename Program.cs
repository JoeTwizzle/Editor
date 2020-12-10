using OpenTK.Windowing.Desktop;
using System;

namespace GameEditor
{
    class Program
    {
        static void Main()
        {
            var a = NativeWindowSettings.Default;
            a.APIVersion = new Version(4, 6);
            a.Flags |= OpenTK.Windowing.Common.ContextFlags.Debug;
            var ga = GameWindowSettings.Default;

            EditorGame g = new EditorGame(ga, a);
            g.Run();
        }
    }
}
