using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public sealed class AddInfoJobComInvoiceHeaderValidation : EU.Business.Declaration.AddInfoJobComInvoiceHeaderValidation
	{
		public AddInfoJobComInvoiceHeaderValidation(EU.Business.Declaration.AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override void CheckZG_CommercialPaymentCode()
		{
			base.CheckZG_CommercialPaymentCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_CommercialPaymentCodeInfo);
		}
	}
}
