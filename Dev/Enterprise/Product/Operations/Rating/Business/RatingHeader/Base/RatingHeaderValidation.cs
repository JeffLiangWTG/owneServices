using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RatingHeaderValidation : AutoRatingHeaderValidation
	{
		public RatingHeaderValidation(AutoRatingHeader parent)
			: base(parent)
		{
		}

		protected override void CheckTH_GlobalRateDescription()
		{
			base.CheckTH_GlobalRateDescription();
			TranslatableDataFieldAttribute.Validate(Parent.TH_GlobalRateDescriptionInfo);
		}
	}
}

