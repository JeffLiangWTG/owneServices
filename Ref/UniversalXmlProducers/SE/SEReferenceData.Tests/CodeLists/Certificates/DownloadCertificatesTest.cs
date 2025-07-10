using System.IO;
using System.Net;
using CargoWise.RefDbRepo.SEReferenceData.Certificates.Services;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.SEReferenceData.Tests;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.SEReferenceData.Certificates.Tests
{
	[TestFixture]
	public class DownloadCertificatesTest
	{
		private const string MEDIA_TYPE = "text/plain";
		private const string CERTIFICATE_URL = "http://example.com/filename.xml.gz.pgp";
		const string InputTestFilesPath = @"UniversalXmlProducers\SE\SEReferenceData.Tests\CodeLists\Certificates\TestFiles\Input\";

		[Test]
		public void DownloadCertificatesIncremental()
		{
			var filepath = Path.Combine(TestHelper.BaseSourcePath, InputTestFilesPath, "IncrementalObjectTraderExport_210604.xml.gz.pgp");
			using (var fileStream = File.OpenRead(filepath))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(CERTIFICATE_URL).Respond(MEDIA_TYPE, fileStream);

				var certificate = new DownloadCertificatesIncremental(mockHttp.ToHttpClient()).DownloadLatestIncrementalAndExtract(CERTIFICATE_URL);
				Assert.That(certificate.Length, Is.EqualTo(1));
				Assert.That(certificate[0].certificateDescriptionPeriod.Length, Is.EqualTo(1));
			}
		}

		[Test]
		public void DownloadCertificatesComplete()
		{
			var filepath = Path.Combine(TestHelper.BaseSourcePath, InputTestFilesPath, "Certificate_210601.xml.gz.pgp");
			using (var fileStream = File.OpenRead(filepath))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(CERTIFICATE_URL).Respond(MEDIA_TYPE, fileStream);

				var certificate = new DownloadCertificatesComplete(mockHttp.ToHttpClient()).DownloadLatestTotAndExtract(CERTIFICATE_URL);
				Assert.That(certificate.Length, Is.EqualTo(659));
			}
		}

		[Test]
		public void DownloadInvalidCertificatesComplete()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(CERTIFICATE_URL).Respond(HttpStatusCode.NotFound);

				var exception = Assert.Throws<CertificatesException>(() => new DownloadCertificatesComplete(mockHttp.ToHttpClient()).DownloadLatestTotAndExtract(CERTIFICATE_URL));
				Assert.That(exception.Message, Does.StartWith($"Unable to Load Certificates XML from the following URL: {CERTIFICATE_URL}"));
			}
		}

		[Test]
		public void DownloadInvalidIncremental()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(CERTIFICATE_URL).Respond(HttpStatusCode.NotFound);

				var exception = Assert.Throws<ObjectTraderExportException>(() => new DownloadCertificatesIncremental(mockHttp.ToHttpClient()).DownloadLatestIncrementalAndExtract(CERTIFICATE_URL));
				Assert.That(exception.Message, Does.StartWith($"Unable to Load IncrementalObjectTraderExport XML from the following URL: {CERTIFICATE_URL}"));
			}
		}
	}
}
