namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Reasons why we don't put the code in ModeComparer as a specific case for BCN/SCN:
	/// - ModeComparer is added in base class SimpleOverriddenRateLinesRemover hence it is inherited in sub classes.
	/// - This comparer is added only in OverriddenRateLineRemover which is a sub class of SimpleOverriddenRateLinesRemover.
	/// </summary>
	class BCNOrSCNComparer : BaseRateLineComparer
	{
		public override int Compare(FastLine line1, FastLine line2)
		{
			var rateEntry1 = line1.ParentRateEntry;
			var rateEntry2 = line2.ParentRateEntry;

			if (line1.ChargeCode == line2.ChargeCode
				&& !rateEntry1.IsAir() && !rateEntry2.IsAir()
				&& rateEntry1.IsFreightEntry() && rateEntry2.IsFreightEntry())
			{
				var result = rateEntry1.IsFCL() ? 1 : 0;
				result += rateEntry2.IsFCL() ? -1 : 0;

				return result;
			}

			return 0;
		}

		protected override string GetName()
		{
			return "BCN/SCN"; // log message, subject to change, more for support people as of now
		}
	}
}
