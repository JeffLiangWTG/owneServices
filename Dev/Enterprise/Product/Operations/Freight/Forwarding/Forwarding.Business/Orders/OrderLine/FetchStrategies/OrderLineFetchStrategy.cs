using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderLineFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrderLineFetchStrategy(OrderLine parent) : base(parent)
		{
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobOrderLineDeliverySchema.J4_JO, Line.PK);
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, Line.PK);
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, Line.PK);
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, Line.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(JobOrderLineDeliverySchema.J4_JO, Line.PK);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var isOrderHintNeeded = false;
			var isOrderLineHintNeeded = false;
			var isOrderLineDeliveryHintNeeded = false;
			var isBuyerAddressHintNeeded = false;
			var isManufacturerAddressHintNeeded = false;

			foreach (var column in columns)
			{
				if (column.ColumnName.StartsWith("Order+", StringComparison.Ordinal)) // Property name
				{
					isOrderHintNeeded = true;
				}

				switch (column.ColumnName)
				{
					case OrderLine.Schema.JO_Calc_OrderLineNoAndSubLineNo:
						isOrderHintNeeded = true;
						isOrderLineHintNeeded = true;
						break;

					case nameof(OrderLine.QtyReceivedToDate):
						isOrderHintNeeded = true;
						break;

					case nameof(OrderLine.JO_ContainersVisible):
						isOrderHintNeeded = true;
						isBuyerAddressHintNeeded = true;
						break;

					case nameof(OrderLine.JO_Calc_TotalQtyReceived):
						isOrderHintNeeded = true;
						isOrderLineDeliveryHintNeeded = true;
						break;

					case nameof(OrderLine.JO_Calc_TotalQtyInvoiced):
						isOrderHintNeeded = true;
						isOrderLineDeliveryHintNeeded = true;
						break;

					case nameof(OrderLine.ManufacturerNameOrPK):
						isManufacturerAddressHintNeeded = true;
						break;
				}
			}

			if (isOrderHintNeeded)
			{
				Factory.AddFetchHint(JobOrderHeaderSchema.PK, Line.JO_JD);
			}

			if (isOrderLineHintNeeded)
			{
				Factory.AddFetchHint(JobOrderLineSchema.JO_JD, Line.JO_JD);
			}

			if (isOrderLineDeliveryHintNeeded)
			{
				Factory.AddFetchHint(JobOrderLineDeliverySchema.J4_JO, Line.PK);
			}

			if (isBuyerAddressHintNeeded)
			{
				var query = new ZDBOnlyQuery(typeof(OrgAddress));
				var subQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_OA_BuyerAddress);
				subQuery.AddToFilter(JobOrderHeaderSchema.PK, Line.JO_JD);
				query.AddSubQuery(subQuery, JoinCondition.And);

				Factory.AddFetchHint(typeof(OrgAddress), query);
			}

			if (isManufacturerAddressHintNeeded)
			{
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, Line.PK);
			}
		}

		OrderLine Line
		{
			get { return (OrderLine)BusinessObject; }
		}
	}
}
