using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AdditionalProcedureCodeCollection : EU.Business.AdditionalProcedureCodeCollection
{
	public AdditionalProcedureCodeCollection(IAdditionalProcedureParent master) : base(master)
	{
		MaxCountValidationEnable(Master.MaxNumberOfAdditionalProcedureCode);

		if (Master.BusinessObject is JobComInvoiceLine invoiceLine && invoiceLine.Declaration is JobDeclaration declaration)
		{
			declaration.JE_MessageTypeInfo.ValueChanged -= DeclarationMessageTypeChanged;
			declaration.JE_MessageTypeInfo.ValueChanged += DeclarationMessageTypeChanged;
		}
	}

	void DeclarationMessageTypeChanged(object sender, System.EventArgs e)
	{
		MaxCountValidationEnable(Master.MaxNumberOfAdditionalProcedureCode);
	}

	public new AdditionalProcedureCode this[int i] => (AdditionalProcedureCode)base[i];

	public new AdditionalProcedureCode AddNew() => (AdditionalProcedureCode)base.AddNew();
}
