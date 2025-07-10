using System;
using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.IHSReferenceData.Tests
{
	internal static class TestHelper
	{
		internal static string ReadManifestResourceContent(string resourceDetails)
		{
			var result = string.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			{
				if (stream == null)
				{
					throw new Exception($"Missing resource: {resourceDetails}");
				}

				using (var reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}
			}
			return result;
		}
	}
}
