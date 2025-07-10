using System.Net.Http;
using NUnit.Framework;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using System.Linq;
using System;
using System.Collections.Generic;
using RichardSzalay.MockHttp;
using System.Net;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.BRReferenceData.Business;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class BaseTariffRatesTest
	{
		protected static MockHttpMessageHandler GetMockedHttpClient(bool useUpdatedXml = false)
		{
			var tariffs = new[] { "29039917", "01041019", "01012100" };
			using var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_NCM_LVL6_SUBITEM"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TABELAS_ADUANEIRAS_SOURCES.NCM_LEVEL6_SUBITEM_GET.html"));
			mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_NCM_LVL6_SUBITEM"]).Respond(Respond);

			Task<HttpResponseMessage> Respond(HttpRequestMessage request)
			{
				var response = new HttpResponseMessage(HttpStatusCode.OK);
				var content = request.Content?.ReadAsStringAsync()?.Result ?? string.Empty;
				if (content.Length > 0)
				{
					var tariffNumber = tariffs.FirstOrDefault(tariff => content.Contains(tariff));
					if (content.Contains("j_id111%3AelementList%3A0%3Aj_id201"))
					{
						response.Content = new StreamContent(Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TariffRates.TariffDetail_{tariffNumber}.txt"));
					}
					else if (content.Contains("j_id111%3Aj_id229"))
					{
						var updated = useUpdatedXml ? "_updated" : string.Empty;
						response.Content = new StreamContent(Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TariffRates.Ncms{updated}.xml"));
					}
					else
					{
						response.Content = new StreamContent(Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TariffRates.TariffSearch_{tariffNumber}.txt"));
					}
				}
				else
				{
					response.StatusCode = HttpStatusCode.NotFound;
				}
				return Task.FromResult(response);
			}

			return mockHttp;
		}


		protected void AssertDownloadedTariffs(IEnumerable<TariffDTO> downloadedTariffs)
		{
			Assert.AreEqual(2, downloadedTariffs.Count());

			var tariffDTO = downloadedTariffs.First(t => t.Code == "29039917");
			Assert.AreEqual("29039917", tariffDTO.Code);
			Assert.AreEqual(4, tariffDTO.TariffRates.Count());

			AssertRates(tariffDTO.TariffRates, Constants.Rates.Codes.Duty, "0.00000", "01092022", "31129999");
			AssertRates(tariffDTO.TariffRates, Constants.Rates.Codes.IPI, "0.00", "01082022", "31129999");
			AssertRates(tariffDTO.TariffRates, Constants.Rates.Codes.PIS, "2.10", "01012017", "31129999");
			AssertRates(tariffDTO.TariffRates, Constants.Rates.Codes.COFINS, "9.65", "01012017", "31129999");

			tariffDTO = downloadedTariffs.First(t => t.Code == "01041019");
			Assert.AreEqual("01041019", tariffDTO.Code);
			Assert.AreEqual(4, tariffDTO.TariffRates.Count());

			AssertRates(tariffDTO.TariffRates, Constants.Rates.Codes.Duty, "0.00000", "01042022", "31129999");
			AssertRates(tariffDTO.TariffRates, Constants.Rates.Codes.IPI, string.Empty, "01082022", "31129999");
			AssertRates(tariffDTO.TariffRates, Constants.Rates.Codes.PIS, "2.10", "01012017", "31129999");
			AssertRates(tariffDTO.TariffRates, Constants.Rates.Codes.COFINS, "9.65", "01012017", "31129999");

			void AssertRates(IEnumerable<TariffRateDTO> rates, string rateCode, string percentual, string startDate, string endDate)
			{
				var rate = rates.FirstOrDefault(r => r.Code == rateCode);

				Assert.AreEqual(percentual, rate.Percentual);
				Assert.AreEqual(startDate, rate.StartDate);
				Assert.AreEqual(endDate, rate.EndDate);
			}
		}

		[SetUp]
		protected void setUp()
		{
			CaptchaSolvingUtils.Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });
		}

		[TearDown]
		protected void tearDown()
		{
			CaptchaSolvingUtils.Instance.SetResultToGetLoginResponse(null);
		}
	}
}
