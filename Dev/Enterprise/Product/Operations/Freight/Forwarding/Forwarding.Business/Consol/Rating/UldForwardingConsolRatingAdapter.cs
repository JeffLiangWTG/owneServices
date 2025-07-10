using Enterprise.Freight.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.Forwarding.Business
{
	/// <summary>
	///		An adapter for a special case when we autorate costs for ULD consols.
	///
	///		The standard <see cref="ForwardingConsolRatingAdapter"/> will try to autorate all pack lines - packed and unpacked.
	///		But, we want unpacked lines to be calculated with LSE rates (<see cref="LseInUldForwardingConsolRatingAdapter"/>) rather than ULD rates.
	///
	///		So, this adapter enhances <see cref="ForwardingConsolRatingAdapter"/> to autorate only packed lines while unpacked lines
	///		will be autorated by <see cref="LseInUldForwardingConsolRatingAdapter"/>.
	/// </summary>
	public class UldForwardingConsolRatingAdapter : ForwardingConsolRatingAdapter
	{
		public UldForwardingConsolRatingAdapter(IRatingRoute<IRoutingSupport> ratingRoute, bool dontAutorateServices = false)
			: base(ratingRoute, dontAutorateServices)
		{
		}

		protected override RateablePartList GetPackages()
		{
			return GetPackages(Parent, includePacked: true, includeUnpacked: false, fallbackToShipmentMeasures: false);
		}
	}
}