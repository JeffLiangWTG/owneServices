using System;
using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	public static class TestSourcePathHelper
	{
		public static string DATTestSourcePath => Environment.GetEnvironmentVariable(DAT_TestSourcePath) ?? rootPath;
		public static string DATTestSupplementaryContentPath => Environment.GetEnvironmentVariable(DAT_TestSupplementaryContentPath) ?? rootPath;

		static string rootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..");

		public const string DAT_TestSourcePath = "DAT_TestSourcePath";
		public const string DAT_TestSupplementaryContentPath = "DAT_TestSupplementaryContentPath";
	}
}
