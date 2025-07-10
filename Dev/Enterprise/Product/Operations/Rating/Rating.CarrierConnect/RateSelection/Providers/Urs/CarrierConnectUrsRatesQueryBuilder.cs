using System.Linq;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Urs.Api.Integration.DTOs.Request;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Rating.CarrierConnect
{
	internal static class CarrierConnectUrsRatesQueryBuilder
	{
		public static QueryRequestDto Build(RateQueryBusinessObject rateQuery)
		{
			var queryRequest = new QueryRequestDto
			{
				QuerySources = [QuerySource.Database, QuerySource.OceanOnDemand]
			};

			var mappedContainerMode = MapContainerMode(rateQuery.ContainerMode, rateQuery.TransportMode);

			// TODO: Does URS just use a single date? If so, what do we use?
			if (rateQuery.EffectiveDate != null)
			{
				queryRequest.ViewDate.ViewDate = rateQuery.EffectiveDate.Value;
			}

			queryRequest.Route.Origin = rateQuery.Origin;
			queryRequest.Route.Destination = rateQuery.Destination;
			queryRequest.Filters.ModesOfTransport.AddRange(UrsWiseRatesConverter.MapTransportMode(rateQuery.TransportMode));
			queryRequest.Filters.FreightShippingTerms.Add(mappedContainerMode);
			queryRequest.AllowedNamedAccounts = RatingHelper.GetAllowedNamedAccounts();

			return queryRequest;
		}

		static string MapContainerMode(string containerMode, string transportMode)
		{
			if ((containerMode == Constants.ContainerModes.BuyersConsol || containerMode == Constants.ContainerModes.Groupage)
				&& EnabledInRatesSubscription(containerMode, transportMode))
			{
				return GroupageAndBuyersConsolMapping(transportMode);
			}

			return containerMode;
		}

		static string GroupageAndBuyersConsolMapping(string transportMode) =>
			transportMode switch
			{
				Constants.TransportModes.Sea => Constants.ContainerModes.FCL,
				Constants.TransportModes.Air => Constants.ContainerModes.ULD,
				_ => null
			};

		static bool EnabledInRatesSubscription(string containerMode, string transportMode) =>
			DataRegistryRating.Instance.RatesServiceSubscription.Value.Cast<RatesServiceRegistrySettings>()
				.Any(setting => setting.TransportMode == transportMode && setting.ContainerMode == containerMode && setting.IsSubscriptionEnabled == true);
	}
}
