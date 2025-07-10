using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Moq;
using Urs.Api.Integration;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.DTOs.Request;
using WiseRates.Tools.Exceptions;

namespace Enterprise.Rating.CarrierConnect.Test
{
	public class UrsRateSelectorProviderTest : TestCaseWithFactory
	{
		public void TestFetchTradeServices_SuccessfulCall_ReturnsExpectedResults()
		{
			var mockUrsClient = new Mock<IUrsClient>();
			var expectedResults = new List<TradeServiceDto> { new TradeServiceDto() };
			mockUrsClient.Setup(client => client.GetTradeServicesAsync(It.IsAny<QueryRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedResults);

			var urselectorProvider = new UrsRateSelectorProvider(Factory, new LoggerDecorator(Logger));
			var actualResults = urselectorProvider.FetchTradeServices(mockUrsClient.Object, new QueryRequestDto(), "requestID", CancellationToken.None);

			AssertEquals(expectedResults, actualResults);
			Assert(!Logger.Errors.Any());
		}

		public void TestFetchTradeServices_ReturnsEmpty_AndLogsWarning_WhenHttpResponseExceptionOccurs()
		{
			var testException = new HttpResponseException(HttpStatusCode.InternalServerError, "Not Found");
			var mockUrsClient = new Mock<IUrsClient>();
			mockUrsClient.Setup(client => client.GetTradeServicesAsync(It.IsAny<QueryRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(testException);

			var urselectorProvider = new UrsRateSelectorProvider(Factory, new LoggerDecorator(Logger));
			var actualResults = urselectorProvider.FetchTradeServices(mockUrsClient.Object, new QueryRequestDto(), "requestID", CancellationToken.None);

			AssertEquals(0, actualResults.Count());
			AssertContainsExactElementsInAnyOrder(new[] { "Error:[URS-SERVICE] Request failure | RequestID:requestID | Code:InternalServerError | Detail:Not Found" }, Logger.Errors);
		}

		public void TestFetchTradeServices_ReturnsEmpty_AndLogsGenericWarning_WhenOtherExceptionOccurs()
		{
			var testException = new Exception("Generic error");
			var mockUrsClient = new Mock<IUrsClient>();
			mockUrsClient.Setup(client => client.GetTradeServicesAsync(It.IsAny<QueryRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(testException);

			var urselectorProvider = new UrsRateSelectorProvider(Factory, new LoggerDecorator(Logger));
			var actualResults = urselectorProvider.FetchTradeServices(mockUrsClient.Object, new QueryRequestDto(), "requestID", CancellationToken.None);

			AssertEquals(0, actualResults.Count());
			AssertContainsExactElementsInAnyOrder(new[] { "Error:[URS-SERVICE] Request failure | RequestID:requestID | Code:UNKNOWN | Detail:Generic error" }, Logger.Errors);
		}

		protected TestLogger Logger { get; private set; }

		protected override void SetUp()
		{
			base.SetUp();
			Logger = new TestLogger();
		}
	}
}
