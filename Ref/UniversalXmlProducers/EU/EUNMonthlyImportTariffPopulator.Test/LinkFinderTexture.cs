using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator.Test
{
	[TestFixture]
	internal class LinkFinderTexture
	{
		[Test]
		public void GetPublicationTime()
		{
			var finder = new LinkFinder(helper, configProvider);
			Assert.AreEqual(new DateTime(2022, 04, 01, 23, 00, 00), finder.GetPublicationTime());
		}

		[Test]
		public void GetMeasureTypeLink()
		{
			var finder = new LinkFinder(helper, configProvider);
			Assert.AreEqual("http://abc.com/MeasureType_de76251b-1a4d-42c6-b28c-cdaf6cf8ef0f_220401.xml.gz.pgp", finder.GetMeasureTypeLink());
		}

		[Test]
		public void GetMeasureLink()
		{
			var finder = new LinkFinder(helper, configProvider);
			Assert.AreEqual("http://abc.com/Measure_de76251b-1a4d-42c6-b28c-cdaf6cf8ef0f_220401.xml.gz.pgp", finder.GetMeasureLink());
		}

		[Test]
		public void GetDeclarableGoodsNomenclatureLink()
		{
			var finder = new LinkFinder(helper, configProvider);
			Assert.AreEqual("http://abc.com/DeclarableGoodsNomenclature_de76251b-1a4d-42c6-b28c-cdaf6cf8ef0f_220401.xml.gz.pgp", finder.GetDeclarableGoodsNomenclatureLink());
		}

		IHttpClientHelper helper;
		IConfigProvider configProvider;

		[SetUp]
		public void SetUp()
		{
			var webContentPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "tulltaxan_xml_tot.html");
			var webContent = File.ReadAllText(webContentPath);
			var helperMock = new Mock<IHttpClientHelper>();
			helperMock.Setup(x => x.GetWebPageAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(webContent));
			helper = helperMock.Object;
			var configMock = new Mock<IConfigProvider>();
			configMock.Setup(x => x.XMLDistributionUrl).Returns("http://abc.com/");
			configProvider = configMock.Object;
		}
	}
}
