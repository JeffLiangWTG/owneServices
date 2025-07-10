
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobComInvoiceLineValidation_IPT : AddInfoJobComInvoiceLineValidation_CUSDEC
	{
		public AddInfoJobComInvoiceLineValidation_IPT(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckSG_LastSellingPrice()
		{
			base.CheckSG_LastSellingPrice();
			if (!Declaration.SG_SupplyIndicator.IsEmpty && Parent.SG_LastSellingPrice.IsEmpty)
			{
				Parent.SG_LastSellingPriceInfo.AddMessageError(LastSellingPriceRequired);
			}
		}

		protected override void CheckSG_EndUseDescription()
		{
			base.CheckSG_EndUseDescription();

			if (Tariff != null)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.SG_EndUseDescriptionInfo);
			}
		}
	}
}
