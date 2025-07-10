using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Compliance;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ComplianceSubType : IComplianceSubType
	{
		public ComplianceSubType(
			ZString code,
			Func<IMultilingualString> description,
			Func<ZString> localDescription,
			Func<ZString> internalImplemenationNote,
			LedgerOfUse ledger = LedgerOfUse.ALL,
			ZString? taxStatusCode = null,
			TransactionTypeOfUse transactionType = TransactionTypeOfUse.ALL,
			TransactionCreatingMode transactionCreatingMode = TransactionCreatingMode.All)
		{
			Code = code;
			Description = description;
			LocalDescription = localDescription;
			InternalImplemenationNote = internalImplemenationNote;
			Ledger = ledger;
			TransactionType = transactionType;
			TaxStatusCode = taxStatusCode ?? ZString.Empty;
			TransactionCreatingMode = transactionCreatingMode;
		}

		public ZString Code { get; }
		public Func<IMultilingualString> Description { get; }
		public Func<ZString> LocalDescription { get; }
		internal Func<ZString> InternalImplemenationNote { get; }
		public LedgerOfUse Ledger { get; }
		public TransactionTypeOfUse TransactionType { get; }
		public ZString TaxStatusCode { get; }
		public TransactionCreatingMode TransactionCreatingMode { get; }
	}
}
