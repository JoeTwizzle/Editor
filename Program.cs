using OpenTK.Windowing.Desktop;
using System;

namespace Editor
{
    class Program
    {
        static void Main()
        {
            var a = NativeWindowSettings.Default;
            a.APIVersion = new Version(4, 6);
            var ga = GameWindowSettings.Default;
            ga.IsMultiThreaded = false;
            TestGame g = new TestGame(ga, a);
            g.Run();
        }
    }
}
