using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class PricingPageRateLineListList : List<PricingPageRateLineList>
	{
		public PricingPageRateLineListList() { }

		public PricingPageRateLineListList(List<PricingPageRateLineList> lineSets, Dictionary<ZGuid, RateLine> clonedRateLines)
		{
			AddRange(lineSets);
			ClonedRateLines = clonedRateLines;
		}

		Dictionary<ZGuid, RateLine> ClonedRateLines { get; set; }

		List<ZGuid> LinePks => linePks ??= this.SelectMany(x => x.Select(l => l.PK).ToList()).ToList();
		List<ZGuid> linePks;

		public bool ContainsLine(RateLine line)
			=> LinePks.Contains(line.PK)
				|| (ClonedRateLines != null
				&& ClonedRateLines.ContainsKey(line.PK));
	}
}
