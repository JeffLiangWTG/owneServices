using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.NOReferenceData.Tests
{
	static class TestHelper
	{
		internal static void DeleteTestOutput(this string outputFile)
		{
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
		}
	}
}
