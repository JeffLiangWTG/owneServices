using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Hawking.UnitTest.Tools.Xml;
using Hawking.Xslt.Compiler.Helper;
using Xunit;

namespace Hawking.Xslt.Compiler.Tests.ScriptsCompiler
{
    public class CsharpScriptsCompilerFixtures
    {
        [Fact]
        public void TestCsharpScriptsCompilation_ReturnsAssemblyBytesInMemoryStream()
        {
            var compiler = new CsharpScriptsCompiler();
            var xslFiles = Directory.GetFiles("xsl-files", "*.xsl", SearchOption.TopDirectoryOnly);

            foreach (var xslFile in xslFiles)
            {
                if (File.Exists(xslFile))
                {
                    var document = XslScriptHelper.ExtractCsharpScripts(File.ReadAllText(xslFile),
                        out var prefixNamespace,
                        out var csharpScripts,
                        out _);

                    if (!string.IsNullOrEmpty(csharpScripts))
                    {
                        Assert.NotNull(document);
                        Assert.False(string.IsNullOrEmpty(prefixNamespace));
                        Assert.NotNull(compiler.CompileCsharpCode(ref csharpScripts));
                    }
                }
            }
        }

        [Fact]
        public void TestCsharpRuntimeCompilation_CreateInstance_InvokeMethod()
        {
            var xslFile = Directory.GetFiles("xsl-files", "Circumference.xsl", SearchOption.TopDirectoryOnly).First();
            var xslContent = File.ReadAllText(xslFile);
            var compiler = new CsharpScriptsCompiler();
            var document = XslScriptHelper.ExtractCsharpScripts(xslContent,
                out var prefixNamespace,
                out var csharpScripts,
                out _);

            var xsltFile = Path.GetTempFileName();
            document.Save(xsltFile);

            Assert.False(string.IsNullOrEmpty(csharpScripts));
            Assert.False(string.IsNullOrEmpty(prefixNamespace));
            var emit = compiler.CompileCsharpCode(ref csharpScripts);
            Assert.True(emit.EmitSuccess);

            var assembly = Assembly.Load(emit.DllMemoryStream.ToArray());
            emit.Dispose();

            var className = CsharpScriptsCompiler.DefaultNamespace + "." + CsharpScriptsCompiler.DefaultClassName;
            var classInstance = assembly.CreateInstance(className);
            Assert.NotNull(classInstance);

            var type = assembly.GetType(className);
            var methodInfo = type.GetMethod("circumference");
            var circumference = methodInfo.Invoke(classInstance, new object[] { 0.5 });
            Assert.Equal(3.14, circumference);
        }

        [Fact]
        public void TestXsltExecutionWithCompiledScripts_ProduceExpectedResult()
        {
            #region xml data

            var xml = @"<?xml version='1.0'?>
<data>
  <circle>
    <radius>12</radius>
  </circle>
  <circle>
    <radius>37.5</radius>
  </circle>
</data>";

            var expectedOutput = @"<?xml version=""1.0""?>
<circles>
    <circle>
        <radius>12</radius>
        <circumference>75.36</circumference>
    </circle>
    <circle>
        <radius>37.5</radius>
        <circumference>235.5</circumference>
    </circle>
</circles>";

            #endregion

            var xslFile = Directory.GetFiles("xsl-files", "Circumference.xsl", SearchOption.TopDirectoryOnly).First();
            var xslContent = File.ReadAllText(xslFile);
            var compiler = new CsharpScriptsCompiler();
            var xdoc = XslScriptHelper.ExtractCsharpScripts(xslContent,
                out var prefixNamespace,
                out var csharpScripts,
                out _);

            var modifiedXslFile = Path.GetTempFileName();
            xdoc.Declaration = new XDeclaration("1.0", "utf8", null);
            xdoc.Save(modifiedXslFile);

            Assert.False(string.IsNullOrEmpty(csharpScripts));
            Assert.False(string.IsNullOrEmpty(prefixNamespace));
            var emit = compiler.CompileCsharpCode(ref csharpScripts);
            Assert.True(emit.EmitSuccess);

            var assembly = Assembly.Load(emit.DllMemoryStream.ToArray());
            emit.Dispose();

            var classInstance = assembly.CreateInstance(compiler.CompiledClassTypeFullName);
            Assert.NotNull(classInstance);

            var compiledTransform = new XslCompiledTransform();
            compiledTransform.Load(modifiedXslFile, new XsltSettings(true, true), new XmlUrlResolver());

            var xpath = new XPathDocument(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
            var xlstArgsList = new XsltArgumentList();
            xlstArgsList.RemoveExtensionObject(prefixNamespace);
            xlstArgsList.AddExtensionObject(prefixNamespace, classInstance);

            using (var expected = new MemoryStream(Encoding.UTF8.GetBytes(expectedOutput)))
            {
                using (var stream = new MemoryStream())
                {
                    using (var xmlWriter = XmlHelper.CreateFormattedXmlWriter(stream))
                    {
                        compiledTransform.Transform(xpath, xlstArgsList, xmlWriter);

                        var diffResult = XmlDiffTool.Execute(stream, expected);
                        Assert.True(diffResult.Success);
                    }
                }
            }
        }
    }
}
