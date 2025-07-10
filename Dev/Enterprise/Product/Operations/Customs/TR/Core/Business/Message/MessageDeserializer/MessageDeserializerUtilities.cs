using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Enterprise.Customs.TR.Business
{
	public static class MessageDeserializerUtilities
	{
		public static XmlNodeList GetElementsByTagName(this XmlNode nodeToSearch, string elementName, string namespaceUrl = "")
		{
			XmlNodeList result = null;
			if (nodeToSearch is XmlElement element)
			{
				result = element.GetElementsByTagName(elementName, namespaceUrl);
			}
			else if (nodeToSearch is XmlDocument document)
			{
				result = document.GetElementsByTagName(elementName, namespaceUrl);
			}

			return result;
		}

		public static IEnumerable<XmlElement> GetElementsByPath(this XmlNode rootNode, (string elementName, string namespaceUrl)[] path)
		{
			var results = new List<XmlElement>();

			if (path.Length > 1)
			{
				var childNodes = rootNode.GetElementsByTagName(path[0].elementName, path[0].namespaceUrl).Cast<XmlElement>();
				foreach (var childNode in childNodes)
				{
					results.AddRange(childNode.GetElementsByPath(path.Skip(1).ToArray()));
				}
			}
			else if (path.Length == 1)
			{
				results.AddRange(rootNode.GetElementsByTagName(path[0].elementName, path[0].namespaceUrl).Cast<XmlElement>());
			}

			return results;
		}

		public static XmlElement GetElementByPath(this XmlNode rootNode, (string elementName, string namespaceUrl)[] path) => rootNode.GetElementsByPath(path).FirstOrDefault();

		public static string ToFormattedString(this XmlNode xmlNode)
		{
			var stringBuilder = new StringBuilder();
			using (var stringWriter = new StringWriter(stringBuilder))
			using (var xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented, IndentChar = '\t', Indentation = 1 })
			{
				xmlNode.WriteTo(xmlWriter);
			}

			return stringBuilder.ToString();
		}
	}
}
