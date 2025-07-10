using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business
{
	public interface IDISDocument : IDISData
	{
		ZString PreparerID { get; }
		ZString PreparerSiteCode { get; }
		ZString ActionCodeForSubmission { get; }

		ZString PortCode { get; }
		TransactionCategory TransactionCategory { get; }
		IEnumerable<IDISTradeTransaction> TradeTransactions { get; }
		IDISCBPRequest CBPRequest { get; }
		ZString DocumentID { get; }
		ZString DocumentLabel { get; }
		ZString DocumentLabelUSDISDocCode { get; }
		ZString CompleteFileName { get; }
		ZString DocumentDescription { get; }
		ZDateTime DocumentSentDateEST { get; }
		ZBool PreviouslySubmitted { get; }

		IEnumerable<ZString> PGAs { get; }

		ZString Comment { get; }

		IDISInvoice Invoice { get; }
		IDISBondData BondData { get; }
		IDISPackingList PackingList { get; }
		IDISCertificate CertificateData { get; }
		IDISPermit PermitData { get; }
		IDISPackageIdentifier PackageIdentifierData { get; }

		IDISToxicSubstanceData ToxicSubstanceData { get; }
		IEnumerable<IDISCommodityLine> CommodityData { get; }
		IEnumerable<IDISAdditionalData> AdditionalData { get; }

		ZGuid eDocsDocumentPK { get; }

		EDIMessage CreateMessage(string messageType);
	}

	public interface IDISPackingList : IDISData
	{
		ZString PackingListNumber { get; }
		ZString InvoiceNumber { get; }
		ZString PurchaseOrderNumber { get; }
	}

	public interface IDISCertificate : IDISData
	{
		ZString Number { get; }
		ZString Type { get; }
		ZString Statement { get; }
		ZDateTime IssueDate { get; }
		ZDateTime ExpiryDate { get; }
		ZString ImporterOfRecord { get; }
		ZString InspectionLocation { get; }
		ZDecimal GrossTonnage { get; }
		ZDecimal NetTonnage { get; }
	}

	public interface IDISPermit : IDISData
	{
		ZString Number { get; }
		ZString Type { get; }
		ZString ApprovalNumber { get; }
		ZString Statement { get; }
		ZDateTime StartDate { get; }
		ZDateTime EndDate { get; }
		ZString ImporterOfRecord { get; }
	}

	public interface IDISToxicSubstanceData : IDISData
	{
		ZString CASNumber { get; }
		ZString EPARegoNumber { get; }
		ZString EPAProducerEstNumber { get; }
	}

	public interface IDISAdditionalData
	{
		ZString FieldName { get; }
		ZString Value { get; }
	}

	public interface IDISCBPRequest : IDISData
	{
		ZString ID { get; }
		ZString Type { get; }
		ZDateTime RequestDate { get; }
	}

	public interface IDISSubmissionAdditionalData
	{
		ZString WithdrawalReasonCode { get; }
		ZString Comment { get; }
	}

	public interface IDISPackageIdentifier : IDISData
	{
		ZString PackageCategory { get; }
		ZString ImporterOfRecordNumber { get; }
	}
}
