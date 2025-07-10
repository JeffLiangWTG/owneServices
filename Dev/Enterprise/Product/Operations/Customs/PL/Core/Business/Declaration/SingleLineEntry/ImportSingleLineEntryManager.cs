namespace Enterprise.Customs.PL.Business.Declaration;

public sealed class ImportSingleLineEntryManager : SingleLineEntryManager
{
	public ImportSingleLineEntryManager(JobDeclaration declaration) : base(declaration, new ImportSingleLineEntryProvider())
	{
	}

	public new ImportSingleLineEntry SingleLineEntry => (ImportSingleLineEntry)base.SingleLineEntry;

	protected override void ExecuteCore(EU.Business.Declaration.JobComInvoiceHeader invoice, EU.Business.Declaration.JobComInvoiceLine invoiceLine)
	{
		base.ExecuteCore(invoice, invoiceLine);
		var goodsOrigin = SingleLineEntry.GoodsOrigin;
		var previousDocumentCode = SingleLineEntry.PreviousDocument;

		if (!goodsOrigin.IsEmpty)
		{
			invoiceLine.JI_CountryOfOrigin = goodsOrigin;
		}

		if (!previousDocumentCode.IsEmpty)
		{
			var previousDocument = Declaration.PreviousDocuments.AddNew();
			previousDocument.CSI_ReferenceNumber = SingleLineEntry.PreviousDocumentNumber;
			previousDocument.CSI_Code = previousDocumentCode;
			previousDocument.CSI_LineNo = 1;
			previousDocument.CSI_SubType = Constants.SubStyleCodes.Z;
		}
	}
}
