using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Hawking.Xslt.Compiler.Helper
{
    public static class XslScriptHelper
    {
        public const string LanguageAttributeName = "language";
        public const string ImplementsPrefixAttributeName = "implements-prefix";
        public static readonly XNamespace msxslNamespace ="urn:schemas-microsoft-com:xslt";

        static readonly string[] ScriptLanguages = { "c#", "csharp" };

        public static XDocument ExtractCsharpScripts(
            string xslFileContent,
            out string userCSharpNamespace,
            out string userCSharpScripts,
            out IEnumerable<string> namespaces)
        {
            userCSharpNamespace = string.Empty;
            userCSharpScripts = string.Empty;
            namespaces = new string[] { };

            var document = XDocument.Parse(xslFileContent);
            if (document.Root == null)
            {
                return document;
            }

            var namespacesDictionary = document.Root.Attributes().Where(a => a.IsNamespaceDeclaration)
                .GroupBy(
                    a => a.Name.Namespace == XNamespace.None ? string.Empty : a.Name.LocalName, 
                    a => XNamespace.Get(a.Value))
                .ToDictionary(x => x.Key, x => x.First());

            var scriptElement = document.Descendants().FirstOrDefault(x => x.Name.Namespace == msxslNamespace);
            if (scriptElement == null)
            {
                return document;
            }

            var language = scriptElement.Attribute(LanguageAttributeName);
            var prefix = scriptElement.Attribute(ImplementsPrefixAttributeName);
            if (language == null || prefix == null || !ScriptLanguages.Contains(language.Value.ToLower()))
            {
                return document;
            }

            namespaces = namespacesDictionary.Values.Select(x => x.ToString()).ToArray();
            userCSharpNamespace = namespacesDictionary.FirstOrDefault(x => x.Key == prefix.Value).Value.ToString();
            userCSharpScripts = scriptElement.Value.Trim();
            scriptElement.Remove();

            return document;
        }
    }
}
