using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AddInfoJobComInvoiceHeaderLookups : EU.Business.Declaration.AddInfoJobComInvoiceHeaderLookups
	{
		public AddInfoJobComInvoiceHeaderLookups(EU.Business.Declaration.AddInfoJobComInvoiceHeader parent) : base(parent)
		{
		}

		public new AddInfoJobComInvoiceHeader Parent => (AddInfoJobComInvoiceHeader)base.Parent;

		public CodeDescriptionPairList InvoicePaymentCodeList => Factory.GetCachedValue<InvoicePaymentCodeList>();
	}
}
