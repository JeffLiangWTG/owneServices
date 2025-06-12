using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Hawking.Xslt.Compiler;
using Hawking.Xslt.Compiler.Helper;
using Hawking.Xslt.ExtensionObjects.ExtensionObjectsConfig;

namespace Hawking.Xsltc
{
    class Program
    {
        const string XslFileExtension = "*.xsl";

        static void Main(string[] args)
        {
            var argsParser = new CommandLineParser.CommandLineParser();

            var commandlineArgs = new XsltCompilerArgs();
            try
            {
                argsParser.ExtractArgumentAttributes(commandlineArgs);
                argsParser.ParseCommandLine(args);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine("For usage:");
                Console.WriteLine(string.Join(Environment.NewLine, argsParser.ShowUsageCommands));
                Environment.Exit(0);
            }

            var targetDir = Path.GetFullPath(commandlineArgs.RootDirectory);
            var zipFileName = DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss");
            var outputDir = Path.Combine(Path.GetTempPath(), zipFileName);
            var xslFiles = Directory.GetFiles(targetDir, XslFileExtension, SearchOption.AllDirectories);
            if (!xslFiles.Any())
            {
                Console.WriteLine($"No {XslFileExtension} files found under directory '{commandlineArgs.RootDirectory}'");
                Environment.Exit(0);
            }

            foreach (var xslFile in xslFiles)
            {
                if (!File.Exists(xslFile))
                {
                    continue;
                }

                var path = Path.GetDirectoryName(xslFile);
                var fileName = Path.GetFileNameWithoutExtension(xslFile);
                var extxmlFile = Path.Combine(path, $"{fileName}_extxml.xml");

                var folderStructure = path.Substring(Path.GetPathRoot(path).Length);
                var namespaceName = folderStructure.Replace('\\', '.');

                if (namespaceName.EndsWith(fileName))
                {
                    var length = namespaceName.Length - fileName.Length;
                    namespaceName = namespaceName.Substring(0, length);
                    namespaceName = namespaceName.TrimEnd('.');
                }

                var outputPath = Path.Combine(outputDir, folderStructure);
                Directory.CreateDirectory(outputPath);

                var outputXslFilePath = Path.Combine(outputPath, $"{fileName}.xsl");
                var outputDllFilePath = Path.Combine(outputPath, $"{fileName}.dll");
                var outputPdbFilePath = Path.Combine(outputPath, $"{fileName}.pdb");
                var outputCsharplFilePath = Path.Combine(outputPath, $"{fileName}.cs");
                var outputExtXmlFilePath = Path.Combine(outputPath, Path.GetFileName(extxmlFile));

                Console.WriteLine($"namespace {namespaceName}; class {fileName}");
                using (var reader = new StreamReader(xslFile))
                {
                    var xslContent = reader.ReadToEnd();
                    var compiler = new CsharpScriptsCompiler();
                    var xdoc = XslScriptHelper.ExtractCsharpScripts(xslContent,
                        out var userCsharpNamespaceName,
                        out var userCSharpScripts,
                        out var xslNamespaces);

                    xdoc.Save(outputXslFilePath);

                    if (string.IsNullOrEmpty(userCSharpScripts))
                    {
                        if (File.Exists(extxmlFile))
                        {
                            File.Copy(extxmlFile, outputExtXmlFilePath);
                        }

                        continue;
                    }

                    using (var emit = compiler.CompileCsharpCode(ref userCSharpScripts, namespaceName, fileName))
                    {
                        if (emit.EmitSuccess)
                        {
                            emit.SeekOrigin();

                            using (var fileStream =
                                new FileStream(outputDllFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                emit.DllMemoryStream.WriteTo(fileStream);
                            }

                            using (var fileStream =
                                new FileStream(outputPdbFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                emit.PdbMemoryStream.WriteTo(fileStream);
                            }

                            if (File.Exists(extxmlFile))
                            {
                                var extxmlContent = File.ReadAllText(extxmlFile);

                                AddUserCsharpExtensionXml(ref extxmlContent, userCsharpNamespaceName, fileName, compiler.CompiledClassTypeFullName);
                                File.WriteAllText(outputExtXmlFilePath, extxmlContent);
                            }
                        }
                    }

                    File.WriteAllText(outputCsharplFilePath, userCSharpScripts);
                }
            }

            var zipFilePath = Path.Combine(Environment.CurrentDirectory, $"{zipFileName}.zip");
            ZipFile.CreateFromDirectory(outputDir, zipFilePath);
        }

        static void AddUserCsharpExtensionXml(ref string extxmlContent, string userCsharpNamespace, string assemblyName, string className)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(extxmlContent)))
            {
                var extXml = new ExtensionXml(stream);
                if (!extXml.ContainsNamespace(userCsharpNamespace))
                {
                    extXml.AddExtensionObject(userCsharpNamespace, assemblyName, className);
                    extxmlContent = extXml.ToString();
                }
            }
        }
    }
}
