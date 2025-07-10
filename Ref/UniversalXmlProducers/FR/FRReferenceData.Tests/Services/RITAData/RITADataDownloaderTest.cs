using System.IO;
using System.Net;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests
{
	[TestFixture]
	class RITADataDownloaderTest
	{
		[Test]
		public void TestGetWebPageIfInvalidOrUnresolvableURL()
		{
			var downloader = new RITADataDownloader();
			try
			{
				downloader.GetWebPage("", "hdgfrfkfodj", "");
			}
			catch (RITAWebSiteException e)
			{
				Assert.That(e.Message == $"Queried URL hdgfrfkfodj is invalid. The application will now close and retry next run.\r\nIf the problem persists, it is likely that GetWebPage() method is not fed with correct parameters. Please Fix it in FRReferenceData.RitaDataDownloader class", "Case of invalid URL.");
			}

			try
			{
				downloader.GetWebPage("", "https://ghjsiyebvs.com", "");
			}
			catch (RITAWebSiteException e)
			{
				Assert.That(e.Message == $"RITA webSite returned an unexpected error when querying https://ghjsiyebvs.com . Reported error is:\r\nNo such host is known. (ghjsiyebvs.com:443).\r\n The application will now close.", "Case of unresolvable URL.");
			}
		}

		[Test]
		public void TestGetWebPageWithOtherTypeOfError()
		{
			ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfTechnicalError = 1000;
			ApplicationConfig.Instance.MaxAttemptsCountInCaseOfTechnicalError = 1;

			var downloaderMock = new Mock<RITADataDownloader>();
			downloaderMock.CallBase = true;
			var response = new Mock<HttpWebResponse>();
			response.Setup(c => c.StatusCode).Returns(HttpStatusCode.NotFound);
			downloaderMock.Setup(x => x.GetRequestStream(It.IsAny<HttpWebRequest>())).Returns(Stream.Null);
			downloaderMock.Setup(x => x.GetResponse(It.IsAny<HttpWebRequest>())).Throws(new WebException("(404) Page Not Found", null, WebExceptionStatus.ProtocolError, response.Object));
			downloaderMock.Setup(x => x.GetTimeOut()).Returns(10000);
			var downloader = downloaderMock.Object;
			try
			{
				downloader.GetWebPage("", "https://www.douane.gouv.fr/whatever", "");
			}
			catch (RITAWebSiteException e)
			{
				Assert.That(e.Message == $"RITA webSite returned an unexpected error when querying https://www.douane.gouv.fr/whatever . Reported error is:\r\n(404) Page Not Found.\r\n The application will now close.", "Case of hard web exception.");
			}
		}

		[Test]
		public void TestGetWebPageWithTemporaryTypeOfError()
		{
			ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfTechnicalError = 1000;
			ApplicationConfig.Instance.MaxAttemptsCountInCaseOfTechnicalError = 1;

			var downloaderMock = new Mock<RITADataDownloader>();
			downloaderMock.CallBase = true;
			var exception = new WebException();
			var response = new Mock<HttpWebResponse>();
			response.Setup(c => c.StatusCode).Returns(HttpStatusCode.InternalServerError);
			downloaderMock.Setup(x => x.GetRequestStream(It.IsAny<HttpWebRequest>())).Returns(Stream.Null);
			downloaderMock.Setup(x => x.GetResponse(It.IsAny<HttpWebRequest>())).Throws(new WebException("(500) Internal Server Error", null, WebExceptionStatus.ProtocolError, response.Object));
			downloaderMock.Setup(x => x.GetTimeOut()).Returns(50000);

			var downloader = downloaderMock.Object;
			try
			{
				downloader.GetWebPage("", "https://www.douane.gouv.fr/whatever", "");
			}
			catch (RITAWebSiteException e)
			{
				Assert.That(e.Message == $"Couldn't download data from https://www.douane.gouv.fr/whatever . The application will now close and retry next run.\r\nIf the problem persists, it is likely that GetWebPage() method is not fed with correct parameters. Please Fix it in FRReferenceData.RitaDataDownloader class", "Case of temporary web exception.");
			}
		}
	}
}
