using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	class OrderDocManagerInfo : DocManagerInfo
	{
		public OrderDocManagerInfo(Order order)
			: base(order, Constants.DocManagerCodes.Order)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var list = new List<BusinessObject>(base.GetRelatedObjects());

			var order = (Order)BusinessEntity;
			if (order.RelatedWarehouseReceive != null)
			{
				list.Add(order.RelatedWarehouseReceive);
			}

			list.AddRange(GetRelatedSupplierBookingDetailsEDocs());
			list.AddRange(GetRelatedContainerLoadListHeaderDetailsEDocs());
			list.AddRange(GetRelatedConsolDetailsEDocs());
			list.AddRange(GetRelatedShipmentDetailsEDocs());

			return list.ToArray();
		}

		IEnumerable<BusinessObject> GetRelatedShipmentDetailsEDocs()
		{
			var order = (Order)BusinessEntity;

			var orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
			orderLineQuery.AddToFilter(JobOrderLineSchema.JO_JD, order.PK);

			var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.PK);
			supplierBookingLineQuery.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, orderLineQuery, JoinCondition.And);

			var containerLoadListLineQuery = new ZDBOnlySubQuery(typeof(ContainerLoadListLine), ContainerLoadListLineSchema.CLL_JL_PackLine);
			containerLoadListLineQuery.AddSubQuery(ContainerLoadListLineSchema.CLL_JSL_BookingLine, supplierBookingLineQuery, JoinCondition.And);

			var packLineQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
			packLineQuery.AddSubQuery(containerLoadListLineQuery, JoinCondition.And);

			var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipmentQuery.AddSubQuery(packLineQuery, JoinCondition.And);

			return order.Factory.Load<ForwardingShipment>(shipmentQuery);
		}

		IEnumerable<BusinessObject> GetRelatedConsolDetailsEDocs()
		{
			var order = (Order)BusinessEntity;

			var orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
			orderLineQuery.AddToFilter(JobOrderLineSchema.JO_JD, order.PK);

			var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.JSL_JSB_Booking);
			supplierBookingLineQuery.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, orderLineQuery, JoinCondition.And);

			var containerQuery = new ZDBOnlySubQuery(typeof(ForwardingContainer), JobContainerSchema.JC_JK);
			containerQuery.AddSubQuery(JobContainerSchema.JC_JSB_SupplierBooking, supplierBookingLineQuery, JoinCondition.And);

			var consolQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
			consolQuery.AddSubQuery(JobConsolSchema.PK, containerQuery, JoinCondition.And);

			return order.Factory.Load<ForwardingConsol>(consolQuery);
		}

		IEnumerable<BusinessObject> GetRelatedContainerLoadListHeaderDetailsEDocs()
		{
			var order = (Order)BusinessEntity;

			var orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
			orderLineQuery.AddToFilter(JobOrderLineSchema.JO_JD, order.PK);

			var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.PK);
			supplierBookingLineQuery.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, orderLineQuery, JoinCondition.And);

			var containerLoadListLineQuery = new ZDBOnlySubQuery(typeof(ContainerLoadListLine), ContainerLoadListLineSchema.CLL_CLH_LoadListHeader);
			containerLoadListLineQuery.AddSubQuery(ContainerLoadListLineSchema.CLL_JSL_BookingLine, supplierBookingLineQuery, JoinCondition.And);

			var containerLoadListHeaderQuery = new ZDBOnlyQuery(typeof(CommonContainerLoadList));
			containerLoadListHeaderQuery.AddSubQuery(containerLoadListLineQuery, JoinCondition.And);

			return order.Factory.Load<CommonContainerLoadList>(containerLoadListHeaderQuery);
		}

		IEnumerable<BusinessObject> GetRelatedSupplierBookingDetailsEDocs()
		{
			var order = (Order)BusinessEntity;

			var orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
			orderLineQuery.AddToFilter(JobOrderLineSchema.JO_JD, order.PK);

			var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.JSL_JSB_Booking);
			supplierBookingLineQuery.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, orderLineQuery, JoinCondition.And);

			var supplierBookingQuery = new ZDBOnlyQuery(typeof(JobSupplierBooking));
			supplierBookingQuery.AddSubQuery(supplierBookingLineQuery, JoinCondition.And);

			return order.Factory.Load<JobSupplierBooking>(supplierBookingQuery);
		}
	}
}
