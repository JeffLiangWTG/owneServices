using System;
using System.Linq;
using System.Threading;
using System.Web.Http.Results;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class PerformanceReportingControllerTest : TestCaseWithFactory
	{
		public void TestGetUrl_NoReport()
		{
			SetupContactPrincipal();
			var result = controller.GetUrl(null, CancellationToken.None);

			AssertType<BadRequestResult>(result);
		}

		public void TestGetUrl_ErrorWhenRetrievingToken()
		{
			var contact = SetupContactPrincipal();
			mockPerformanceReportingUrlGenerator
				.Setup(x => x.GetToken(It.IsAny<string>(), It.Is<OrgContact>(c => c.PK == contact.PK), CancellationToken.None))
				.Returns(new PerformanceReportingAuthTokenResult(null, "Error message while getting token."));

			var result = (JsonResult<PerformanceReportingUrlResult>)controller.GetUrl("report1", CancellationToken.None);
			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("Error message while getting token.", result.Content.Errors.SingleOrDefault());
			});
		}

		public void TestGetUrl_ErrorWhenGeneratingUrl()
		{
			var contact = SetupContactPrincipal();
			var token = new PerformanceReportingAuthToken("sometoken");
			mockPerformanceReportingUrlGenerator
				.Setup(x => x.GetToken(It.IsAny<string>(), It.Is<OrgContact>(c => c.PK == contact.PK), CancellationToken.None))
				.Returns(new PerformanceReportingAuthTokenResult(token, null));

			mockPerformanceReportingUrlGenerator
				.Setup(x => x.Generate("reports/report1", token, true))
				.Returns((null, "Error message from url generator."));

			var result = (JsonResult<PerformanceReportingUrlResult>)controller.GetUrl("report1", CancellationToken.None);
			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("Error message from url generator.", result.Content.Errors.SingleOrDefault());
			});
		}

		public void TestGetUrl()
		{
			var contact = SetupContactPrincipal();
			var token = new PerformanceReportingAuthToken("sometoken");
			mockPerformanceReportingUrlGenerator
				.Setup(x => x.GetToken(It.IsAny<string>(), It.Is<OrgContact>(c => c.PK == contact.PK), CancellationToken.None))
				.Returns(new PerformanceReportingAuthTokenResult(token, null));

			mockPerformanceReportingUrlGenerator
				.Setup(x => x.Generate("reports/report1", token, true))
				.Returns((new Uri("https://wisegrid.net/reporturl"), string.Empty));

			var result = (JsonResult<PerformanceReportingUrlResult>)controller.GetUrl("report1", CancellationToken.None);
			CombineAssertions(() =>
			{
				AssertEquals("https://wisegrid.net/reporturl", result.Content.Url);
				AssertNull(result.Content.Errors);
			});
		}

		public void TestGetUrl_WithStaff()
		{
			SetupStaffPrincipal();

			var token = new PerformanceReportingAuthToken("sometoken");
			mockPerformanceReportingUrlGenerator
				.Setup(x => x.GetToken(It.IsAny<string>(), null, CancellationToken.None))
				.Returns(new PerformanceReportingAuthTokenResult(token, null));

			mockPerformanceReportingUrlGenerator
				.Setup(x => x.Generate("reports/report1", token, true))
				.Returns((new Uri("https://wisegrid.net/reporturl"), string.Empty));

			var result = (JsonResult<PerformanceReportingUrlResult>)controller.GetUrl("report1", CancellationToken.None);
			CombineAssertions(() =>
			{
				AssertEquals("https://wisegrid.net/reporturl", result.Content.Url);
				AssertNull(result.Content.Errors);
			});
		}

		OrgContact SetupContactPrincipal()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			return contact;
		}

		GlbStaff SetupStaffPrincipal()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			return staff;
		}

		protected override void SetUp()
		{
			controller = new PerformanceReportingController();
			mockPerformanceReportingUrlGenerator = new Mock<IPerformanceReportingUrlGenerator>();
			ObjectFactory.Substitute(nameof(IPerformanceReportingUrlGenerator), mockPerformanceReportingUrlGenerator.Object);
		}

		PerformanceReportingController controller;
		Mock<IPerformanceReportingUrlGenerator> mockPerformanceReportingUrlGenerator;
	}
}
