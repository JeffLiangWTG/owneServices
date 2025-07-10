using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// A set of values, that if present, should be set as default filters
	/// and values in the RatingForm's RateEntry filters. This class only
	/// specifies the values, but not the filters that it applies to. The
	/// class processing this must know to which filter it is set.
	/// </summary>
	public class RateEntryFilterValue
	{
		public RateEntryFilterValue()
		{
		}
		// Set when called with a Carrier Contract or a Carrier Contract Allocation
		public ZDate StartDate { get; set; }
		public ZDate ExpiryDate { get; set; } // Can be ZDate.Empty
		public ZString ContractNumber { get; set; }
		public ZString TransportMode { get; set; }

		// Set only when called with Carrier Contract
		public ZString ContainerType { get; set; } // e.g. 'FLT' (Flat rack), 'DRY' (Dry storage)

		// Set only when called with a Carrier Contract Allocation
		public ZString Origin { get; set; }
		public ZString Destination { get; set; }
		public ZBool AllowHazardousCommodity { get; set; }
		public ZString ContainerCode { get; set; } // e.g. '20FR', '40GP'
		public ZString CommodityCode { get; set; }
	}
}
