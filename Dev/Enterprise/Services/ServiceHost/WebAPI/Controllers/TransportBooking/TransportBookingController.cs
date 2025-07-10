using System;
using System.Linq;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class TransportBookingController : ApiController
	{
		[Route("api/TransportBooking/TransportBooking/{parentPK}/{parentTableCode}/{direction}/{isCombineContainers}/{isCreate}")]
		[HttpGet]
		public IHttpActionResult TransportBooking([FromUri] Guid parentPK, [FromUri] string parentTableCode, [FromUri] string direction, [FromUri] bool isCombineContainers, [FromUri] bool isCreate = true)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var bookingDirection = (DtbBookingDirection)Enum.Parse(typeof(DtbBookingDirection), direction);
				var deliveryManagerFactory = ObjectFactory.Get<IDtbDeliveryManagerFactory>();
				var errorManager = ObjectFactory.Get<IAutomatedDtbBookingCreationErrorManager>();
				var factory = new BusinessObjectFactory();
				var businessObject = factory.Load(parentTableCode, parentPK);

				if (businessObject is IDtbBookingParent parent)
				{
					var dtbDeliveryManager = deliveryManagerFactory.CreateManager(factory, parent, bookingDirection, isCombineContainers, null, errorManager);
					var transportBookings = dtbDeliveryManager.DeliverTransportBooking();
					if (transportBookings == null || !transportBookings.Any())
					{
						return Json(
							new JsonResponse(
								MessageTypes.Error,
								isCreate ? Res.GetString("0993c5e3-98c0-46b4-b98b-9c8e9df70e02", "Cannot Create Transport Booking. Check DEX logs for details.") : Res.GetString("6618de88-ac75-41a7-8ea3-ccb5cf3ff084", "Cannot Update Transport Booking. Check DEX logs for details.")
							));
					}
				}
				else
				{
					return Json(new JsonResponse(MessageTypes.Error, Res.GetString("1c8e088a-03e7-45cf-a3a2-b74c2b21f060", "{0} does not support Transport Booking.", parentTableCode)));
				}
			}

			return Json(
				new JsonResponse(
					MessageTypes.Success,
					isCreate ? Res.GetString("dbe1910e-76f6-4b08-9c9d-0e638dff9660", "Transport Booking Created Successfully.") : Res.GetString("0fe6eb0b-1c9a-4d19-934a-7115534f6ac5", "Transport Booking Updated Successfully.")
				));
		}

		public class JsonResponse
		{
			public JsonResponse(string messageType, string message)
			{
				this.messageType = messageType;
				this.message = message;
			}
			public string messageType { get; set; }
			public string message { get; set; }
		}
	}
}
