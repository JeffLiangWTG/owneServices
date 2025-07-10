using System;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Web.Http.Results;
using CargoWise.EntityFramework.Testing;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class StatementsControllerTest : TestCaseWithFactory
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

			var companyPK = Guid.NewGuid();
			var printResult = new Mock<IWebTrackerPrintResult>();
			mockStatementService.Setup(x => x.Print(contactPK, companyPK)).Returns(printResult.Object);

			var result = controller.Print(companyPK);

			AssertEquals(printResult.Object, ((WebTrackerPrintActionResult)result).WebTrackerPrintResult);
		}

		public void TestPrint_Staff()
		{
			var companyPK = Guid.NewGuid();
			var contents = new MemoryStream(new byte[8] { 0, 1, 2, 3, 4, 5, 6, 7 });
			var printResult = new Mock<IWebTrackerPrintResult>();
			mockStatementService.Setup(x => x.Print(It.IsAny<Guid>(), companyPK)).Returns(printResult.Object);

			var result = controller.Print(companyPK);

			mockStatementService.Verify(x => x.Print(Guid.Empty, companyPK), Times.Never);
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
			mockStatementService = new Mock<IStatementService>();

			controller = new StatementsController(mockStatementService.Object);
		}
		StatementsController controller;
		Mock<IStatementService> mockStatementService;
	}
}
