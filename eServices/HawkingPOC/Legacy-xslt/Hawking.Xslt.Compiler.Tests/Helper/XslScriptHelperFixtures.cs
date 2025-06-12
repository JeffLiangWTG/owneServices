using System;
using System.IO;
using System.Linq;
using Hawking.Xslt.Compiler.Helper;
using Xunit;

namespace Hawking.Xslt.Compiler.Tests.Helper
{
    public class XslScriptHelperFixtures
    {
        [Fact]
        public void TestExtractCsharpScripts()
        {
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
                        Assert.False(string.IsNullOrEmpty(prefixNamespace));
                        Assert.True(document.Descendants().All(x => x.Name.Namespace != XslScriptHelper.msxslNamespace));
                    }
                }
            }
        }
    }
}
