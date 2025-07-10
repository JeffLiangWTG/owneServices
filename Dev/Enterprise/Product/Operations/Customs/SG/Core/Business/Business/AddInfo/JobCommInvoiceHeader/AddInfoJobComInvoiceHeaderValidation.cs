using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobComInvoiceHeaderValidation : SGAddInfoValidation
	{
		public AddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override void CheckSG_GSTRate()
		{
			base.CheckSG_GSTRate();
			CompareValidation.CheckNumberNotNegative(Parent.SG_GSTRateInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_GSTRateInfo, 20);
		}

		#region Implementation

		protected new AddInfoJobComInvoiceHeader Parent
		{
			get { return (AddInfoJobComInvoiceHeader)base.Parent; }
		}

		protected AddInfoJobComInvoiceHeaderLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		#endregion
	}
}
