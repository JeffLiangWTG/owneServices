using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class BaseProducerFixture
	{
		[Test]
		public void DownloadFiles()
		{
			var downloaderWrapper = new Mock<IFileDownloaderWrapper>();
			downloaderWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(true);
			var producer = new BaseProducerForTest(downloaderWrapper.Object);
			var lastModificationTime = new DateTime(2024, 4, 1);
			var fileInfo1 = new WebFileInfo("file1.txt", "http://localhost/downloadPath1", lastModificationTime);
			var fileInfo2 = new WebFileInfo("file2.txt", "http://localhost/downloadPath2", lastModificationTime);
			var downloadsFolder = ApplicationConfig.DownloadsFolder;
			if (!Directory.Exists(downloadsFolder))
			{
				Directory.CreateDirectory(downloadsFolder);
			}
			var filePath1 = Path.Combine(downloadsFolder, "file1.txt");
			File.WriteAllText(filePath1, "1234");
			File.SetLastWriteTime(filePath1, lastModificationTime);
			var filePath2 = Path.Combine(downloadsFolder, "file2.txt");
			File.WriteAllText(filePath2, "1234");
			File.SetLastWriteTime(filePath2, lastModificationTime);

			var downloadResult = producer.DownloadFiles(new[] { fileInfo1, fileInfo2 });
			Assert.True(downloadResult);
			Assert.AreEqual(lastModificationTime, producer.PublishTime);
			Assert.AreEqual(lastModificationTime, ApplicationConfig.PublishDate);
			downloaderWrapper.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()));

			if (Directory.Exists(ApplicationConfig.DownloadsFolder))
			{
				Directory.Delete(downloadsFolder, true);
			}
		}

		[SetUp]
		public void Setup()
		{
			ApplicationConfig.ConfigEnvironment();
		}
	}

	class BaseProducerForTest : BaseProducer
	{
		readonly IFileDownloaderWrapper fileDownloaderWrapper;
		public BaseProducerForTest(IFileDownloaderWrapper downloaderWrapper)
		{
			fileDownloaderWrapper = downloaderWrapper;
		}

		public override IEnumerable<string> FilesToLocate => Enumerable.Empty<string>();

		public override string EUNLibraryBasePage => string.Empty;

		protected override IFileDownloaderWrapper GetFileDownloaderWrapper()
		{
			return fileDownloaderWrapper;
		}
	}
}
