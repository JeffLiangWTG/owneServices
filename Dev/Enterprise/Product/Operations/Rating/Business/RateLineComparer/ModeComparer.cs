using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class ModeComparer : BaseRateLineComparer
	{
		public override int Compare(FastLine line1, FastLine line2)
		{
			var entry1 = line1.ParentRateEntry;
			var entry2 = line2.ParentRateEntry;

			if (entry1.TI_RateCategory != entry2.TI_RateCategory)
			{
				return 0;
			}

			if (RatingConstants.RateMode.IsFirstModeMoreSpecific(entry1.TI_Mode, entry2.TI_Mode, entry1.TI_RateCategory))
			{
				return 1;
			}

			if (RatingConstants.RateMode.IsFirstModeMoreSpecific(entry2.TI_Mode, entry1.TI_Mode, entry1.TI_RateCategory))
			{
				return -1;
			}

			return 0;
		}

		protected override string GetName()
		{
			return (NoResString)"Mode and Category"; // log message, subject to change, more for support people as of now
		}
	}
}
