namespace Enterprise.Customs.PL.Business.Declaration;

public class AddInfoJobComInvoiceHeaderValidation : EU.Business.Declaration.AddInfoJobComInvoiceHeaderValidation
{
	public AddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
		: base(parent)
	{
	}

	protected new AddInfoJobComInvoiceHeader Parent => (AddInfoJobComInvoiceHeader)base.Parent;

	protected new AddInfoJobComInvoiceHeaderLookups Lookups => (AddInfoJobComInvoiceHeaderLookups)base.Lookups;

	protected JobComInvoiceHeader InvoiceHeader => Parent.Parent as JobComInvoiceHeader;

	protected override void CheckZG_AgreedPlaceCode()
	{
		if (!Parent.ZG_AgreedPlaceCodeInfo.ReadOnly)
		{
			base.CheckZG_AgreedPlaceCode();
		}
	}

	protected override void CheckZG_TransportChargesMethodOfPayment()
	{
		base.CheckZG_TransportChargesMethodOfPayment();
		if (InvoiceHeader.JobDeclaration.IsExitSummary && Parent.ZG_TransportChargesMethodOfPayment.IsEmpty)
		{
			Parent.ZG_TransportChargesMethodOfPaymentInfo.AddMessageError(Res.GetString("1cdfe663-b5e3-48f1-9bc0-a0f05cd797ec", "Method of Payment is required for EXS - Exit Summary Declaration."));
		}
	}
}
