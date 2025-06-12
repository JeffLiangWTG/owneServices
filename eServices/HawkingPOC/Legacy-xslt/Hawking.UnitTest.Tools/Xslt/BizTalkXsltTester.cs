using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Hawking.UnitTest.Tools.Resource;
using Hawking.UnitTest.Tools.Xml;
using Hawking.Xslt.Compiler;
using Hawking.Xslt.Compiler.Helper;

namespace Hawking.UnitTest.Tools.Xslt
{
    public static class BizTalkXsltTester
    {
        public static XmlDiffResult Execute(
            Assembly resourceAssembly,
            string xslPath,
            string inputPath,
            string expectedOutputPath,
            IDictionary<string, object> extensionObjects)
        {
            if (resourceAssembly == null)
            {
                throw new ArgumentNullException(nameof(resourceAssembly));
            }

            using (var stream = ResourceHelper.GetEmbeddedResource(resourceAssembly, xslPath))
            using (var reader = new StreamReader(stream))
            {
                var xslContent = reader.ReadToEnd();
                var compiler = new CsharpScriptsCompiler();
                var xdoc = XslScriptHelper.ExtractCsharpScripts(xslContent,
                    out var userCSharpNamespace,
                    out var userCSharpScripts,
                    out _);

                using (var xslStream = new MemoryStream())
                {
                    xdoc.Save(xslStream);

                    using (var emit = compiler.CompileCsharpCode(ref userCSharpScripts))
                    {
                        var assembly = Assembly.Load(emit.DllMemoryStream.ToArray());
                        emit.Dispose();
                        var userCSharpInstance = assembly.CreateInstance(compiler.CompiledClassTypeFullName);

                        extensionObjects.Add(userCSharpNamespace, userCSharpInstance);

                        using (var inputStream = ResourceHelper.GetEmbeddedResource(resourceAssembly, inputPath))
                        using (var expectedOutputStream =
                            ResourceHelper.GetEmbeddedResource(resourceAssembly, expectedOutputPath))
                        {
                            return XsltTester.Execute(xslStream, inputStream, expectedOutputStream, extensionObjects);
                        }
                    }
                }
            }
        }

        public static IDictionary<string, object> ParseExtensionXml(Stream extensionXmlFileStream)
        {
            return ParseExtensionXml(new StreamReader(extensionXmlFileStream).ReadToEnd());
        }

        public static IDictionary<string, object> ParseExtensionXml(string extensionXmlFilePath)
        {
            var extensionObjects = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(extensionXmlFilePath))
            {
                return extensionObjects;
            }

            var document = new XmlDocument();
            document.Load(extensionXmlFilePath);

            foreach (XmlNode node in document.SelectNodes("/ExtensionObjects/ExtensionObject"))
            {
                var extensionNamespace = node.Attributes["Namespace"].Value;
                var extensionAssembly = node.Attributes["AssemblyName"].Value;
                var extensionClass = node.Attributes["ClassName"].Value;
                var assemblyQualifiedName = $"{extensionClass}, {extensionAssembly}";
                var extensionObject = Activator.CreateInstance(Type.GetType(assemblyQualifiedName));

                extensionObjects.Add(extensionNamespace, extensionObject);
            }

            return extensionObjects;
        }
    }
}
