using System;
using CargoWise.RefDbRepo.IHSReferenceData.Services.Vessel;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IHSReferenceData.Tests.Vessel
{
	[TestFixture]
	class VesselListFileDownloaderTest
	{
		[Test]
		public void DownloadInvalidXML()
		{
			var exception = Assert.Throws<IHSReferenceData.Services.DownloadException>(() => VesselListFileDownloader.DownloadAndExtractVesselFile("InvalidURL", string.Empty, string.Empty, string.Empty, string.Empty, out var publicationDateTime, out string flagCodeCsv));
			Assert.That(exception.Message, Does.StartWith("Unable to Load IHS Vessel List from the following host: InvalidURL"));
		}
	}
}
