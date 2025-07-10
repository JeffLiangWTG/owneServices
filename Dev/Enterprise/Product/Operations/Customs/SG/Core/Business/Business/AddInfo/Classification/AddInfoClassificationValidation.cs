using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoClassificationValidation : SGAddInfoValidation
	{
		public AddInfoClassificationValidation(AddInfoClassification parent)
			: base(parent)
		{
		}

		public new AddInfoClassification Parent
		{
			get { return (AddInfoClassification)base.Parent; }
		}

		protected override void CheckSG_PercAlcohol()
		{
			base.CheckSG_PercAlcohol();
			CompareValidation.CheckNumberNotNegative(Parent.SG_PercAlcoholInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_PercAlcoholInfo, 100);
		}
	}
}
