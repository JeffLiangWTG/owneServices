using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Hawking.Xslt.Nupack.Templates
{
    public static class TemplateHelper
    {
        const string ClassTemplateFilePath = "Templates.ClassTemplate.txt";
        const string ProjectTemplateFilePath = "Templates.ProjectTemplate.xml";

        const string VariableNamespaceName = "$namespaceName$";
        const string VariableClassName = "$className$";
        const string VariableUserCSharpNamespaceName = "$userCSharpNamespaceName$";
        const string VariableXslContentResourceFilePath = "$xslContentResourceFilePath$";
        const string VariableUserCSharpScripts = "$userCSharpScripts$";
        const string VariableExtxmlFilePath = "$extxmlContentResourceFilePath$";

        const string ItemGroupElementName = "ItemGroup";
        const string EmbeddedResourceElementName = "EmbeddedResource";

        static TemplateHelper()
        {
            var assembly = Assembly.GetExecutingAssembly();

            ClassTemplate = ReadEmbeddedResouceContent(assembly, ClassTemplateFilePath);
            ProjectTemplate = ReadEmbeddedResouceContent(assembly, ProjectTemplateFilePath);
        }

        static string ReadEmbeddedResouceContent(Assembly assembly, string filePath)
        {
            using (var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + filePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static string ClassTemplate { get; private set; }

        public static string ProjectTemplate { get; private set; }

        public static string BuildUserCSharpClass(
            string projectNamespaceName,
            string subDir,
            string className,
            string userCSharpNamespace,
            string userCSharpScripts,
            string xslContentFilePath,
            string extxmlContentFilePath)
        {
            var classNamespaceName = projectNamespaceName;
            var namespaceSuffix = string.Empty;
            if (!string.IsNullOrEmpty(subDir))
            {
                namespaceSuffix = string.Join('.', subDir.Split('\\', '/'));
            }

            if (!string.IsNullOrEmpty(namespaceSuffix))
            {
                classNamespaceName += "." + namespaceSuffix;
            }

            var stringBuilder = new StringBuilder();
            using (var stringReader = new StringReader(ClassTemplate))
            {
                while (stringReader.Peek() > 0)
                {
                    var line = stringReader.ReadLine();
                    if (line.Contains(VariableNamespaceName))
                    {
                        stringBuilder.AppendLine(line.Replace(VariableNamespaceName, classNamespaceName));
                    }
                    else if (line.Contains(VariableClassName))
                    {
                        stringBuilder.AppendLine(line.Replace(VariableClassName, className));
                    }
                    else if (line.Contains(VariableUserCSharpNamespaceName))
                    {
                        stringBuilder.AppendLine(line.Replace(VariableUserCSharpNamespaceName, userCSharpNamespace ?? string.Empty));
                    }
                    else if (line.Contains(VariableXslContentResourceFilePath))
                    {
                        var xslResourceFilePath = Path.GetFileName(xslContentFilePath);
                        if (!string.IsNullOrEmpty(namespaceSuffix))
                        {
                            xslResourceFilePath = namespaceSuffix + "." + xslResourceFilePath;
                        }

                        stringBuilder.AppendLine(line.Replace(VariableXslContentResourceFilePath, xslResourceFilePath));
                    }
                    else if (line.Contains(VariableUserCSharpScripts))
                    {
                        if (!string.IsNullOrEmpty(userCSharpScripts))
                        {
                            stringBuilder.AppendLine(line.Replace(VariableUserCSharpScripts, userCSharpScripts));
                        }
                    }
                    else if(line.Contains(VariableExtxmlFilePath))
                    {
                        var extxmlFilePath = string.Empty;
                        if (!string.IsNullOrEmpty(extxmlContentFilePath))
                        {
                            extxmlFilePath = Path.GetFileName(xslContentFilePath);
                            if (!string.IsNullOrEmpty(namespaceSuffix))
                            {
                                extxmlFilePath = namespaceSuffix + "." + extxmlFilePath;
                            }
                        }
                        
                        stringBuilder.AppendLine(line.Replace(VariableExtxmlFilePath, extxmlFilePath));
                    }
                    else
                    {
                        stringBuilder.AppendLine(line);
                    }
                }
            }

            return stringBuilder.ToString();
        }

        public static string BuildCSharpProject(IList<string> xslFileNames)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ProjectTemplate)))
            {
                var document = XDocument.Load(stream);
                var embeddedResourceItems = document.Root.Elements().FirstOrDefault(
                    x =>
                    x.Name == ItemGroupElementName &&
                    x.Attributes().Any(a => a.Name == "Group") &&
                    x.Attribute("Group").Value == EmbeddedResourceElementName);

                if (embeddedResourceItems != null)
                {
                    embeddedResourceItems.Remove();
                }

                embeddedResourceItems = new XElement(ItemGroupElementName);
                document.Root.Add(embeddedResourceItems);

                foreach (var xslFileName in xslFileNames)
                {
                    var embeddedReource = new XElement(EmbeddedResourceElementName);
                    embeddedReource.SetAttributeValue("Include", xslFileName);
                    embeddedResourceItems.Add(embeddedReource);
                }

                using (var memoryStream = new MemoryStream())
                {
                    var settings = new XmlWriterSettings
                    {
                        CloseOutput = false,
                        Indent = true,
                        Encoding = new UTF8Encoding(false),
                        OmitXmlDeclaration = true
                    };

                    using (var writer = XmlWriter.Create(memoryStream, settings))
                    {
                        document.WriteTo(writer);
                        writer.Flush();
                        memoryStream.Position = 0;
                    }

                    using (var streamReader = new StreamReader(memoryStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
}
