using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface ITransactionQRCodeDataProvider
	{
		ZString ComplianceSubType { get; }
		ZString TransactionType { get; }
		ZString TransactionReference { get; }
		ZDateTime InvoiceDate { get; }
		ZString TransactionCurrency { get; }
		ZDecimal ExchangeRate { get; }
		ZDecimal OSInvoiceTotal { get; }
		ZDecimal OSTaxTotal { get; }
		ZDecimal LocalInvoiceTotal { get; }
		ZDecimal LocalTaxTotal { get; }
		ZDecimal GSTTotal { get; }
		ZString EInvoicingGovernmentAllocatedNumber { get; }
		OrgHeader OrgHeader { get; }
		GlbCompany Company { get; }
		GlbBranch Branch { get; }
		ITransactionHeaderWithLines GetTransactionDataForPortugalOnly_Obsolete_MustBeRefactoredToUseMembersOfThisInterface();
	}
}
