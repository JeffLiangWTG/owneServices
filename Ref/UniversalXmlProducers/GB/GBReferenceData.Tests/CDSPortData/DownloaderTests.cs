using System;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Downloader;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData
{
	[TestFixture]
	class DownloaderTests
	{
		[Test]
		public void GetCSVContent()
		{
			var expected = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Output.CSVContent_001.txt");
			var source = new CDSPortSource { PageURL = "CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.PageContent_001.html", AnchorText = "Download CSV" };
			var webClient = new Mock<IWebClientWrapper>();

			webClient.Setup(x => x.GetContent(It.IsAny<string>())).Returns<string>((url) => TestHelper.ReadManifestResourceContent(url));

			var manager = new DownloadManager(webClient.Object) as IDownloadManager;

			var result = manager.GetCSVContent(source);

			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void GetCSVContentWhenAnchorTextDoesNotMatch()
		{
			var expected = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Output.OtherLocationCodes.csv");
			var source = new CDSPortSource { PageURL = "CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.OtherLocationCodes.html", AnchorText = "If we cant find this anchor text, then we can try and find an anchor with a url ending in .CSV" };
			var webClient = new Mock<IWebClientWrapper>();

			webClient.Setup(x => x.GetContent(It.IsAny<string>())).Returns<string>((url) => TestHelper.ReadManifestResourceContent(url));

			var manager = new DownloadManager(webClient.Object) as IDownloadManager;

			var result = manager.GetCSVContent(source);

			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void GetBinaryContent()
		{
			var source = new CDSPortSource { PageURL = "CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.PageContent_005.html", AnchorText = "GVMS codes", ODSDataTag = "Location & Facility Name" };
			var webClient = new Mock<IWebClientWrapper>();

			webClient.Setup(x => x.GetContent(It.IsAny<string>())).Returns<string>((url) => TestHelper.ReadManifestResourceContent(url));
			webClient.Setup(x => x.GetContentAsByteArray(It.IsAny<string>())).Returns<string>((url) => TestHelper.ReadManifestResourceContentBytes(url));

			var manager = new DownloadManager(webClient.Object) as IDownloadManager;

			var result = manager.GetBinaryData(source);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Length, Is.EqualTo(4276));
		}

		[Test]
		public void GetBinaryContent_MissingAnchor()
		{
			var source = new CDSPortSource { PageURL = "unimportant.html", AnchorText = "GVMS codes", ODSDataTag = "Location & Facility Name" };
			var webClient = new Mock<IWebClientWrapper>();
			webClient.Setup(x => x.GetContent(It.IsAny<string>())).Returns<string>(_ => "<html><body><h3>Nope</h3></body></html>");

			var manager = new DownloadManager(webClient.Object) as IDownloadManager;

			var ex = Assert.Throws<ApplicationException>(() => manager.GetBinaryData(source));
			Assert.That(ex.ToString(), Does.Contain("Anchor text 'GVMS codes' was not found on the page at 'unimportant.html'"));
		}

		[Test]
		public void AbortOnFailure()
		{
			var webClient = new Mock<IWebClientWrapper>();
			var source = new CDSPortSource { PageURL = "CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.PageContent_001.html" };

			webClient.Setup(x => x.GetContent(It.IsAny<string>())).Returns<string>((url) =>
			{
				if (url.Contains("CSVContent_001"))
				{
					throw new Exception("Simulated failure on csv content");
				}

				return TestHelper.ReadManifestResourceContent(url);
			});

			var manager = new DownloadManager(webClient.Object) as IDownloadManager;
			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith($"Processing of {source.PageURL} failed"), () => manager.GetCSVContent(source));
		}

		[Test]
		public void NoCSVLinkFound()
		{
			var source = new CDSPortSource { PageURL = "CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.PageContent_002.html", AnchorText = "Download CSV" };
			var webClient = new Mock<IWebClientWrapper>();

			webClient.Setup(x => x.GetContent(It.IsAny<string>())).Returns<string>((url) => TestHelper.ReadManifestResourceContent(url));

			var manager = new DownloadManager(webClient.Object) as IDownloadManager;
			var ex = Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith($"Processing of {source.PageURL} failed"), () => manager.GetCSVContent(source));
			Assert.That(ex.GetBaseException().Message, Does.Contain("CSV download link 'Download CSV' not found."));
		}
	}
}
