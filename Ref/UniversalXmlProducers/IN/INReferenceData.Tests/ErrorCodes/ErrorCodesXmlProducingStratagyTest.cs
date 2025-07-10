using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using NUnit.Framework;
using MessageType = CargoWise.RefDbRepo.INReferenceData.Business.Constants.ErrorCodes.MessageType;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class ErrorCodesXmlProducingStratagyTest
	{
		[Test]
		public void TestProduceXml_AllErrorCodesDownload_NoFileExported()
		{
			downloaderMock.Setup(m => m.GetErrorCodes(It.IsAny<ErrorCodeType>())).Returns(new List<ErrorCodeItem>());
			var tempFolder = TestHelper.CreateTempFolder();
			var stratagy = GetStratagy(tempFolder);
			stratagy.ProduceXml();

			downloaderMock.Verify(m => m.GetErrorCodes(ErrorCodeType.BE), Times.Once);
			downloaderMock.Verify(m => m.GetErrorCodes(ErrorCodeType.AirCgm), Times.Once);
			downloaderMock.Verify(m => m.GetErrorCodes(ErrorCodeType.SeaCgm), Times.Once);

			TestHelper.AssertFileCount(tempFolder, 0, "Expected no files to be exported");
			Directory.Delete(tempFolder, true);
		}

		[Test]
		public void TestProduceXml_ErrorCodesExported()
		{
			var errorCodes = new List<ErrorCodeItem>
			{
				new ErrorCodeItem { ErrorCode = "E001", Description = "Error Desc Fresh", MessageType = MessageType.Fresh },
				new ErrorCodeItem { ErrorCode = "E002", Description = "Error Desc Ammendment", MessageType = MessageType.Amendment },
			};

			downloaderMock.Setup(m => m.GetErrorCodes(It.IsAny<ErrorCodeType>())).Returns(errorCodes);
			dateTimeProviderMock.Setup(m => m.GetDateTimeNow()).Returns(new DateTime(2025, 3, 3, 17, 26, 42));

			var tempFolder = TestHelper.CreateTempFolder();
			var stratagy = GetStratagy(tempFolder);
			stratagy.ProduceXml();

			loggerMock.Verify(m => m.Log(LogType.Info, It.Is<string>(s => s.Contains("Exporting IN Test Codes")), It.IsAny<object>()), Times.AtLeastOnce);
			TestHelper.AssertFileCount(tempFolder, 4, "Expected 4 files to be exported");
			TestHelper.AssertContainFileNames(tempFolder, "Expected files to be exported", "RefErrorCodesZZ_IN_BE_Fresh.xml", "RefErrorCodesZZ_IN_BE_Amendment.xml", "RefErrorCodesZZ_IN_AirCgm_Fresh.xml", "RefErrorCodesZZ_IN_SeaCgm_Fresh.xml");
			AssertContent(tempFolder, "RefErrorCodesZZ_IN_BE_Fresh.xml");
			AssertContent(tempFolder, "RefErrorCodesZZ_IN_BE_Amendment.xml");
			AssertContent(tempFolder, "RefErrorCodesZZ_IN_AirCgm_Fresh.xml");
			AssertContent(tempFolder, "RefErrorCodesZZ_IN_SeaCgm_Fresh.xml");
			Directory.Delete(tempFolder, true);
		}

		[Test]
		public void TestProduceXml_OnlyForExistingCodeTypes()
		{
			var errorCodes = new List<ErrorCodeItem>
			{
				new ErrorCodeItem { ErrorCode = "E001", Description = "Error Desc Fresh", MessageType = MessageType.Fresh },
			};

			downloaderMock.Setup(m => m.GetErrorCodes(It.Is<ErrorCodeType>(x => x == ErrorCodeType.BE))).Returns(errorCodes);

			var tempFolder = TestHelper.CreateTempFolder();
			var stratagy = GetStratagy(tempFolder);
			stratagy.ProduceXml();

			loggerMock.Verify(m => m.Log(LogType.Info, It.Is<string>(s => s.Contains("Exporting IN Test Codes")), It.IsAny<object>()), Times.AtLeastOnce);
			TestHelper.AssertFileCount(tempFolder, 1, "Expected 1 file to be exported");
			TestHelper.AssertContainFileNames(tempFolder, "Expected file to be exported", "RefErrorCodesZZ_IN_BE_Fresh.xml");
			Directory.Delete(tempFolder, true);
		}

		[SetUp]
		public void SetUp()
		{
			downloaderMock = new Mock<IErrorCodesDownloader>();
			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			loggerMock = new Mock<ILogger>();
		}

		void AssertContent(string folderName, string fileName)
		{
			var expected = TestHelper.ReadContentString("ErrorCodes\\INTestFiles\\Output\\" + fileName);
			var actual = File.ReadAllText(Path.Combine(folderName, fileName));
			Assert.That(actual, Is.EqualTo(expected).NoClip);
		}

		ErrorCodesXmlProducingStratagy GetStratagy(string outputFolder) => new ErrorCodesXmlProducingStratagy("IN Test Codes", downloaderMock.Object, dateTimeProviderMock.Object, outputFolder, loggerMock.Object);

		Mock<IErrorCodesDownloader> downloaderMock;
		Mock<IDateTimeProvider> dateTimeProviderMock;
		Mock<ILogger> loggerMock;
	}
}
