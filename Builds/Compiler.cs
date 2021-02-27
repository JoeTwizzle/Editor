using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Basic.Reference.Assemblies;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.Json;
using McMaster.NETCore.Plugins;

namespace GameEditor.Builds
{
    public static class Compiler
    {
        public static PluginLoader GameLoader;
        static string Directory = "Test/Testing";
        static string ProjectName = "myapp";

        public static void CreateNewProject()
        {
            Process.Start("dotnet", $@"new sln --name {ProjectName} --output {Directory}").WaitForExit();
            Process.Start("dotnet", $@"new classlib --name {ProjectName} --output {Directory}\{ProjectName}").WaitForExit();
            //Process.Start("dotnet", @"new classlib -o mylib1");
            //Process.Start("dotnet", @"new classlib -o mylib2");
            Process.Start("dotnet", $@"sln {Directory}\{ProjectName}.sln add {Directory}\{ProjectName}\{ProjectName}.csproj").WaitForExit();
            //Process.Start("dotnet", @"sln mysolution.sln add mylib1\mylib1.csproj --solution-folder mylibs");
            //Process.Start("dotnet", @"sln mysolution.sln add mylib2\mylib2.csproj --solution-folder mylibs");
            //Process.Start("dotnet", $@"new sln list -h");

            Process.Start("dotnet", $@"add {Directory}\{ProjectName}\{ProjectName}.csproj package RasterDraw --source {Environment.CurrentDirectory}\EnginePackages").WaitForExit();
            //Process.Start("dotnet", "new classlib --name TestProj --output Test/Testing");
        }
        public static void BuildProject()
        {
            Process.Start("dotnet", $@"build {Directory}\{ProjectName}.sln --configuration Release --output {Environment.CurrentDirectory}\Libraries").WaitForExit();
        }

        public static void LoadProject()
        {
           
            if (GameLoader != null)
            {
                GameLoader.Dispose();
            }

            var sharedTypes = Assembly.GetAssembly(typeof(RasterDraw.Core.Scripting.GameObject))!.GetTypes();
            GameLoader = PluginLoader.CreateFromAssemblyFile(assemblyFile: $@"{Environment.CurrentDirectory}\Libraries\{ProjectName}.dll", sharedTypes: sharedTypes, isUnloadable: true, configure: configure);
            GameLoader.Reloaded += GameLoader_Reloaded;
        }

        private static void GameLoader_Reloaded(object sender, PluginReloadedEventArgs eventArgs)
        {
            Console.WriteLine("Reloaded DLL");
        }

        static void configure(PluginConfig pluginConfig)
        {
            pluginConfig.EnableHotReload = true;
            pluginConfig.IsUnloadable = true;
        }

        //private static string GenerateDeps()
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true }))
        //        {
        //            writer.WriteStartObject();
        //            string name = ".NETCoreApp,Version=v5.0";
        //            writer.WriteStartObject("runtimeTarget");
        //            writer.WriteString("name", name);
        //            writer.WriteString("signature", "");
        //            writer.WriteEndObject();

        //            writer.WriteStartObject("compilationOptions");
        //            writer.WriteStartObject("targets");
        //            writer.WriteStartObject(name);

        //            writer.WriteEndObject();
        //            writer.WriteEndObject();
        //            writer.WriteEndObject();
        //        }

        //        return Encoding.UTF8.GetString(stream.ToArray());
        //    }
        //}

        //private static string GenerateRuntimeConfig()
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        using (var writer = new Utf8JsonWriter(
        //            stream,
        //            new JsonWriterOptions() { Indented = true }
        //        ))
        //        {
        //            writer.WriteStartObject();
        //            writer.WriteStartObject("runtimeOptions");
        //            writer.WriteString("tfm", "net5.0");
        //            writer.WriteStartObject("framework");
        //            writer.WriteString("name", "Microsoft.NETCore.App");
        //            writer.WriteString(
        //                "version",
        //                RuntimeInformation.FrameworkDescription.Replace(".NET ", "")
        //            );
        //            writer.WriteEndObject();
        //            writer.WriteEndObject();
        //            writer.WriteEndObject();
        //        }

        //        return Encoding.UTF8.GetString(stream.ToArray());
        //    }
        //}

        //    public static bool TryCompile(string sourceCode, Stream stream)
        //    {
        //        var result = GenerateCode(sourceCode).Emit(stream);

        //        if (!result.Success)
        //        {
        //            Console.WriteLine("Compilation done with error.");

        //            var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
        //            foreach (var diagnostic in failures)
        //            {
        //                Console.Error.WriteLine("[Error] {0}: {1}", diagnostic.Id, diagnostic.GetMessage());
        //            }
        //            return false;
        //        }
        //        var warnings = result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning);

        //        foreach (var diagnostic in warnings)
        //        {
        //            Console.Error.WriteLine("[Warning] {0}: {1}", diagnostic.Id, diagnostic.GetMessage());
        //        }
        //        Console.WriteLine("Compilation done without any error.");

        //        return true;

        //    }
        //    static List<PortableExecutableReference> metadataReferences = new List<PortableExecutableReference>();

        //    private static CSharpCompilation GenerateCode(string sourceCode)
        //    {

        //        var codeString = SourceText.From(sourceCode);
        //        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);

        //        var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

        //        metadataReferences.Clear();
        //        var assemblies = Runtime.GetLoadedAssemblies();
        //        for (int i = 0; i < assemblies.Length; i++)
        //        {
        //            metadataReferences.Add(PortableExecutableReference.CreateFromFile(assemblies[i].Location));
        //        }
        //        metadataReferences.Add(MetadataReference.CreateFromFile(typeof(EditorManager).Assembly.Location));
        //        var a = new PortableExecutableReference[]
        //        {
        //              PortableExecutableReference.CreateFromFile(typeof(DateTime).Assembly.Location),
        //              PortableExecutableReference.CreateFromFile(typeof(object).Assembly.Location),
        //              PortableExecutableReference.CreateFromFile(typeof(Console).Assembly.Location),
        //              PortableExecutableReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
        //              PortableExecutableReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
        //        };
        //        metadataReferences.AddRange(a);
        //        var references = metadataReferences.ToArray();
        //        var list = ReferenceAssemblies.Net50.ToList();
        //        list.AddRange(references);
        //        return CSharpCompilation.Create("Hello.dll",
        //            new[] { parsedSyntaxTree },
        //            references: list,
        //            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release, assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        //    }
    }
}