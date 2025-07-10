using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AccDraftInvoiceHeaderData),
	Enterprise.Core.Constants.DocManagerCodes.AccountingDraftInvoiceHeader)]

namespace Enterprise.MasterFiles.Business
{
	class AccDraftInvoiceHeaderData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(AccDraftInvoiceHeader);

		public override string ReferenceType => Core.Constants.ReferenceTypes.Accounting;

		protected override Type CollectionType => null;

		public override ZString GetFriendlyName(BusinessObject businessObject)
		{
			return businessObject is AccDraftInvoiceHeader draftHeader
				? FormattableString.Invariant($"{GetNamePrefix(draftHeader.AIH_TransactionType)} {draftHeader.AIH_InternalReference}").Trim()
				: ZString.Empty;
		}

		string GetNamePrefix(string transactionType) => transactionType switch
		{
			TransactionTypes.Invoice => ResString.GetMultilingualString("2c295d1f-d2a2-47d8-91d0-da5b0301edc0", "Draft Invoice"),
			TransactionTypes.CreditNote => ResString.GetMultilingualString("f31dd7ef-1d23-40e4-a018-46948ee7aeee", "Draft Credit Note"),
			_ => throw new InvalidOperationException($"Invalid transaction type{(string.IsNullOrEmpty(transactionType) ? "." : $": {transactionType}.")}")
		};
	}
}
