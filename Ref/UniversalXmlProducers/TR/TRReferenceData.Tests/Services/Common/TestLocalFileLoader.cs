using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.TRReferenceData.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Common
{
    class TestLocalFileLoader
    {
		string testFilePath;

		[SetUp]
		public void Setup()
		{
			testFilePath = Path.Combine(Path.GetTempPath(), "testfile.txt");
		}

		[Test]
		public async Task TestLocalFileLoader_LoadAsync_WithValidFile_ReturnsContent()
		{
			var expectedContent = "Hello, World!";
			File.WriteAllText(testFilePath, expectedContent);

			using (var loader = new LocalFileLoader(new Uri(testFilePath)))
			{
				var result = await loader.LoadAsync();
				Assert.AreEqual(expectedContent, result);
			}
		}

		[Test]
		public async Task TestLocalFileLoader_LoadAsync_FileNotFound_ReturnsEmptyString()
		{
			string testFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "non_existent_file.txt");

			using (var loader = new LocalFileLoader(new Uri(testFilePath)))
			{
				var result = await loader.LoadAsync();
				Assert.AreEqual(string.Empty, result);
			}
		}

		[TearDown]
		public void Cleanup()
		{
			if (File.Exists(testFilePath))
			{
				File.Delete(testFilePath);
			}
		}
	}
}
