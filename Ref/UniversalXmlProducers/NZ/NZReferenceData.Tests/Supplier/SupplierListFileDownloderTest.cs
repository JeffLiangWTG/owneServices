using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	[TestFixture]
	class SupplierListFileDownloderTest
	{
		[Test]
		public void TestDownloadText()
		{
			var resourcePath = assembly.GetName().Name + ".Supplier.TestFiles.Input.Suppliercodesupdates_20211001.gz";
			var localFilePath = Path.GetTempFileName();

			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, "https://www.customs.govt.nz/api/datafiles/Supplier").Respond("application/zip", assembly.GetManifestResourceStream(resourcePath));

				using (var client = mockHttp.ToHttpClient())
				{
					var donwloader = new SupplierListFileDownloderForTest("Suppliercodesupdates_20211001.gz");
					var result = donwloader.Download(client, "https://www.customs.govt.nz/api/datafiles/Supplier", localFilePath);

					Assert.IsTrue(result.IsSuccess);
					Assert.That(result.PublishDate, Is.EqualTo(new DateTime(2021, 10, 01)), "Should get a correct publish date from the file name.");
				}

				var outputFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"Supplier\TestFiles\Output\Suppliercodesupdates_20211001");
				Assert.AreEqual(File.ReadAllText(outputFilePath), File.ReadAllText(localFilePath), "Should use uzip success.");
			}
		}

		[Test]
		public void TestDownloadWithInvalidUrl()
		{
			var localFilePath = Path.GetTempFileName();
			using (var mockHttp = new MockHttpMessageHandler())
			using (var client = mockHttp.ToHttpClient())
			{
				var donwloader = new SupplierListFileDownloderForTest("Suppliercodesupdates_20211001");
				var exception = Assert.Throws<InvalidOperationException>(() => donwloader.Download(client, "InvalidURL", localFilePath));

				Assert.That(exception.Message, Does.StartWith(@"Unable to Load NZ Supplier List from the following URL: InvalidURL"));
			}
		}

		[Test]
		public void TestDownloadWithUnexpectedFileName()
		{
			var resourcePath = assembly.GetName().Name + ".Supplier.TestFiles.Input.Suppliercodesupdates_INVALID_2021_NoDate.gz";
			var localFilePath = Path.GetTempFileName();
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, "https://www.customs.govt.nz/api/datafiles/Supplier").Respond("application/zip", assembly.GetManifestResourceStream(resourcePath));

				using (var client = mockHttp.ToHttpClient())
				{
					var donwloader = new SupplierListFileDownloderForTest("Suppliercodesupdates_INVALID_2021_NoDate");

					var exception = Assert.Throws<InvalidOperationException>(() => donwloader.Download(client, "https://www.customs.govt.nz/api/datafiles/Supplier", localFilePath));
					Assert.That(exception.Message, Does.Contain(@"Can not get a valid publish date from the file name: Suppliercodesupdates_INVALID_2021_NoDate"));
				}
			}
		}

		public sealed class SupplierListFileDownloderForTest : SupplierListFileDownloder
		{
			public SupplierListFileDownloderForTest(string fileName)
			{
				this.fileName = fileName;
			}

			readonly string fileName;

			protected override string GetDownloadFileName(HttpResponseMessage httpResponseMessage)
			{
				var contentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("ContentDisposition");
				contentDisposition.FileName = fileName;

				httpResponseMessage.Content.Headers.ContentDisposition = contentDisposition;
				return base.GetDownloadFileName(httpResponseMessage);
			}
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		Assembly assembly;
	}
}
