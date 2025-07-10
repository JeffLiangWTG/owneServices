
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceLineCOValidation : JobComInvoiceLineValidation
	{
		public JobComInvoiceLineCOValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CustomsQuantityInfo, "Item Quantity");
		}

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CustomsUnitQtyInfo, "Item Unit of Quantity");
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_LinePriceInfo);
		}

		protected override void CheckCertItemDescription()
		{
			base.CheckCertItemDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CertItemDescriptionInfo, "Item Description");
		}
	}
}
