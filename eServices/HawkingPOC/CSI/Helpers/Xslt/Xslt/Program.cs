using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Xslt
{
    class Program
    {
        static void Main(string[] args)
        {
            TestXsltc2();
        }

            private static void TestXsltc2()
        {
            var xml = @"<?xml version='1.0'?>
<data>
  <circle>
    <radius>12</radius>
  </circle>
  <circle>
    <radius>37.5</radius>
  </circle>
</data>";

            var xsl = @"
<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform""
  xmlns:msxsl=""urn:schemas-microsoft-com:xslt""
  xmlns:helper=""http://schemas.microsoft.com/BizTalk/2003/userCSharp"">
  <xsl:template match=""data"">
    <circles>
      <xsl:for-each select=""circle"">
        <circle>
          <xsl:copy-of select=""node()""/>
          <circumference>
            <xsl:value-of select=""helper:circumference(radius)""/>
          </circumference>
        </circle>
      </xsl:for-each>
    </circles>
  </xsl:template>
</xsl:stylesheet>";

 
            var script = new XPathDocument(new StringReader(xsl));
            var xslt = new XslCompiledTransform();
            xslt.Load(script, new XsltSettings { EnableScript = true }, new XmlUrlResolver());

            var doc = new XPathDocument(new StringReader(xml));
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true
            };
            var sb = new StringBuilder();
            var writer = XmlWriter.Create(sb, settings);

            var arguments = new XsltArgumentList();
            arguments.AddExtensionObject("http://schemas.microsoft.com/BizTalk/2003/userCSharp", new Helper.Helper());

            xslt.Transform(doc, arguments, writer);
            writer.Close();

            Console.WriteLine(sb.ToString());
        }
    }
}
