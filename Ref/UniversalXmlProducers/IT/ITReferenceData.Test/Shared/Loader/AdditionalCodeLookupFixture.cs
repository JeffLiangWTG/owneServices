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
	public class AdditionalCodeLookupFixture
	{
		[Test]
		public async Task AdditionalCodeLookup_OnSuccessfulConnectionLoadsSuccessfully()
		{
			var additionalCodes = new[]
			{
				new AdditionalCodeData { Code = "U012", Description = "Burrata" },
				new AdditionalCodeData { Code = "U013", Description = "Ricotta" },
				new AdditionalCodeData { Code = "U014", Description = "Mascarpone" }
			};

			var jsonContent = new AdditionalCodeDataWrapper
			{
				Value = additionalCodes,
			};

			var json = JsonConvert.SerializeObject(jsonContent);
			using var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using var httpClient = HttpClientTestHelper.GetHttpClientWithResponseMessage(responseMessage);
			var additionalCodeLookup = new AdditionalCodeLookup(_loggerMock.Object, httpClient);
			var loadSuccessful = await additionalCodeLookup.LoadAsync(DummyUri);

			Assert.True(loadSuccessful);

			var codes = additionalCodeLookup.LookupDictionary;
			Assert.AreEqual(3, codes.Count);
			CollectionAssert.AreEqual(additionalCodes.Select(t => t.Code), codes.Keys);
		}

		[Test]
		public async Task AdditionalCodeLookup_OnUnsuccessfulConnection_LogsErrorMessage()
		{
			using var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent(string.Empty)
			};

			using var httpClient = HttpClientTestHelper.GetHttpClientWithResponseMessage(responseMessage);
			var additionalCodeLookup = new AdditionalCodeLookup(_loggerMock.Object, httpClient);
			var actual = await additionalCodeLookup.LoadAsync(DummyUri);

			Assert.IsFalse(actual);

			_loggerMock.Verify(x => x.Log(It.IsAny<Exception>(), Constants.ErrorMessages.AdditionalCodeLookupNoResponseFromDatabase));
		}

		[Test]
		public async Task AdditionalCodeLookup_OnInvalidAdditionalCodeLookup_ErrorIsLogged()
		{
			var additionalCodes = new AdditionalCodeData { Code = "U012", Description = "Burrata" };

			var json = JsonConvert.SerializeObject(additionalCodes);

			using var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using var httpClient = HttpClientTestHelper.GetHttpClientWithResponseMessage(responseMessage);
			var additionalCodeLookup = new AdditionalCodeLookup(_loggerMock.Object, httpClient);
			var actual = await additionalCodeLookup.LoadAsync(DummyUri);

			Assert.IsFalse(actual);
			_loggerMock.Verify(x => x.Log(Constants.ErrorMessages.AdditionalCodeLookupUnableToLoadFromDatabase));
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
