using CargoWise.Common;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class IEInvoicingTransactionExtensions
	{
		public static bool IsInvalidStatusToReQueue(this IEInvoicingTransaction transaction)
		{
			Argument.NotNull(transaction, nameof(transaction));
			return transaction.CurrentStatus == Constants.EInvoicingPivotState.Succeed
				|| transaction.CurrentStatus == Constants.EInvoicingPivotState.Sent
				|| transaction.CurrentStatus == Constants.EInvoicingPivotState.Delivered
				|| transaction.CurrentStatus == Constants.EInvoicingPivotState.Batched;
		}
	}
}
