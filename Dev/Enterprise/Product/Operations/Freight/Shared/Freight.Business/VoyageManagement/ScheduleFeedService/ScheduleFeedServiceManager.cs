using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public static class ScheduleFeedServiceManager
	{
		public static void Update(IVoyageInformationProvider voyageInformationProvider, BusinessObjectFactory factory)
		{
			Argument.NotNull(voyageInformationProvider, nameof(voyageInformationProvider));
			Argument.NotNull(voyageInformationProvider.Origins, nameof(voyageInformationProvider.Origins));
			Argument.NotNull(voyageInformationProvider.Destinations, nameof(voyageInformationProvider.Destinations));

			if (!FreightDataRegistry.Instance.EnableScheduleFeedService.Value)
			{
				return;
			}

			foreach (var origin in voyageInformationProvider.Origins)
			{
				new VoyagePortSubscriptionManager(origin, voyageInformationProvider, factory).Update();
			}

			foreach (var destination in voyageInformationProvider.Destinations)
			{
				new VoyagePortSubscriptionManager(destination, voyageInformationProvider, factory).Update();
			}
		}

		public static void Update(ITrackableVoyagePort voyagePort, IVoyageInformationProvider voyageInformationProvider, BusinessObjectFactory factory)
		{
			Argument.NotNull(voyagePort, nameof(voyagePort));
			Argument.NotNull(voyageInformationProvider, nameof(voyageInformationProvider));

			if (!FreightDataRegistry.Instance.EnableScheduleFeedService.Value)
			{
				return;
			}

			new VoyagePortSubscriptionManager(voyagePort, voyageInformationProvider, factory).Update();
		}
	}
}
