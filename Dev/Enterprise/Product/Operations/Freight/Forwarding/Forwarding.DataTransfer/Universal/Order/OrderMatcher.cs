using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderMatcher : CombinationKeyMatcher<Order, OrderReferences>
	{
		internal OrderMatcher(BusinessObjectFactory factory, OrderReferences references, IXmlImportLogger logger)
			: base(factory, references, logger)
		{
		}

		protected override bool CheckLatestParent(Order order, Order orderToCompareTo)
		{
			return order.JD_SystemCreateTimeUtc > orderToCompareTo.JD_SystemCreateTimeUtc;
		}

		protected override ZQuery GetFullQuery(ZQuery initialMatchingQuery, OrderReferences orderReferences)
		{
			var query = new ZQuery();
			var orderNumber = orderReferences.OrderNumber;
			var orderNumberSplit = orderReferences.OrderNumberSplit;

			if (!orderNumber.IsEmpty)
			{
				var orderNumberAndClientQuery = new ZQuery();
				orderNumberAndClientQuery.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);

				if (orderNumberSplit.HasValue)
				{
					orderNumberAndClientQuery.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, orderNumberSplit.Value);
				}

				var buyer = factory.Load<OrgHeader>(orderReferences.BuyerPK);
				if (buyer != null)
				{
					orderNumberAndClientQuery.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyer.Addresses.Select(x => x.PK));
				}

				query.AddToFilter(orderNumberAndClientQuery);
			}

			query.AddToFilter(initialMatchingQuery);

			return query;
		}

		protected override void BuildMatchingQueryAndMatchDelegates(OrderReferences referencesParent)
		{
			AddPossibleMatch(JobOrderHeaderSchema.JD_OrderNumber, referencesParent.OrderNumber, order => GetMatchCount(order.JD_OrderNumber, referencesParent.OrderNumber));

			BuildMasterBillQueryAndMatchDelegate(referencesParent);
			BuildHouseBillQueryAndMatchDelegate(referencesParent);

			AddPossibleMatch(JobOrderHeaderSchema.JD_InvoiceNumber, referencesParent.InvoiceNumber, order => GetMatchCount(order.JD_InvoiceNumber, referencesParent.InvoiceNumber));
			AddPossibleMatch(JobOrderHeaderSchema.JD_BookingConfRef, referencesParent.ShippersReference, order => GetMatchCount(order.JD_BookingConfRef, referencesParent.ShippersReference));
		}

		void BuildMasterBillQueryAndMatchDelegate(OrderReferences referencesParent)
		{
			BuildWayBillQueryAndMatchDelegate(JobOrderHeaderSchema.JD_MasterWaybill, referencesParent.MAWBNumber, true);
			BuildWayBillQueryAndMatchDelegate(JobOrderHeaderSchema.JD_MasterWaybill, referencesParent.MBOLNumber, false);
		}

		void BuildHouseBillQueryAndMatchDelegate(OrderReferences referencesParent)
		{
			BuildWayBillQueryAndMatchDelegate(JobOrderHeaderSchema.JD_Waybill, referencesParent.HAWBNumber, true);
			BuildWayBillQueryAndMatchDelegate(JobOrderHeaderSchema.JD_Waybill, referencesParent.HBOLNumber, false);
		}

		void BuildWayBillQueryAndMatchDelegate(SchemaColumn column, ZString wayBill, bool isAir)
		{
			if (!wayBill.IsEmpty)
			{
				var query = new ZQuery(column, wayBill);

				if (isAir)
				{
					query.AddToFilter(JobOrderHeaderSchema.JD_TransportMode, Constants.TransportModes.Air);
				}
				else
				{
					query.AddToFilter(JobOrderHeaderSchema.JD_TransportMode, SQLComparisonOperator.NotEqual, Constants.TransportModes.Air);
				}

				AddPossibleMatch(query, order => GetMatchCount((IZType)order[column], wayBill));
			}
		}

		protected override void BuildFallbackMatchDelegates(OrderReferences referencesParent)
		{
			AddFallbackMatch(referencesParent.OriginUNLOCO, order => GetMatchCount(order.JD_RL_NKGoodsAvailableAt, referencesParent.OriginUNLOCO));
			AddFallbackMatch(referencesParent.DestinationUNLOCO, order => GetMatchCount(order.JD_RL_NKGoodsDeliveredTo, referencesParent.DestinationUNLOCO));
		}
	}
}
