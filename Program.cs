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
            ga.IsMultiThreaded = false;
            BaseGame g = new BaseGame(ga, a);
            g.Run();
        }
    }
}
