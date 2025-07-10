using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class OutstandingAmountValidationProvider
	{
		public OutstandingAmountValidationProvider(AccTransactionHeader transaction)
		{
			Transaction = transaction;
		}

		public string ValidateOverseas()
		{
			var wrapper = new OSOutstandingAmountValidationWrapper(Transaction);
			return Validation.Validate(wrapper);
		}

		public string ValidateLocal()
		{
			var wrapper = new LocalOutstandingAmountValidationWrapper(Transaction);
			return Validation.Validate(wrapper);
		}

		IOutstandingAmountValidation Validation
		{
			get
			{
				if (validation == null)
				{
					if (Transaction.AH_Ledger == LedgerTypes.AccountsPayable || Transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						validation = new ARAPOutstandingAmountValidation();
					}
					else if (Transaction.AH_TransactionType == TransactionTypes.OpeningPayment || Transaction.AH_TransactionType == TransactionTypes.OpeningReceipt)
					{
						validation = new OPYORCTransactionOutstandingAmountValidation();
					}
					else
					{
						validation = new OutstandingAmountValidation();
					}
				}

				return validation;
			}
		}
		IOutstandingAmountValidation validation;
		readonly AccTransactionHeader Transaction;
	}
}
