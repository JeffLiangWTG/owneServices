using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Enterprise.Customs.FR.TransportSvc.Utilities
{
	[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "The object is supposed to be disposed by a caller")]
	public static class ToolBox
	{
		public static string CleanBadlyEncodedString(string dirtyString) => dirtyString.Replace("Ã©", "é")
																					.Replace("Ã¨", "è")
																					.Replace("Ã ", "à")
																					.Replace("Ã¯", "ï")
																					.Replace("Ã´", "ô")
																					.Replace("Ã§", "ç")
																					.Replace("Ãª", "ê")
																					.Replace("Ã¹", "ù")
																					.Replace("Ã¦", "æ")
																					.Replace("Å", "œ")
																					.Replace("Ã«", "ë")
																					.Replace("Ã¼", "ü")
																					.Replace("Ã¢", "â")
																					.Replace("â¬", "€")
																					.Replace("Â©", "©")
																					.Replace("Â¤", "¤")
																					.Replace("Â°", "°");

		public static string GetDirectionFromMessageDocument(XmlDocument messageDocument)
		{
			var flux = GetElementTextByTagNameSafely(messageDocument, "Flux");
			switch (flux)
			{
				case Constants.MessageFlows.FluxImpType:
					return Constants.Directions.FluxImpValue;
				case Constants.MessageFlows.FluxExpType:
					return Constants.Directions.FluxExpValue;
			}

			return flux;
		}

		public static string StringXmlIndent(string xmlString)
		{
			if (!string.IsNullOrEmpty(xmlString))
			{
				var result = XElement.Parse("<a>" + xmlString + "</a>").ToString();
				result = result.Replace("<a>" + System.Environment.NewLine, string.Empty);
				result = result.Replace(System.Environment.NewLine + "</a>", string.Empty);

				return result.Trim();
			}
			else
			{
				return xmlString;
			}
		}

		public static bool IsNumericFromTryParse(string str)
		{
			double result = 0;
			return double.TryParse(str, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out result);
		}

		public static string StreamToString(Stream stream)
		{
			var result = string.Empty;

			if (stream != null)
			{
				stream.Position = 0;

				var reader = new StreamReader(stream, Encoding.UTF8);
				result = reader.ReadToEnd();
			}

			return result;
		}

		public static Stream FileToStream(string file)
		{
			Stream theFileStream = null;

			if (!File.Exists(file))
			{
				throw new FileNotFoundException(file);
			}

			using (var fileStream = new FileStream(file, FileMode.Open))
			{
				fileStream.CopyTo(theFileStream);
				return theFileStream;
			}
		}

		public static MemoryStream GenerateStreamFromString(string s)
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(s));
		}

		public static string XmlNoNamespace(string xmlString)
		{
			//Regex below finds strings that start with xmlns, may or may not have :and some text, then continue with =
			//and ", have a streach of text that does not contain quotes and end with ". similar, will happen to an attribute
			// that starts with xsi.
			var strXMLPattern = @" xmlns(:\w+)?=""([^""]+)""|xsi(:\w+)?=""([^""]+)""";
			return Regex.Replace(xmlString, strXMLPattern, string.Empty, RegexOptions.IgnoreCase);
		}

		public static string XmlDocToString(XmlDocument xmlDoc)
		{
			var sw = new StringWriter();

			if (xmlDoc != null)
			{
				var tx = new XmlTextWriter(sw);
				xmlDoc.WriteTo(tx);
			}

			return sw.ToString();
		}

		public static string GetElementTextByTagNameSafely(XmlDocument xmlDoc, string myTagName, string defaultValue = "")
		{
			var outputValue = defaultValue;

			if (xmlDoc != null)
			{
				var nodeList = xmlDoc.GetElementsByTagName(myTagName);

				if (nodeList != null && nodeList.Count > 0)
				{
					outputValue = nodeList[0].InnerText;
				}
			}

			return outputValue;
		}

		public static XmlElement GetElementSafely(XmlElement myElement)
		{
			var xmlReturnElement = myElement;
			return xmlReturnElement;
		}

		public static void AddElement(XmlNode xmlNode, XmlDocument xmlDoc, string elementname, string stringvalue)
		{
			if (xmlNode != null && xmlDoc != null)
			{
				var xmlNewElement = xmlDoc.CreateElement(elementname, xmlDoc.DocumentElement.NamespaceURI);
				xmlNewElement.InnerText = stringvalue;
				if (!string.IsNullOrEmpty(stringvalue))
				{
					xmlNode.AppendChild(xmlNewElement);
				}
			}
		}

		public static string GetAttributeValueSafely(XmlDocument xmlDoc, string myTagName, string attribute, string defaultValue = "")
		{
			var result = defaultValue;

			if (xmlDoc != null)
			{
				var nodeList = xmlDoc.GetElementsByTagName(myTagName);
				XmlNode nodeToGetAttributeValueFrom = null;
				if (nodeList != null && nodeList.Count > 0)
				{
					nodeToGetAttributeValueFrom = nodeList[0];
				}

				result = nodeToGetAttributeValueFrom?.Attributes?[attribute]?.Value ?? defaultValue;
			}

			return result;
		}

		public static string GetFirstChildNameByTagNameSafely(XmlDocument xmlDoc, string myTagName) => GetFirstNodeByTagNameSafely(xmlDoc, myTagName).ChildNodes[0].Name;

		public static XmlNode GetFirstNodeByTagNameSafely(XmlDocument xmlDoc, string myTagName)
		{
			XmlNode result = null;

			if (xmlDoc != null)
			{
				var nodeList = xmlDoc.GetElementsByTagName(myTagName);

				if (nodeList != null && nodeList.Count > 0)
				{
					result = nodeList[0];
				}
			}

			return result;
		}

		public static XmlDocument UpdateElementTextByTagName(XmlDocument xmlDoc, string myTagName, string newText)
		{
			if (xmlDoc != null)
			{
				var nodeList = xmlDoc.GetElementsByTagName(myTagName);
				if (nodeList != null && nodeList.Count > 0)
				{
					var node = nodeList[0];
					node.InnerText = newText;
				}
			}
			return xmlDoc;
		}

		public static string UpdateTP5NameSpaces(string message, string messageType)
		{
			message = message.Replace($"<{messageType}", $@"<{messageType} xmlns=""{Constants.DeltaTPhase5NameSpace}""");
			foreach (var tag in Constants.DeltaTPhase5TagsNeedingNameSpace)
			{
				message = message.Replace($"<{tag}>", $@"<{tag} xmlns="""">");
			}
			message = message.Replace("<Header xmlns=\"\"><SenderID>", "<Header><SenderID>");

			return message;
		}
	}
}
