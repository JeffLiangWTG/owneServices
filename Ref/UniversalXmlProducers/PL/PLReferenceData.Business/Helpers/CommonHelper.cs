using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Services;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Helpers
{
	internal class CommonHelper
	{
		public static string GetOutputFilePath(string filename)
		{
			var outputPath = ApplicationConfig.Instance.OutputDirectory;
			return Path.Combine(outputPath, string.IsNullOrEmpty(filename) ? "UniversalReferenceData.xml" : filename);
		}

		public static IsztarHistoryResponse GetBaseTaric4Data()
		{
			var normalizedFilePath = Taric4.Taric4Constants.NORMALIZED_BASE_FILE_FULL_PATH;

			if (!File.Exists(normalizedFilePath))
			{
				throw new FileNotFoundException($"File {normalizedFilePath} does not exist.");
			}

			var fi = new FileInfo(normalizedFilePath);
			if (fi.Length > Constants.MaxAcceptableSizeOfNormalizedBaseFileInBytes)
			{
				throw new ArgumentOutOfRangeException($"File {normalizedFilePath} is too big. Please use startup argument NORMALIZE and retry.");
			}

			var xDoc = XDocument.Load(normalizedFilePath);

			return XmlParser.Deserialize<IsztarHistoryResponse>(xDoc);
		}
	}
}
