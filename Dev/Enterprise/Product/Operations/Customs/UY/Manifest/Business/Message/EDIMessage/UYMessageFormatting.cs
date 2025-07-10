using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public static class UYMessageFormatting
	{
		public static ZString GetMessageTextWithCorrectFormat(string messageText)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(messageText);

			return FormatWithXMLRepresentation(RemoveSignature(doc.InnerText));
		}

		public static ZString FormatWithXMLRepresentation(string messageText)
		{
			ZString result = "";
			using (var mStream = new MemoryStream())
			{
				using (var writer = new XmlTextWriter(mStream, Encoding.Unicode))
				{
					var document = new XmlDocument();
					try
					{
						document.LoadXml(messageText);
						writer.Formatting = Formatting.Indented;
						document.WriteContentTo(writer);
						writer.Flush();
						mStream.Flush();
						mStream.Position = 0;
						using (var sReader = new StreamReader(mStream))
						{
							result = sReader.ReadToEnd();
						}
					}
					catch (XmlException)
					{
						result = messageText;
					}
					mStream.Close();
				}
			}
			return result;
		}

		static ZString RemoveSignature(ZString bodyText)
		{
			ZString newXML = bodyText;

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(bodyText);
			XmlNode node = doc.SelectSingleNode(UYMessageConstants.Xpath);
			if (node != null)
			{
				XmlNode parent = node.ParentNode;
				parent.RemoveChild(node);
				newXML = doc.OuterXml;
			}

			return newXML;
		}
	}
}
