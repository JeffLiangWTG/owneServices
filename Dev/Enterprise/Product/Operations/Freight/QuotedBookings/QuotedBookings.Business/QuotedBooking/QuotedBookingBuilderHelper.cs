using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	static class QuotedBookingBuilderHelper
	{
		public static QuotedBooking PopulateQuotedBookingFromOrder(Order order, QuotedBooking booking)
		{
			booking.TransportMode = order.JD_TransportMode;
			booking.ContainerMode = order.JD_ContainerMode;
			booking.QuotedBookingContainers.AddRange(order.PlannedContainers.Cast<OrderContainer>().Select(CreateForwardingContainerFromOrderContainer));

			booking.Booking.ConsigneeDocumentaryAddress.ShouldAlwaysUpdateSecondary = true;
			booking.Booking.ConsignorDocumentaryAddress.ShouldAlwaysUpdateSecondary = true;
			booking.Booking.ConsigneeDocumentaryAddress.E2_OA_Address = order.JD_OA_BuyerAddress;
			booking.Booking.ConsignorDocumentaryAddress.E2_OA_Address = order.JD_OA_SupplierAddress;

			booking.Booking.JS_HouseBill = order.JD_Waybill.Left(20);
			booking.Booking.JS_INCO = order.JD_IncoTerm;

			booking.ControllingCustomerDocumentaryAddress.CopyPersistentValuesFrom(order.ControllingCustomerDocAddress);
			booking.ControllingAgentDocumentaryAddress.CopyPersistentValuesFrom(order.ControllingAgentDocAddress);

			var manufacturerAddress = order.DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer);
			var consignorPickupAddress = order.GoodsAvailableAtAddress.Address
				?? manufacturerAddress?.Address
				?? order.SupplierAddress;
			if (consignorPickupAddress != null)
			{
				booking.Booking.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.PK;
			}

			var consingeeDeliveryAddress = order.GoodsDeliveredToAddress.Address ?? order.BuyerAddress;
			if (consingeeDeliveryAddress != null)
			{
				booking.Booking.ConsigneeDeliveryAddress.E2_OA_Address = consingeeDeliveryAddress.PK;
			}

			booking.ServiceLevel = order.JD_RS_NKServiceLevel_NI;

			using (booking.Booking.SuspendSettingConsignorFromOrigin())
			using (booking.Booking.SuspendSettingConsigneeFromDestination())
			{
				booking.Origin = order.JD_RL_NKGoodsAvailableAt;
				booking.Destination = order.JD_RL_NKGoodsDeliveredTo;
			}

			booking.OH_Carrier = order.JD_OH_Carrier;

			booking.LoadPort = order.JD_RL_NKPortOfLoading;
			booking.DischargePort = order.JD_RL_NKPortOfDischarge;
			order.JD_JS = booking.Booking.PK;

			if (booking.Booking.UpdatePackLines && booking.Booking.OuterPackLines.Count > 0)
			{
				using (new DisposableAction(() => booking.Booking.SuppressPackLinesUpdate = true, () => booking.Booking.SuppressPackLinesUpdate = false))
				{
					UpdateBookingFromOrderPlan(order, booking);
				}
			}
			else
			{
				UpdateBookingFromOrderPlan(order, booking);
			}

			return booking;
		}

		static void UpdateBookingFromOrderPlan(Order order, QuotedBooking booking)
		{
			(booking as IBuyerSupplierRelationshipConsumer).GoodsDescription = order.JD_OrderGoodsDescription;
			booking.Booking.JS_F3_NKPackType = order.JD_F3_NKPackType;
			booking.Booking.JS_OuterPacks = order.JD_Packs;
			booking.Booking.JS_ActualVolume = order.JD_ActualVolume;
			booking.Booking.JS_UnitOfVolume = order.JD_UnitOfVolume;
			booking.Booking.JS_ActualWeight = order.JD_ActualWeight;
			booking.Booking.JS_UnitOfWeight = order.JD_UnitOfWeight;
		}

		public static ZQuery GetSailingScheduleQuery(Order order)
		{
			var carrierQuery = ConstructQuery(JobVoyageSchema.JV_OH_Line, order.JD_OH_Carrier);
			var vesselQuery = ConstructQuery(JobVoyageSchema.JV_RV_NKVessel, order.JD_RV_NKDepartureVessel);
			var voyageQuery = ConstructQuery(JobVoyageSchema.JV_VoyageFlight, order.JD_DepartureVoyage);

			var sailingQuery = new ZDBOnlyQuery(typeof(JobSailing));
			sailingQuery.AddToFilter(carrierQuery, JoinCondition.And);
			sailingQuery.AddToFilter(vesselQuery, JoinCondition.And);
			sailingQuery.AddToFilter(voyageQuery, JoinCondition.And);

			return sailingQuery;
		}

		#region Implementation

		static ZQuery ConstructQuery(SchemaColumn column, IZType property)
		{
			var query = new ZQuery(column, property);

			var jobVoyageQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
			jobVoyageQuery.AddToFilter(query);

			var originQuery = new ZDBOnlyQuery(typeof(VoyageOrigin));
			originQuery.AddSubQuery(jobVoyageQuery, JoinCondition.And);

			var originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			originSubQuery.AddToFilter(originQuery);

			var sailingQuery = new ZDBOnlyQuery(typeof(JobSailing));
			sailingQuery.AddSubQuery(originSubQuery, JoinCondition.And);

			return sailingQuery;
		}

		static ForwardingContainer CreateForwardingContainerFromOrderContainer(OrderContainer orderContainer)
		{
			var container = orderContainer.Factory.New<ForwardingContainer>();
			return OrderContainerHelper.PopulateForwardingContainerFromOrderContainer(orderContainer, container);
		}

		#endregion
	}
}
