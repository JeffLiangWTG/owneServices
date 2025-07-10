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
	public class CusTariffLoaderFixture
	{
		[Test]
		public async Task TariffCodesDatabase_OnUnsuccessfulConnection_ErrorIsLogged()
		{
			using var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent(string.Empty)
			};

			using var httpClient = HttpClientTestHelper.GetHttpClientWithResponseMessage(responseMessage);
			var testCusTariffLoader = new CusTariffLoader(_loggerMock.Object, httpClient, CusTariffLoader.TariffType.Import);
			var actual = await testCusTariffLoader.LoadAsync(DummyUri);

			Assert.IsFalse(actual);
			_loggerMock.Verify(x => x.Log(It.IsAny<Exception>(), Constants.ErrorMessages.TariffCodeLoaderNoResponseFromDatabase));

		}

		[Test]
		public async Task TariffCodesDatabase_OnSuccessfulConnection_WhenTariffCodeIsNotDeserializable_ThenErrorIsLogged()
		{
			var tariffCodes = new CusTariffData { TariffCode = "0102030405" };

			var json = JsonConvert.SerializeObject(tariffCodes);

			using var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using var httpClient = HttpClientTestHelper.GetHttpClientWithResponseMessage(responseMessage);
			var testCusTariffLoader = new CusTariffLoader(_loggerMock.Object, httpClient, CusTariffLoader.TariffType.Import);
			var actual = await testCusTariffLoader.LoadAsync(DummyUri);

			Assert.IsFalse(actual);
			_loggerMock.Verify(x => x.Log(Constants.ErrorMessages.TariffCodeLoaderUnableToLoadFromDatabase));
		}

		[Test]
		public async Task TariffCodesDatabase_OnSuccessfulConnection_WhenTariffCodeIsPresent_ThenTariffCodesAreRetrieved()
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

			using var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(json)
			};

			using var httpClient = HttpClientTestHelper.GetHttpClientWithResponseMessage(responseMessage);
			var testCusTariffLoader = new CusTariffLoader(_loggerMock.Object, httpClient, CusTariffLoader.TariffType.Import);
			var loadSuccessful = await testCusTariffLoader.LoadAsync(DummyUri);

			Assert.True(loadSuccessful);

			var codes = testCusTariffLoader.AllCodes.ToList();
			Assert.AreEqual(4, codes.Count);
			CollectionAssert.AreEqual(tariffCodes.Select(t => t.TariffCode), codes);
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
