using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ImmutableObject(true)]
	public class WhsDocketLineTypeDecider : TypeDecider
	{
		public override Type GetTypeForNew() => typeof(WhsDocketLine);

		public override Type GetTypeForBinding() => typeof(WhsDocketLine);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var docketLineType = (string)row[WhsDocketLineSchema.Constants.WE_DocketLineType];
			switch (docketLineType)
			{
				case DocketType.Codes.Receive:
					return typeof(WhsReceiveLine);
				case DocketType.Codes.Order:
					return typeof(WhsOrderLine);
				case DocketType.Codes.WorkOrder:
					return typeof(WhsWorkOrderLine);
				case DocketType.Codes.DynamicWorkOrder:
					return typeof(WhsDynamicWorkOrderLine);
				case DocketType.Codes.Transfer:
					return typeof(WhsTransferLine);
				case DocketType.Codes.Adjustment:
					return typeof(WhsAdjustmentLine);
				default:
					return null;
			}
		}
	}
}
