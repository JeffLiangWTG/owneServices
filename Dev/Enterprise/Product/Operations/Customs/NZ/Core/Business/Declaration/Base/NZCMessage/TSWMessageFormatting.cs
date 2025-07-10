using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public static class TSWMessageFormatting
	{
		public static ZString FormatWithXMLRepresentation(string messageText)
		{
			ZString result = "";
			MemoryStream mStream = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
			XmlDocument document = new XmlDocument();

			try
			{
				document.LoadXml(messageText);
				writer.Formatting = Formatting.Indented;
				document.WriteContentTo(writer);
				writer.Flush();
				mStream.Flush();
				mStream.Position = 0;
				StreamReader sReader = new StreamReader(mStream);
				result = sReader.ReadToEnd();
			}
			catch (XmlException)
			{
				result = messageText;
			}

			mStream.Close();
			return result;
		}
	}
}
