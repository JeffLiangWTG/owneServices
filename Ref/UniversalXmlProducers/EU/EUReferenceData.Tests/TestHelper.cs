using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	internal static class TestHelper
	{
		internal static Stream ReadManifestResourceContentAsStream(string resourceDetails) => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails);

		internal static string ReadManifestResourceContentAsString(string resourceDetails)
		{
			var result = string.Empty;
			using (var stream = ReadManifestResourceContentAsStream(resourceDetails))
			{
				using (var reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}
			}
			return result;
		}
	}
}
