using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Test;
using Enterprise.Registry.Business;
using Moq;
using WTG.CreditCheck;
using WTG.ROPE.Model;
using static Enterprise.MasterFiles.GUI.Test.ImageBitmapTestHelper;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class CreditCheckUserControlTest : TestCaseWithFactory
	{
		public void TestGetMainPageModel()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var creditReportItem = new CreditReportItem();
			creditReportItem.CountryEnabledForCompany = true;
			creditReportItem.CountryEnabledForOrganisation = true;
			creditReportItem.ComprehensiveReportEnabled = true;
			var creditReportItemCollection = new CreditReportItemCollection
			{
				creditReportItem
			};

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			using (var creditReportUserControl = new CreditReportUserControl())
			{
				var mockService = new Mock<ISupportCreditCheckService>();
				mockService.Setup(service => service.GetHttpClientHandler(It.IsAny<string>())).Returns(new MockHttpMessageHandler());
				mockService.Setup(service => service.IsProductionSystem).Returns(creditReportUserControl.ServiceWrapper.IsProductionSystem);
				mockService.Setup(service => service.EndpointAddresses).Returns(creditReportUserControl.ServiceWrapper.EndpointAddresses);
				creditReportUserControl.ServiceWrapper = mockService.Object;
				creditReportUserControl.SetDataBinding(orgHeader, string.Empty);
				var mainPageControls = creditReportUserControl.Controls.Find("MainPageControl", true).Cast<MainPageControl>();
				AssertEquals(1, mainPageControls.Count());
				var mainPageControl = mainPageControls.Single();
				var model = mainPageControl.BindingSource.Current as MainPageModel;
				AssertEquals(1, model.ReportsModel.Reports.Count);
				var reportInfo = model.ReportsModel.Reports[0];
				AssertEquals("Get Report", reportInfo.ButtonCaption);
				AssertEquals(CreditReportType.ComprehensiveReport, reportInfo.CreditReportType);
				Assert(reportInfo.IsGet);
				AssertEquals(null, reportInfo.LastReportDate);
				AssertEquals("More Info", reportInfo.LastReportDateOrMoreInfo);
				AssertEquals(0, reportInfo.RelatedLatestEvents.Count());
				AssertEquals(false, reportInfo.ToolTipIconVisible);
				AssertEquals("#EE443A", reportInfo.ToolTipBackgroundColor);
				AssertEquals("There have been 0 events since the last report. Renew now for current information.", reportInfo.ToolTipCaption);
				AssertImageBitsEquals(Properties.Resources.WarningRed, reportInfo.ToolTipIcon);
			}
		}

		public void TestCreditCheckServiceThrowCertificateMismatchedExceptionFromWinForm()
		{
			AssertCCSCertificateClearedWhenCertificateMismatchedExceptionThrown((OrgHeader orgHeader, CreditReportUserControl creditReportUserControl, CreditCheckUserControl _) =>
			{
				var mainPageControl = creditReportUserControl.Controls.Find("MainPageControl", true).Cast<MainPageControl>().Single();
				var eventsBannerModel = (mainPageControl.BindingSource.DataSource as MainPageModel)?.EventsBannerModel;
				AssertNotNull(eventsBannerModel);
				AssertEquals("Credit report service is unavailable, please reload the form and try again later.", eventsBannerModel.ErrorMessage);
			});
		}

		void AssertCCSCertificateClearedWhenCertificateMismatchedExceptionThrown(Action<OrgHeader, CreditReportUserControl, CreditCheckUserControl> assertCCSRequestWithCertificateMismatchedException)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var customerCode = orgHeader.CustomsCodes.AddNew();
			customerCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			customerCode.OK_CustomsRegNo = "750506131";

			Factory.Save();

			var creditReportItem = new CreditReportItem();
			creditReportItem.CountryEnabledForCompany = true;
			creditReportItem.CountryEnabledForOrganisation = true;
			creditReportItem.CommercialBureauEnquiryEnabled = true;
			creditReportItem.FailureRiskEnabled = true;
			creditReportItem.ComprehensiveReportEnabled = true;
			creditReportItem.LatePaymentRiskEnabled = true;

			var creditReportItemCollection = new CreditReportItemCollection();
			creditReportItemCollection.Add(creditReportItem);

			CreditCheckUserControl creditCheckUserControl = null;
			ICreditCheckService creditCheckService = null;
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.CreditReportsPublicCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new byte[] { 1, 2, 3 }))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				using (var creditReportUserControl = new CreditReportUserControl())
				{
					var mockService = new Mock<ISupportCreditCheckService>();
					mockService.Setup(service => service.GetHttpClientHandler(It.IsAny<string>())).Returns(new HttpMessageHandlerWithCertificateMismatchedException());
					mockService.Setup(service => service.IsProductionSystem).Returns(true);
					mockService.Setup(service => service.EndpointAddresses).Returns(creditReportUserControl.ServiceWrapper.EndpointAddresses);
					creditReportUserControl.ServiceWrapper = mockService.Object;
					AssertEquals(new byte[] { 1, 2, 3 }, OrganisationsDataRegistry.Instance.CreditReportsPublicCertificate.Value);
					creditReportUserControl.SetDataBinding(orgHeader, string.Empty);
					creditCheckUserControl = creditReportUserControl.Controls.Find("CreditCheckControl", true).Cast<CreditCheckUserControl>().Single();
					creditCheckService = GetCreditCheckService(creditCheckUserControl);
					AssertNotNull(creditCheckService);
					assertCCSRequestWithCertificateMismatchedException?.Invoke(orgHeader, creditReportUserControl, creditCheckUserControl);
					AssertEquals(Array.Empty<byte>(), OrganisationsDataRegistry.Instance.CreditReportsPublicCertificate.Value);
				}

				creditCheckService = GetCreditCheckService(creditCheckUserControl);
				AssertNull(creditCheckService);
			}
		}

		ICreditCheckService GetCreditCheckService(CreditCheckUserControl creditCheckUserControl)
		{
			return creditCheckUserControl.GetType().GetField("creditCheckService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(creditCheckUserControl) as ICreditCheckService;
		}
	}

	public class HttpMessageHandlerWithCertificateMismatchedException : HttpClientHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			throw new ApiException(string.Empty, (int)HttpStatusCode.InternalServerError, "ErrorCode:1004, ErrorMessage:Decrypt payload failed", null, null);
		}
	}
}
