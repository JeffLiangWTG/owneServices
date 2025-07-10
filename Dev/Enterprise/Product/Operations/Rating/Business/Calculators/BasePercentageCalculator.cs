namespace Enterprise.Rating.Business
{
	public abstract class BasePercentageCalculator : Calculator, IDependentCalculator
	{
		protected BasePercentageCalculator(IRateLine master)
			: base(master)
		{
		}

		public override void ValidateTM_AC(RateLineItem lineItem)
		{
			base.ValidateTM_AC(lineItem);
			((IDependentCalculator)this).ValidateTM_AC(lineItem);
		}

		public override void ValidateTM_Text(RateLineItem lineItem)
		{
			base.ValidateTM_Text(lineItem);
			((IDependentCalculator)this).ValidateTM_Text(lineItem);
		}

		IRateLine IDependentCalculator.Master
		{
			get { return Line; }
		}
	}
}

