using System;
using System.Threading;
using CargoWise.Application;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(TransportReports))]
	sealed class OceanCarrierBookingAnalysisReportModuleTest : ZEmbeddedModuleBasherTest
	{
		public void TestGetOceanCarrierBookingAnalysisReportUri_TokenError()
		{
			using (MockPerformanceReportingUrlGenerator(tokenValue: null, tokenErrorMessage: "error 1001"))
			{
				var module = new OceanCarrierBookingAnalysisReportModule();
				var returnUrl = module.Url;
				module.Dispose();
				AssertContains("While attempting to create an authentication token for global tracking performance reports the following error was encountered. error 1001.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(returnUrl);
			}
		}

		public void TestGetOceanCarrierBookingAnalysisReportUri_UrlError()
		{
			using (MockPerformanceReportingUrlGenerator(url: null, urlErrorMessage: "error 1002"))
			{
				var module = new OceanCarrierBookingAnalysisReportModule();
				var returnUrl = module.Url;
				module.Dispose();
				AssertContains("Unable to create performance report URL : error 1002", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(returnUrl);
			}
		}

		public void TestGetOceanCarrierBookingAnalysisReportUri()
		{
			using (MockPerformanceReportingUrlGenerator())
			{
				var module = new OceanCarrierBookingAnalysisReportModule();
				var returnUrl = module.Url;
				module.Dispose();
				AssertEquals(returnUrl, "http://www.pr.com");
			}
		}

		public void TestGetOceanCarrierBookingAnalysisReportModuleId()
		{
			using (MockPerformanceReportingUrlGenerator())
			{
				var module = new OceanCarrierBookingAnalysisReportModule();
				var moduleId = module.ID;
				module.Dispose();
				AssertEquals(moduleId.ID, ModuleId.OceanCarrierBookingAnalysisReport);
				AssertEquals(moduleId.Description, "Ocean Carrier Booking Analysis Report");
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.TransportReports;
		}

		IDisposable MockPerformanceReportingUrlGenerator(string tokenValue = "authToken", string tokenErrorMessage = null, string url = "http://www.pr.com", string urlErrorMessage = null)
		{
			var token = new PerformanceReportingAuthToken(tokenValue);
			var tokenResult = new PerformanceReportingAuthTokenResult(token, tokenErrorMessage);
			var uri = string.IsNullOrEmpty(url) ? null : new Uri(url);
			var tokenProviderMock = new Mock<IPerformanceReportingUrlGenerator>();
			tokenProviderMock.CallBase = true;
			tokenProviderMock.Setup(m => m.GetToken(It.IsAny<string>(), It.IsAny<OrgContact>(), It.IsAny<CancellationToken>())).Returns(tokenResult);
			tokenProviderMock.Setup(t => t.Generate("login", token, false)).Returns((uri, urlErrorMessage));
			return ObjectFactory.Substitute(tokenProviderMock.Object);
		}
	}
}

