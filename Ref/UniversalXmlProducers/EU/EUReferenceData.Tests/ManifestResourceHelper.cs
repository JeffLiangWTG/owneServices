using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.EUReferenceData.Services;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	static class ManifestResourceHelper
	{
		public static string ReadManifestResourceContent(string resourceDetails, Encoding encoding)
		{
			var result = string.Empty;
			using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails);
			using var reader = new StreamReader(stream, encoding);
			result = reader.ReadToEnd();
			return result;
		}

		public static byte[] GetManifestResourceContent(string resourceDetails)
		{
			using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails);
			using var memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			return memoryStream.ToArray();
		}

		public static void ExtractAdditionalTranslationsResources()
		{
			foreach (var (fileName, key) in additionalTranslationsResources)
			{
				var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(key);
				using var fileStream = File.Create(Path.Combine(ApplicationConfig.Instance.RefCusCodeListAdditionalTranslationsPath, fileName));

				resourceStream.Seek(0, SeekOrigin.Begin);
				resourceStream.CopyTo(fileStream);
			}
		}

		public static string ReadManifestResourceContentUTF8(string resourceDetails) => ReadManifestResourceContent(resourceDetails, Encoding.UTF8);

		static readonly (string FileName, string Key)[] additionalTranslationsResources =
		[
			("fr.json", "CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.Translation.TestFiles.fr.json"),
			("IT.json", "CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.Translation.TestFiles.IT.json"),
			("PL.json", "CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.Translation.TestFiles.PL.json"),
		];
	}
}
