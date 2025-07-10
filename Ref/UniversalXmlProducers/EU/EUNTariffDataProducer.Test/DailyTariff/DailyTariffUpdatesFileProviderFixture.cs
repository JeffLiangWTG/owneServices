using System;
using System.IO;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test.DailyTariff
{
	[TestFixture]
	sealed class DailyTariffUpdatesFileProviderFixture
	{
		[Test]
		public void DownloadAndExtractDailyUpdates_WhenMinDateIsGreaterThanReferenceDate()
		{
			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderForTest();
			dailyTariffUpdatesFileProvider.DownloadAndExtractDailyUpdates(
				referenceDate: new DateTime(2024, 07, 30, 19, 0, 0),
				monthlyPublishDate: new DateTime(2024, 08, 01));

			Assert.That(dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection, Is.Not.Null);
			Assert.That(dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection.Count, Is.EqualTo(0));

			Assert.That(dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection, Is.Not.Null);
			Assert.That(dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection.Count, Is.EqualTo(0));

			Assert.That(dailyTariffUpdatesFileProvider.LastDailyPublishTime, Is.Null);
			Assert.That(Path.Combine(ApplicationConfig.DownloadsFolder, "DailyUpdates"), Does.Not.Exist);
		}

		[Test]
		public void DownloadAndExtractDailyUpdates()
		{
			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderForTest();
			dailyTariffUpdatesFileProvider.DownloadAndExtractDailyUpdates(
				referenceDate: new DateTime(2024, 07, 05, 19, 0, 0),
				monthlyPublishDate: new DateTime(2024, 06, 30));

			Assert.That(dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection, Is.Not.Null);
			Assert.That(dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection.Count, Is.EqualTo(28));

			Assert.That(dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection, Is.Not.Null);
			Assert.That(dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection.Count, Is.EqualTo(20));

			Assert.That(dailyTariffUpdatesFileProvider.LastDailyPublishTime, Is.EqualTo(new DateTime(2024, 07, 04, 19, 34, 00)));
		}

		[Test]
		public void DownloadAndExtractDailyUpdates_CalledTwice()
		{
			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderForTest();
			dailyTariffUpdatesFileProvider.DownloadAndExtractDailyUpdates(
				referenceDate: new DateTime(2024, 07, 06),
				monthlyPublishDate: new DateTime(2024, 06, 30));

			AssertResult();

			dailyTariffUpdatesFileProvider.DownloadAndExtractDailyUpdates(
				referenceDate: new DateTime(2024, 07, 06),
				monthlyPublishDate: new DateTime(2024, 06, 30));

			AssertResult();

			void AssertResult()
			{
				Assert.That(dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection, Is.Not.Null);
				Assert.That(dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection.Count, Is.EqualTo(35));

				Assert.That(dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection, Is.Not.Null);
				Assert.That(dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection.Count, Is.EqualTo(25));

				Assert.That(dailyTariffUpdatesFileProvider.LastDailyPublishTime, Is.EqualTo(new DateTime(2024, 07, 05, 19, 36, 00)));

			}
		}

		[Test]
		public void DownloadAndExtractDailyUpdates_WhenTwoPackagesContainSameFiles()
		{
			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderForTest(x => "SamePackage");
			dailyTariffUpdatesFileProvider.DownloadAndExtractDailyUpdates(
				referenceDate: new DateTime(2024, 07, 06),
				monthlyPublishDate: new DateTime(2024, 06, 30));

			Assert.That(dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection, Is.Not.Null);
			Assert.That(dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection.Count, Is.EqualTo(7));

			Assert.That(dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection, Is.Not.Null);
			Assert.That(dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection.Count, Is.EqualTo(5));

			Assert.That(dailyTariffUpdatesFileProvider.LastDailyPublishTime, Is.EqualTo(new DateTime(2024, 07, 01, 19, 21, 00)));

		}

		[Test]
		public void CleanAll()
		{
			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderForTest();
			dailyTariffUpdatesFileProvider.DownloadAndExtractDailyUpdates(
				referenceDate: new DateTime(2024, 07, 05, 19, 0, 0),
				monthlyPublishDate: new DateTime(2024, 06, 30));

			var nomenclatureDailyRawRecordCollection = dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection;
			Assert.That(nomenclatureDailyRawRecordCollection, Is.Not.Null);
			Assert.That(nomenclatureDailyRawRecordCollection.Count, Is.EqualTo(28));
			Assert.AreSame(nomenclatureDailyRawRecordCollection, dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection);

			var rateDailyRawRecordCollection = dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection;
			Assert.That(rateDailyRawRecordCollection, Is.Not.Null);
			Assert.That(rateDailyRawRecordCollection.Count, Is.EqualTo(20));
			Assert.AreSame(rateDailyRawRecordCollection, dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection);

			Assert.That(Path.Combine(ApplicationConfig.DownloadsFolder, "DailyUpdates"), Does.Exist);

			dailyTariffUpdatesFileProvider.CleanAll();

			Assert.AreNotSame(nomenclatureDailyRawRecordCollection, dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection);
			Assert.That(dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection, Is.Not.Null);
			Assert.That(dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection.Count, Is.EqualTo(0));

			Assert.AreNotSame(rateDailyRawRecordCollection, dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection);
			Assert.That(dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection, Is.Not.Null);
			Assert.That(dailyTariffUpdatesFileProvider.RateDailyRawRecordCollection.Count, Is.EqualTo(0));

			Assert.That(dailyTariffUpdatesFileProvider.LastDailyPublishTime, Is.Null);
			Assert.That(Path.Combine(ApplicationConfig.DownloadsFolder, "DailyUpdates"), Does.Not.Exist);
		}

		[Test]
		public void DownloadIsCached_UntilCleanAll()
		{
			var referenceDate = new DateTime(2024, 07, 05, 19, 0, 0);
			var monthlyPublishDate = new DateTime(2024, 06, 30);

			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderForTest();

			dailyTariffUpdatesFileProvider.DownloadAndExtractDailyUpdates(referenceDate,monthlyPublishDate);
			Assert.That(dailyTariffUpdatesFileProvider.DownloadCount, Is.EqualTo(1));

			dailyTariffUpdatesFileProvider.DownloadAndExtractDailyUpdates(referenceDate, monthlyPublishDate);
			Assert.That(dailyTariffUpdatesFileProvider.DownloadCount, Is.EqualTo(1));

			dailyTariffUpdatesFileProvider.CleanAll();
			dailyTariffUpdatesFileProvider.DownloadAndExtractDailyUpdates(referenceDate, monthlyPublishDate);
			Assert.That(dailyTariffUpdatesFileProvider.DownloadCount, Is.EqualTo(2));
		}

		[SetUp]
		public void Setup()
		{
			ApplicationConfig.ConfigEnvironment();
			TearDown();
		}

		[TearDown]
		public void TearDown()
		{
			var downloadPath = Path.Combine(ApplicationConfig.DownloadsFolder, "DailyUpdates");
			if (Directory.Exists(downloadPath))
			{
				Directory.Delete(downloadPath, true);
			}
		}

		#region Mock Implementation

		sealed class DailyTariffUpdatesFileProviderForTest : DailyTariffUpdatesFileProvider
		{
			public DailyTariffUpdatesFileProviderForTest(Func<string, string> packageUniqueIdFunc = null)
			{
				this.packageUniqueIdFunc = packageUniqueIdFunc ?? (x => Path.GetFileName(x));
			}

			public int DownloadCount => downloadCount;

			protected override IWebDriverHelper GetWebDriverHelper()
			{
				var webDriverHelperMock = new Mock<IWebDriverHelper>();
				webDriverHelperMock
					.Setup(x => x.GetWebPage(It.IsAny<string>(), It.IsAny<int>()))
					.Returns(File.ReadAllText(DailyTariffTestFiles.DailyTaricWebPageHtmlWithTimeFile));
				downloadCount++;
				return webDriverHelperMock.Object;
			}

			protected override IDailyPackageDownloader GetDailyPackageDownloader()
			{
				var dailyPackageDownloaderMock = new Mock<IDailyPackageDownloader>();
				dailyPackageDownloaderMock
					.Setup(x => x.DownloadAndExtract(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
					.Callback<string, string, string, bool>((url, destinationPath, extractionFolder, deletePackage) => DownloadAndExtractMock(extractionFolder));
				return dailyPackageDownloaderMock.Object;
			}

			void DownloadAndExtractMock(string destinationPath)
			{
				var packageUniqueId = packageUniqueIdFunc(destinationPath);

				var goodsNomenclatureFile = Path.Combine(destinationPath, $"Goods_Nomenclature_{packageUniqueId}.xls");
				File.Copy(DailyTariffTestFiles.SampleDailyGoodsNomenclature, goodsNomenclatureFile,overwrite: true);

				var measuresFile = Path.Combine(destinationPath, $"Measures_{packageUniqueId}.xls");
				File.Copy(DailyTariffTestFiles.SampleDailyMeasuresDailyPath, measuresFile, overwrite: true);

				var otherFile = Path.Combine(destinationPath, $"OtherFile_{packageUniqueId}.xls");
				File.Create(otherFile).Dispose();
			}

			readonly Func<string, string> packageUniqueIdFunc;
			int downloadCount;
		}

		#endregion
	}
}
