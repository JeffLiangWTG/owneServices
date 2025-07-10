using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IAccTransactionHeader
	{
		ZGuid AH_AB { get; set; }
		ZGuid AH_AG { get; set; }
		ZInt AH_AgePeriod { get; set; }
		ZGuid AH_AH_InvoiceStatement { get; set; }
		ZBool AH_CashBasisGSTIndicator { get; set; }
		ZBool AH_CashBasisGSTRealisedToGL { get; set; }
		ZString AH_ChequeDrawer { get; set; }
		ZString AH_ChequeOrReference { get; set; }
		ZString AH_ConsolidatedInvoiceRef { get; set; }
		ZDateTime AH_DateClearedInCashbook { get; set; }
		ZString AH_Desc { get; set; }
		ZString AH_DrawerBank { get; set; }
		ZString AH_DrawerBranch { get; set; }
		ZDateTime AH_DueDate { get; set; }
		ZDecimal AH_ExchangeRate { get; set; }
		ZDateTime AH_FullyPaidDate { get; set; }
		ZGuid AH_GB { get; set; }
		ZGuid AH_GC { get; set; }
		ZGuid AH_GE { get; set; }
		ZDecimal AH_GSTAmount { get; set; }
		ZDecimal AH_InvoiceAmount { get; set; }
		ZBool AH_InvoiceApproved { get; set; }
		ZDateTime AH_InvoiceDate { get; set; }
		ZBool AH_InvoicePrinted { get; set; }
		ZString AH_InvoiceTerm { get; set; }
		ZByte AH_InvoiceTermDays { get; set; }
		ZBool AH_IsCancelled { get; set; }
		ZGuid AH_JH { get; set; }
		ZString AH_Ledger { get; set; }
		ZBool AH_NotAllocated { get; set; }
		ZGuid AH_OH { get; set; }
		ZDecimal AH_OSTotal { get; set; }
		ZDecimal AH_OutstandingAmount { get; set; }
		ZBool AH_POST1 { get; set; }
		ZBool AH_POST2 { get; set; }
		ZBool AH_POST3 { get; set; }
		ZBool AH_POST4 { get; set; }
		ZDateTime AH_PostDate { get; set; }
		ZBool AH_PostedInternal { get; set; }
		ZBool AH_PostedToEFT { get; set; }
		ZInt AH_PostPeriod { get; set; }
		ZString AH_PostToGL { get; set; }
		ZString AH_ReceiptBatchNo { get; set; }
		ZString AH_ReceiptType { get; set; }
		ZDateTime AH_RequisitionDate { get; set; }
		ZString AH_RequisitionStatus { get; set; }
		ZString AH_RX_NKTransactionCurrency { get; set; }
		ZGuid AH_TransactionBelongsToGroup { get; set; }
		ZString AH_TransactionCategory { get; set; }
		ZByte AH_TransactionCount { get; set; }
		ZBool AH_TransactionCreatedByMatching { get; set; }
		ZString AH_TransactionNum { get; set; }
		ZString AH_TransactionReference { get; set; }
		ZString AH_TransactionType { get; set; }
		ZDecimal AH_WithholdingTax { get; set; }
		ZString AH_GovernmentAllocatedID { get; set; }
	}
}
