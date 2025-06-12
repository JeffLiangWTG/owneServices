using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace CargoWise.eHub.Products.Dakosy.BT.Transforms.Helper
{
	public class ListHelper
	{
		public IEnumerable<string> SplitTextToList(string stringToSplit, int maxLineLength)
		{
			string[] words = stringToSplit.Split(' ');
			StringBuilder line = new StringBuilder();

			foreach (string word in words)
			{
				if (word.Length + line.Length <= maxLineLength)
				{
					line.Append(word + " ");
				}
				else
				{
					if (line.Length > 0)
					{
						yield return line.ToString().Trim();
						line.Clear();
					}

					string overflow = word;
					while (overflow.Length > maxLineLength)
					{
						yield return overflow.Substring(0, maxLineLength);
						overflow = overflow.Substring(maxLineLength);
					}

					line.Append(overflow + " ");
				}
			}
			yield return line.ToString().Trim();
		}

		public XPathNodeIterator ConvertToXMLNodes(string elementName, string inputText, int maxLineLength)
		{
			if (string.IsNullOrEmpty(inputText))
			{
				return null;
			}

			XmlDocument doc = new XmlDocument();
			var elementCollection = doc.CreateElement(elementName + "Collection");
			doc.AppendChild(elementCollection);

			var valueList = SplitTextToList(inputText, maxLineLength);

			if (valueList.Any())
			{
				foreach (var item in valueList)
				{
					if (!string.IsNullOrEmpty(item))
					{
						var node = doc.CreateElement(elementName);
						node.InnerText = item;
						elementCollection.AppendChild(node);
					}
				}
			}
			return doc.DocumentElement?.CreateNavigator().Select(elementName);
		}
	}
}