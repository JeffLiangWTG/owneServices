using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WTG.WiseTechAcademy;

namespace Enterprise.MasterFiles.Business.Testing
{
	class WiseTechAcademyApiClientTest : TransactionedTestCase
	{
		public void TestApiClientBaseAddress()
		{
			var apiClient = CreateClient();
			AssertBaseAddress(apiClient, "http://nothing/");
		}

		public void TestApiClientBaseAddress_WhenNotRunningInTest()
		{
			RunOutsideIsRunningTestsFlag(() =>
			{
				var apiClient = CreateClient();
				AssertBaseAddress(apiClient, "https://lms.wisetechacademy.com");
			});
		}

		public void TestApiClientBaseAddress_WhenOverriddenInRegistry()
		{
			RunOutsideIsRunningTestsFlag(() =>
			{
				const string overriddenBaseAddress = "https://something/";
				RawDataRegistry.Instance.WiseTechAcademyLmsApiBaseAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overriddenBaseAddress);
				var apiClient = CreateClient();
				AssertBaseAddress(apiClient, overriddenBaseAddress);
			});
		}

		static void RunOutsideIsRunningTestsFlag(Action action)
		{
			TestingState.IsRunningTests = false;
			try
			{
				action();
			}
			finally
			{
				TestingState.IsRunningTests = true;
			}
		}

		IWiseTechAcademyApiClient CreateClient() => ObjectFactory.Get<IWiseTechAcademyApiClient>(nameof(IWiseTechAcademyApiClient), messageHandlerMock.Object);

		void AssertBaseAddress(IWiseTechAcademyApiClient apiClient, string expectedBaseAddress)
		{
			string requestUri = default;
			messageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("Course Name") })
				.Callback<HttpRequestMessage, CancellationToken>((message, token) => requestUri = message.RequestUri.AbsoluteUri);

			_ = apiClient.GetLearningUnitName("123");
			AssertStartsWith("Request absolute URI should start with expected base address", expectedBaseAddress, requestUri);
		}

		protected override void SetUp()
		{
			base.SetUp();
			messageHandlerMock = new(MockBehavior.Strict);
		}

		Mock<HttpMessageHandler> messageHandlerMock;
	}
}
