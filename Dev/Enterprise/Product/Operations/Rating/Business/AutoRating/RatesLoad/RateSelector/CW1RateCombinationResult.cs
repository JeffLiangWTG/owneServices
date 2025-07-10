using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Rating.Business.RateSelector
{
	public class CW1RateCombinationResult
	{
		public RatingCriteria Criteria { get; set; }
		public ZGuid ContainerTypePk { get; set; }
		public ZString CommodityCode { get; set; }
		public IRateEntry HeadEntry { get; set; }
		public IEnumerable<IRateLine> Lines { get; set; }
	}
}
