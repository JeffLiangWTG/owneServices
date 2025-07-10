using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPermitWithdrawalRequestDetail : WhsPermitTransactionDetail, IPermitWithdrawalRequestDetail
	{
		public WhsPermitWithdrawalRequestDetail(WhsOrderLine orderLine, ZDecimal qty)
			: base(orderLine)
		{
			Argument.NotNull(orderLine, nameof(orderLine));
			Argument.NotNull(orderLine.Order, nameof(orderLine.Order));
			var customsData = Argument.NotNull(orderLine.CustomsData, nameof(orderLine.CustomsData));

			Qty = qty;
			ReceiveTotalQty = customsData.WB_BondedWhsQty;
			ReceiveTotalCustomsValue = customsData.WB_CustomsQty;
		}

		#region IPermitWithdrawalRequestDetail Members

		public ZDecimal Qty { get; }
		public ZDecimal ReceiveTotalQty { get; }
		public ZDecimal ReceiveTotalCustomsValue { get; }

		#endregion
	}
}
