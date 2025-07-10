using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AddInfoJobComInvoiceHeaderLookups : EU.Business.Declaration.AddInfoJobComInvoiceHeaderLookups
{
	public AddInfoJobComInvoiceHeaderLookups(AddInfoJobComInvoiceHeader parent) : base(parent)
	{
	}

	public CodeDescriptionPairList ValuationMethodList => Factory.GetCachedValue<ValuationMethodList>();

	public CodeDescriptionPairList TransportMethodOfPaymentList => Factory.GetCachedValue<ExportTransportMethodOfPaymentList>();
}
