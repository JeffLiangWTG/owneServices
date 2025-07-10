using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.Forwarding.Business
{
	/// <summary>
	///		An adapter for a special case when we autorate costs for ULD consols.
	///
	///		The standard <see cref="ForwardingConsolRatingAdapter"/> will try to autorate all pack lines - packed and unpacked.
	///		But, we want unpacked lines to be calculated with LSE rates rather than ULD rates.
	///
	///		So, this adapter enhances <see cref="ForwardingConsolRatingAdapter"/> to autorate only unpacked lines while packed lines
	///		will be autorated by <see cref="UldForwardingConsolRatingAdapter"/>.
	/// </summary>
	public class LseInUldForwardingConsolRatingAdapter : ForwardingConsolRatingAdapter
	{
		public LseInUldForwardingConsolRatingAdapter(IRatingRoute<IRoutingSupport> ratingRoute, bool dontAutorateServices = false) : base(ratingRoute, dontAutorateServices)
		{
		}

		public static bool CanAutoRate(IRatingRoute<IRoutingSupport> ratingRoute)
		{
			// No need to autorate Lse rates if there are no lse measures (i.e. unpacked lines)
			var consol = ratingRoute.Parent as ForwardingConsol;
			return consol != null && GetPackages(consol, includePacked: false, includeUnpacked: true, fallbackToShipmentMeasures: false) != null;
		}

		public override ZString ContainerMode => Constants.ContainerModes.Loose;

		public override ZBool ShouldDiscardPerJobCharges => true;

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet();

				// This adapter is only about pack lines measures. All other measures (like container count, shipment count) will be
				// calculated by UldForwardingConsolRatingAdapter as before.
				//
				// Also, by default shipment includes its own measures as a package if it doesn't have pack lines at all, but
				// for LSE adapter in ULD mode we only care about pack lines measures, so, we don't fallback in this case.
				var packs = GetPackages(Parent, includePacked: false, includeUnpacked: true, fallbackToShipmentMeasures: false);
				result.AddPartList(MeasureType.Weight, packs);
				result.AddPartList(MeasureType.Volume, packs);
				result.AddPartList(MeasureType.Package, packs);
				result.AddPartList(MeasureType.LoadingMeters, packs);

				return result;
			}
		}
	}
}