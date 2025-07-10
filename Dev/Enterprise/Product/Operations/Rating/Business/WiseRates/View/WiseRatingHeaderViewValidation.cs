namespace Enterprise.Rating.Business
{
	public class WiseRatingHeaderViewValidation : WiseRatesViewsValidation<WiseRatingHeaderView>
	{
		public WiseRatingHeaderViewValidation(WiseRatingHeaderView parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			parent.WiseEntryViews.RunPreSaveValidation();
		}

		protected override void ValidateAllCore()
		{
		}
	}
}
