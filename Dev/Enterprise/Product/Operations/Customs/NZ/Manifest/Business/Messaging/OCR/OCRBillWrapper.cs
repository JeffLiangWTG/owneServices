using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class OCRBillWrapper : IOCRConsignment
	{
		public OCRBillWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "bill cannot be null");
		}
		readonly AsycudaBill bill;

		public ZString CustomsClearanceNo => bill.CustomsEntryNumber;

		public IAssociatedTransportDocument BillNumber => billNumber ?? (billNumber = new BillNumberWrapper(bill));
		IAssociatedTransportDocument billNumber;
	}
}
