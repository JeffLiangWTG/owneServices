using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test
{
	[TestFixture]
	public class ScrapePageHtmlFixtures
	{
		[Test]
		public void ScrapeData()
		{
			var scrapper = new ScrapePageHTML(_clientHelperMock.Object, MdbTestHelper.Url);
			Assert.That(!string.IsNullOrEmpty(scrapper.UNLOCOHyperLink));
			Assert.That(scrapper.PublicationTime, Is.Not.EqualTo(DateTime.MinValue));

			Assert.That(scrapper.UNLOCOHyperLink, Is.EqualTo(@"https://service.unece.org/trade/locode/loc211mdb.zip"));
			Assert.That(scrapper.PublicationTime, Is.EqualTo(new DateTime(2021, 7, 6)));
		}

		[SetUp]
		public void SetUp()
		{
			_clientHelperMock = new Mock<IHttpClientHelper>();

			using (var stream = GetType().Assembly.GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.ref.unece_codes.html"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				htmlContent = reader.ReadToEnd();
			}

			_clientHelperMock.Setup(x => x.GetWebPageAsync(MdbTestHelper.Url)).Returns(Task.FromResult(htmlContent));
		}

		Mock<IHttpClientHelper> _clientHelperMock;
		string htmlContent;
	}
}
