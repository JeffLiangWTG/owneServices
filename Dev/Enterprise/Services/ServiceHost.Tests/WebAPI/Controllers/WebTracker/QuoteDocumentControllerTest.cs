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
	class QuoteDocumentsControllerTest : TestCaseWithFactory
	{
		public void TestPrint_NoPK()
		{
			var contactPK = Guid.NewGuid();
			SetupAuthenticatedContact(contactPK);

			var result = controller.Print(null);

			AssertType<NotFoundResult>(result);
		}

		public void TestPrint()
		{
			var contactPK = Guid.NewGuid();
			SetupAuthenticatedContact(contactPK);

			var quotePK = Guid.NewGuid();
			var printResult = new Mock<IWebTrackerPrintResult>();
			mockQuoteDocumentsService.Setup(x => x.Print(contactPK, quotePK)).Returns(printResult.Object);

			var result = controller.Print(quotePK);

			AssertEquals(printResult.Object, ((WebTrackerPrintActionResult)result).WebTrackerPrintResult);
		}

		public void TestPrint_Staff()
		{
			var quotePK = Guid.NewGuid();
			var printResult = new Mock<IWebTrackerPrintResult>();
			mockQuoteDocumentsService.Setup(x => x.Print(It.IsAny<Guid>(), quotePK)).Returns(printResult.Object);

			var result = controller.Print(quotePK);

			mockQuoteDocumentsService.Verify(x => x.Print(Guid.Empty, quotePK), Times.Never);
			AssertEquals(HttpStatusCode.Forbidden, ((StatusCodeResult)result).StatusCode);
		}

		void SetupAuthenticatedContact(Guid contactPK)
		{
			var mockIdentity = new Mock<IGlowAuthenticationTicketIdentity>();
			mockIdentity.SetupGet(x => x.IsAuthenticated).Returns(true);
			mockIdentity.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);
			mockIdentity.SetupGet(x => x.ProviderKey).Returns(contactPK);
			controller.User = new GenericPrincipal(mockIdentity.Object, null);
		}

		protected override void SetUp()
		{
			mockQuoteDocumentsService = new Mock<IQuoteDocumentsService>();

			controller = new QuoteDocumentsController(mockQuoteDocumentsService.Object);
		}
		Mock<IQuoteDocumentsService> mockQuoteDocumentsService;
		QuoteDocumentsController controller;
	}
}
