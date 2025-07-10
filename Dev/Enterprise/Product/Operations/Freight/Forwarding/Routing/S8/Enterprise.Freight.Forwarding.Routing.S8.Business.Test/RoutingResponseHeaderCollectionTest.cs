using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Http;
using static Enterprise.Freight.Forwarding.Routing.S8.Business.Test.S8ClientTest;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	[TestedType(typeof(RoutingResponseHeaderCollection))]
	public class RoutingResponseHeaderCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RoutingResponseHeaderCollection>
	{
		public void TestLoadSuccessfully()
		{
			var routingRequest = new RoutingRequest(Factory);
			var routingResponseText = "132 SYD BOM 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 1+09:45 ZZ   123            0 9012   > <SIN 4 BOM A 1+10:45 2+08:30 XX   090            3 1003   >";
			routingResponseText += "\n";
			routingResponseText += "132 SYD BOM 15:45   23:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 1+09:45 ZZ   123            0 9012   >";

			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.SetupSequence(x => x.SolveRouting(It.IsAny<RoutingRequest>()))
				.Returns(new MethodCallResult<string>(routingResponseText, string.Empty))
				.Returns(new MethodCallResult<string>(routingResponseText, string.Empty));
			var s8Client = s8ClientWrapper.Object;

			var responseHeadersWrapper = new Mock<RoutingResponseHeaderCollection>(Factory) { CallBase = true };
			responseHeadersWrapper.SetupSequence(x => x.GetS8Client(null)).Returns(s8Client).Returns(s8Client);
			var responseHeaders = responseHeadersWrapper.Object;

			var errorMessage = responseHeaders.Load(new[] { routingRequest });
			AssertEquals(string.Empty, errorMessage);
			AssertEquals(2, responseHeaders.Count);
			AssertEquals("13:30", responseHeaders[0].Duration);
			AssertEquals("15:45", responseHeaders[1].Duration);

			errorMessage = responseHeaders.Load(new[] { routingRequest });
			AssertEquals(string.Empty, errorMessage);
			AssertEquals("Cleared when load called subsequent times", 2, responseHeaders.Count);
		}

		public void TestLoadMultipleRequests()
		{
			var routingRequest1 = new RoutingRequest(Factory);
			var routingRequest2 = new RoutingRequest(Factory);

			var routingResponseText1 = "132 SYD BOM 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 1+09:45 ZZ   123            0 9012   > <SIN 4 BOM A 1+10:45 2+08:30 XX   090            3 1003   >";
			routingResponseText1 += "\n";
			routingResponseText1 += "132 SYD BOM 15:45   23:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 1+09:45 ZZ   123            0 9012   >";

			var routingResponseText2 = "132 SYD BOM 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 1+09:45 ZZ   123            0 9012   > <SIN 4 BOM A 1+10:45 2+08:30 XX   090            3 1003   >";

			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.SetupSequence(x => x.SolveRouting(It.IsAny<RoutingRequest>()))
				.Returns(new MethodCallResult<string>(routingResponseText1, string.Empty))
				.Returns(new MethodCallResult<string>(routingResponseText2, string.Empty));
			var s8Client = s8ClientWrapper.Object;

			var responseHeadersWrapper = new Mock<RoutingResponseHeaderCollection>(Factory) { CallBase = true };
			responseHeadersWrapper.SetupSequence(x => x.GetS8Client(null)).Returns(s8Client).Returns(s8Client);
			var responseHeaders = responseHeadersWrapper.Object;

			var errorMessage = responseHeaders.Load(new[] { routingRequest1, routingRequest2 });
			AssertEquals(string.Empty, errorMessage);
			AssertEquals(3, responseHeaders.Count);
			AssertEquals("13:30", responseHeaders[0].Duration);
			AssertEquals("15:45", responseHeaders[1].Duration);
			AssertEquals("13:30", responseHeaders[2].Duration);
		}

		public void TestLoadRoutingResponseHeader_Cancelled()
		{
			var routingRequest = new RoutingRequest(Factory);

			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.SolveRouting(It.IsAny<RoutingRequest>()))
				.Returns(new MethodCallResult<string>(default, "Operation was cancelled")
				{
					WasOperationCancelledByUser = true
				});
			var s8Client = s8ClientWrapper.Object;

			var responseHeadersWrapper = new Mock<RoutingResponseHeaderCollection>(Factory) { CallBase = true };
			responseHeadersWrapper.Setup(x => x.GetS8Client(It.IsAny<Action<string>>())).Returns(s8Client);
			var responseHeaders = responseHeadersWrapper.Object;

			var errorMessage = responseHeaders.Load(new[] { routingRequest }, status =>
			{
				throw new OperationCanceledException("Operation was cancelled");
			});
			AssertEquals(string.Empty, errorMessage);
			AssertEquals("No headers are loaded", 0, responseHeaders.Count);
		}

		public void TestLoadRoutingResponseHeader_WithError()
		{
			var routingRequest = new RoutingRequest(Factory);
			var expectedErrorMessage = "System.Net.WebException: The request was aborted: The operation has timed out.";

			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.SolveRouting(It.IsAny<RoutingRequest>()))
				.Returns(new MethodCallResult<string>(default, expectedErrorMessage));
			var s8Client = s8ClientWrapper.Object;

			var responseHeadersWrapper = new Mock<RoutingResponseHeaderCollection>(Factory) { CallBase = true };
			responseHeadersWrapper.Setup(x => x.GetS8Client(It.IsAny<Action<string>>())).Returns(s8Client);
			var responseHeaders = responseHeadersWrapper.Object;

			var errorMessage = responseHeaders.Load(new[] { routingRequest }, status => { });
			AssertEquals(expectedErrorMessage, errorMessage);
			AssertEquals("No headers are loaded", 0, responseHeaders.Count);
		}

		public void TestAllowNewRemove()
		{
			RoutingResponseHeaderCollection result = new RoutingResponseHeaderCollection(Factory);
			AssertEquals(false, result.AllowNew);
			AssertEquals(false, result.AllowRemove);
		}

		public void TestLoadRouting_ShouldNotReportError_WhenServerReturnErrorMessage()
		{
			var routingRequest = new RoutingRequest(Factory);
			var responseHeaders = new RoutingResponseHeaderCollectionWithMockClient(Factory);

			using (var mockServer = new MockFlightScheduleClient(null, MockFlightScheduleClient.RaiseExceptionStatus.RaiseSignInInvalidOrExpiredExceptionOneTime))
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
			{
				var errorMessage = responseHeaders.Load(new[] { routingRequest }, status => { });
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestLoadRouting_ShouldInformUser_WhenItIsUnableToProvideRoute()
		{
			var routingRequest = new RoutingRequest(Factory);
			var responseHeaders = new RoutingResponseHeaderCollectionWithMockClient(Factory);

			using (var mockServer = new MockFlightScheduleClient(null, MockFlightScheduleClient.RaiseExceptionStatus.RaiseTimeoutExceptionInSolveRoutingMoreThanOneTime))
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
			{
				var errorMessage = responseHeaders.Load(new[] { routingRequest }, status => { });
				AssertEquals($@"Please try again. If the issue persists, please raise an eRequest. Issue details: {TimeOutExceptionMessage}", errorMessage);
			}
			ErrorReporter.Clear();
		}

		public void TestLoadRouting_ShouldNotReportError_WhenExceptionIsTimeout()
		{
			var routingRequest = new RoutingRequest(Factory);
			var responseHeaders = new RoutingResponseHeaderCollectionWithMockClient(Factory);

			using (var mockServer = new MockFlightScheduleClient(null, MockFlightScheduleClient.RaiseExceptionStatus.RaiseTimeoutExceptionInSolveRoutingMoreThanOneTime))
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
			{
				var errorMessage = responseHeaders.Load(new[] { routingRequest }, status => { });
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}

			ErrorReporter.Clear();
		}

		public void TestLoadRouting_ShouldNotReportErrot_WhenExceptionIsUnableToRetrieveLoginMutex()
		{
			var routingRequest = new RoutingRequest(Factory);
			var responseHeaders = new RoutingResponseHeaderCollectionForMutexTest(Factory);
			var errorMessage = responseHeaders.Load(new[] { routingRequest }, status => { });
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestLoadRouting_ShouldNotReportError_WhenExceptionIsTheUnderlyingConnectionWasClosed()
		{
			var routingRequest = new RoutingRequest(Factory);
			var responseHeaders = new RoutingResponseHeaderCollectionWithMockClient(Factory);

			using (var mockServer = new MockFlightScheduleClient(null, MockFlightScheduleClient.RaiseExceptionStatus.RaiseUnderlyingConnectionWasClosedExceptionInSolveRouting))
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
			{
				var errorMessage = responseHeaders.Load(new[] { routingRequest }, status => { });
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
			ErrorReporter.Clear();
		}

		#region Implementation

		public class RoutingResponseHeaderCollectionWithMockClient : RoutingResponseHeaderCollection
		{
			internal S8Client MockClient { get; private set; }

			public RoutingResponseHeaderCollectionWithMockClient(BusinessObjectFactory factory)
			: base(factory)
			{
			}

			public override IS8Client GetS8Client(Action<string> updateStatus)
			{
				MockClient = new S8Client();
				return MockClient;
			}

			protected override bool ShouldReportError()
			{
				return true;
			}
		}

		public class RoutingResponseHeaderCollectionForMutexTest : RoutingResponseHeaderCollection
		{
			public RoutingResponseHeaderCollectionForMutexTest(BusinessObjectFactory factory)
			: base(factory)
			{ }

			public override IS8Client GetS8Client(Action<string> updateStatus)
			{
				return new S8ClientForMutexTest();
			}

			protected override bool ShouldReportError()
			{
				return true;
			}
		}

		protected override RoutingResponseHeaderCollection GetCollectionToTest()
		{
			return new RoutingResponseHeaderCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RoutingResponseHeader("", Factory);
		}

		#endregion
	}
}
