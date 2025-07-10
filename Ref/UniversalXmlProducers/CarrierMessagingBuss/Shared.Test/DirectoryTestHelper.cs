using System;
using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test
{
	public class DirectoryTestHelper(string directoryName) : IDisposable
	{
		readonly string directoryPath = Path.Combine(Setup.TestFolderPath, directoryName);

		public string DirectoryPath => directoryPath;

		public void AssertFileExists(string fileName)
		{
			var filePath = Path.Combine(directoryPath, fileName);
			Assert.That(File.Exists(filePath), Is.True);
		}

		public void AssertFileContent(string fileName, string expectedFileContent)
		{
			AssertFileExists(fileName);

			var filePath = Path.Combine(directoryPath, fileName);
			var fileContent = File.ReadAllText(filePath);
			Assert.That(fileContent, Is.EqualTo(expectedFileContent));
		}

		public void Dispose()
		{
			if (Directory.Exists(directoryPath))
			{
				Directory.Delete(directoryPath, recursive: true);
			}
		}
	}
}
