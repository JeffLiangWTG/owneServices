using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class AccountingCountrySpecificValidationHelper
	{
		#region Validation Functions

		#region CanReprint & INV Doc Type cannot be changed or deleted

		public static bool CanReprint(AccTransactionHeader transactionHeader)
		{
			Argument.NotNull(transactionHeader, nameof(transactionHeader));

			var isArInvCrdNote = transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable
				&& (transactionHeader.AH_TransactionType == TransactionTypes.Invoice || transactionHeader.AH_TransactionType == TransactionTypes.CreditNote);

			return !(isArInvCrdNote)
				|| AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.GetValueWithoutFallback(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
		}

		public static bool CanChangeINVDocTypeOnPostedTransaction(AccTransactionHeader transactionHeader) => CanReprint(transactionHeader);

		#endregion

		#endregion

		#region Implementations

		public static bool IsEmptyPortugalIVA(ZString registrationNumber)
		{
			return registrationNumber.IsEmpty || registrationNumber == EmptyPortugalIVA || registrationNumber.EqualsIgnoringCase(Core.Constants.CountryCodes.Portugal + EmptyPortugalIVA);
		}

		public const string EmptyPortugalIVA = "999999990";

		#endregion
	}
}
