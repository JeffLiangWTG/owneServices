using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsPermitWithdrawRequestResponseForTest : IPermitWithdrawRequestResponse
	{
		public WhsPermitWithdrawRequestResponseForTest(WhsOrderLine line, SuccessOrFailure successOrFailure, ZDecimal? availableQty, ZString? outwardEntryNumber, string failureReason = "")
		{
			Request = new WhsPermitWithdrawRequest(line);
			SuccessOrFailure = successOrFailure;
			AvailableQty = availableQty;
			OutwardEntryNumber = outwardEntryNumber;
			FailureReason = failureReason;
		}

		public IPermitWithdrawRequest Request { get; }

		public SuccessOrFailure SuccessOrFailure { get; }

		public ZString FailureReason { get; set; }

		public ZDecimal? AvailableQty { get; }

		public ZString? OutwardEntryNumber { get; }
	}
}
