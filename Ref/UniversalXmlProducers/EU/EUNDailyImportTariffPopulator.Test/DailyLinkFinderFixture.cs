using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator.Test
{
	[TestFixture]
	internal class DailyLinkFinderFixture
	{
		[Test]
		public void GetPublicationTime()
		{
			var finder = new DailyLinkFinder(helper, configProvider);
			Assert.AreEqual(new DateTime(2022, 09, 14, 23, 00, 00), finder.GetPublicationTime());
		}

		[Test]
		public void GetIncrementalObjectTraderExportLink()
		{
			var finder = new DailyLinkFinder(helper, configProvider);
			var linksList = finder.GetIncrementalObjectTraderExportLinkOrderByPublishDateAscending();
			Assert.Greater(linksList.Count(), 1);
			CollectionAssert.AreEqual(new[]
			{
				@"http://abc.com/IncrementalObjectTraderExport_69c8cc3c-f0ea-47df-a417-13b4a07a58e5_220901.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_bb7131e0-07a5-4d91-aa9c-ef00b3f65d86_220902.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_b1b05055-3d71-46bc-8165-e36458578878_220905.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_17b7812a-8193-4169-9ecc-3923625f2f88_220906.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_a35d444a-9173-4b42-8a37-31de66f83954_220907.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_f9fa7ca9-51ea-4877-801f-8ca8cfd30ddb_220908.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_206765bb-8bad-45b0-b4b2-cf8648102a95_220909.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_70b2f7e5-68b4-4216-aac3-e9092ba247b9_220912.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_337e0c68-832a-4177-8f89-856e8ce9a7bc_220913.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_363e1330-84d9-4381-939b-d5307f2528d5_220914.xml.gz.pgp"
			}, linksList);
		}

		[Test]
		public void GetIncrementalObjectTraderExport_DeclarableGoodsNomenclatureLink()
		{
			var finder = new DailyLinkFinder(helper, configProvider);
			var linksList = finder.GetIncrementalObjectTraderExport_DeclarableGoodsNomenclatureLinkOrderByPublishDateAscending();
			Assert.Greater(linksList.Count(), 1);

			CollectionAssert.AreEqual(new[]
			{
				@"http://abc.com/IncrementalObjectTraderExport_DeclarableGoodsNomenclature_69c8cc3c-f0ea-47df-a417-13b4a07a58e5_220901.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_DeclarableGoodsNomenclature_bb7131e0-07a5-4d91-aa9c-ef00b3f65d86_220902.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_DeclarableGoodsNomenclature_b1b05055-3d71-46bc-8165-e36458578878_220905.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_DeclarableGoodsNomenclature_17b7812a-8193-4169-9ecc-3923625f2f88_220906.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_DeclarableGoodsNomenclature_a35d444a-9173-4b42-8a37-31de66f83954_220907.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_DeclarableGoodsNomenclature_f9fa7ca9-51ea-4877-801f-8ca8cfd30ddb_220908.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_DeclarableGoodsNomenclature_206765bb-8bad-45b0-b4b2-cf8648102a95_220909.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_DeclarableGoodsNomenclature_70b2f7e5-68b4-4216-aac3-e9092ba247b9_220912.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_DeclarableGoodsNomenclature_337e0c68-832a-4177-8f89-856e8ce9a7bc_220913.xml.gz.pgp",
				@"http://abc.com/IncrementalObjectTraderExport_DeclarableGoodsNomenclature_363e1330-84d9-4381-939b-d5307f2528d5_220914.xml.gz.pgp"
			}, linksList);
		}

		IHttpClientHelper helper;
		IDailyConfigProvider configProvider;

		[SetUp]
		public void SetUp()
		{
			var webContentPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "tulltaxan_xml_dif.html");
			var webContent = File.ReadAllText(webContentPath);
			var helperMock = new Mock<IHttpClientHelper>();
			helperMock.Setup(x => x.GetWebPageAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(webContent));
			helper = helperMock.Object;
			var configMock = new Mock<IDailyConfigProvider>();
			configMock.Setup(x => x.DailyXMLDistributionUrl).Returns("http://abc.com/");
			configProvider = configMock.Object;
		}
	}
}
