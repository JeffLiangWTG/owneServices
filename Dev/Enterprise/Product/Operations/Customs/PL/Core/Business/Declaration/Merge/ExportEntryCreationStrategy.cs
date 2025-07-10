using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportEntryCreationStrategy : EntryCreationStrategy
{
	public ExportEntryCreationStrategy(JobDeclaration declaration)
		: base(declaration)
	{
	}

	public override MergeKey GetKeyForLine(BaseJobComInvoiceLine invoiceLine)
	{
		var line = (JobComInvoiceLine)invoiceLine;
		var result = base.GetKeyForLine(invoiceLine);

		result.Add(line.PacksMeasure);

		return result;
	}

	protected override void ProcessInvoiceHeaderForLineMergeKey(MergeKey mergeKey, JobComInvoiceHeader invoice)
	{
		base.ProcessInvoiceHeaderForLineMergeKey(mergeKey, invoice);

		mergeKey.Add(invoice.ZG_TransportChargesMethodOfPayment);
		mergeKey.Add(invoice.JZ_OA_ConsigneeAddress);
	}

	protected override string[] GetPreviousDocumentKeysCore() => new[]
	{
		PreviousDocument.Schema.CSI_DateOfIssue,
		PreviousDocument.Schema.CSI_LineNo,
		PreviousDocument.Schema.CSI_SubType,
		PreviousDocument.Schema.CSI_ReferenceNumber,
		PreviousDocument.Schema.CSI_Code
	};

	protected override string[] GetSupportingDocumentKeysCore() => new[]
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_Status,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_RN_NKCountryCode,
		SupportingDocument.Schema.CSI_RX_NKCurrency,
		SupportingDocument.Schema.CSI_ItemNumber,
		SupportingDocument.Schema.CSI_AdditionalDescription,
	};

	protected override string[] GetAdditionalInfoKeysCore() => new[]
	{
		AdditionalInfo.Schema.CSI_Code,
		AdditionalInfo.Schema.CSI_Description,
		AdditionalInfo.Schema.CSI_ReferenceNumber,
	};
}
