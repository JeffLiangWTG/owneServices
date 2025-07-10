using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.Shared
{
	[TestFixture]
	public class TradeGroupLookupFixture
	{
		[Test]
		public async Task TradeGroupLoader_OnSuccessfulConnectionLoadsSuccessfully()
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
			using var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using var httpClient = HttpClientTestHelper.GetHttpClientWithResponseMessage(responseMessage);
			var tradeGroupLookup = new TradeGroupLookup(_loggerMock.Object, httpClient);
			var loadSuccessful = await tradeGroupLookup.LoadAsync(DummyUri);

			Assert.True(loadSuccessful);

			var codes = tradeGroupLookup.LookupDictionary;
			Assert.AreEqual(3, codes.Count);
			CollectionAssert.AreEqual(tradeGroups.Select(t => t.Code), codes.Values);
		}

		[Test]
		public async Task TradeGroupLoader_OnSuccessfulConnectionLoadsSuccessfully_DuplicateDescription()
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

			using var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using var httpClient = HttpClientTestHelper.GetHttpClientWithResponseMessage(responseMessage);
			var tradeGroupLookup = new TradeGroupLookup(_loggerMock.Object, httpClient);
			var loadSuccessful = await tradeGroupLookup.LoadAsync(DummyUri);

			Assert.True(loadSuccessful);

			var codes = tradeGroupLookup.LookupDictionary;
			Assert.AreEqual(1, codes.Count);
		}

		[Test]
		public async Task TradeGroupLoader_OnUnsuccessfulConnection_LogsErrorMessage()
		{
			using var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent(string.Empty)
			};

			using var httpClient = HttpClientTestHelper.GetHttpClientWithResponseMessage(responseMessage);
			var tradeGroupLookup = new TradeGroupLookup(_loggerMock.Object, httpClient);
			var actual = await tradeGroupLookup.LoadAsync(DummyUri);

			Assert.IsFalse(actual);

			_loggerMock.Verify(x => x.Log(It.IsAny<Exception>(), Constants.ErrorMessages.TradeGroupLookupNoResponseFromDatabase));
		}

		[Test]
		public async Task TradeGroupLoader_OnInvalidTradeGroupLookup_ErrorIsLogged()
		{
			var tradeGroup = new TradeGroupData { Code = "1011", Description = "ERGA OMNES" };

			var json = JsonConvert.SerializeObject(tradeGroup);

			using var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using var httpClient = HttpClientTestHelper.GetHttpClientWithResponseMessage(responseMessage);
			var tradeGroupLookup = new TradeGroupLookup(_loggerMock.Object, httpClient);
			var actual = await tradeGroupLookup.LoadAsync(DummyUri);

			Assert.IsFalse(actual);
			_loggerMock.Verify(x => x.Log(Constants.ErrorMessages.TradeGroupLookupUnableToLoadFromDatabase));
		}

		[SetUp]
		public void Setup()
		{
			_loggerMock = new Mock<ILogger>();
			DummyUri = new Uri("http://localhost/dummyuri");
		}

		Mock<ILogger> _loggerMock;
		Uri DummyUri;
	}
}
