using System;
using System.Net;
using System.Security.Principal;
using System.Web.Http.Results;
using CargoWise.EntityFramework.Testing;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class InvoicingControllerTest : TestCaseWithFactory
	{
		public void TestPrint_NoPK()
		{
			var contactPK = Guid.NewGuid();
			var identity = SetupAuthenticatedContact(contactPK);
			mockGlowContactSecurityService.Setup(x => x.HasGroupRole(identity, "webinvoiceviewer")).Returns(true);

			var result = controller.Print(null);

			AssertType<NotFoundResult>(result);
		}

		public void TestPrint()
		{
			var contactPK = Guid.NewGuid();
			var identity = SetupAuthenticatedContact(contactPK);
			mockGlowContactSecurityService.Setup(x => x.HasGroupRole(identity, "webinvoiceviewer")).Returns(true);

			var invoicePK = Guid.NewGuid();
			var printResult = new Mock<IWebTrackerPrintResult>();
			mockInvoicingService.Setup(x => x.Print(contactPK, invoicePK)).Returns(printResult.Object);

			var result = controller.Print(invoicePK);

			AssertEquals(printResult.Object, ((WebTrackerPrintActionResult)result).WebTrackerPrintResult);
		}

		public void TestPrint_NoRole()
		{
			var contactPK = Guid.NewGuid();
			var identity = SetupAuthenticatedContact(contactPK);
			mockGlowContactSecurityService.Setup(x => x.HasGroupRole(identity, "webinvoiceviewer")).Returns(false);

			var invoicePK = Guid.NewGuid();
			var printResult = new Mock<IWebTrackerPrintResult>();
			mockInvoicingService.Setup(x => x.Print(It.IsAny<Guid>(), invoicePK)).Returns(printResult.Object);

			var result = controller.Print(invoicePK);

			mockInvoicingService.Verify(x => x.Print(Guid.Empty, invoicePK), Times.Never);
			AssertEquals(HttpStatusCode.Forbidden, ((StatusCodeResult)result).StatusCode);
		}

		public void TestPrint_Staff()
		{
			var mockIdentity = new Mock<IGlowAuthenticationTicketIdentity>();
			mockIdentity.SetupGet(x => x.IsAuthenticated).Returns(true);
			mockIdentity.SetupGet(x => x.ProviderType).Returns(GlbStaffSchema.Constants.Prefix);
			mockIdentity.SetupGet(x => x.ProviderKey).Returns(Guid.NewGuid());
			controller.User = new GenericPrincipal(mockIdentity.Object, null);
			mockGlowContactSecurityService.Setup(x => x.HasGroupRole(It.IsAny<IGlowAuthenticationTicketIdentity>(), It.IsAny<string>()));

			var invoicePK = Guid.NewGuid();
			var result = controller.Print(invoicePK);

			mockGlowContactSecurityService.Verify(x => x.HasGroupRole(It.IsAny<IGlowAuthenticationTicketIdentity>(), It.IsAny<string>()), Times.Never);
			AssertEquals(HttpStatusCode.Forbidden, ((StatusCodeResult)result).StatusCode);
		}

		IGlowAuthenticationTicketIdentity SetupAuthenticatedContact(Guid contactPK)
		{
			var mockIdentity = new Mock<IGlowAuthenticationTicketIdentity>();
			mockIdentity.SetupGet(x => x.IsAuthenticated).Returns(true);
			mockIdentity.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);
			mockIdentity.SetupGet(x => x.ProviderKey).Returns(contactPK);
			controller.User = new GenericPrincipal(mockIdentity.Object, null);

			return mockIdentity.Object;
		}

		protected override void SetUp()
		{
			mockInvoicingService = new Mock<IInvoicingService>();
			mockGlowContactSecurityService = new Mock<IGlowContactSecurityService>();

			controller = new InvoicingController(mockInvoicingService.Object, mockGlowContactSecurityService.Object);
		}
		InvoicingController controller;
		Mock<IInvoicingService> mockInvoicingService;
		Mock<IGlowContactSecurityService> mockGlowContactSecurityService;
	}
}
