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
	public class BookingDocumentsControllerTest : TestCaseWithFactory
	{
		public void TestPrintFreightLabels_NoPK()
		{
			var contactPK = Guid.NewGuid();
			SetupAuthenticatedContact(contactPK);

			var result = controller.PrintFreightLabels(null);

			AssertType<NotFoundResult>(result);
		}

		public void TestPrintFreightLabels()
		{
			var contactPK = Guid.NewGuid();
			SetupAuthenticatedContact(contactPK);

			var bookingPK = Guid.NewGuid();
			var printResult = new Mock<IWebTrackerPrintResult>();
			mockBookingDocumentsService.Setup(x => x.PrintFreightLabel(contactPK, bookingPK)).Returns(printResult.Object);

			var result = controller.PrintFreightLabels(bookingPK);

			AssertEquals(printResult.Object, ((WebTrackerPrintActionResult)result).WebTrackerPrintResult);
		}

		public void TestPrintFreightLabels_Staff()
		{
			var bookingPK = Guid.NewGuid();
			mockBookingDocumentsService.Setup(x => x.PrintFreightLabel(It.IsAny<Guid>(), bookingPK));

			var result = controller.PrintFreightLabels(bookingPK);

			mockBookingDocumentsService.Verify(x => x.PrintFreightLabel(Guid.Empty, bookingPK), Times.Never);
			AssertEquals(HttpStatusCode.Forbidden, ((StatusCodeResult)result).StatusCode);
		}

		public void TestPrintHouseBill_NoPK()
		{
			var contactPK = Guid.NewGuid();
			SetupAuthenticatedContact(contactPK);

			var result = controller.PrintFreightLabels(null);

			AssertType<NotFoundResult>(result);
		}

		public void TestPrintHouseBill()
		{
			var contactPK = Guid.NewGuid();
			SetupAuthenticatedContact(contactPK);

			var bookingPK = Guid.NewGuid();
			var printResult = new Mock<IWebTrackerPrintResult>();
			mockBookingDocumentsService.Setup(x => x.PrintHouseBill(contactPK, bookingPK)).Returns(printResult.Object);

			var result = controller.PrintHouseBill(bookingPK);

			AssertEquals(printResult.Object, ((WebTrackerPrintActionResult)result).WebTrackerPrintResult);
		}

		public void TestPrintHouseBill_Staff()
		{
			var bookingPK = Guid.NewGuid();
			var contents = new MemoryStream(new byte[8] { 0, 1, 2, 3, 4, 5, 6, 7 });
			mockBookingDocumentsService.Setup(x => x.PrintHouseBill(It.IsAny<Guid>(), bookingPK));

			var result = controller.PrintHouseBill(bookingPK);

			mockBookingDocumentsService.Verify(x => x.PrintHouseBill(Guid.Empty, bookingPK), Times.Never);
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
			mockBookingDocumentsService = new Mock<IBookingDocumentsService>();

			controller = new BookingDocumentsController(mockBookingDocumentsService.Object);
		}
		BookingDocumentsController controller;
		Mock<IBookingDocumentsService> mockBookingDocumentsService;
	}
}
