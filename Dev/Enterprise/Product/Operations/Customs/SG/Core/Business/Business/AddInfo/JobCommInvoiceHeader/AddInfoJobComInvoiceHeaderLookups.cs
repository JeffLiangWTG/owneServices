

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobComInvoiceHeaderLookups : SGAddInfoLookups
	{
		public AddInfoJobComInvoiceHeaderLookups(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public new AddInfoJobComInvoiceHeader Parent
		{
			get { return (AddInfoJobComInvoiceHeader)base.Parent; }
		}
	}
}
