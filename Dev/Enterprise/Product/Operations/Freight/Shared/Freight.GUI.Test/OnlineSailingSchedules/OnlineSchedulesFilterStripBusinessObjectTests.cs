using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.OnlineSailingSchedules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(OnlineSchedulesFilterStripBusinessObject))]
	sealed class OnlineSchedulesFilterStripBusinessObjectTests : FilterStripBusinessObjectTestCase
	{
		public void TestCustomSQlFilterNotIncluded()
		{
			var factory = new BusinessObjectFactory();
			var routesProvider = new RoutesProvider(factory);
			var onlineSchedules = new OnlineSchedules(factory, routesProvider);

			using (var form = new OnlineSchedulesForm(onlineSchedules))
			{
				form.Show();
				var filterBusinessObject = form.FilterControl.FilterBusinessObject;

				CombineAssertions(() =>
				{
					AssertEquals(null, filterBusinessObject[FilterStripBusinessObject.CustomSqlFilterDescription]);
					AssertNotEquals(null, filterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Origin]);
				});
			}
		}

		public void TestOriginAndDestinationFiltersInLocationsGroup()
		{
			var factory = new BusinessObjectFactory();
			var routesProvider = new RoutesProvider(factory);
			var onlineSchedules = new OnlineSchedules(factory, routesProvider);

			using (var form = new OnlineSchedulesForm(onlineSchedules))
			{
				form.Show();
				var filterBusinessObject = form.FilterControl.FilterBusinessObject;

				CombineAssertions(() =>
				{
					var originFilter = filterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Origin];
					AssertEquals("Locations", originFilter.Category.ToString());

					var destinationFilter = filterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Destination];
					AssertEquals("Locations", destinationFilter.Category.ToString());
				});
			}
		}

		public void TestBuildRequest()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "AAAAA";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "ABCD";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "OLGA MAERSK";
			vessel.RV_LloydsNumber = "2048";

			Factory.Save();

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);
			var filter = new OnlineSchedulesFilterStripBusinessObject(onlineSchedules);

			ModuleNkFilter originFilter = (ModuleNkFilter)filter["Origin"];
			AssertEquals(FilterVisibility.AlwaysVisible, originFilter.Visibility);
			originFilter.IsActive = true;
			originFilter.Property = "AUSYD";

			ModuleNkFilter destinationFilter = (ModuleNkFilter)filter["Destination"];
			AssertEquals(FilterVisibility.AlwaysVisible, destinationFilter.Visibility);
			destinationFilter.IsActive = true;
			destinationFilter.Property = "AUBNE";

			ModuleDateFilter departureFilter = (ModuleDateFilter)filter["Departure"];
			AssertEquals(FilterVisibility.AlwaysVisible, departureFilter.Visibility);
			departureFilter.IsActive = true;
			departureFilter.Property1 = new ZDateTime(2016, 8, 10);
			departureFilter.Property2 = new ZDateTime(2016, 8, 15);
			departureFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			ModuleDateFilter arrivalFilter = (ModuleDateFilter)filter["Arrival"];
			AssertEquals(FilterVisibility.AlwaysVisible, arrivalFilter.Visibility);
			arrivalFilter.IsActive = true;
			arrivalFilter.Property1 = new ZDateTime(2016, 9, 10);
			arrivalFilter.Property2 = new ZDateTime(2016, 9, 15);
			arrivalFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			ModuleGuidFilter carrierFilter = (ModuleGuidFilter)filter["Carrier"];
			AssertNotEquals(FilterVisibility.AlwaysVisible, carrierFilter.Visibility);
			carrierFilter.IsActive = true;
			carrierFilter.Property = carrier.PK;

			OnlineSchedulesVoyageVesselFilter voyageVesselFilter = (OnlineSchedulesVoyageVesselFilter)filter["Voyage # and Vessel"];
			AssertEquals(FilterVisibility.AlwaysVisible, voyageVesselFilter.Visibility);
			voyageVesselFilter.IsActive = true;
			voyageVesselFilter.Property = "754N";
			voyageVesselFilter.Vessel = "OLGA MAERSK";

			ModuleNumberFilter transitTimeFilter = (ModuleNumberFilter)filter["Transit Time"];
			AssertNotEquals(FilterVisibility.AlwaysVisible, transitTimeFilter.Visibility);
			transitTimeFilter.IsActive = true;
			transitTimeFilter.Property = "23";

			ModuleNumberFilter legsCountFilter = (ModuleNumberFilter)filter["Max. Legs"];
			AssertNotEquals(FilterVisibility.AlwaysVisible, legsCountFilter.Visibility);
			legsCountFilter.IsActive = true;
			legsCountFilter.Property = "10";

			var includeRelatedPortsFilter = (ModuleFlagsFilter)filter["Show Related UNLOCOs"];
			AssertEquals(FilterVisibility.AlwaysVisible, includeRelatedPortsFilter.Visibility);
			includeRelatedPortsFilter.IsActive = true;
			includeRelatedPortsFilter.Property0 = false;

			var routesOptionsFilter = (ModuleFlagsFilter)filter["Routes Options"];
			AssertNotEquals(FilterVisibility.AlwaysVisible, routesOptionsFilter.Visibility);
			routesOptionsFilter.IsActive = true;
			routesOptionsFilter.Property0 = false;
			routesOptionsFilter.Property1 = true;

			var serviceStringFilter = (ModuleTextFilter)filter["Service String"];
			AssertNotEquals(FilterVisibility.AlwaysVisible, serviceStringFilter.Visibility);
			serviceStringFilter.IsActive = true;
			serviceStringFilter.Property = "AL1";

			var filterRequest = filter.BuildRequest();

			AssertEquals("AUSYD", filterRequest.LoadPort);
			AssertEquals("AUBNE", filterRequest.DischargePort);

			AssertEquals("2016-08-10", filterRequest.EtdFrom);
			AssertEquals("2016-08-15", filterRequest.EtdTo);
			AssertEquals("2016-09-10", filterRequest.EtaFrom);
			AssertEquals("2016-09-15", filterRequest.EtaTo);

			AssertEquals("ABCD", filterRequest.CarrierCode);
			AssertEquals("754N", filterRequest.VoyageNumber);
			AssertEquals("OLGA MAERSK", filterRequest.VesselName);
			AssertEquals("2048", filterRequest.ImoNumber);

			AssertEquals("23", filterRequest.TransitTime);
			AssertEquals("1", filterRequest.LegsCount);

			AssertEquals("False", filterRequest.IncludeRelatedPorts);
			AssertEquals("True", filterRequest.SameCarrierRoutes);
			AssertEquals("AL1", filterRequest.ServiceString);

			// Test Carrier SCAC filter takes precedence over Carrier filter
			ModuleTextFilter carrierSCACFilter = (ModuleTextFilter)filter["Carrier SCAC"];
			AssertNotEquals(FilterVisibility.AlwaysVisible, carrierFilter.Visibility);
			carrierSCACFilter.IsActive = true;
			carrierSCACFilter.Property = "AMMA";

			filterRequest = filter.BuildRequest();
			AssertEquals("AMMA", filterRequest.CarrierCode);
		}

		public void TestValidateTransitTime()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);
			var filter = new OnlineSchedulesFilterStripBusinessObject(onlineSchedules);

			ModuleNumberFilter transitTimeFilter = (ModuleNumberFilter)filter["Transit Time"];
			transitTimeFilter.IsActive = true;
			transitTimeFilter.Property = "23";
			AssertNoError(transitTimeFilter.PropertyInfo, "Transit Time must be a number greater than or equal to zero.");

			transitTimeFilter.Property = "aa";
			AssertHasError(transitTimeFilter.PropertyInfo, "Transit Time must be a number greater than or equal to zero.");

			transitTimeFilter.Property = "-10";
			AssertHasError(transitTimeFilter.PropertyInfo, "Transit Time must be a number greater than or equal to zero.");
		}

		public void TestValidateLegsCount()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);
			var filter = new OnlineSchedulesFilterStripBusinessObject(onlineSchedules);

			ModuleNumberFilter legCountFilter = (ModuleNumberFilter)filter["Max. Legs"];
			legCountFilter.IsActive = true;
			legCountFilter.Property = "23";
			AssertNoError(legCountFilter.PropertyInfo, "Legs Count must be a number greater than zero.");

			legCountFilter.Property = "aa";
			AssertHasError(legCountFilter.PropertyInfo, "Legs Count must be a number greater than zero.");

			legCountFilter.Property = "0";
			AssertHasError(legCountFilter.PropertyInfo, "Legs Count must be a number greater than zero.");

			legCountFilter.Property = "-10";
			AssertHasError(legCountFilter.PropertyInfo, "Legs Count must be a number greater than zero.");
		}

		public void TestValidateCarrier()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "AAAAA";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "ABCD";
			carrier1.OH_RSL_ShippingLine = shippingLine.PK;

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "BBBBB";

			Factory.Save();

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);
			var filter = new OnlineSchedulesFilterStripBusinessObject(onlineSchedules);

			ModuleGuidFilter carrierFilter = (ModuleGuidFilter)filter["Carrier"];
			carrierFilter.IsActive = true;
			carrierFilter.Property = carrier1.PK;

			AssertNoError(carrierFilter.PropertyInfo, "Carrier doesn't have SCAC.");

			carrierFilter.Property = carrier2.PK;
			AssertHasError(carrierFilter.PropertyInfo, "Carrier doesn't have SCAC.");
		}

		public void TestValidateCarrierScac()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);
			var filter = new OnlineSchedulesFilterStripBusinessObject(onlineSchedules);

			var errorMsg = "Carrier SCAC must consist of 4 characters.";
			ModuleTextFilter carrierScacFilter = (ModuleTextFilter)filter[OnlineSchedulesFilterStripBusinessObject.Descriptions.CarrierScac];
			carrierScacFilter.IsActive = true;

			carrierScacFilter.Property = "test";
			AssertEquals(false, carrierScacFilter.PropertyInfo.HasNotification(errorMsg));

			carrierScacFilter.Property = string.Empty;
			AssertEquals(false, carrierScacFilter.PropertyInfo.HasNotification(errorMsg));

			carrierScacFilter.Property = "abcde";
			AssertEquals(true, carrierScacFilter.PropertyInfo.HasNotification(errorMsg));
		}

		public void TestSetFilterDefaults()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			Factory.Save();

			var filterDefaults = new OnlineSchedulesFilterStripBusinessObject.FilterDefaults()
			{
				Origin = "AUSYD",
				Destination = "NZAKL",
				Departure = ZDateTime.Today,
				Arrival = ZDateTime.Today.AddDays(10),
				Voyage = "100",
				Vessel = "OLGA MAERSK",
				Carrier = "MAERSK",
				TransitTime = "5",
				LegsCount = "1",
				ServiceString = "AL1",
				IncludeRelatedPorts = ZBool.True
			};

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

			var filterStripBizo = new OnlineSchedulesFilterStripBusinessObject(onlineSchedules);
			filterStripBizo.SetFilterDefaults(filterDefaults);

			var origin = ((ModuleNkFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.Origin]);
			AssertEquals("AUSYD", origin.Property);

			var destination = ((ModuleNkFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.Destination]);
			AssertEquals("NZAKL", destination.Property);

			var departure = ((ModuleDateFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.Departure]);
			AssertEquals(filterDefaults.Departure, departure.Property1);
			AssertEquals(ZDateTime.Empty, departure.Property2);

			var arrival = ((ModuleDateFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.Arrival]);
			AssertEquals(ZDateTime.Empty, arrival.Property1);
			AssertEquals(filterDefaults.Arrival, arrival.Property2);

			var vesselVoyage = ((OnlineSchedulesVoyageVesselFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.VoyageVessel]);
			AssertEquals("100", vesselVoyage.VoyageFlightNo);
			AssertEquals("OLGA MAERSK", vesselVoyage.Vessel);

			var carrier = ((ModuleGuidFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.Carrier]);
			AssertEquals(carrierOrganisation.PK, carrier.Property);

			var transitTime = ((ModuleNumberFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.TransitTime]);
			AssertEquals("5", transitTime.Property);

			var legsCount = ((ModuleNumberFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.LegsCount]);
			AssertEquals("1", legsCount.Property);

			var includeRelatedPorts = ((ModuleFlagsFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.IncludeRelatedPorts]);
			AssertEquals(true, includeRelatedPorts.Property0);

			var routesOptions = ((ModuleFlagsFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.RoutesOptions]);
			AssertEquals(false, routesOptions.Property0);
			AssertEquals(false, routesOptions.Property1);

			var serviceString = ((ModuleTextFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.ServiceString]);
			AssertEquals("AL1", serviceString.Property);
		}

		public void TestSetFilterVisibility()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

			var filterStripBizo = new OnlineSchedulesFilterStripBusinessObject(onlineSchedules);
			filterStripBizo.SetFilterVisibility();

			var expectedAlwaysVisibleFilters = typeof(OnlineSchedulesFilterStripBusinessObject.Descriptions)
				.GetFields()
				.Select(info => info.GetValue(null).ToString());

			foreach (var filterDescription in expectedAlwaysVisibleFilters)
			{
				AssertEquals($"{filterDescription} should be AlwaysVisible", FilterVisibility.AlwaysVisible, filterStripBizo[filterDescription].Visibility);
			}
		}

		public void TestSetFilterSpecifiedDateRangeOptions()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

			var filterStripBizo = new OnlineSchedulesFilterStripBusinessObject(onlineSchedules);

			var departure = ((ModuleDateFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.Departure]);
			departure.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			var arrival = ((ModuleDateFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.Arrival]);
			arrival.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			filterStripBizo.SetFilterSpecifiedDateRangeOptions();

			departure = ((ModuleDateFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.Departure]);
			AssertEquals(ModuleDateFilter.SpecifiedDateRange, departure.PropertySearch);

			arrival = ((ModuleDateFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.Arrival]);
			AssertEquals(ModuleDateFilter.SpecifiedDateRange, arrival.PropertySearch);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);
			return new OnlineSchedulesFilterStripBusinessObject(onlineSchedules);
		}

		#endregion
	}
}
