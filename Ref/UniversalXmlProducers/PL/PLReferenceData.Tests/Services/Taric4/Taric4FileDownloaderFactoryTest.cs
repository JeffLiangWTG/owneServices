using CargoWise.RefDbRepo.PLReferenceData.Services.Taric4;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Services.Taric4;

[TestFixture]
sealed class Taric4FileDownloaderFactoryTest
{
	[Test]
	public void TestCreateFileDownloader()
	{
		var downloader = Taric4FileDownloaderFactory.CreateFileDownloader();
		Assert.That(downloader, Is.Not.Null);
		Assert.That(downloader, Is.InstanceOf<Taric4FileDownloader>());
	}
}
