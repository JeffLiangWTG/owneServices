using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class QuickPackItemValidation : AutoQuickPackItemValidation
	{
		public QuickPackItemValidation(AutoQuickPackItem parent) : base(parent)
		{
		}

		public new QuickPackItem Parent => (QuickPackItem)base.Parent;

		protected override void CheckPackedQty()
		{
			var info = Parent.PackedQtyInfo;
			if (Parent.PackedQty > Parent.NotPackedQty)
			{
				info.AddError(Res.GetString("4D948AB9-AEFB-4F83-AD24-7E6F7983AC9B", "Packed Qty cannot be greater than Not Packed Qty."));
			}
			if (!Parent.Pack.IsEmpty)
			{
				MandatoryValidation.CheckNotZero(info);
				MandatoryValidation.CheckNotNegative(info);
			}
		}
	}
}
