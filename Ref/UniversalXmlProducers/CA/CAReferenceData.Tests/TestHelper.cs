using System;
using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	public static class TestHelper
	{
		public static void DeleteFile(string filename)
		{
			try
			{
				File.Delete(filename);
			}
			catch (Exception)
			{
			}
		}

		public static string GetCurrentFolder()
		{
			var location = Assembly.GetExecutingAssembly().Location;
			return Path.GetDirectoryName(location);
		}

		public static string GetFolderPath()
		{
			var assembly = Assembly.GetExecutingAssembly();
			return Path.Combine(Path.GetDirectoryName(assembly.Location), $@"TestFiles\Input\CAExciseTaxDataSource");
		}

		public static Stream GetTestInputFile(string filename)
		{
			return GetTestFile("Input", filename);
		}

		public static Stream GetTestOutputFile(string filename)
		{
			return GetTestFile("Output", filename);
		}

		public static string CreateTempFile(string folderPath, string fileName, string fileContent)
		{
			var filePath = Path.Combine(folderPath, fileName);
			var localStream = File.Create(filePath);
			localStream?.Dispose();
			File.WriteAllText(filePath, fileContent);
			return filePath;
		}

		static Stream GetTestFile(string ioName, string filename)
		{
			var assembly = Assembly.GetExecutingAssembly();

			var results = assembly.GetManifestResourceNames();

			return assembly.GetManifestResourceStream($"CargoWise.RefDbRepo.CAReferenceData.Tests.TestFiles.{ioName}.{filename}");
		}

		public static string RemoveReturnCharactersIncaseEnvironmentNewlineDifference(string input)
		{
			return input.Replace("\r", "").Replace("\n", "");
		}
	}
}
