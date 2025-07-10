using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class TransitTimeOrFrequencyComparer : BaseRateLineComparer
	{
		public TransitTimeOrFrequencyComparer(IRateEntry parent)
		{
			this.parent = parent;
		}

		readonly IRateEntry parent;

		public override int Compare(FastLine line1, FastLine line2)
		{
			if (IsTransitTimeOrFrequencyOverridden(line2, line1))
			{
				return 1;
			}

			if (IsTransitTimeOrFrequencyOverridden(line1, line2))
			{
				return -1;
			}

			return 0;
		}

		bool IsTransitTimeOrFrequencyOverridden(FastLine line, FastLine possibleOverride)
		{
			if (possibleOverride.ParentRateEntry.TI_TransitTime == parent.TI_TransitTime
				&& line.ParentRateEntry.TI_TransitTime != parent.TI_TransitTime)
			{
				return true;
			}

			if (possibleOverride.ParentRateEntry.TI_FrequencyUnit == parent.TI_FrequencyUnit
				&& line.ParentRateEntry.TI_FrequencyUnit != parent.TI_FrequencyUnit)
			{
				return true;
			}

			if (possibleOverride.ParentRateEntry.TI_FrequencyUnit == parent.TI_FrequencyUnit
				&& line.ParentRateEntry.TI_FrequencyUnit == parent.TI_FrequencyUnit
				&& possibleOverride.ParentRateEntry.TI_Frequency == parent.TI_Frequency
				&& line.ParentRateEntry.TI_Frequency != parent.TI_Frequency)
			{
				return true;
			}

			return false;
		}

		protected override string GetName()
		{
			return (NoResString)"Transit Time or Frequency"; // log message, subject to change, more for support people as of now
		}
	}
}
