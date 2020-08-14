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
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using System.Data;");
            builder.AppendLine("using System.Drawing;");
            builder.AppendLine("using System.IO;");
            builder.AppendLine("using System.Linq;");
            builder.AppendLine("using System.Reflection;");
            builder.AppendLine("using System.Runtime.InteropServices;");
            builder.AppendLine("using System.Text;");
            builder.AppendLine("using System.Text.RegularExpressions;");
            builder.AppendLine("using System.Threading;");
            builder.AppendLine("using System.Threading.Tasks;");
            builder.AppendLine("using System.Windows.Forms;");
            builder.AppendLine("using System.Xml;");
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
                        builder.AppendLine(Indent(3) + line.Replace(@"\", @"\\"));
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
            parameter.ReferencedAssemblies.Add("System.dll");
            parameter.ReferencedAssemblies.Add("System.Data.dll");
            parameter.ReferencedAssemblies.Add("System.Drawing.dll");
            parameter.ReferencedAssemblies.Add("System.Linq.dll");
            parameter.ReferencedAssemblies.Add("System.Windows.Forms.dll");

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
