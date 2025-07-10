using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	public static class ExchangeRateTestHelper
	{
		public static string TestFilesPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"ExchangeRate\TestFiles");
	}
}
