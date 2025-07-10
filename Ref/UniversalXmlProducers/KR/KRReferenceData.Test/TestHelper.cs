using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	internal static class TestHelper
	{
		internal static string ReadManifestResourceContent(string resourceDetails)
		{
			var result = string.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			{
				using (var reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}
			}
			return result;
		}
		internal static string ConfigJsonFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CargoWise.RefDbRepo.KRReferenceData.CmdLine.config.json");
		internal static string BaseTestFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles");
		internal static string BaseRealFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Res");
	}
}
