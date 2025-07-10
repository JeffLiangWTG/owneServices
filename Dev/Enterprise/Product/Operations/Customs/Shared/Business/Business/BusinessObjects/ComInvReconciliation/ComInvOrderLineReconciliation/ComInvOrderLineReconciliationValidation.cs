using CargoWise.EntityFramework;

using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Customs.Business
{
	public class ComInvOrderLineReconciliationValidation : JobOrderLineValidation
	{
		public ComInvOrderLineReconciliationValidation(AutoJobOrderLine parent) : base(parent)
		{
		}

		new ComInvOrderLineReconciliation Parent
		{
			get { return (ComInvOrderLineReconciliation)base.Parent; }
		}

		public void ValidateJO_Recon_ItemPrice()
		{
			ValidateCalculatedProperty(Parent.JO_Recon_ItemPriceInfo);
		}

		protected void CheckJO_Recon_ItemPrice()
		{
			TypeValidation.CheckValidDecimal(Parent.JO_Recon_ItemPriceInfo, 19, 4);
		}

		public void ValidateJO_Recon_Quantity()
		{
			ValidateCalculatedProperty(Parent.JO_Recon_QuantityInfo);
		}

		protected void CheckJO_Recon_Quantity()
		{
			TypeValidation.CheckValidDecimal(Parent.JO_Recon_QuantityInfo, 19, 4);
		}

		public void ValidateJO_Recon_LinePrice()
		{
			ValidateCalculatedProperty(Parent.JO_Recon_LinePriceInfo);
		}

		protected void CheckJO_Recon_LinePrice()
		{
			TypeValidation.CheckValidDecimal(Parent.JO_Recon_LinePriceInfo, 19, 4);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJO_Recon_ItemPrice();
			ValidateJO_Recon_LinePrice();
			ValidateJO_Recon_Quantity();
		}
	}
}
