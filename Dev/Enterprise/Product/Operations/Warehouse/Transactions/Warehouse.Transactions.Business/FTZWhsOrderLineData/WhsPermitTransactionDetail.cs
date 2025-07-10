using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPermitTransactionDetail : IPermitTransactionDetail
	{
		public WhsPermitTransactionDetail(WhsOrderLine orderLine)
		{
			Argument.NotNull(orderLine, nameof(orderLine));
			var order = Argument.NotNull(orderLine.Order, nameof(orderLine.Order));
			var customsData = Argument.NotNull(orderLine.CustomsData, nameof(orderLine.CustomsData));
			var warehouse = Argument.NotNull(order.Warehouse, nameof(order.Warehouse));
			Warehouse = warehouse.WarehouseAddress;

			PermitTransactionRefNumber = GetPermitTransactionRefNumber(orderLine);
			ZoneStatus = customsData.WB_ZoneStatus;
		}

		#region GetPermitTransactionRefNumber

		public static ZString GetPermitTransactionRefNumber(WhsOrderLine orderLine)
		{
			Argument.NotNull(orderLine, nameof(orderLine));
			var order = Argument.NotNull(orderLine.Order, nameof(orderLine.Order));

			return string.Format(Culture.Invariant, "{0}_{1}", order.WD_DocketID, orderLine.WE_LineNo);
		}

		#endregion

		#region IPermitTransactionDetail Members

		public ZString PermitTransactionRefNumber { get; }
		public ZString ZoneStatus { get; }
		public IOrgAddress Warehouse { get; }
		public PermitType PermitType => PermitType.FTZ; // No other types now.

		#endregion
	}
}
