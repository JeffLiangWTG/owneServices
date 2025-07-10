using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

public class InvoiceLineCompleteCollection : EU.Business.Declaration.InvoiceLineCompleteCollection
{
	public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
		: base(jobDeclaration)
	{
	}

	public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

	public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var invoiceLine = (JobComInvoiceLine)child;
		if (JobDeclaration.IsImport)
		{
			invoiceLine.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
		}
	}
}
