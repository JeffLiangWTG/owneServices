using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class CrossTradeComparer : BaseRateLineComparer
	{
		public CrossTradeComparer()
		{
		}

		public override int Compare(FastLine line1, FastLine line2)
		{
			var line1CrossTrade = line1.ParentRateEntry.IsCrossTrade();
			var line2CrossTrade = line2.ParentRateEntry.IsCrossTrade();

			if (line1CrossTrade || line2CrossTrade)
			{
				if (line1CrossTrade && line2CrossTrade)
				{
					return 0;
				}

				if (line1CrossTrade)
				{
					return 1;
				}

				return -1;
			}

			return 0;
		}

		protected override string GetName()
		{
			return (NoResString)"Cross Trade"; // log message, subject to change, more for support people as of now
		}
	}
}
