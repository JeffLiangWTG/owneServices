using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Hawking.Xslt.ExtensionObjects.ExtensionObjectsConfig
{
    public class ExtensionXmlConfigProvider : IExtensionXmlParser
    {
        public ExtensionXmlConfigProvider()
        {
            ExtensionObjects = new ConcurrentDictionary<string, object>();
        }

        public IDictionary<string, object> ExtensionObjects { get; }

        public IDictionary<string, object> ParseExtensionXml(Stream extensionXmlStream)
        {
            if (extensionXmlStream == null)
            {
                throw new FileNotFoundException("The provided extension xml file does not exist");
            }

            ExtensionObjects.Clear();

            var extensionXml = new ExtensionXml(extensionXmlStream);
            foreach (var element in extensionXml.ExtensionObjectElements)
            {
                var extensionNamespace = element.Attribute(ExtensionXml.Attribute.Namespace).Value;
                var extensionAssembly = element.Attribute(ExtensionXml.Attribute.AssemblyName).Value;
                var extensionClass = element.Attribute(ExtensionXml.Attribute.ClassName).Value;

                var assemblyQualifiedName = $"{extensionClass}, {extensionAssembly}";
                var extensionObject = Activator.CreateInstance(Type.GetType(assemblyQualifiedName));

                ExtensionObjects.Add(extensionNamespace, extensionObject);
            }

            return ExtensionObjects;
        }

        public object GetExtensionObjectForNamespace(string namespaceName)
        {
            return ExtensionObjects.TryGetValue(namespaceName, out var value) ? value : null;
        }
    }
}
