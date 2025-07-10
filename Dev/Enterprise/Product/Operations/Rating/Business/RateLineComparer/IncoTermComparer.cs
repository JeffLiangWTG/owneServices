using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class IncoTermComparer : BaseRateLineComparer
	{
		public override int Compare(FastLine line1, FastLine line2)
		{
			if (line1.ParentRateEntry.IsSpotEntry != line2.ParentRateEntry.IsSpotEntry)
			{
				return 0;
			}

			var line2Importance = line2.GetOrgImportance();
			var line1Importance = line1.GetOrgImportance();

			return line2Importance - line1Importance;
		}

		protected override string GetName()
		{
			return (NoResString)"Sell Rate Priorities"; // log message, subject to change, more for support people as of now
		}
	}
}
