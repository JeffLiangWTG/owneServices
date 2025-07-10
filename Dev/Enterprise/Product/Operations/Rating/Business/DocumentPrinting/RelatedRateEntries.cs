namespace Enterprise.Rating.Business
{
	using System.Collections.Generic;

	public class RelatedRateEntries
	{
		public List<RateEntry> FreightRateEntries { get; set; }
		public List<RateEntry> OriginRateEntries { get; set; }
		public List<RateEntry> DestinationRateEntries { get; set; }
	}
}
