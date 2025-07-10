//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusPackingListValidation
//
//    This class should be used for overriding validation in AutoCusPackingListValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	using System.Linq;
	using Enterprise.MasterFiles.Business;

	public class CusPackingListValidation : AutoCusPackingListValidation
	{
		public CusPackingListValidation(AutoCusPackingList parent) : base(parent)
		{
		}

		public new CusPackingList Parent => (CusPackingList)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTotalNetWeight();
		}

		protected override void CheckCUL_CustomAttribute1()
		{
			base.CheckCUL_CustomAttribute1();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CUL_CustomAttribute1Info);
		}

		protected override void CheckCUL_CustomAttribute2()
		{
			base.CheckCUL_CustomAttribute2();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CUL_CustomAttribute2Info);
		}

		protected override void CheckCUL_CustomDate1()
		{
			base.CheckCUL_CustomDate1();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CUL_CustomDate1Info);
		}

		protected override void CheckCUL_CustomDate2()
		{
			base.CheckCUL_CustomDate2();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CUL_CustomDate2Info);
		}

		protected override void CheckCUL_CustomDecimal1()
		{
			base.CheckCUL_CustomDecimal1();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CUL_CustomDecimal1Info);
		}

		protected override void CheckCUL_CustomDecimal2()
		{
			base.CheckCUL_CustomDecimal2();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CUL_CustomDecimal2Info);
		}

		public void ValidateTotalNetWeight()
		{
			ValidateCalculatedProperty(Parent.TotalNetWeightInfo);
		}

		protected void CheckTotalNetWeight()
		{
			var totalNetWeight = Parent.TotalNetWeight;
			var invoiceLinesNetWeight = Parent.PackableItems.Cast<CusPackableItem>().Sum(item => Core.Constants.Weight.ConvertSafe(item.CUI_NetWeight, item.CUI_NetWeightUQ, Core.Constants.Weight.Kilograms));

			if (invoiceLinesNetWeight != totalNetWeight)
			{
				Parent.TotalNetWeightInfo.AddWarning(Res.GetString("8DAD32C5-694B-43EF-B76C-D6C567B45054", "The sum of all Invoice Line Net Weight {0} KG does not balance with the Packing List total Net Weight {1} KG.", invoiceLinesNetWeight, totalNetWeight));
			}
		}

		CustomLabelPropertyValidation CustomLabelPropertyValidation => customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory));
		CustomLabelPropertyValidation customLabelPropertyValidation;

		CusPackingListCustomLabelsProvider CustomLabelsProvider => customLabelsProvider ?? (customLabelsProvider = new CusPackingListCustomLabelsProvider(Parent));
		CusPackingListCustomLabelsProvider customLabelsProvider;
	}
}
