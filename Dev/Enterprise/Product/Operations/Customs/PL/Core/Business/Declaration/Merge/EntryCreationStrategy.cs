using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class EntryCreationStrategy : EU.Business.Declaration.EntryCreationStrategy
{
	public EntryCreationStrategy(JobDeclaration declaration)
		: base(declaration)
	{
	}

	public override MergeKey GetKeyForLine(BaseJobComInvoiceLine invoiceLine)
	{
		var line = (JobComInvoiceLine)invoiceLine;
		var result = base.GetKeyForLine(invoiceLine);

		ProcessInvoiceHeaderForLineMergeKey(result, line.InvoiceHeader);
		ProcessPreviousDocumentsForLineMergeKey(result, invoiceLine);
		ProcessCustomsUnitsForLineMergeKey(result, line);
		if(Declaration.IsExport)
		{
			result.Add(line.JI_TargetEntryLineNumber);
		}

		return result;
	}

	void ProcessCustomsUnitsForLineMergeKey(MergeKey mergeKey, JobComInvoiceLine invoiceLine)
	{
		mergeKey.Add(invoiceLine.JI_CustomsUnitQty);
		mergeKey.Add(invoiceLine.JI_CustomsSecondUnitQty);
		mergeKey.Add(invoiceLine.JI_CustomsThirdUnitQty);
		mergeKey.Add(invoiceLine.JI_CustomsFourthUnitQty);
		mergeKey.Add(invoiceLine.JI_CustomsFifthUnitQty);
		mergeKey.Add(invoiceLine.JI_BondedWhsUnitQty);
	}

	protected override ZString GetTariffAndDescriptionKey(BaseJobComInvoiceLine baseInvoiceLine) => baseInvoiceLine.JI_NDescription.IsEmpty ? baseInvoiceLine.JI_Description : baseInvoiceLine.JI_NDescription;

	protected virtual void ProcessInvoiceHeaderForLineMergeKey(MergeKey mergeKey, JobComInvoiceHeader invoice)
	{
		mergeKey.Add(invoice.JZ_OA_SellerAddress);
		mergeKey.Add(invoice.SellerOrgPK);
		mergeKey.Add(invoice.JZ_OA_BuyerAddress);
		mergeKey.Add(invoice.BuyerOrgPK);
	}

	void ProcessPreviousDocumentsForLineMergeKey(MergeKey mergeKey, BaseJobComInvoiceLine invoiceLine)
	{
		var line = (JobComInvoiceLine)invoiceLine;
		var previousDocumentsInfos = new List<BusinessObject>();
		previousDocumentsInfos.AddRange(line.InvoiceHeader.PreviousDocuments);
		previousDocumentsInfos.AddRange(line.PreviousDocuments);
		AddResults(mergeKey, previousDocumentsInfos, GetPreviousDocumentKeys());
	}
}
