using System;
using System.IO;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class AdditionalCodesProducerFixture
	{
		[Test]
		public void TestCreateRefCusCodeListXML()
		{
			var additionalCodesProducer = SetupProducer("AdditionalCodes.xlsx");

			using (var memStream = new MemoryStream())
			using (var errorOutput = new StreamWriter(memStream))
			{
				Console.SetError(errorOutput);

				additionalCodesProducer.Run();
				Assert.That(File.ReadAllText(outputFileAndPath), Is.EqualTo(File.ReadAllText(Path.Combine(testFilesPath, "RefCusCodeListZZ_EU_AdditionalCodes.xml"))));

				errorOutput.Flush();
				memStream.Position = 0;
				using (var textReader = new StreamReader(memStream))
				{
					var errors = textReader.ReadToEnd();
					Assert.That(errors, Is.EqualTo(""));
				}
			}
		}

		[Test]
		public void TestDoNotCreateEmptyFile()
		{
			var additionalCodesProducer = SetupProducer("AdditionalCodesEmpty.xlsx");

			using (var memStream = new MemoryStream())
			using (var errorOutput = new StreamWriter(memStream))
			{
				Console.SetError(errorOutput);

				if (File.Exists(outputFileAndPath))
				{
					File.Delete(outputFileAndPath);
				}
				Assert.That(File.Exists(outputFileAndPath), Is.False, "Pre-req - File should not exist");

				additionalCodesProducer.Run();
				Assert.That(File.Exists(outputFileAndPath), Is.False);

				errorOutput.Flush();
				memStream.Position = 0;
				using (var textReader = new StreamReader(memStream))
				{
					var errors = textReader.ReadToEnd();
					Assert.That(errors, Is.EqualTo("AdditionalCodesData is empty => Nothing to import.\t\r\n"));
				}
			}
		}

		AdditionalCodesProducer SetupProducer(string inputFilename)
		{
			var webFileInfoMock = new Mock<IWebFileInfo>();
			webFileInfoMock.Setup(x => x.FileName).Returns(inputFilename);
			var additionalCodesProducerMock = new Mock<AdditionalCodesProducer>();
			var fileInfos = new IWebFileInfo[] { webFileInfoMock.Object };
			additionalCodesProducerMock.Setup(x => x.LocateWebFiles()).Returns(fileInfos);
			additionalCodesProducerMock.Setup(x => x.DownloadFiles(fileInfos)).Returns(true);
			var additionalCodesProducer = additionalCodesProducerMock.Object;

			return additionalCodesProducer;
		}

		[SetUp]
		public void Setup()
		{
			testFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles");
			ApplicationConfig.ConfigEnvironment();
			ApplicationConfig.SetDownloadsFolder(testFilesPath);
			outputFileAndPath = ApplicationConfig.AdditionalCodesUXmlFile;
		}
		string outputFileAndPath;
		string testFilesPath;

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(outputFileAndPath))
			{
				File.Delete(outputFileAndPath);
			}
		}
	}
}
