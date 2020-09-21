using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Stroke
{
    public static class Script
    {
        private static Type Scripts;
        private static object Instance;
        private static Dictionary<string, string> Functions = new Dictionary<string, string>();

        private static string Indent(int level)
        {
            return (new String(' ', level * 4));
        }

        public static string GenerateSource()
        {
            StringBuilder builder = new StringBuilder();
            string[] namespaces = { "System", "System.Collections", "System.Collections.Concurrent", "System.Collections.Generic", "System.Collections.ObjectModel", "System.Collections.Specialized", "System.Diagnostics", "System.Drawing", "System.Drawing.Imaging", "System.Drawing.Text", "System.IO", "System.IO.Compression", "System.IO.Pipes", "System.IO.Ports", "System.Linq", "System.Linq.Expressions", "System.Media", "System.Net", "System.Net.Http", "System.Net.Http.Headers", "System.Net.Security", "System.Net.Sockets", "System.Numerics", "System.Reflection", "System.Security", "System.Security.Authentication", "System.Security.Cryptography", "System.Text", "System.Text.RegularExpressions", "System.Threading", "System.Threading.Tasks", "System.Timers", "System.Web", "System.Windows.Forms", "System.Xml", "System.Xml.Linq", "System.Xml.Schema", "System.Xml.Serialization" };
            foreach (string name in namespaces)
            {
                builder.AppendLine($"using {name};");
            }
            builder.AppendLine();
            builder.AppendLine("namespace Stroke");
            builder.AppendLine("{");
            builder.AppendLine();
            builder.AppendLine(Indent(1) + "public class Scripts");
            builder.AppendLine(Indent(1) + "{");
            builder.AppendLine();

            uint index = 0;
            foreach (ActionPackage package in Settings.ActionPackages)
            {
                foreach (Action action in package.Actions)
                {
                    if (Functions.ContainsKey($"{package.Name}.{action.Name}"))
                    {
                        continue;
                    }

                    Functions.Add($"{package.Name}.{action.Name}", "Function_" + index++);
                    builder.AppendLine(Indent(2) + $" // {package.Name}.{action.Name}");
                    builder.AppendLine(Indent(2) + $"static public void {Functions[$"{package.Name}.{action.Name}"]}()");
                    builder.AppendLine(Indent(2) + "{");
                    foreach (string line in action.Code.Replace("\r", "").Split('\n'))
                    {
                        builder.AppendLine(Indent(3) + line);
                    }
                    builder.AppendLine(Indent(2) + "}");
                    builder.AppendLine();
                }
            }

            builder.AppendLine(Indent(1) + "}");
            builder.AppendLine("}");
            Console.Write(builder.ToString());
            return builder.ToString();
        }

        public static void CompileScript()
        {
            CompilerParameters parameter = new CompilerParameters();
            parameter.GenerateExecutable = false;
            parameter.GenerateInMemory = true;
            parameter.TreatWarningsAsErrors = false;

            string[] assemblies = { "System.dll", "System.Core.dll", "System.Drawing.dll", "System.Net.Http.dll", "System.Web.dll", "System.Windows.Forms.dll", "System.Xml.dll", "System.Xml.Linq.dll" };
            foreach (string assembly in assemblies)
            {
                parameter.ReferencedAssemblies.Add(assembly);
            }

            FileInfo[] files = (new DirectoryInfo(Application.StartupPath)).GetFiles("*.dll");
            for (int i = 0; i < files.Length; i++)
            {
                parameter.ReferencedAssemblies.Add(files[i].FullName);
            }

            CompilerResults results = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(parameter, GenerateSource());

            if (results.Errors.HasErrors)
            {
                List<string> errors = new List<string>();
                foreach (CompilerError error in results.Errors)
                {
                    if (!errors.Contains(error.ErrorText))
                    {
                        errors.Add(error.ErrorText);
                    }
                }
                MessageBox.Show(String.Join("\n", errors));
                Environment.Exit(0);
            }

            Instance = results.CompiledAssembly.CreateInstance("Stroke.Scripts");
            Scripts = results.CompiledAssembly.GetType("Stroke.Scripts");
        }

        public static void RunScript(string name)
        {
            try
            {
                Scripts.GetMethod(Functions[name]).Invoke(Instance, null);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

    }
}
