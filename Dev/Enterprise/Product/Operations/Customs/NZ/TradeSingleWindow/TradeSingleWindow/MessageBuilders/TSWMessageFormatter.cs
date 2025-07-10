using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public static class TSWMessageFormatter
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

		public static string FormatAcceptableFileNameForNZC(ZString fileName)
		{
			string result = fileName;
			ZString fileExtension = Path.GetExtension(fileName);
			if (!fileExtension.IsEmpty)
			{
				var name = RemoveFileExtension(fileName, fileExtension);
				name = name.Replace(".", "_");
				result = name + fileExtension;
			}

			result = Regex.Replace(result, "[~#£≠β≥]", ""); // Also remove from the stored attachment file name any invalid characters that may be in the file name that are removed from the file name imbedded in the generated xml string
			result = Regex.Replace(result, @"[:%\[\]\(\)\{\}]", "_");
			result = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(result).Where(x => x > 31 && x != 127).ToArray());//remove control characters
			return result.Replace("?", "_");
		}

		static string RemoveFileExtension(string fullFileName, string fileExtension)
		{
			int extensionIndex = fullFileName.LastIndexOf(fileExtension, System.StringComparison.OrdinalIgnoreCase);
			return fullFileName.Remove(extensionIndex, fileExtension.Length);
		}
	}
}
