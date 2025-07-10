using System.IO;
using CargoWise.RefDbRepo.GBReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.Helpers;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff
{
	[TestFixture]
	class FileManagerTests
	{
		[Test]
		public void DownloadManager()
		{
			var fileManager = new WebClientFileManagerForTest();

			Assert.That(fileManager.GetDownloadManagerExposed().GetType(), Is.EqualTo(typeof(WebClientDownloadManager)));
		}

		[Test]
		public void DownloadFiles()
		{
			var callCount = 0;
			var contentFolder = string.Empty;

			var mockDLManager = new Mock<IDownloadManager>();
			mockDLManager.Setup(x => x.RunDownloadProcess(It.IsAny<string>())).Callback<string>((cf) =>
			{
				callCount++;
				contentFolder = cf;
			});

			var folder = Path.Combine(tempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(folder);

			var fileManager = new FileManagerForTest(mockDLManager.Object, folder);
			(fileManager as IFileManager).GetFiles();

			Assert.That(callCount, Is.EqualTo(1));
			Assert.That(contentFolder, Is.EqualTo(folder));
		}

		[Test]
		public void GetContentFolder()
		{
			var fileManager = new WebClientFileManagerForTest();
			Assert.That(fileManager.GetContentFolderExposed(), Contains.Substring(@"GBTariffData\Content"));
		}

		[OneTimeSetUp]
		public void OnewTimeSetup()
		{
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
		}

		[OneTimeTearDown]
		public void OnewTimeTearDown()
		{
			if (Directory.Exists(tempFolder))
			{
				Directory.Delete(tempFolder, true);
			}
		}

		string tempFolder;
	}
}
