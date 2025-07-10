using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Dash.Business.Extensions;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class FeatureToggleHandlerTest : TestCaseWithFactory
	{
		public void TestEAdaptorNextFeatureEnabled()
		{
			var mockIFeatureData = new Mock<IFeatureData>();
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.EAdaptorNextFeature, CancellationToken.None))
				.Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			using (var handler = new FeatureToggleHandler())
			using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo.fr/eAdaptorNext"))
			{
				handler.InnerHandler = new TestHandler(HttpStatusCode.OK);
				using (var invoker = new HttpMessageInvoker(handler))
				{
					var result = invoker.SendAsync(httpRequestMessage, new CancellationToken()).Result;
					AssertEquals(HttpStatusCode.OK, result.StatusCode);
				}
			}
		}

		public void TestEAdaptorNextFeatureDisabled()
		{
			using (var handler = new FeatureToggleHandler())
			using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo.fr/eAdaptorNext"))
			{
				httpRequestMessage.SetConfiguration(new HttpConfiguration());
				handler.InnerHandler = new TestHandler(HttpStatusCode.OK);
				using (var invoker = new HttpMessageInvoker(handler))
				{
					var result = invoker.SendAsync(httpRequestMessage, CancellationToken.None).Result;
					AssertEquals(HttpStatusCode.NotFound, result.StatusCode);
					AssertEquals("\"eAdaptorNext features are deactivated, please contact WiseTech Global for further information.\"", result.Content.ReadAsString());
				}
			}
		}

		class TestHandler : DelegatingHandler
		{
			public TestHandler(HttpStatusCode httpStatusCode)
			{
				this.httpStatusCode = httpStatusCode;
			}

			readonly HttpStatusCode httpStatusCode;

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return Task.FromResult(new HttpResponseMessage(httpStatusCode));
			}
		}
	}
}
