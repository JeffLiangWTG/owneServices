using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Urs.Api.Integration.DTOs.Request;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.CarrierConnect.Test
{
	public class CarrierConnectUrsRatesQueryBuilderTest : TestCaseWithFactory
	{
		public void TestMapsQuerySource()
		{
			var rateQuery = GetValidQuery();
			var rateQueryBizo = new RateQueryBusinessObject(rateQuery);
			var queryRequest = CarrierConnectUrsRatesQueryBuilder.Build(rateQueryBizo);

			AssertContainsExactElementsInAnyOrder([QuerySource.Database, QuerySource.OceanOnDemand], queryRequest.QuerySources);
		}
		public void TestMapsNamedAccounts()
		{
			var rateQuery = GetValidQuery();
			rateQuery.TransportMode = "SEA";
			var rateQueryBizo = new RateQueryBusinessObject(rateQuery);
			var queryRequest = CarrierConnectUrsRatesQueryBuilder.Build(rateQueryBizo);

			// Current user should have full access
			AssertContainsExactElementsInAnyOrder([], queryRequest.AllowedNamedAccounts);
		}

		public void TestMapsTransportModes_AIR() =>
			AssertTransportModes(
				TransportModes.Air,
				[UrsConstants.ModeOfTransport.Air]);

		public void TestMapsTransportModes_SEA() =>
		AssertTransportModes(
			TransportModes.Sea,
			[
				UrsConstants.ModeOfTransport.Ocean,
				UrsConstants.ModeOfTransport.ShortSea,
				UrsConstants.ModeOfTransport.InlandNavigation,
			]);

		public void TestMapsContainerMode_BCN_SEA_RatesServiceTrue() =>
			AssertContainerMode(ContainerModes.BuyersConsol, TransportModes.Sea, true, new[] { ContainerModes.FCL });

		public void TestMapsContainerMode_BCN_SEA_RatesServiceFalse() =>
			AssertContainerMode(ContainerModes.BuyersConsol, TransportModes.Sea, false, new[] { ContainerModes.BuyersConsol });

		public void TestMapsContainerMode_BCN_AIR_RatesServiceTrue() =>
			AssertContainerMode(ContainerModes.BuyersConsol, TransportModes.Air, true, new[] { ContainerModes.ULD });

		public void TestMapsContainerMode_BCN_AIR_RatesServiceFalse() =>
			AssertContainerMode(ContainerModes.BuyersConsol, TransportModes.Air, false, new[] { ContainerModes.BuyersConsol });

		public void TestMapsContainerMode_GRP_RatesServiceTrue() =>
			AssertContainerMode(ContainerModes.Groupage, TransportModes.Sea, true, new[] { ContainerModes.FCL });

		public void TestMapsContainerMode_GRP_RatesServiceFalse() =>
			AssertContainerMode(ContainerModes.Groupage, TransportModes.Sea, false, new[] { ContainerModes.Groupage });

		void AssertContainerMode(string containerMode, string transportMode, bool isEnabledForRatesService, string[] expectedContainerModes)
		{
			var rateQuery = GetValidQuery();
			rateQuery.ContainerMode = containerMode;
			rateQuery.TransportMode = transportMode;
			rateQuery.JobInfo = new JobInfoDto
			{
				Containers = new List<JobContainerDto>()
			};

			var rateServiceSettings = new RatesServiceRegistrySettingsCollection();
			rateServiceSettings.Add(new RatesServiceRegistrySettings(transportMode, containerMode, isEnabledForRatesService));

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rateServiceSettings))
			{
				var rateQueryBizo = new RateQueryBusinessObject(rateQuery);
				var queryRequest = CarrierConnectUrsRatesQueryBuilder.Build(rateQueryBizo);
				AssertContainsExactElementsInAnyOrder(expectedContainerModes, queryRequest.Filters.FreightShippingTerms);
			}
		}

		void AssertTransportModes(string transportMode, string[] expectedTransportModes)
		{
			var rateQuery = GetValidQuery();
			rateQuery.TransportMode = transportMode;
			var rateQueryBizo = new RateQueryBusinessObject(rateQuery);
			var queryRequest = CarrierConnectUrsRatesQueryBuilder.Build(rateQueryBizo);
			AssertContainsExactElementsInAnyOrder(expectedTransportModes, queryRequest.Filters.ModesOfTransport);
		}

		RateQueryDto GetValidQuery()
		{
			return new RateQueryDto
			{
				Origin = "AUMEL",
				Destination = "USLAX",
				EffectiveDate = DateTime.UtcNow,
				TransportMode = "SEA",
				ContainerMode = "LCL",
				RateTypes = new[] { "Forwarding" },
				Context = RateQueryDto.RateSearchContext.RateSearch,
			};
		}
	}
}
