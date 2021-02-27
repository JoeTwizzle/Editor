using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameEditor.Builds
{
    public static class Runtime
    {
        public static Assembly[] BaseAssemblies { get; } = AppDomain.CurrentDomain.GetAssemblies();
        public static Assembly[] GetLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).ToArray();
        }

        public static string[] GetSolutionAssemblies()
        {
            var assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            return assemblies;
        }
    }
}
