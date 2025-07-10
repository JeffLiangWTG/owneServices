using System;
using System.IO;
using System.Linq;
using System.Web;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class SectionDetailsScrapperFixture
	{
		[Test]
		public void GetAllSections()
		{
			var webDriverHelper = new Mock<IWebDriverHelper>();
			var page = HttpUtility.HtmlDecode(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\SampleSectionDetails.html")));
			webDriverHelper.Setup(x => x.GetWebPage(It.IsAny<string>(), It.IsAny<int>())).Returns(page);
			webDriverHelper.Setup(x => x.GetWebPageByLinkId(It.IsAny<string>())).Returns(page);

			var mapper = new SectionDetailsScrapper(webDriverHelper.Object);
			var result = mapper.ExtractChapterToSectionMap("dummy url");

			Assert.IsNotNull(result);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual(1, result[4].Number);
			Assert.False(result.Values.Any(x => string.IsNullOrEmpty(x.Description.Trim())));
		}

		[Test]
		public void ShouldReturnEmptyDictWhenNoNodesAreFound()
		{
			var webDriverHelper = new Mock<IWebDriverHelper>();
			var page = HttpUtility.HtmlDecode(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\EmptySectionDetails.html")));
			webDriverHelper.Setup(x => x.GetWebPage(It.IsAny<string>(), It.IsAny<int>())).Returns(page);

			var mapper = new SectionDetailsScrapper(webDriverHelper.Object);
			var result = mapper.ExtractChapterToSectionMap("dummy url");

			Assert.IsNotNull(result);
			Assert.That(result.Count, Is.EqualTo(0));
		}

		[SetUp]
		public void SetUp()
		{
			ApplicationConfig.ConfigEnvironment();
		}
	}
}
