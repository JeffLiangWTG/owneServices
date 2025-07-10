using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public static class Utils
	{
		public static (XDocument SourceXml, DateTime PublicationTime) ConcatXmlsFromZipFileAndPublicationTime(string downloadFilePath, string containsValue, string endsWithValue, string[] dataRootNames)
		{
			var filePath = Path.GetFullPath(downloadFilePath);
			using (var archive = ZipFile.OpenRead(filePath))
			{
				XDocument output = null;
				var xmlEntries = archive.Entries.Where(x => x.FullName.Contains(containsValue) && x.FullName.EndsWith(endsWithValue, StringComparison.InvariantCulture));
				if (!xmlEntries.Any())
				{
					return (null, DateTime.MinValue);
				}

				foreach (var xmlEntry in xmlEntries)
				{
					using (var reader = new StreamReader(xmlEntry.Open()))
					{
						var result = XDocument.Load(reader);
						if (output == null)
						{
							output = result;
						}
						else
						{
							foreach (var root in dataRootNames)
							{
								output.Descendants().First(x => x.Name.LocalName == Constants.XmlTags.TariffHistoryItem).Add(result.Descendants(root));
							}
						}
					}
				}

				return (output, xmlEntries.First()?.LastWriteTime.DateTime ?? DateTime.Now);
			}
		}

		public static string Clean(this string input)
		{
			//\a Alert   0x0007
			//\f Form feed   0x000C
			return input.Replace('\f', ' ').Replace('\a', ' ');
		}

		/// <summary>
		/// Remove illegal XML characters from a string.
		/// </summary>
		public static string SanitizeXmlString(string xml)
		{
			if (xml == null)
			{
				throw new ArgumentNullException(nameof(xml));
			}

			var buffer = new StringBuilder(xml.Length);

			foreach (var c in xml)
			{
				if (IsLegalXmlChar(c))
				{
					buffer.Append(c);
				}
			}

			return buffer.ToString();
		}

		/// <summary>
		/// Whether a given character is allowed by XML 1.0.
		/// </summary>
		public static bool IsLegalXmlChar(int character)
		{
			return
			(
				 character == 0x9 /* == '\t' == 9   */          ||
				 character == 0xA /* == '\n' == 10  */          ||
				 character == 0xD /* == '\r' == 13  */          ||
				(character >= 0x20 && character <= 0xD7FF) ||
				(character >= 0xE000 && character <= 0xFFFD) ||
				(character >= 0x10000 && character <= 0x10FFFF)
			);
		}
	}
}
