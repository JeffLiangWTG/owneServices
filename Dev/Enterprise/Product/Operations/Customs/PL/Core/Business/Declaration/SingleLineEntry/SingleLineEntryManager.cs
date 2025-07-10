using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class SingleLineEntryManager : EU.Business.Declaration.SingleLineEntryManager
{
	public SingleLineEntryManager(EU.Business.Declaration.JobDeclaration declaration,
		EU.Business.Declaration.ISingleLineEntryProvider singleLineEntryProvider) : base(declaration, singleLineEntryProvider)
	{
	}

	public SingleLineEntryManager(EU.Business.Declaration.JobDeclaration declaration) : base(declaration, new SingleLineEntryProvider())
	{
	}

	protected override void ExecuteCore(EU.Business.Declaration.JobComInvoiceHeader invoice, EU.Business.Declaration.JobComInvoiceLine invoiceLine)
	{
		base.ExecuteCore(invoice, invoiceLine);
		var invoiceNumber = SingleLineEntry.InvoiceNumber;
		if (!invoiceNumber.IsEmpty)
		{
			if (Declaration.IsExport)
			{
				invoice.SupportingDocuments.AddNew(EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N380, invoiceNumber);
			}
			else if (Declaration.IsImport)
			{
				invoice.SupportingDocuments.AddNew(EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N935, invoiceNumber);
			}
		}

		var cpc = SingleLineEntry.CPCCode;
		if (!cpc.IsEmpty)
		{
			var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = SubStyleCodes.A;
			entryInstruction.CEI_Procedure = cpc.SubstringSafe(0, 2);
			invoiceLine.JI_CEI = entryInstruction.PK;
		}
	}
}
