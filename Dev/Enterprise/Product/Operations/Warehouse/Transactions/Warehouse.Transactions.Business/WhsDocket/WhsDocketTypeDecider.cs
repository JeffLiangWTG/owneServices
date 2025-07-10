using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Immutable]
	public class WhsDocketTypeDecider : TypeDecider
	{
		public override Type GetTypeForNew()
		{
			throw new NotSupportedException("Abstract type.");
		}

		public override Type GetTypeForBinding()
		{
			return typeof(WhsDocket); // throw new NotSupportedException("Abstract type.");
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			switch ((string)row[WhsDocketSchema.WD_DocketType.Name])
			{
				case DocketType.Codes.Receive:
					return typeof(WhsReceive);
				case DocketType.Codes.Order:
					return typeof(WhsOrder);
				case DocketType.Codes.Transfer:
					return typeof(WhsTransfer);
				case DocketType.Codes.Adjustment:
					return typeof(WhsAdjustment);
				case DocketType.Codes.WorkOrder:
					return typeof(WhsWorkOrder);
				case DocketType.Codes.DynamicWorkOrder:
					return typeof(WhsDynamicWorkOrder);
				default:
					return null;
			}
		}

		public string GetDocketTypeCodeFromType(Type whsDocketType)
		{
			var result = "";

			if (typeof(WhsReceive).IsAssignableFrom(whsDocketType))
			{
				result = DocketType.Codes.Receive;
			}
			else if (typeof(WhsOrder).IsAssignableFrom(whsDocketType))
			{
				result = DocketType.Codes.Order;
			}
			else if (typeof(WhsTransfer).IsAssignableFrom(whsDocketType))
			{
				result = DocketType.Codes.Transfer;
			}
			else if (typeof(WhsAdjustment).IsAssignableFrom(whsDocketType))
			{
				result = DocketType.Codes.Adjustment;
			}
			else if (typeof(WhsWorkOrder).IsAssignableFrom(whsDocketType))
			{
				result = DocketType.Codes.WorkOrder;
			}
			else if (typeof(WhsDynamicWorkOrder).IsAssignableFrom(whsDocketType))
			{
				result = DocketType.Codes.DynamicWorkOrder;
			}

			return result;
		}
	}
}
