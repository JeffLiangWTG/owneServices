namespace Enterprise.MasterFiles.Business.Rating
{
	public class RatingDocumentsChargeGroupingOrRollupValidation : AutoRatingDocumentsChargeGroupingOrRollupValidation
	{
		public RatingDocumentsChargeGroupingOrRollupValidation(AutoRatingDocumentsChargeGroupingOrRollup parent)
			: base(parent)
		{
		}

		new RatingDocumentsChargeGroupingOrRollup Parent => (RatingDocumentsChargeGroupingOrRollup)base.Parent;

		protected override void CheckRCG_Module()
		{
			Parent.Helper.ValidateModule();
		}

		protected override void CheckRCG_JobType()
		{
			Parent.Helper.ValidateJobType();
		}

		protected override void CheckRCG_TransportMode()
		{
			Parent.Helper.ValidateTransportMode();
		}

		protected override void CheckRCG_Display()
		{
			Parent.Helper.ValidateDisplay();
		}

		protected override void CheckRCG_Style()
		{
			Parent.Helper.ValidateStyle();
		}
	}
}
