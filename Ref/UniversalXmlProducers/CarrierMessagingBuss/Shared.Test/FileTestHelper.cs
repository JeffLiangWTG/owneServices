using System;
using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test
{
	class FileTestHelper(string fileName) : IDisposable
	{
		readonly string filePath = Path.Combine(Setup.TestFolderPath, fileName);

		public string GetPath() => filePath;

		public string GetFileContent()
		{
			if (File.Exists(filePath))
			{
				return File.ReadAllText(filePath);
			}

			return string.Empty;
		}

		public void AssertFileContent(string expectedFileContent)
		{
			Assert.That(File.Exists(filePath), Is.True);
			var fileContent = GetFileContent();
			Assert.That(fileContent, Is.EqualTo(expectedFileContent));
		}

		public void Dispose()
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
	}
}
