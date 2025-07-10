using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers
{
	internal class TestHelper
	{
		internal static string ReadManifestResourceContent(string resourceDetails)
		{
			var result = string.Empty;
			using (var stream = GetResourceStream(resourceDetails))
			using (var reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}

			// Annoying VS XML editor adds \r\n at the end of the file when saving
			if (resourceDetails.ToUpperInvariant().EndsWith(".XML") && result.EndsWith("\r\n"))
			{
				result = result.Substring(0, result.Length - 2);
			}

			return result;
		}

		internal static Stream GetResourceStream(string resourceDetails)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resources = assembly.GetManifestResourceNames();

			if (!resources.Contains(resourceDetails))
			{
				throw new ApplicationException($"Could not find '{resourceDetails}'. Your options are\n {string.Join("\n", resources)}");
			}

			return assembly.GetManifestResourceStream(resourceDetails);
		}
	}
}
