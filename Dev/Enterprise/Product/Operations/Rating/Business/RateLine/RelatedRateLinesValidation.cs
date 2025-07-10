namespace Enterprise.Rating.Business
{
	public class RelatedRateLinesValidation : RateLinesValidation
	{
		public RelatedRateLinesValidation(RelatedRateLine parent)
			: base(parent)
		{
		}
	}

	public class RateLinesValidation : AutoRateLinesValidation
	{
		public RateLinesValidation(AutoRateLines parent)
			: base(parent)
		{
		}

		#region ValidateUseOnlyActualWeightMeasure

		public void ValidateUseOnlyActualWeightMeasure()
		{
			ValidateCalculatedProperty(Parent.UseOnlyActualWeightMeasureInfo);
		}

		protected virtual void CheckUseOnlyActualWeightMeasure()
		{
		}

		#endregion

		new RateLine Parent
		{
			get { return (RateLine)base.Parent; }
		}
	}
}
