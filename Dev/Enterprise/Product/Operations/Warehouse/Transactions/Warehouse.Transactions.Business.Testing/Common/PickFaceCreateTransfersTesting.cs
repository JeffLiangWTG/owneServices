using System;

namespace Enterprise.Warehouse.Transactions.Business.Testing.Common
{
	public class PickFaceCreateTransfersTesting : PickFaceCreateTransfers
	{
		public override void PreTransferValidationCore(WhsTransfer transfer)
		{
			RunActionBeforeTransferValidation?.Invoke(transfer);
		}

		public Action<WhsTransfer> RunActionBeforeTransferValidation;
	}
}
