namespace Enterprise.Freight.Business
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Integration.Accounting;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Business.UniversalCopy;
	using Enterprise.ZArchitecture.Schema;

	public interface IRoutingSupport
	{
		BusinessObjectFactory Factory { get; }
		RoutingCollection TransportsIncludingRelated { get; }

		[UniversalCopyCollectionEntity(JobConsolTransportSchema.Constants.TableName, JobConsolTransportSchema.Constants.JW_ParentGUID)]
		TransportCollection Transports { get; }

		ZString TransportMode { get; }

		string AdditionalETAUpdateMsg { get; }
		string AdditionalETDUpdateMsg { get; }
	}

	public static class IRoutingSupportExtensions
	{
		public static ReadOnlyCollection<RouteSetRatingRoute> GetRatingRoutes(this IRoutingSupport routingSupport, CostSell costOrSell)
		{
			var ratingRoutes = new List<RouteSetRatingRoute>();
			if (RatingDataRegistry.Instance.MultiModalRatingCost.Value && costOrSell == CostSell.Cost)
			{
				ratingRoutes.AddRange(routingSupport.TransportsIncludingRelated.RouteSets.Select(x => new RouteSetRatingRoute(x, routingSupport)));
			}

			return new ReadOnlyCollection<RouteSetRatingRoute>(ratingRoutes);
		}
	}
}
