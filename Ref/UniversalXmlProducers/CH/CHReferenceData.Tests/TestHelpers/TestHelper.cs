using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests
{
	internal class TestHelper
	{
		internal static void DeleteLogFiles()
		{
			var assembly = typeof(BaseCodeListsParser<>).Assembly;
			var logDir = Path.GetDirectoryName(assembly.Location);
			var logNamePrefix = Path.GetFileNameWithoutExtension(assembly.Location);
			foreach (var file in Directory.GetFiles(logDir, logNamePrefix + "*.log"))
			{
				if (File.Exists(file))
				{
					File.Delete(file);
				}
			}
		}

		internal static void DeleteTestDirectory(string testDirectory)
		{
			if (Directory.Exists(testDirectory))
			{
				Directory.Delete(testDirectory, true);
			}
		}

		public static XDocument RemoveIgnoreTagsFromXml(XDocument document)
		{
			document.Root.Element("AppProgramArgs")?.Remove();
			document.Root.Element("AppName")?.Remove();
			return document;
		}

		public static string RemoveIgnoreTagsFromXml(string xmlValue)
		{
			xmlValue = new Regex(@"  <AppProgramArgs>(.*?)</AppProgramArgs>\r\n").Replace(xmlValue, "");
			xmlValue = new Regex(@"  <AppName>(.*?)</AppName>\r\n").Replace(xmlValue, "");
			return xmlValue;
		}
	}
}
