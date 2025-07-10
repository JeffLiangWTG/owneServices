using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentRatingRouteAdapter : ForwardingShipmentRatingAdapter
	{
		internal ForwardingShipmentRatingRouteAdapter(IRatingRoute<IRoutingSupport> ratingRoute)
			: base((ForwardingShipment)ratingRoute.Parent)
		{
			RatingRoute = Argument.NotNull(ratingRoute, nameof(ratingRoute));
		}

		public ZString TransportMode => RatingRoute.TransportMode;

		public override OrgHeader Carrier => RatingRoute.Carrier;

		public override Creditors Creditors => RatingRoute.Creditors;

		public override IJobDatesProvider JobDatesProvider => RatingRoute.JobDatesProvider;

		public override ILocation Origin => RatingRoute.Origin;

		public override ILocation Destination => RatingRoute.Destination;

		public override ILocation GetVia(CostSell costOrSell) => RatingRoute.GetVia(costOrSell);

		public override ZInt RouteSetNumber => RatingRoute.RouteSetNumber;

		IRatingRoute<IRoutingSupport> RatingRoute { get; }
	}
}
