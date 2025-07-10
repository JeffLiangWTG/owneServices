using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;

namespace CargoWise.RefDbRepo.SEReferenceData.Tests
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
					throw new FileNotFoundException($"Missing resource: {resourceDetails}");
				}

				using (var reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}
				return result;
			}
		}

		internal static void DeleteTestOutput(this string outputFile)
		{
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
		}

		internal static string Dev_TestSourcePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\..");

		internal static string BaseSourcePath => Environment.GetEnvironmentVariable("DAT_TestSourcePath") ?? Dev_TestSourcePath;
	}
}
