
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobComInvoiceHeader : AddInfo
	{
		public AddInfoJobComInvoiceHeader(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		#region Validation / Lookups

		public new AddInfoJobComInvoiceHeaderLookups Lookups
		{
			get { return (AddInfoJobComInvoiceHeaderLookups)base.Lookups; }
		}

		public new AddInfoJobComInvoiceHeaderValidation Validation
		{
			get { return (AddInfoJobComInvoiceHeaderValidation)base.Validation; }
		}

		protected override SGAddInfoLookups GetNewLookups()
		{
			return new AddInfoJobComInvoiceHeaderLookups(this);
		}

		protected override SGAddInfoValidation GetNewValidation()
		{
			return new AddInfoJobComInvoiceHeaderValidation(this);
		}

		#endregion
	}
}
