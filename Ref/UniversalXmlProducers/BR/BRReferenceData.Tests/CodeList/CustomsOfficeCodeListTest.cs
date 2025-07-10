using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class CustomsOfficeCodeListTest
	{
		public void TestDownloadAndValidateStructureURFXmlWithoutMock()
		{
			using (var client = new HttpClient())
			{
				var downloader = new CustomsOfficeCodeListDownloader();
				var bXml = downloader.Download(client);
				using (var stream = new MemoryStream(bXml))
				{
					var xml = XDocument.Load(stream);
					Assert.NotNull(xml, nameof(xml));

					var urfs = xml?.Root?.Descendants(CustomsOfficeCodeListContants.TagCustomsOffice);
					Assert.NotNull(urfs, nameof(urfs));
				}
			}
		}

		[Test]
		public void TestDownloadAndValidateStructureURFXml()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_UNIDADE_LOCAL_RFB"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.UNIDADE_LOCAL_URF_GET.html"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_UNIDADE_LOCAL_RFB"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.UnidadeLocalRfbURF.xml"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new CustomsOfficeCodeListDownloader();
					var bXml = downloader.Download(client);
					using (var stream = new MemoryStream(bXml))
					{
						var xml = XDocument.Load(stream);
						Assert.NotNull(xml, nameof(xml));

						var urfs = xml?.Root?.Descendants(CustomsOfficeCodeListContants.TagCustomsOffice);
						Assert.NotNull(urfs, nameof(urfs));
					}
				}
			}
		}

		[Test]
		public void TestConvertCustomsOfficeToRefDataRepoXml()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeList_BR_CUSOF.xml"))
			using (var expectedTypeStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeType_BR_CUSOF.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.UnidadeLocalRfbURF.xml"))
			{
				CustomsOfficeCodeListParser parser = new CustomsOfficeCodeListParser("BR Customs Office Code List");

				using (var outputStream = new MemoryStream())
				using (var typeOutputStream = new MemoryStream())
				{
					parser.ExportToStream(inputStream, outputStream, typeOutputStream, publicationDateTime: new DateTime(2020, 10, 14, 09, 50, 00));
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
					StreamCompareHelper.CompareStreamContent(expectedTypeStream, typeOutputStream);
				}
			}
		}
	}
}
