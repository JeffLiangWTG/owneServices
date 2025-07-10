using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusPackableItemValidation : AutoCusPackableItemValidation
	{
		public CusPackableItemValidation(AutoCusPackableItem parent) : base(parent)
		{
		}

		protected override void CheckCUI_PackableQty()
		{
			base.CheckCUI_PackableQty();
			MandatoryValidation.CheckNotNegative(Parent.CUI_PackableQtyInfo);
		}

		protected override void CheckCUI_PackableUQ()
		{
			base.CheckCUI_PackableUQ();

			var targetInfo = Parent.CUI_PackableUQInfo;
			if (!Parent.CUI_PackableQty.IsEmpty)
			{
				MandatoryValidation.CheckEntered(targetInfo);
			}
			ListValidation.WarnIfInvalidCode(targetInfo);
		}

		protected override void CheckCUI_NetWeight()
		{
			base.CheckCUI_NetWeight();

			var totalPackedNetWeight = Parent.TotalPackedNetWeight;
			var invoiceLineNetWeight = Parent.CUI_NetWeight;
			if (totalPackedNetWeight != invoiceLineNetWeight)
			{
				Parent.CUI_NetWeightInfo.AddWarning(Res.GetString("78400DC7-D581-43E3-ABB9-247C350BE750",
					"The sum of Packed Item Net Weight {0} {1} does not balance with the Invoice Line Net Weight {2} {3}.",
					totalPackedNetWeight,
					Parent.CUI_NetWeightUQ,
					invoiceLineNetWeight,
					Parent.CUI_NetWeightUQ));
			}
		}

		new CusPackableItem Parent => base.Parent as CusPackableItem;
	}
}
