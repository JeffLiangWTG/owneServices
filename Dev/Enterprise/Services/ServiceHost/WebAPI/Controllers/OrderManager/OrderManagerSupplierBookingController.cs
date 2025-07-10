using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Services.ServiceHost
{
	[RoutePrefix("api/orderManager/supplierBooking")]
	[GlowTicketAuthentication]
	public sealed class OrderManagerSupplierBookingController : OrderManagerController
	{
		public OrderManagerSupplierBookingController(IGlowContactSecurityService securityService) : base(securityService)
		{
		}

		public OrderManagerSupplierBookingController() : this(new GlowContactSecurityService())
		{
		}

		[Route("cancelSupplierBooking")]
		[HttpPost]
		public IHttpActionResult CancelSupplierBooking([FromBody] CancelSupplierBookingArgs args)
		{
			var supplierBookingPK = args.SupplierBookingPK;
			if (supplierBookingPK == Guid.Empty)
			{
				return BadRequest(InvalidParameters);
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = GetNewBusinessObjectFactory();
				var collection = new DynamicBusinessObjectCollection(factory);
				var query = FormattableString.Invariant($@"
					SELECT packLine.JL_PK, bookingLine.JSL_BookingLineId
					FROM dbo.JobPackLines AS packLine
					INNER JOIN dbo.JobSupplierBookingLine AS bookingLine ON packLine.JL_JSL_BookingLine = bookingLine.JSL_PK
					WHERE bookingLine.JSL_JSB_Booking = @supplierBookingPK AND NOT EXISTS (SELECT 1 FROM ContainerLoadListLine WHERE CLL_JL_PackLine = packLine.JL_PK)
				");
				var parameters = new ZSqlParameterCollection
				{
					{ "@supplierBookingPK", supplierBookingPK, Schema.GenericGuidSchemaColumn }
				};
				collection.Load(query, parameters);

				var supplierBooking = factory.Load<JobSupplierBooking>(supplierBookingPK);

				var recordsByPackLine = collection.Select(dynamicObject => new
				{
					JL_PK = (ZGuid)dynamicObject[JobPackLinesSchema.Constants.PK],
					JSL_BookingLineId = (ZString)dynamicObject[JobSupplierBookingLineSchema.Constants.JSL_BookingLineId]
				}).ToDictionary(x => x.JL_PK);

				var packLinesQuery = new ZQuery(JobPackLinesSchema.PK, recordsByPackLine.Keys);
				var packLines = factory.Load<PackLine>(packLinesQuery);

				foreach (var packLine in packLines)
				{
					var supplierBookingLineId = recordsByPackLine[packLine.PK].JSL_BookingLineId;

					var eventParameters = new KeyValuePair<string, string>[]
					{
							new (Params.DeclarationID, $"Pack line {packLine.JL_PackLineId} with supplier booking line {supplierBookingLineId}"),
							new (Params.Reason, $"Supplier booking {supplierBooking.JSB_BookingId} was cancelled"),
					};

					packLine.Shipment.Logs.CreateOrRecreateEventLog(
						Events.DeletedARecordInTheSystem,
						EstimateActual.Actual,
						ZDateTimeOffset.Now,
						$"Deleted a record in the system pack line {packLine.JL_PackLineId} with supplier booking line {supplierBookingLineId} because supplier booking {supplierBooking.JSB_BookingId} was cancelled.",
						eventParameters);

					packLine.DeactivateActiveBusinessObjectCollections();
					packLine.Delete();
				}

				if (supplierBooking != null)
				{
					supplierBooking.OrderShipmentPlannings.DeleteAll();
				}

				factory.Save();
			}

			return Ok();
		}

		static string InvalidParameters => Res.GetString("32029992-044b-d1c0-a825-995b2d149697", "Please provide valid Supplier Booking details.");

		static BusinessObjectFactory GetNewBusinessObjectFactory() => new BusinessObjectFactory() { NameForDebugging = "OrderManagerSupplierBookingController Factory" };
	}

	public sealed class CancelSupplierBookingArgs
	{
		[DataMember(Name = "supplierBookingPK")]
		public Guid SupplierBookingPK { get; set; }
	}
}
