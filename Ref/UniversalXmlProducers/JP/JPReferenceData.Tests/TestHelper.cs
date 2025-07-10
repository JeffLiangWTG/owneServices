using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	internal static class TestHelper
	{
		internal static string ReadManifestResourceContent(string resourceDetails)
		{
			var result = string.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			using (var reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}
			return result;
		}


		internal static string ReadManifestResourceContent(string resourceDetails, string charset)
		{
			var result = string.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			using (var reader = new StreamReader(stream, Encoding.GetEncoding(charset)))
			{
				result = reader.ReadToEnd();
			}
			return result;
		}


		internal static Stream ReadManifestResourceContentAsStream(string resourceDetails) => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails);
	}
}
