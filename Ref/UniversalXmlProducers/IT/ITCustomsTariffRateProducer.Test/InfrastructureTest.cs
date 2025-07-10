using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	public class InfrastructureTest
	{
		[Test]
		public void TariffCodesDatabase_OnUnsuccessfulConnection_ErrorIsLogged()
		{
			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent(string.Empty)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var testCusTariffLoader = new CusTariffLoader(_loggerMock.Object, _httpHandlerMock.Object);
				var actual = testCusTariffLoader.Load(DummyUrl);

				Assert.IsFalse(actual);
				_loggerMock.Verify(x => x.Log(ErrorMessagesConstant.TariffCodeLoaderNoResponseFromDatabase));
			}
		}

		[Test]
		public void TariffCodesDatabase_OnSuccessfulConnection_WhenTariffCodeIsNotDeserializable_ThenErrorIsLogged()
		{
			var tariffCodes = new CusTariffData { TariffCode = "0102030405" };

			var json = JsonConvert.SerializeObject(tariffCodes);

			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var testCusTariffLoader = new CusTariffLoader(_loggerMock.Object, _httpHandlerMock.Object);
				var actual = testCusTariffLoader.Load(DummyUrl);

				Assert.IsFalse(actual);
				_loggerMock.Verify(x => x.Log(ErrorMessagesConstant.TariffCodeLoaderUnableToLoadFromDatabase));
			}
		}

		[Test]
		public void TariffCodesDatabase_OnSuccessfulConnection_WhenTariffCodeIsPresent_ThenTariffCodesAreRetrieved()
		{
			var tariffCodes = new[]
			{
				new CusTariffData { TariffCode = "0102030405" },
				new CusTariffData { TariffCode = "0102030406" },
				new CusTariffData { TariffCode = "0102030407" },
				new CusTariffData { TariffCode = "0102030408" }
			};

			var jsonContent = new CusTariffDataWrapper
			{
				Value = tariffCodes
			};

			var json = JsonConvert.SerializeObject(jsonContent);

			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var testCusTariffLoader = new CusTariffLoader(_loggerMock.Object, _httpHandlerMock.Object);
				var loadSuccessful = testCusTariffLoader.Load(DummyUrl);

				Assert.True(loadSuccessful);

				var codes = testCusTariffLoader.AllCodes.ToList();
				Assert.AreEqual(4, codes.Count);
				CollectionAssert.AreEqual(tariffCodes.Select(t => t.TariffCode), codes);
			}
		}

		[Test]
		public void DownloadResponseFile_WhenInvalidLengthTariffCodeIsSupplied_ThenErrorIsLogged()
		{
			var webResponseHandler = new WebResponseHandler(_httpHandlerMock.Object, new DateTimeProvider());
			var response = webResponseHandler.GetWebRepsonseForTariffAsync(DummyUrl, "01020304").Result;

			Assert.IsFalse(response.IsValidResponse);
			Assert.AreEqual(ErrorMessagesConstant.InvalidLengthTariffCode, response.ResponseMessage);
		}

		[Test]
		public void DownloadResponseFile_OnSuccessfulConnection_ResponseMessageIsRetrieved()
		{
			const string dummyMessage = "<html><body>some dummy message</body></html>";

			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(dummyMessage)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.PostAsync(DummyUrl, It.IsAny<HttpContent>()))
					.Returns(Task.FromResult(responseMessage));

				var webResponseHandler = new WebResponseHandler(_httpHandlerMock.Object, new DateTimeProvider());
				var response = webResponseHandler.GetWebRepsonseForTariffAsync(DummyUrl, "0102030405").Result;

				Assert.IsTrue(response.IsValidResponse);
				Assert.AreEqual(dummyMessage, response.ResponseMessage);
			}
		}

		[Test]
		public void TradeGroupLoader_OnSuccessfulConnectionLoadsSucessfully()
		{
			var tradeGroups = new[]
			{
				new TradeGroupData { Code = "1011", Description = "ERGA OMNES" },
				new TradeGroupData { Code = "1009", Description = "All destinations - export refund" },
				new TradeGroupData { Code = "AU", Description = "Australia" }
			};

			var jsonContent = new TradeGroupDataWrapper
			{
				Value = tradeGroups
			};

			var json = JsonConvert.SerializeObject(jsonContent);
			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var tradeGroupLookup = new TradeGroupLookup(_loggerMock.Object, _httpHandlerMock.Object);
				var loadSuccessful = tradeGroupLookup.Load(DummyUrl);

				Assert.True(loadSuccessful);

				var codes = tradeGroupLookup.LookupDictionary;
				Assert.AreEqual(3, codes.Count);
				CollectionAssert.AreEqual(tradeGroups.Select(t => t.Code), codes.Values);
			}
		}

		[Test]
		public void TradeGroupLoader_OnSuccessfulConnectionLoadsSucessfully_DuplicateDescription()
		{
			var tradeGroups = new[]
			{
				new TradeGroupData { Code = "5001", Description = "Countries subject to safeguard measures" },
				new TradeGroupData { Code = "5002", Description = "Countries subject to safeguard measures" }
			};

			var jsonContent = new TradeGroupDataWrapper
			{
				Value = tradeGroups
			};

			var json = JsonConvert.SerializeObject(jsonContent);

			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var tradeGroupLookup = new TradeGroupLookup(_loggerMock.Object, _httpHandlerMock.Object);
				var loadSuccessful = tradeGroupLookup.Load(DummyUrl);

				Assert.True(loadSuccessful);

				var codes = tradeGroupLookup.LookupDictionary;
				Assert.AreEqual(1, codes.Count);
			}
		}

		[Test]
		public void TradeGroupLoader_OnUnsuccessfulConnection_LogsErrorMessage()
		{
			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent(string.Empty)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var tradeGroupLookup = new TradeGroupLookup(_loggerMock.Object, _httpHandlerMock.Object);
				var actual = tradeGroupLookup.Load(DummyUrl);

				Assert.IsFalse(actual);

				_loggerMock.Verify(x => x.Log(ErrorMessagesConstant.TradeGroupLookupNoResponseFromDatabase));
			}
		}

		[Test]
		public void TradeGroupLoader_OnInvalidTradeGroupLookup_ErrorIsLogged()
		{
			var tradeGroup = new TradeGroupData { Code = "1011", Description = "ERGA OMNES" };

			var json = JsonConvert.SerializeObject(tradeGroup);

			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var tradeGroupLookup = new TradeGroupLookup(_loggerMock.Object, _httpHandlerMock.Object);
				var actual = tradeGroupLookup.Load(DummyUrl);

				Assert.IsFalse(actual);
				_loggerMock.Verify(x => x.Log(ErrorMessagesConstant.TradeGroupLookupUnableToLoadFromDatabase));
			}
		}

		[Test]
		public void TaxOrFeeCodeLookup_OnSuccessfulConnectionLoadsSucessfully()
		{
			var taxOrFeeCodeData = new[]
			{
				new TaxOrFeeCodeData { Code = "ESE", Value = 0.00f },
				new TaxOrFeeCodeData { Code = "MIN", Value = 0.04f },
				new TaxOrFeeCodeData { Code = "RID", Value = 0.1f },
				new TaxOrFeeCodeData { Code = "ORD", Value = 0.22f }
			};

			var jsonContent = new TaxOrFeeCodeDataWrapper
			{
				Value = taxOrFeeCodeData
			};

			var json = JsonConvert.SerializeObject(jsonContent);

			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var taxOrFeeCodeLookup = new TaxOrFeeCodeLookup(_loggerMock.Object, _httpHandlerMock.Object);
				var loadSuccessful = taxOrFeeCodeLookup.Load(DummyUrl);

				Assert.True(loadSuccessful);

				var codes = taxOrFeeCodeLookup.LookupDictionary;
				Assert.AreEqual(4, codes.Keys.Count);

				Assert.AreEqual("ESE", taxOrFeeCodeLookup.Lookup("0"));
				Assert.AreEqual("MIN", taxOrFeeCodeLookup.Lookup("4"));
				Assert.AreEqual("RID", taxOrFeeCodeLookup.Lookup("10"));
				Assert.AreEqual("ORD", taxOrFeeCodeLookup.Lookup("22"));
			}
		}

		[Test]
		public void TaxOrFeeCodeLookup_OnUnsuccessfulConnection_LogsErrorMessage()
		{
			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent(string.Empty)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var taxOrFeeCodeLookup = new TaxOrFeeCodeLookup(_loggerMock.Object, _httpHandlerMock.Object);
				var actual = taxOrFeeCodeLookup.Load(DummyUrl);

				Assert.IsFalse(actual);
				_loggerMock.Verify(x => x.Log(ErrorMessagesConstant.TaxOrFeeCodeLookupNoResponseFromDatabase));
			}
		}

		[Test]
		public void TaxOrFeeCodeLookup_OnInvalidTaxOrFeeCodeLookup_ErrorIsLogged()
		{
			var taxOrFeeCodeData = new TaxOrFeeCodeData { Code = "ESE", Value = 0.00f };

			var json = JsonConvert.SerializeObject(taxOrFeeCodeData);

			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var taxOrFeeCodeLookup = new TaxOrFeeCodeLookup(_loggerMock.Object, _httpHandlerMock.Object);
				var actual = taxOrFeeCodeLookup.Load(DummyUrl);

				Assert.IsFalse(actual);
				_loggerMock.Verify(x => x.Log(ErrorMessagesConstant.TaxOrFeeCodeLookupUnableToLoadFromDatabase));
			}
		}

		[Test]
		public void RateCodeLookup_OnSuccessfulConnectionLoadsSucessfully()
		{
			var rateCodeData = new[]
			{
				new RateCodeData { Description = "Sovrimposta di confine sugli oli minerali, loro derivati e prodotti analoghi, spettante all’Erario.", RateCode = "125", RefCusRateType = new RefCusRateType { RateType = "EXC", RateTypeDataGrouping = "IT" } },
				new RateCodeData { Description = "Contributo Stazione Sperimentale Combustibili.", RateCode = "912", RefCusRateType = new RefCusRateType { RateType = "LEV", RateTypeDataGrouping = "IT" } }
			};

			var jsonContent = new RateCodeDataWrapper
			{
				Value = rateCodeData
			};

			var json = JsonConvert.SerializeObject(jsonContent);

			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var rateCodeLookup = new RateCodeDataLookup(_loggerMock.Object, _httpHandlerMock.Object);
				var loadSuccessful = rateCodeLookup.Load(DummyUrl);

				Assert.True(loadSuccessful);

				var codes = rateCodeLookup.GetLookupDictionary();
				Assert.AreEqual(13, codes.Keys.Count);

				var result = rateCodeLookup.Lookup("Imposta di consumo", null);
				Assert.IsNotNull(result);
				Assert.AreEqual("125", result.Code);

				result = rateCodeLookup.Lookup("Imposta di consumo", "1302010110");
				Assert.IsNotNull(result);
				Assert.AreEqual("116", result.Code);

				result = rateCodeLookup.Lookup("Imposta di consumo", "000000000");
				Assert.IsNotNull(result);
				Assert.AreEqual("125", result.Code);

				result = rateCodeLookup.Lookup("Something random", null);
				Assert.IsNull(result);
			}
		}

		[Test]
		public void RateCodeLookup_OnUnsuccessfulConnection_LogsErrorMessage()
		{
			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent(string.Empty)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var rateCodeDataLookup = new RateCodeDataLookup(_loggerMock.Object, _httpHandlerMock.Object);
				var actual = rateCodeDataLookup.Load(DummyUrl);

				Assert.IsFalse(actual);
				_loggerMock.Verify(x => x.Log(ErrorMessagesConstant.RateCodeDataLookupNoResponseFromDatabase));
			}
		}

		[Test]
		public void RateCodeLookup_OnInvalidRateCodeLookup_ErrorIsLogged()
		{
			var rateCodeData = new RateCodeData { Description = "Imposta di consumo", RateCode = "125", RefCusRateType = new RefCusRateType { RateType = "EXC", RateTypeDataGrouping = "IT" } };

			var json = JsonConvert.SerializeObject(rateCodeData);

			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var rateCodeDataLookup = new RateCodeDataLookup(_loggerMock.Object, _httpHandlerMock.Object);
				var actual = rateCodeDataLookup.Load(DummyUrl);

				Assert.IsFalse(actual);
				_loggerMock.Verify(x => x.Log(ErrorMessagesConstant.RateCodeDataLookupUnableToLoadFromDatabase));
			}
		}

		[Test]
		public void UomCodeMappingLookup()
		{
			var uomCodeLookup = new UomCodeLookup();
			Assert.AreEqual("TNE", uomCodeLookup.Lookup("1000 kg"));
			Assert.AreEqual(null, uomCodeLookup.Lookup("DUMMY"));
			Assert.AreEqual("MTQ", uomCodeLookup.Lookup("m3"));
		}

		[SetUp]
		public void Setup()
		{
			_httpHandlerMock = new Mock<IHttpHandler>();
			_loggerMock = new Mock<ILogger>();
		}

		Mock<IHttpHandler> _httpHandlerMock;
		Mock<ILogger> _loggerMock;
		const string DummyUrl = "http://localhost/dummyurl";
	}
}
