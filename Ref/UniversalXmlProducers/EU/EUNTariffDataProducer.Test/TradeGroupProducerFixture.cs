using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class TradeGroupProducerFixture
	{
		[Test]
		public void TestCreateTradeGroupXML()
		{
			tradeGroupProducer.Run();
			Assert.That(File.ReadAllText(OutputFileAndPath), Is.EqualTo(File.ReadAllText(Path.Combine(testFilesPath, "EUNTradeGroup.xml"))));
		}

		[Test]
		public void TestCreateTradeGroupXMLWithError()
		{
			var tradeGroupProducerMock = new Mock<TradeGroupProducer>();
			var fileInfoWithError = new Mock<IWebFileInfo>();
			fileInfoWithError.Setup(x => x.Exception).Returns(new Exception($"Geographical Areas Composition not found"));
			var fileInfos = new IWebFileInfo[] { webFileInfoMock.Object, fileInfoWithError.Object };
			tradeGroupProducerMock.Setup(x => x.LocateWebFiles()).Returns(fileInfos);
			tradeGroupProducerMock.Setup(x => x.DownloadFiles(new List<IWebFileInfo>() { webFileInfoMock.Object })).Returns(true);
			tradeGroupProducerMock.Setup(x => x.FilesToLocate).Returns(new[] { "Geographical areas Composition", "Geographical area composition" });
			tradeGroupProducer = tradeGroupProducerMock.Object;
			Assert.DoesNotThrow(() => tradeGroupProducer.Run());
			Assert.That(File.ReadAllText(OutputFileAndPath), Is.EqualTo(File.ReadAllText(Path.Combine(testFilesPath, "EUNTradeGroup.xml"))));
		}

		[Test]
		public void TestCreateTradeGroupXMLJoinGroup1010And1011()
		{
			webFileInfoMock.Setup(x => x.FileName).Returns("SampleGeographicalAreasComposition1010And1011.xlsx");
			var tradeGroupProducerMock = new Mock<TradeGroupProducer>();
			var fileInfos = new IWebFileInfo[] { webFileInfoMock.Object };
			tradeGroupProducerMock.Setup(x => x.LocateWebFiles()).Returns(fileInfos);
			tradeGroupProducerMock.Setup(x => x.DownloadFiles(fileInfos)).Returns(true);
			tradeGroupProducer = tradeGroupProducerMock.Object;
			Assert.DoesNotThrow(() => tradeGroupProducer.Run());
			Assert.That(File.ReadAllText(OutputFileAndPath), Is.EqualTo(File.ReadAllText(Path.Combine(testFilesPath, "EUNTradeGroup1010And1011.xml"))));
		}

		[Test]
		public void TestCreateTradeGroupXMLJoinGroup1010And1011WithDuplicates()
		{
			webFileInfoMock.Setup(x => x.FileName).Returns("SampleGeographicalAreasComposition1010And1011Duplicates.xlsx");
			var tradeGroupProducerMock = new Mock<TradeGroupProducer>();
			var fileInfos = new IWebFileInfo[] { webFileInfoMock.Object };
			tradeGroupProducerMock.Setup(x => x.LocateWebFiles()).Returns(fileInfos);
			tradeGroupProducerMock.Setup(x => x.DownloadFiles(fileInfos)).Returns(true);
			tradeGroupProducer = tradeGroupProducerMock.Object;
			Assert.DoesNotThrow(() => tradeGroupProducer.Run());
			Assert.That(File.ReadAllText(OutputFileAndPath), Is.EqualTo(File.ReadAllText(Path.Combine(testFilesPath, "EUNTradeGroup1010And1011.xml"))));
		}

		[Test]
		public void TestCreateTradeGroupXMLJoinGroup1010And1011NoSortedFile()
		{
			webFileInfoMock.Setup(x => x.FileName).Returns("SampleGeographicalAreasComposition1010And1011NoSorted.xlsx");
			var tradeGroupProducerMock = new Mock<TradeGroupProducer>();
			var fileInfos = new IWebFileInfo[] { webFileInfoMock.Object };
			tradeGroupProducerMock.Setup(x => x.LocateWebFiles()).Returns(fileInfos);
			tradeGroupProducerMock.Setup(x => x.DownloadFiles(fileInfos)).Returns(true);
			tradeGroupProducer = tradeGroupProducerMock.Object;
			Assert.DoesNotThrow(() => tradeGroupProducer.Run());
			Assert.That(File.ReadAllText(OutputFileAndPath), Is.EqualTo(File.ReadAllText(Path.Combine(testFilesPath, "EUNTradeGroup1010And1011.xml"))));
		}

		[SetUp]
		public void Setup()
		{
			testFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\EUNTradeGroup");
			ApplicationConfig.ConfigEnvironment();
			ApplicationConfig.SetDownloadsFolder(testFilesPath);
			webFileInfoMock = new Mock<IWebFileInfo>();
			webFileInfoMock.Setup(x => x.FileName).Returns("SampleGeographicalAreasComposition.xlsx");
			var tradeGroupProducerMock = new Mock<TradeGroupProducer>();
			var fileInfos = new IWebFileInfo[] { webFileInfoMock.Object };
			tradeGroupProducerMock.Setup(x => x.LocateWebFiles()).Returns(fileInfos);
			tradeGroupProducerMock.Setup(x => x.DownloadFiles(fileInfos)).Returns(true);
			tradeGroupProducer = tradeGroupProducerMock.Object;
		}
		TradeGroupProducer tradeGroupProducer;
		Mock<IWebFileInfo> webFileInfoMock;
		string testFilesPath;

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(OutputFileAndPath))
			{
				File.Delete(OutputFileAndPath);
			}
		}

		const string OutputFileAndPath = @"..\..\UXmlFiles\EUNTradeGroup.xml";
	}
}
