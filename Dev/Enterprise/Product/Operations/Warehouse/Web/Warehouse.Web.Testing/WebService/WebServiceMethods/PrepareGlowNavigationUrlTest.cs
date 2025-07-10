using System;
using CargoWise.Application;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;
using static Enterprise.Warehouse.Web.WebService.WhsSecureService;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PrepareGlowNavigationUrlTest : WhsSecureServiceTestCase
	{
		#region TestPrepareGlowNavigationUrl

		public void TestPrepareGlowNavigationUrl_CycleCount()
		{
			var webService = GetNewWebService();

			var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
			singleSignOnHelperMock
				.Setup(h => h.CreateLimitedToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<GlowSingleSignOnTokenOptions>()))
				.Returns("sometoken");

			using (ObjectFactory.Substitute(singleSignOnHelperMock.Object))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			{
				var response = webService.PrepareGlowNavigationUrl(GlowModule.CycleCount);
				AssertSuccessfulResponse(response, webService);
				AssertEquals("https://address/goto/PerformCycleCount?sso_otp=sometoken", response.GlowSSONavigationUrl);
			}
		}

		public void TestPrepareGlowNavigationUrl_Load()
		{
			var webService = GetNewWebService();

			var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
			singleSignOnHelperMock
				.Setup(h => h.CreateLimitedToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<GlowSingleSignOnTokenOptions>()))
				.Returns("sometoken");

			using (ObjectFactory.Substitute(singleSignOnHelperMock.Object))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			{
				var response = webService.PrepareGlowNavigationUrl(GlowModule.Load);
				AssertSuccessfulResponse(response, webService);
				AssertEquals("https://address/goto/StartLoad?sso_otp=sometoken", response.GlowSSONavigationUrl);
			}
		}

		public void TestPrepareGlowNavigationUrl_Service()
		{
			var webService = GetNewWebService();

			var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
			singleSignOnHelperMock
				.Setup(h => h.CreateLimitedToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<GlowSingleSignOnTokenOptions>()))
				.Returns("sometoken");

			using (ObjectFactory.Substitute(singleSignOnHelperMock.Object))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			{
				var response = webService.PrepareGlowNavigationUrl(GlowModule.Service);
				AssertSuccessfulResponse(response, webService);
				AssertEquals("https://address/goto/RFService?sso_otp=sometoken", response.GlowSSONavigationUrl);
			}
		}

		public void TestPrepareGlowNavigationUrl_PackingConsolidation()
		{
			var webService = GetNewWebService();

			var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
			singleSignOnHelperMock
				.Setup(h => h.CreateLimitedToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<GlowSingleSignOnTokenOptions>()))
				.Returns("sometoken");

			using (ObjectFactory.Substitute(singleSignOnHelperMock.Object))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			{
				var response = webService.PrepareGlowNavigationUrl(GlowModule.PackingConsolidation);
				AssertSuccessfulResponse(response, webService);
				AssertEquals("https://address/goto/StartPackingConsolidation?sso_otp=sometoken", response.GlowSSONavigationUrl);
			}
		}

		#endregion
	}
}
