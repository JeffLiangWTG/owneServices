using System;
using System.Net;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.Tracking.Business;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/bookingDocuments")]
	public class BookingDocumentsController : ApiController
	{
		public BookingDocumentsController() : this(new BookingDocumentsService())
		{
		}

		public BookingDocumentsController(IBookingDocumentsService bookingDocumentsService)
		{
			this.bookingDocumentsService = bookingDocumentsService ?? throw new ArgumentNullException(nameof(bookingDocumentsService));
		}
		readonly IBookingDocumentsService bookingDocumentsService;

		[Route("printHouseBill/{shipmentPK}")]
		[HttpGet]
		public IHttpActionResult PrintHouseBill(Guid? shipmentPK)
		{
			if (shipmentPK == null)
			{
				return NotFound();
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
				var contactPK = identity?.GetContactPK();
				if (contactPK == null)
				{
					return StatusCode(HttpStatusCode.Forbidden);
				}

				var printResult = bookingDocumentsService.PrintHouseBill(contactPK.Value, shipmentPK.Value);

				return new WebTrackerPrintActionResult(printResult);
			}
		}

		[Route("printFreightLabels/{shipmentPK}")]
		[HttpGet]
		public IHttpActionResult PrintFreightLabels(Guid? shipmentPK)
		{
			if (shipmentPK == null)
			{
				return NotFound();
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
				var contactPK = identity?.GetContactPK();
				if (contactPK == null)
				{
					return StatusCode(HttpStatusCode.Forbidden);
				}

				var printResult = bookingDocumentsService.PrintFreightLabel(contactPK.Value, shipmentPK.Value);

				return new WebTrackerPrintActionResult(printResult);
			}
		}
	}
}
