using System;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	[TestFixture]
	class VesselListFileDownloderTest
	{
		[Test]
		public void TestDownloadText()
		{
			var _httpClientMock = new Mock<IHttpClientHelper>();
			var localFilePath = Path.GetTempFileName();
			using (var vesselList = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.NZReferenceData.Tests.Vessel.TestFiles.Input.VesselList.txt"))
			{
				using (var reader = new StreamReader(vesselList, Encoding.UTF8))
				{
					string vesselListValue = reader.ReadToEnd();
					_httpClientMock.Setup(x => x.GetWebPageAsync("https://www.customs.govt.nz/api/datafiles/craft")).Returns(Task.FromResult(vesselListValue));
					VesselListFileDownloder.Download(_httpClientMock.Object, "https://www.customs.govt.nz/api/datafiles/craft", localFilePath);
				}
			}
			Assert.That(File.ReadAllLines(localFilePath).Length, Is.EqualTo(12680));
		}

		[Test]
		public void TestDownloadWithInvalidUrl()
		{
			var localFilePath = Path.GetTempFileName();
			var _httpClient = new HttpClientHelper();
			var exception = Assert.Throws<InvalidOperationException>(() => VesselListFileDownloder.Download(_httpClient, "InvalidURL", localFilePath));
			Assert.That(exception.Message, Does.StartWith("Unable to Load NZ Vessel List from the following URL: InvalidURL"));
		}
	}
}
