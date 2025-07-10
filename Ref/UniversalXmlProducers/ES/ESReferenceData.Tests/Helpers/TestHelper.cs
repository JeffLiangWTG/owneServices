using System.IO;
using System.Reflection;
using System.Text;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

internal static class TestHelper
{
	internal static string ReadManifestResourceContent(string resourceDetails)
	{
		var result = string.Empty;
		using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
		using (var reader = new StreamReader(stream, ESReferenceData.Business.Constants.ESFileEncoding))
		{
			result = reader.ReadToEnd();
		}
		return result;
	}

	internal static string ReadManifestResourceContentUTF8(string resourceDetails)
	{
		var bytes = Encoding.Default.GetBytes(ReadManifestResourceContent(resourceDetails));
		return Encoding.UTF8.GetString(bytes);
	}
}
