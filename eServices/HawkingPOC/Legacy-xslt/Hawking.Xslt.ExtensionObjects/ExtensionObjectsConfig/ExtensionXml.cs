using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Hawking.Xslt.ExtensionObjects.ExtensionObjectsConfig
{
    public class ExtensionXml
    {
        const string XPathExtensionObject = "/ExtensionObjects/ExtensionObject";

        public class Attribute
        {
            public const string AssemblyName = "AssemblyName";
            public const string Namespace = "Namespace";
            public const string ClassName = "ClassName";
        }

        public ExtensionXml(Stream extensionXmlStream)
        {
            ExtXmlDocument = XDocument.Load(extensionXmlStream);
            ExtensionObjectElements = ExtXmlDocument.XPathSelectElements(XPathExtensionObject);
        }

        public XDocument ExtXmlDocument { get; }
        public IEnumerable<XElement> ExtensionObjectElements { get; }

        public bool ContainsNamespace(string namespaceName)
        {
            if (ExtensionObjectElements != null)
            {
                return ExtensionObjectElements.Any(x => x.Attribute(Attribute.Namespace).Value == namespaceName);
            }

            return false;
        }

        public bool AddExtensionObject(string namespaceName, string assemblyName, string className)
        {
            if (ContainsNamespace(namespaceName))
            {
                return false;
            }

            var element = new XElement("ExtensionObject");
            element.SetAttributeValue(Attribute.Namespace, namespaceName);
            element.SetAttributeValue(Attribute.AssemblyName, assemblyName);
            element.SetAttributeValue(Attribute.ClassName, className);

            ExtXmlDocument.Root.Add(element);

            return true;
        }

        public override string ToString()
        {
            return ExtXmlDocument.ToString();
        }
    }
}
