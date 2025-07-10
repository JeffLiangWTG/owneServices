using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.Business
{
	public abstract class RatingRouteAdapter<T> : RatingAdapter<T> where T : BusinessObject
	{
		protected RatingRouteAdapter(IRatingRoute<IRoutingSupport> ratingRoute) : base((T)ratingRoute.Parent)
		{
			if (ratingRoute == null)
			{
				throw new ArgumentNullException(nameof(ratingRoute));
			}

			this.ratingRoute = ratingRoute;
		}

		IRatingRoute<IRoutingSupport> ratingRoute;

		public ZString TransportMode
		{
			get { return ratingRoute.TransportMode; }
		}

		public sealed override OrgHeader Carrier
		{
			get { return ratingRoute.Carrier; }
		}

		public sealed override Creditors Creditors
		{
			get { return ratingRoute.Creditors; }
		}

		public sealed override IJobDatesProvider JobDatesProvider
		{
			get { return ratingRoute.JobDatesProvider; }
		}

		public sealed override ILocation Origin
		{
			get { return ratingRoute.Origin; }
		}

		public sealed override ILocation Destination
		{
			get { return ratingRoute.Destination; }
		}

		public sealed override ILocation GetVia(CostSell costOrSell) => ratingRoute.GetVia(costOrSell);

		public sealed override ILocation GetFirstLoad(CostSell costOrSell) => ratingRoute.GetFirstLoad(costOrSell);

		public sealed override ILocation GetLastDischarge(CostSell costOrSell) => ratingRoute.GetLastDischarge(costOrSell);

		public sealed override ILocation GetFirstRouteSetLoad(CostSell costOrSell) => ratingRoute.GetFirstRouteSetLoad(costOrSell);

		public sealed override ILocation GetLastRouteSetDischarge(CostSell costOrSell) => ratingRoute.GetLastRouteSetDischarge(costOrSell);

		public sealed override ZInt RouteSetNumber
		{
			get { return ratingRoute.RouteSetNumber; }
		}

		protected void OverrideRatingRoute(IRatingRoute<IRoutingSupport> newRatingRoute)
		{
			ratingRoute = newRatingRoute;
		}
	}
}
