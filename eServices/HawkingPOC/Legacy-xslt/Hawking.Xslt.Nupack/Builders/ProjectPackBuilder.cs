using System;
using System.IO;
using System.Linq;
using System.Text;
using Unity;
using Hawking.Unity;
using Hawking.Xslt.Compiler;
using Hawking.Xslt.Compiler.Helper;
using Hawking.Xslt.ExtensionObjects.ExtensionObjectsConfig;
using Hawking.Xslt.Nupack.Builders;
using System.Collections.Generic;
using Hawking.Xslt.Nupack.Templates;

namespace Hawking.Xslt.Nupack
{
    public class ProjectPackBuilder
    {
        const string ProjectNameRemovePrefix = "CargoWise.eHub.Products.";
        const string XslFileExtension = "*.xsl";
        const string nuspecFileExtension = "nuspec";
        const string csprojFileExtension = "csproj";
        readonly string outputDir;

        public ProjectPackBuilder(string outputDir)
        {
            this.outputDir = outputDir;
        }

        public void PackProject(string projectFilePath)
        {
            if (!File.Exists(projectFilePath))
            {
                return;
            }

            var inputDir = Path.GetDirectoryName(projectFilePath);
            var outputPath = Path.Combine(outputDir, inputDir.Substring(Path.GetPathRoot(inputDir).Length));
            var xslFiles = Directory.GetFiles(inputDir, XslFileExtension, SearchOption.AllDirectories);
            if (!xslFiles.Any())
            {
                return;
            }

            Directory.CreateDirectory(outputPath);

            var projectName = Path.GetFileNameWithoutExtension(projectFilePath).Substring(ProjectNameRemovePrefix.Length);
            SaveNuspecFile(projectName, outputPath);

            var embeddedResourceFiles = new List<string>();
            foreach (var xslFile in xslFiles)
            {
                if (!File.Exists(xslFile))
                {
                    continue;
                }

                var bizTalkMapFilePath = outputPath;
                var subDir = Path.GetDirectoryName(xslFile).Substring(inputDir.Length);
                if (!string.IsNullOrEmpty(subDir))
                {
                    subDir = subDir.TrimStart('/', '\\');
                    bizTalkMapFilePath = Path.Combine(bizTalkMapFilePath, subDir);
                    if(!Directory.Exists(bizTalkMapFilePath))
                    {
                        Directory.CreateDirectory(bizTalkMapFilePath);
                    }
                }

                var path = Path.GetDirectoryName(xslFile);
                var fileName = Path.GetFileNameWithoutExtension(xslFile);
                var extxmlFile = Path.Combine(path, $"{fileName}_extxml.xml");
                var folderStructure = path.Substring(Path.GetPathRoot(path).Length);

                folderStructure = folderStructure.TrimStart(new char[] { '\\', '/' });
                var namespaceName = folderStructure.Replace('\\', '.');
                if (namespaceName.EndsWith(fileName))
                {
                    var length = namespaceName.Length - fileName.Length;
                    namespaceName = namespaceName.Substring(0, length);
                    namespaceName = namespaceName.TrimEnd('.');
                }

                if (!string.IsNullOrEmpty(subDir))
                {
                  //  namespaceName += string.Join('.', subDir.Split('\\', '/'));
                }

                var outputXslFilePath = Path.Combine(bizTalkMapFilePath, $"{fileName}.xsl");
                var outputDllFilePath = Path.Combine(bizTalkMapFilePath, $"{fileName}.dll");
                var outputPdbFilePath = Path.Combine(bizTalkMapFilePath, $"{fileName}.pdb");
                var outputCsharplFilePath = Path.Combine(bizTalkMapFilePath, $"{fileName}.cs");
                var outputExtXmlFilePath = Path.Combine(bizTalkMapFilePath, Path.GetFileName(extxmlFile));

                Console.WriteLine($"namespace {namespaceName}; class {fileName}");

                // Add xsl file to the embedded resource files
                var embeddedXslFilePath = Path.GetFileName(xslFile);
                if (!string.IsNullOrEmpty(subDir))
                {
                    embeddedXslFilePath = Path.Combine(subDir, embeddedXslFilePath);
                }
                embeddedResourceFiles.Add(embeddedXslFilePath);

                using (var reader = new StreamReader(xslFile))
                {
                    var xslContent = reader.ReadToEnd();
                    var compiler = new CsharpScriptsCompiler();
                    var xdoc = XslScriptHelper.ExtractCsharpScripts(
                        xslContent,
                        out var userCsharpNamespaceName,
                        out var userCSharpScripts,
                        out var xslNamespaces);

                    // the xsl file
                    xdoc.Save(outputXslFilePath);

                    // the ext_xml file
                    // add the extxml file to the embedded resource files
                    var extxmlFilePath = string.Empty;
                    if (File.Exists(extxmlFile))
                    {
                        File.Copy(extxmlFile, outputExtXmlFilePath, true);
                        extxmlFilePath = Path.GetFileName(outputExtXmlFilePath);

                        if (!string.IsNullOrEmpty(subDir))
                        {
                            extxmlFilePath = Path.Combine(subDir, extxmlFilePath);
                        }

                        embeddedResourceFiles.Add(extxmlFilePath);
                    }

                    var csharpCode = TemplateHelper.BuildUserCSharpClass(
                        projectName,
                        subDir,
                        fileName,
                        userCsharpNamespaceName,
                        userCSharpScripts,
                        outputXslFilePath,
                        extxmlFilePath);

                    File.WriteAllText(outputCsharplFilePath, csharpCode);
                }
            }

            // project file
            SaveProjectFile(projectName, outputPath, embeddedResourceFiles);
        }

        void SaveNuspecFile(string projectName, string outputPath)
        {
            var nuspecId = projectName;
            var nuspecFileName = Path.ChangeExtension(projectName, nuspecFileExtension);
            var nuspecFilePath = Path.Combine(outputPath, nuspecFileName);
            Console.WriteLine($"Packing project '{nuspecFileName}'");

            var nuspecFileInfo = DependencyFactory.Container.Resolve<INuspecInfo>();
            nuspecFileInfo.Id = nuspecId;
            nuspecFileInfo.Title = nuspecId;
            nuspecFileInfo.Description = nuspecId;

            nuspecFileInfo.Save(nuspecFilePath);
        }

        void SaveProjectFile(string projectFileName, string outputPath, IList<string> embeddedResourceFiles)
        {
            var projectFilePath = Path.Combine(outputPath, projectFileName + "." + csprojFileExtension);
            File.WriteAllText(projectFilePath, TemplateHelper.BuildCSharpProject(embeddedResourceFiles));
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
