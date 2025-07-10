using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Rateable;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Conversion;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using WebModel = Enterprise.Rating.Web.Model;

namespace Enterprise.Rating.Web.Test.Model
{
	public class RateQueryRatingAdapterTest : RatingTestCase
	{
		public void TestOrigin()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Origin = new Location() { Type = Location.Types.Country, Value = "AU" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("AU", rateQueryBO.Origin.Code);
			AssertEquals("AU", rateQueryBO.Origin.Country.Code);
			AssertEquals(rateQueryBO.Origin, ratingAdapter.Origin);

			rateQuery.Origin = new Location() { Type = Location.Types.IATACity, Value = "SYD" };
			AssertEquals("SYD", rateQueryBO.Origin.Code);
			AssertEquals("AU", rateQueryBO.Origin.Country.Code);
			AssertEquals("SYD", rateQueryBO.Origin.IATACityCode.Code);
			AssertEquals(rateQueryBO.Origin, ratingAdapter.Origin);

			rateQuery.Origin = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" };
			AssertEquals("AUSYD", rateQueryBO.Origin.Code);
			AssertEquals("AU", rateQueryBO.Origin.Country.Code);
			AssertEquals("SYD", rateQueryBO.Origin.IATACityCode.Code);
			AssertEquals("AUSYD", rateQueryBO.Origin.UNLOCO.Code);
			AssertEquals(rateQueryBO.Origin, ratingAdapter.Origin);
		}

		public void TestDestination()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Destination = new Location() { Type = Location.Types.Country, Value = "US" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("US", rateQueryBO.Destination.Code);
			AssertEquals("US", rateQueryBO.Destination.Country.Code);
			AssertEquals(rateQueryBO.Destination, ratingAdapter.Destination);

			rateQuery.Destination = new Location() { Type = Location.Types.IATACity, Value = "LAX" };
			AssertEquals("LAX", rateQueryBO.Destination.Code);
			AssertEquals("US", rateQueryBO.Destination.Country.Code);
			AssertEquals("LAX", rateQueryBO.Destination.IATACityCode.Code);
			AssertEquals(rateQueryBO.Destination, ratingAdapter.Destination);

			rateQuery.Destination = new Location() { Type = Location.Types.UNLOCO, Value = "USLAX" };
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			AssertEquals("USLAX", rateQueryBO.Destination.Code);
			AssertEquals("US", rateQueryBO.Destination.Country.Code);
			AssertEquals("LAX", rateQueryBO.Destination.IATACityCode.Code);
			AssertEquals("USLAX", rateQueryBO.Destination.UNLOCO.Code);
			AssertEquals(rateQueryBO.Destination, ratingAdapter.Destination);
		}

		public void TestVia()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Via = new Location() { Type = Location.Types.Country, Value = "HK" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("HK", rateQueryBO.Via.Code);
			AssertEquals("HK", rateQueryBO.Via.Country.Code);
			AssertEquals(rateQueryBO.Via, ratingAdapter.GetVia(CostSell.Cost));
			AssertEquals(rateQueryBO.Via, ratingAdapter.GetVia(CostSell.Revenue));

			rateQuery.Via = new Location() { Type = Location.Types.UNLOCO, Value = "HKHKG" };
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			AssertEquals("HKHKG", rateQueryBO.Via.Code);
			AssertEquals("HK", rateQueryBO.Via.Country.Code);
			AssertEquals("HKG", rateQueryBO.Via.IATACityCode.Code);
			AssertEquals("HKHKG", rateQueryBO.Via.UNLOCO.Code);
			AssertEquals(rateQueryBO.Via, ratingAdapter.GetVia(CostSell.Cost));
			AssertEquals(rateQueryBO.Via, ratingAdapter.GetVia(CostSell.Revenue));
		}

		public void TestLocations_FirstLoad()
			=> AssertLocation(RateEntryLookups.LocationSourceOption.FirstLoad.Code, ratingAdapter => ratingAdapter.GetFirstLoad);

		public void TestLocations_LastDischarge()
			=> AssertLocation(RateEntryLookups.LocationSourceOption.LastDischarge.Code, ratingAdapter => ratingAdapter.GetLastDischarge);

		public void TestLocations_FirstRouteSetLoad()
			=> AssertLocation(RateEntryLookups.LocationSourceOption.FirstRouteSetLoad.Code, ratingAdapter => ratingAdapter.GetFirstRouteSetLoad);

		public void TestLocations_LastRouteSetDischarge()
			=> AssertLocation(RateEntryLookups.LocationSourceOption.LastRouteSetDischarge.Code, ratingAdapter => ratingAdapter.GetLastRouteSetDischarge);

		void AssertLocation(ZString code, Func<IAutoRating, Func<CostSell, ILocation>> adapterLocationFunction)
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Locations = new[] { new Location { Type = Location.Types.Country, Value = "HK", RelatedField = code } };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			var queryLocation = rateQueryBO.GetRelatedLocation(code);
			var locationFunction = adapterLocationFunction(ratingAdapter);
			AssertEquals("HK", queryLocation.Code);
			AssertEquals("HK", queryLocation.Country.Code);
			AssertEquals(queryLocation, locationFunction.Invoke(CostSell.Cost));
			AssertEquals(queryLocation, locationFunction.Invoke(CostSell.Revenue));

			rateQuery.Locations = new[] { new Location { Type = Location.Types.Country, Value = "HKHKG", RelatedField = code } };
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			queryLocation = rateQueryBO.GetRelatedLocation(code);
			locationFunction = adapterLocationFunction(ratingAdapter);
			AssertEquals("HKHKG", queryLocation.Code);
			AssertEquals("HK", queryLocation.Country.Code);
			AssertEquals("HKG", queryLocation.IATACityCode.Code);
			AssertEquals("HKHKG", queryLocation.UNLOCO.Code);
			AssertEquals(queryLocation, locationFunction.Invoke(CostSell.Cost));
			AssertEquals(queryLocation, locationFunction.Invoke(CostSell.Revenue));
		}

		public void TestPlannedLoad()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.PlannedLoad = new Location() { Type = Location.Types.Country, Value = "US" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("US", rateQueryBO.PlannedLoad.Code);
			AssertEquals("US", rateQueryBO.PlannedLoad.Country.Code);
			AssertEquals(rateQueryBO.PlannedLoad, ratingAdapter.PlannedLoad(CostSell.Cost));
			AssertEquals(rateQueryBO.PlannedLoad, ratingAdapter.PlannedLoad(CostSell.Revenue));

			rateQuery.PlannedLoad = new Location() { Type = Location.Types.IATACity, Value = "LAX" };
			AssertEquals("LAX", rateQueryBO.PlannedLoad.Code);
			AssertEquals("US", rateQueryBO.PlannedLoad.Country.Code);
			AssertEquals("LAX", rateQueryBO.PlannedLoad.IATACityCode.Code);
			AssertEquals(rateQueryBO.PlannedLoad, ratingAdapter.PlannedLoad(CostSell.Cost));
			AssertEquals(rateQueryBO.PlannedLoad, ratingAdapter.PlannedLoad(CostSell.Revenue));

			rateQuery.PlannedLoad = new Location() { Type = Location.Types.UNLOCO, Value = "USLAX" };
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("USLAX", rateQueryBO.PlannedLoad.Code);
			AssertEquals("US", rateQueryBO.PlannedLoad.Country.Code);
			AssertEquals("LAX", rateQueryBO.PlannedLoad.IATACityCode.Code);
			AssertEquals("USLAX", rateQueryBO.PlannedLoad.UNLOCO.Code);
			AssertEquals(rateQueryBO.PlannedLoad, ratingAdapter.PlannedLoad(CostSell.Cost));
			AssertEquals(rateQueryBO.PlannedLoad, ratingAdapter.PlannedLoad(CostSell.Revenue));
		}

		public void TestPlannedDischarge()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.PlannedDischarge = new Location() { Type = Location.Types.Country, Value = "US" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("US", rateQueryBO.PlannedDischarge.Code);
			AssertEquals("US", rateQueryBO.PlannedDischarge.Country.Code);
			AssertEquals(rateQueryBO.PlannedDischarge, ratingAdapter.PlannedDischarge(CostSell.Cost));
			AssertEquals(rateQueryBO.PlannedDischarge, ratingAdapter.PlannedDischarge(CostSell.Revenue));

			rateQuery.PlannedDischarge = new Location() { Type = Location.Types.IATACity, Value = "LAX" };
			AssertEquals("LAX", rateQueryBO.PlannedDischarge.Code);
			AssertEquals("US", rateQueryBO.PlannedDischarge.Country.Code);
			AssertEquals("LAX", rateQueryBO.PlannedDischarge.IATACityCode.Code);
			AssertEquals(rateQueryBO.PlannedDischarge, ratingAdapter.PlannedDischarge(CostSell.Cost));
			AssertEquals(rateQueryBO.PlannedDischarge, ratingAdapter.PlannedDischarge(CostSell.Revenue));

			rateQuery.PlannedDischarge = new Location() { Type = Location.Types.UNLOCO, Value = "USLAX" };
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("USLAX", rateQueryBO.PlannedDischarge.Code);
			AssertEquals("US", rateQueryBO.PlannedDischarge.Country.Code);
			AssertEquals("LAX", rateQueryBO.PlannedDischarge.IATACityCode.Code);
			AssertEquals("USLAX", rateQueryBO.PlannedDischarge.UNLOCO.Code);
			AssertEquals(rateQueryBO.PlannedDischarge, ratingAdapter.PlannedDischarge(CostSell.Cost));
			AssertEquals(rateQueryBO.PlannedDischarge, ratingAdapter.PlannedDischarge(CostSell.Revenue));
		}

		public void TestRateOrigin()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.RateOrigin = new Location() { Type = Location.Types.Country, Value = "AU" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("AU", rateQueryBO.RateOrigin.Code);
			AssertEquals("AU", rateQueryBO.RateOrigin.Country.Code);
			AssertEquals(rateQueryBO.RateOrigin, ratingAdapter.RateOrigin);

			rateQuery.RateOrigin = new Location() { Type = Location.Types.IATACity, Value = "SYD" };
			AssertEquals("SYD", rateQueryBO.RateOrigin.Code);
			AssertEquals("AU", rateQueryBO.RateOrigin.Country.Code);
			AssertEquals("SYD", rateQueryBO.RateOrigin.IATACityCode.Code);
			AssertEquals(rateQueryBO.RateOrigin, ratingAdapter.RateOrigin);

			rateQuery.RateOrigin = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" };
			AssertEquals("AUSYD", rateQueryBO.RateOrigin.Code);
			AssertEquals("AU", rateQueryBO.RateOrigin.Country.Code);
			AssertEquals("SYD", rateQueryBO.RateOrigin.IATACityCode.Code);
			AssertEquals("AUSYD", rateQueryBO.RateOrigin.UNLOCO.Code);
			AssertEquals(rateQueryBO.RateOrigin, ratingAdapter.RateOrigin);
		}

		public void TestRateDestination()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.RateDestination = new Location() { Type = Location.Types.Country, Value = "US" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("US", rateQueryBO.RateDestination.Code);
			AssertEquals("US", rateQueryBO.RateDestination.Country.Code);
			AssertEquals(rateQueryBO.RateDestination, ratingAdapter.RateDestination);

			rateQuery.RateDestination = new Location() { Type = Location.Types.IATACity, Value = "LAX" };
			AssertEquals("LAX", rateQueryBO.RateDestination.Code);
			AssertEquals("US", rateQueryBO.RateDestination.Country.Code);
			AssertEquals("LAX", rateQueryBO.RateDestination.IATACityCode.Code);
			AssertEquals(rateQueryBO.RateDestination, ratingAdapter.RateDestination);

			rateQuery.RateDestination = new Location() { Type = Location.Types.UNLOCO, Value = "USLAX" };
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("USLAX", rateQueryBO.RateDestination.Code);
			AssertEquals("US", rateQueryBO.RateDestination.Country.Code);
			AssertEquals("LAX", rateQueryBO.RateDestination.IATACityCode.Code);
			AssertEquals("USLAX", rateQueryBO.RateDestination.UNLOCO.Code);
			AssertEquals(rateQueryBO.RateDestination, ratingAdapter.RateDestination);
		}

		public void TestServiceProvidersAndCreditors_NonJobCharges()
		{
			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "P1";

			var airLine1 = Factory.NewWithValidTestData<RefAirline>();
			airLine1.RM_TwoCharacterCode = "A1";
			airLine1.RM_EagleAddedAirlinePrefixOrAccountingCode = "AA1";
			var provider2 = Helper.NewOrgHeader();
			provider2.MiscServ.OM_RM_Airline = airLine1.PK;

			var provider3 = Helper.NewOrgHeader();
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "STDC";
			shippingLine1.RSL_CargoWiseOneCode = "C1C";
			provider3.OH_RSL_ShippingLine = shippingLine1.PK;

			var provider4 = Helper.NewOrgHeader();
			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "SCAC";
			shippingLine2.RSL_CargoWiseOneCode = "C1C1";
			provider4.OH_RSL_ShippingLine = shippingLine2.PK;

			var provider5 = Helper.NewOrgHeader();
			provider5.OH_Code = "Pn";

			var provider6 = Helper.NewOrgHeader();
			var airLine2 = Factory.NewWithValidTestData<RefAirline>();
			airLine2.RM_EagleAddedAirlinePrefixOrAccountingCode = "KTH";
			provider6.MiscServ.OM_RM_Airline = airLine2.PK;

			var provider7 = Helper.NewOrgHeader();
			var shippingLine3 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine3.RSL_StandardCarrierAlphaCode = "SCAN";
			shippingLine3.RSL_CargoWiseOneCode = "C1CN";
			provider7.OH_RSL_ShippingLine = shippingLine3.PK;

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.ServiceProviders = new[]
			{
				new Organisation() { CWCode = "P1" },
				new Organisation() { IATACode = "A1" },
				new Organisation() { C1CCode = "C1C" },
				new Organisation() { SCAC = "SCAC" },
			};

			var expectedServiceProviderPKs = new[] { provider1.PK, provider2.PK, provider3.PK, provider4.PK };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertContainsExactElementsInAnyOrder(expectedServiceProviderPKs, rateQueryBO.ServiceProviders.Select(sp => sp.PK));
			AssertContainsExactElementsInAnyOrder(expectedServiceProviderPKs, rateQueryBO.RatingAdapter.Creditors.AllOrgs.Select(r => r.PK));
		}

		public void TestJobDatesProvider()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.EffectiveDate = DateTimeOffset.UtcNow;

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals(new ZDateTime(rateQuery.EffectiveDate.ToLocalTime().DateTime), ratingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(rateQuery.EffectiveDate.ToLocalTime().DateTime), ratingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestCarriers()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "ORG3";
			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();

			// Test null carriers
			rateQuery.Carriers = null;

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertNull(rateQueryBO.Carrier);
			AssertNull(ratingAdapter.Carrier);
			AssertEquals(0, rateQueryBO.PossibleCarriers.Count);
			AssertEquals(0, ratingAdapter.PossibleCarriers.Count());

			// Test empty Carriers
			rateQuery.Carriers = Array.Empty<Organisation>();

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			ratingAdapter = rateQueryBO.RatingAdapter;

			AssertNull(rateQueryBO.Carrier);
			AssertNull(ratingAdapter.Carrier);
			AssertEquals(0, rateQueryBO.PossibleCarriers.Count);
			AssertEquals(0, ratingAdapter.PossibleCarriers.Count());

			// Test Carrier not exists
			rateQuery.Carriers = new[] { new Organisation { CWCode = "ORG0" } };

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			ratingAdapter = rateQueryBO.RatingAdapter;

			AssertNull(rateQueryBO.Carrier);
			AssertNull(ratingAdapter.Carrier);
			AssertEquals(0, rateQueryBO.PossibleCarriers.Count);
			AssertEquals(0, ratingAdapter.PossibleCarriers.Count());

			// Test single item Carriers
			rateQuery.Carriers = new[] { new Organisation { CWCode = "ORG1" } };

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("ORG1", rateQueryBO.Carrier.OH_Code);
			AssertEquals("ORG1", ratingAdapter.Carrier.OH_Code);
			AssertEquals(0, rateQueryBO.PossibleCarriers.Count);
			AssertEquals(0, ratingAdapter.PossibleCarriers.Count());

			// Test multi-item Carriers
			rateQuery.Carriers = new[]
			{
				new Organisation { CWCode = "ORG1" },
				new Organisation { CWCode = "ORG2" },
				new Organisation { CWCode = "ORG3" },
			};

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("ORG1", rateQueryBO.Carrier.OH_Code);
			AssertEquals("ORG1", ratingAdapter.Carrier.OH_Code);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "ORG2", "ORG3" }, rateQueryBO.PossibleCarriers.Select(x => x.OH_Code));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "ORG2", "ORG3" }, ratingAdapter.PossibleCarriers.Select(x => x.OH_Code));
		}

		public void TestCarriers_WhenCarrierProvidedInRateParties()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2";
			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();

			rateQuery.Carriers = new[] { new Organisation { CWCode = "ORG1" } };
			rateQuery.RateParties = new[]
			{
				new OrganisationRole { Code = "ORG2", Role = OrganisationRole.Roles.CAR } // to support old clients when Carriers not provided
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("ORG1", rateQueryBO.Carrier.OH_Code); // Carriers takes priority over CAR organisation in RateParties
			AssertEquals("ORG1", ratingAdapter.Carrier.OH_Code);
			AssertEquals(0, rateQueryBO.PossibleCarriers.Count);
			AssertEquals(0, ratingAdapter.PossibleCarriers.Count());
		}

		public void TestCarrierContractNumbers()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.CarrierContracts = new[] { "P_1000", "P_1001", "P_1002", null, string.Empty };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			var expectedContractNumbers = new[] { new ZString("P_1000"), new ZString("P_1001"), new ZString("P_1002"), new ZString(string.Empty) };

			AssertContainsExactElementsInAnyOrder(expectedContractNumbers, rateQueryBO.CarrierContractNumbers);
			AssertContainsExactElementsInAnyOrder(expectedContractNumbers, ratingAdapter.CarrierContractNumbers);

			rateQuery.CarrierContracts = null;
			AssertEquals(0, rateQueryBO.CarrierContractNumbers.Count());
			AssertEquals(0, ratingAdapter.CarrierContractNumbers.Count());
		}

		public void TestFreightMode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.TransportMode = "AIR";
			rateQuery.ContainerMode = string.Empty;

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals(FreightMode.LSE, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.LSE, ratingAdapter.FreightMode);

			rateQuery.ContainerMode = "ULD";
			AssertEquals(FreightMode.ULD, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.ULD, ratingAdapter.FreightMode);

			rateQuery.ContainerMode = "LSE";
			AssertEquals(FreightMode.LSE, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.LSE, ratingAdapter.FreightMode);

			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = string.Empty;
			AssertEquals(FreightMode.LCL, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.LCL, ratingAdapter.FreightMode);

			rateQuery.ContainerMode = "FCL";
			AssertEquals(FreightMode.FCL, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.FCL, ratingAdapter.FreightMode);

			rateQuery.ContainerMode = "LCL";
			AssertEquals(FreightMode.LCL, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.LCL, ratingAdapter.FreightMode);

			rateQuery.TransportMode = "RAI";
			rateQuery.ContainerMode = string.Empty;
			AssertEquals(FreightMode.LRA, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.LRA, ratingAdapter.FreightMode);

			rateQuery.ContainerMode = "FCL";
			AssertEquals(FreightMode.FRA, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.FRA, ratingAdapter.FreightMode);

			rateQuery.ContainerMode = "LCL";
			AssertEquals(FreightMode.LRA, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.LRA, ratingAdapter.FreightMode);

			rateQuery.TransportMode = "ROA";
			rateQuery.ContainerMode = string.Empty;
			AssertEquals(FreightMode.LRO, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.LRO, ratingAdapter.FreightMode);

			rateQuery.ContainerMode = "FCL";
			AssertEquals(FreightMode.FRO, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.FRO, ratingAdapter.FreightMode);

			rateQuery.ContainerMode = "LCL";
			AssertEquals(FreightMode.LRO, rateQueryBO.FreightMode);
			AssertEquals(FreightMode.LRO, ratingAdapter.FreightMode);
		}

		public void TestContainerMode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();

			rateQuery.ContainerMode = "FCL";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("FCL", rateQueryBO.ContainerMode);
			AssertEquals("FCL", ratingAdapter.ContainerMode);
		}

		public void TestServiceLevel()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();

			rateQuery.ServiceLevels = new string[] { "STD", "SL1", "SL2" };
			rateQuery.GatewayServiceLevels = new string[] { "", "EXP" };
			rateQuery.ShipmentGatewayServiceLevels = new string[] { "", "SGW" };
			rateQuery.CarrierServiceLevels = new[]
			{
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise,  Value = "ABC" },
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise,  Value = "XYZ" },
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.Universal,  Value = "UUU" }
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertContainsExactElementsInAnyOrder(
				new[] { new ServiceLevelInfo("ABC", ServiceLevelType.Carrier), new ServiceLevelInfo("XYZ", ServiceLevelType.Carrier) },
				rateQueryBO.CarrierServiceLevels
			);

			AssertContainsExactElementsInAnyOrder(
				new[] { new ServiceLevelInfo("STD", ServiceLevelType.Client), new ServiceLevelInfo("SL1", ServiceLevelType.Client), new ServiceLevelInfo("SL2", ServiceLevelType.Client) },
				rateQueryBO.ClientServiceLevels
			);

			AssertContainsExactElementsInAnyOrder(
				new[] { new ServiceLevelInfo("", ServiceLevelType.Gateway), new ServiceLevelInfo("EXP", ServiceLevelType.Gateway) },
				rateQueryBO.GatewayServiceLevels
			);

			AssertContainsExactElementsInAnyOrder(
				new[] { new ServiceLevelInfo("", ServiceLevelType.Gateway), new ServiceLevelInfo("SGW", ServiceLevelType.Gateway) },
				rateQueryBO.ShipmentGatewayServiceLevels
			);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					new ServiceLevelInfo("ABC", ServiceLevelType.Carrier),
					new ServiceLevelInfo("XYZ", ServiceLevelType.Carrier),
					new ServiceLevelInfo("STD", ServiceLevelType.Client),
					new ServiceLevelInfo("SL1", ServiceLevelType.Client),
					new ServiceLevelInfo("SL2", ServiceLevelType.Client),
					new ServiceLevelInfo("", ServiceLevelType.Gateway),
					new ServiceLevelInfo("EXP", ServiceLevelType.Gateway),
					new ServiceLevelInfo("", ServiceLevelType.Gateway),
					new ServiceLevelInfo("SGW", ServiceLevelType.Gateway),
				},
				ratingAdapter.ServiceLevel.ServiceLevelData
				);
		}

		public void TestChargeCodeGroups()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertContainsExactElementsInAnyOrder(
				Env.Registry.Rating.FreightRatedCodes,
				ratingAdapter.ChargeCodeGroups
			);
		}

		public void TestDebtorOrgs()
		{
			var org1 = Helper.NewOrgHeader();
			org1.OH_Code = "O1";

			var org2 = Helper.NewOrgHeader();
			org2.OH_Code = "O2";

			var org3 = Helper.NewOrgHeader();
			org3.OH_Code = "O3";

			var org4 = Helper.NewOrgHeader();
			org4.OH_Code = "O4";

			var org5 = Helper.NewOrgHeader();
			org5.OH_Code = "O5";

			var org6 = Helper.NewOrgHeader();
			org6.OH_Code = "O6";

			var org7 = Helper.NewOrgHeader();
			org7.OH_Code = "O7";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateParties = new[]
			{
				new OrganisationRole() { Role = OrganisationRole.Roles.CNE, Code = "O1" },
				new OrganisationRole() { Role = OrganisationRole.Roles.CNR, Code = "O2" },
				new OrganisationRole() { Role = OrganisationRole.Roles.SAG, Code = "XXXX" }, //Org Code Does Not Exists
				new OrganisationRole() { Role = OrganisationRole.Roles.RAG, Code = "O3" },
				new OrganisationRole() { Role = OrganisationRole.Roles.CCUS, Code = "O4" },
				new OrganisationRole() { Role = OrganisationRole.Roles.CCR, Code = "O5" }, // Non Debtor Role
				new OrganisationRole() { Role = OrganisationRole.Roles.LC, Code = "O6" },
				new OrganisationRole() { Role = OrganisationRole.Roles.AG, Code = "O7" },
			};

			var expectedDebtorOrgs = new[]
			{
				new { Code = "O1", Role = RatingDebtorOrgTypes.CNE },
				new { Code = "O2", Role = RatingDebtorOrgTypes.CNR },
				new { Code = "O4", Role = RatingDebtorOrgTypes.CCUS },
				new { Code = "O6", Role = RatingDebtorOrgTypes.LC },
				new { Code = "O7", Role = RatingDebtorOrgTypes.AG },
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertContainsExactElementsInAnyOrder(
				expectedDebtorOrgs,
				ratingAdapter.DebtorOrgs.Select(d => new { Code = d.OrgHeader.OH_Code.ToString(), Role = d.RatingDebtorOrgTypes })
			);
		}

		public void TestImportBroker()
		{
			var org = Helper.NewOrgHeader();
			org.OH_Code = "IBOrg";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateParties = new[]
			{
				new OrganisationRole() { Role = OrganisationRole.Roles.IB, Code = "IBOrg" },
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals(org.PK, ratingAdapter.ImportBroker.PK);
			AssertEquals("IBOrg", ratingAdapter.ImportBroker.OH_Code);
		}

		public void TestExportBroker()
		{
			var org = Helper.NewOrgHeader();
			org.OH_Code = "EBOrg";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateParties = new[]
			{
				new OrganisationRole() { Role = OrganisationRole.Roles.EB, Code = "EBOrg" },
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals(org.PK, ratingAdapter.ExportBroker.PK);
			AssertEquals("EBOrg", ratingAdapter.ExportBroker.OH_Code);
		}

		public void TestGetDebtorsOnRatingCriteria_WhenLocalClientIsImportBroker_DebtorOrgsWillIncludeLCBK()
		{
			var org = Helper.NewOrgHeader();
			org.OH_Code = "LCBKOrg";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateParties = new[]
			{
				new OrganisationRole() { Role = OrganisationRole.Roles.IB, Code = "LCBKOrg" },
				new OrganisationRole() { Role = OrganisationRole.Roles.LC, Code = "LCBKOrg" },
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;
			var ratingCriteria = new RatingCriteria(ratingAdapter, Factory, false);

			Assert(ratingCriteria.GetDebtors().Contains(new DebtorOrg(org, RatingDebtorOrgTypes.LCBK)));
		}

		public void TestGetDebtorsOnRatingCriteria_WhenLocalClientIsExportBroker_DebtorOrgsWillIncludeLCBK()
		{
			var org = Helper.NewOrgHeader();
			org.OH_Code = "LCBKOrg";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateParties = new[]
			{
				new OrganisationRole() { Role = OrganisationRole.Roles.EB, Code = "LCBKOrg" },
				new OrganisationRole() { Role = OrganisationRole.Roles.LC, Code = "LCBKOrg" },
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;
			var ratingCriteria = new RatingCriteria(ratingAdapter, Factory, false);

			Assert(ratingCriteria.GetDebtors().Contains(new DebtorOrg(org, RatingDebtorOrgTypes.LCBK)));
		}

		public void TestRateableMeasures()
		{
			GP20.RC_ISOType = "22G0";
			GP40.RC_ISOType = "42G0";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.ContainerTypes = new[]
			{
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "20GP" },
				new ContainerType() { Type = ContainerType.Types.ISO, Value = "42G0" },
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "ABCD" },
				new ContainerType() { Type = ContainerType.Types.ISO, Value = "ABCD" },
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "" },
				new ContainerType() { Type = ContainerType.Types.ISO, Value = "" },
				new ContainerType() { },
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			var expectedContainerTypePKs = new[] { GP20.PK, GP40.PK };

			AssertContainsExactElementsInAnyOrder(expectedContainerTypePKs, rateQueryBO.ContainerTypes.Values.Select(ct => ct.PK));
			AssertContainsExactElementsInAnyOrder(expectedContainerTypePKs, (ratingAdapter.RateableMeasures as RateableMeasureSet).GetContainerTypePKs());

			rateQuery.ContainerTypes = null;
			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
				new JobContainer()
				{
					ContainerTypeCWCode = "20GP"
				}
			}
			};

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			ratingAdapter = rateQueryBO.RatingAdapter;
			expectedContainerTypePKs = new[] { GP20.PK };
			AssertContainsExactElementsInAnyOrder(expectedContainerTypePKs, rateQueryBO.ContainerTypes.Values.Select(ct => ct.PK));
			AssertContainsExactElementsInAnyOrder(expectedContainerTypePKs, (ratingAdapter.RateableMeasures as RateableMeasureSet).GetContainerTypePKs());
		}

		public void TestIsApplicableToPaymentTermFiltering()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Incoterm = "EXW";
			rateQuery.CarrierPayTerm = "CCX";
			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);

			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals(true, ratingAdapter.IsApplicableToPaymentTermFiltering(ChargeCodeGroupList.Codes.Freight, CostSell.Revenue));
			AssertEquals(true, ratingAdapter.IsApplicableToPaymentTermFiltering(ChargeCodeGroupList.Codes.Freight, CostSell.Cost));

			rateQuery.CarrierPayTerm = "PPD";

			AssertEquals(false, ratingAdapter.IsApplicableToPaymentTermFiltering(ChargeCodeGroupList.Codes.Freight, CostSell.Revenue));
			AssertEquals(false, ratingAdapter.IsApplicableToPaymentTermFiltering(ChargeCodeGroupList.Codes.Freight, CostSell.Cost));
		}

		public void TestPaymentTerm()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Origin.Value = "AUMEL";
			rateQuery.Destination.Value = "AUSYD";

			rateQuery.Incoterm = "";
			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;
			AssertEquals(0, ratingAdapter.PaymentTerm.PaymentTermInfoCollection.Count);

			rateQuery.Incoterm = DomesticPaymentTerms.Prepaid;
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			ratingAdapter = rateQueryBO.RatingAdapter;

			var expectedPaymentTermInfos = new[]
			{
				new PaymentTermInfo(PaymentTermType.DomesticPaymentTerm, CostSell.Cost, DomesticPaymentTerms.Prepaid),
				new PaymentTermInfo(PaymentTermType.DomesticPaymentTerm, CostSell.Revenue, DomesticPaymentTerms.Prepaid),
			};

			AssertEquals(expectedPaymentTermInfos.Length, ratingAdapter.PaymentTerm.PaymentTermInfoCollection.Count);
			for (int i = 0; i < expectedPaymentTermInfos.Length; i++)
			{
				AssertEquals(expectedPaymentTermInfos[i].InfoType, ratingAdapter.PaymentTerm.PaymentTermInfoCollection[i].InfoType);
				AssertEquals(expectedPaymentTermInfos[i].CostOrSell, ratingAdapter.PaymentTerm.PaymentTermInfoCollection[i].CostOrSell);
				AssertEquals(expectedPaymentTermInfos[i].Value, ratingAdapter.PaymentTerm.PaymentTermInfoCollection[i].Value);
			}

			rateQuery.Origin.Value = "AUSYD";
			rateQuery.Destination.Value = "USLAX";
			rateQuery.Incoterm = "EXW";
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			ratingAdapter = rateQueryBO.RatingAdapter;

			expectedPaymentTermInfos = new[]
			{
				new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Cost, "EXW"),
				new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"),
			};

			AssertEquals(expectedPaymentTermInfos.Length, ratingAdapter.PaymentTerm.PaymentTermInfoCollection.Count);
			for (int i = 0; i < expectedPaymentTermInfos.Length; i++)
			{
				AssertEquals(expectedPaymentTermInfos[i].InfoType, ratingAdapter.PaymentTerm.PaymentTermInfoCollection[i].InfoType);
				AssertEquals(expectedPaymentTermInfos[i].CostOrSell, ratingAdapter.PaymentTerm.PaymentTermInfoCollection[i].CostOrSell);
				AssertEquals(expectedPaymentTermInfos[i].Value, ratingAdapter.PaymentTerm.PaymentTermInfoCollection[i].Value);
			}
		}

		public void TestMonetaryValues()
		{
			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var currencyConverter = new TestCurrencyConverter(Factory);

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.JobInfo = new JobInfo()
			{
				GoodsValue = 19.8M,
				GoodsValueCurrency = "AUD"
			};

			AssertGoodsMonetaryValue(rateQuery, new ZDecimal(19.8), "AUD");

			rateQuery.JobInfo = new JobInfo()
			{
				GoodsValue = 0,
				GoodsValueCurrency = "AUD"
			};

			AssertGoodsMonetaryValue(rateQuery, ZDecimal.Zero, "AUD");

			rateQuery.JobInfo = new JobInfo()
			{
				GoodsValue = null,
				GoodsValueCurrency = "AUD"
			};

			AssertGoodsMonetaryValue(rateQuery, ZDecimal.Zero, "AUD");

			rateQuery.JobInfo = new JobInfo()
			{
				GoodsValue = 18.7M,
				GoodsValueCurrency = null
			};

			AssertGoodsMonetaryValue(rateQuery, ZDecimal.Zero, "AUD");

			rateQuery.JobInfo = new JobInfo()
			{
				GoodsValue = 18.7M,
				GoodsValueCurrency = ""
			};

			AssertGoodsMonetaryValue(rateQuery, ZDecimal.Zero, "AUD");

			void AssertGoodsMonetaryValue(RateQuery query, ZDecimal expectedValue, string expectedCurrencyCode)
			{
				var rateQueryBO = new RateQueryBusinessObject(Factory, query, SourceEndpoint.JobCharges);
				var ratingAdapter = rateQueryBO.RatingAdapter;

				var goodsValue = ratingAdapter.MonetaryValues.GetMoney(MoneyType.ValueType.GoodsValue, aud, currencyConverter);

				AssertEquals(expectedValue, goodsValue.Amount);
				AssertEquals(expectedCurrencyCode, goodsValue.Currency.Code);
			}
		}

		public void TestConditionSupporter_DGClassAndSubstance()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_UNNO = "1400";
			substance.DG_Variant = "S";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								DGSubstance = "1400S"
							},
							new JobPackLine()
							{
								PackageType = "BOX",
								DGClass = "1.45"
							},
						}
					}
				}
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;
			AssertNotNull(((IAutoRatingFreightConditionsSupportable)ratingAdapter).ConditionsSupporter);
			var supporter = ((IAutoRatingFreightConditionsSupportable)ratingAdapter).ConditionsSupporter as ShipmentRateLineConditionsSupporter;
			AssertNotNull(supporter);

			var shipment = supporter.ObjectToWrap as CommonShipment;
			AssertEquals(true, shipment.OuterPackLines.HasDangerousGoods);
			AssertEquals(1, shipment.OuterPackLines.Where(p => p.JL_F3_NKPackType == "PLT").Count());
			AssertEquals(1, shipment.OuterPackLines.Where(p => p.JL_F3_NKPackType == "BOX").Count());

			var pltPackLine = shipment.OuterPackLines.Where(p => p.JL_F3_NKPackType == "PLT").Cast<PackLine>().Single();
			AssertEquals("1400S", pltPackLine.JL_Calc_DGSubstance);

			var boxPackLine = shipment.OuterPackLines.Where(p => p.JL_F3_NKPackType == "BOX").Cast<PackLine>().Single();
			AssertEquals("1.45", boxPackLine.JL_Calc_DGClass);
		}

		public void TestJobServices()
		{
			var contractor = Factory.New<OrgHeader>();
			contractor.OH_Code = "CT01";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			var serviceDate = DateTime.Now.Date;
			rateQuery.JobInfo.JobServices = new[]
			{
				new WebModel.JobService()
				{
					Contractor = new Organisation { CWCode = "CT01" },
					Type =  Core.Constants.FreightServiceType.Codes.Fumigation,
					Booked = serviceDate.AddDays(-2),
					Completed = serviceDate,
					Count = 10,
					Location = new Location { Type = Location.Types.UNLOCO, Value = "AUSYD" },
					Duration = 5000,
					Rate = 17.9M,
					RateCurrency = "USD"
				}
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertNotNull(ratingAdapter.JobServices);
			AssertEquals(1, ratingAdapter.JobServices.Count(s => s.ServiceCode == "FUM"));

			var service = ratingAdapter.JobServices.Single(s => s.ServiceCode == "FUM");
			AssertEquals(contractor.PK, service.Contractor.PK);
			AssertEquals(new ZDateTime(serviceDate), service.CompletedDate);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, service.ServiceCode);
			AssertEquals(new ZDecimal(10), service.ServiceCount);
			AssertEquals(new TimeSpan(83, 20, 0), service.ServiceDuration); // 5000 minutes
			AssertEquals("AUSYD", service.LocationCode);
			AssertEquals("AU", service.LocationCountryCode);
			AssertEquals(new ZDecimal(17.9M), service.Rate);
			AssertEquals("USD", service.Currency);
		}

		public void TestPickupCartageEquipment()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.JobInfo.PickupDropMode = "ANY";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("ANY", ratingAdapter.PickupCartageEquipment);

			rateQuery.JobInfo = null;
			AssertEquals(string.Empty, ratingAdapter.PickupCartageEquipment);
		}

		public void TestDeliveryCartageEquipment()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.JobInfo.DeliveryDropMode = "ANY";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("ANY", ratingAdapter.DeliveryCartageEquipment);

			rateQuery.JobInfo = null;
			AssertEquals(string.Empty, ratingAdapter.DeliveryCartageEquipment);
		}

		public void TestIAutoRatingCustomsInfo_Entries()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.JobInfo.CustomsValue = 18.9M;

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = (IAutoRatingCustomsInfo)rateQueryBO.RatingAdapter;

			AssertEquals(1, ratingAdapter.Entries.Count);
			AssertEquals(18.9M, ratingAdapter.Entries.Cast<EntryInfo>().Single().CustomsValue);

			rateQuery.JobInfo.CustomsValue = null;

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			ratingAdapter = (IAutoRatingCustomsInfo)rateQueryBO.RatingAdapter;

			AssertEquals(0, ratingAdapter.Entries.Count);
		}

		public void TestPaymentTermOverride()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.PaymentTermOverride = "PPD";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("PPD", ratingAdapter.PaymentTermOverride);

			rateQuery.PaymentTermOverride = null;

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals(ZString.Empty, ratingAdapter.PaymentTermOverride);

			rateQuery.PaymentTermOverride = "";

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals(ZString.Empty, ratingAdapter.PaymentTermOverride);
		}

		public void TestHBLDeliveryMode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.ClientRates);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.HBLDeliveryMode = "DOOR/DOOR";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.ClientRates);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("DOOR/DOOR", ratingAdapter.HBLDeliveryMode);

			rateQuery.HBLDeliveryMode = null;

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.ClientRates);
			ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals(ZString.Empty, ratingAdapter.HBLDeliveryMode);

			rateQuery.HBLDeliveryMode = "";

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.ClientRates);
			ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals(ZString.Empty, ratingAdapter.HBLDeliveryMode);
		}

		public void TestFMCTariffID()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.ClientRates);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.FMCTariffID = "1234";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.ClientRates);
			var ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals("1234", ratingAdapter.FMCTariffID);

			rateQuery.FMCTariffID = null;

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.ClientRates);
			ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals(ZString.Empty, ratingAdapter.FMCTariffID);

			rateQuery.FMCTariffID = "";

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.ClientRates);
			ratingAdapter = rateQueryBO.RatingAdapter;

			AssertEquals(ZString.Empty, ratingAdapter.FMCTariffID);
		}

		public void TestPickupAddress_Matches()
		{
			var consignorFirstAddress = Consignor.Addresses[0];
			consignorFirstAddress.Postcode = "1234";
			consignorFirstAddress.City = "MELB";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.PickupOrg = Consignor.OH_Code;
			rateQuery.PickupAddrCode = consignorFirstAddress.AddressCode;

			var rateQueryBo = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var pickupAddress = rateQueryBo.PickupAddress;

			AssertNotNull(pickupAddress);
			AssertEquals("1234", pickupAddress.E2_Postcode);
			AssertEquals("MELB", pickupAddress.E2_City);
		}

		public void TestPickupAddress_Missing_AddressShortCode()
		{
			var consignorFirstAddress = Consignor.Addresses[0];
			consignorFirstAddress.Postcode = "1234";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.PickupOrg = Consignor.OH_Code;
			rateQuery.PickupAddrCode = "DOESNOTEXIST";

			var rateQueryBo = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var pickupAddress = rateQueryBo.PickupAddress;

			AssertNull(pickupAddress);
		}

		public void TestPickupAddress_Missing_OrganisationCode()
		{
			var consignorFirstAddress = Consignor.Addresses[0];
			consignorFirstAddress.Postcode = "1234";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.PickupOrg = "DOESNOTEXIST";
			rateQuery.PickupAddrCode = "DOESNOTEXIST";

			var rateQueryBo = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var pickupAddress = rateQueryBo.PickupAddress;

			AssertNull(pickupAddress);
		}

		public void TestDeliveryAddress_Matches()
		{
			var consigneeFirstAddress = Consignee.Addresses[0];
			consigneeFirstAddress.Postcode = "1234";
			consigneeFirstAddress.City = "MELB";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.DeliveryOrg = Consignee.OH_Code;
			rateQuery.DeliveryAddrCode = consigneeFirstAddress.AddressCode;

			var rateQueryBo = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var deliveryAddress = rateQueryBo.DeliveryAddress;

			AssertNotNull(deliveryAddress);
			AssertEquals("1234", deliveryAddress.E2_Postcode);
			AssertEquals("MELB", deliveryAddress.E2_City);
		}

		public void TestDeliveryAddress_Missing_AddressShortCode()
		{
			var consigneeFirstAddress = Consignee.Addresses[0];
			consigneeFirstAddress.Postcode = "1234";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.DeliveryOrg = Consignee.OH_Code;
			rateQuery.DeliveryAddrCode = "DOESNOTEXIST";

			var rateQueryBo = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var deliveryAddress = rateQueryBo.DeliveryAddress;

			AssertNull(deliveryAddress);
		}

		public void TestDeliveryAddress_Missing_OrganisationCode()
		{
			var consigneeFirstAddress = Consignee.Addresses[0];
			consigneeFirstAddress.Postcode = "1234";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.DeliveryOrg = "DOESNOTEXIST";
			rateQuery.DeliveryAddrCode = "DOESNOTEXIST";

			var rateQueryBo = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			var deliveryAddress = rateQueryBo.DeliveryAddress;

			AssertNull(deliveryAddress);
		}

		public void TestJobInfo_CustomFields_Specified()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.JobInfo.CustomFields = new CustomField[]
			{
				new CustomField("key", "value")
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);

			var expectedKeys = new[] { "key" };
			AssertSequencesEqual(expectedKeys, rateQueryBO.CustomFields.Keys);
			AssertEquals("value", rateQueryBO.CustomFields["key"]);
		}

		public void TestJobInfo_CustomFields_Null()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.JobInfo.CustomFields = null;

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);

			AssertNull(rateQueryBO.CustomFields);
		}

		public void TestWharfCTOAddress_MirrorsAddressListDefault()
		{
			ZGuid AddAddress(OrgAddressType addressType, bool isDefault)
			{
				var newAddress = NewClient.Addresses.AddNew(addressType, isDefault);
				newAddress.Address1 = $"123 {addressType} St";
				return newAddress.PK;
			}

			void AssertEquals(string message)
			{
				var rateQuery = RatesAPITestHelper.GetValidRateQuery();
				rateQuery.RateParties = new[]
				{
					new OrganisationRole() { Code = NewClient.OH_Code, Role = OrganisationRole.Roles.DCTO }
				};

				var rateQueryBo = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
				var ratingAdapter = rateQueryBo.RatingAdapter;

				var check = ratingAdapter.WharfCTOAddress;
				var expected = NewClient.Address_List.PICAddressOrFallback;
				HtmlAssertEquals(message, expected, check.PK);
			}

			// Roughly mirrors addresses added in ZAddressListTest.TestPICAddressOrFallback()
			AssertEquals("Should find OFC address");

			AddAddress(OrgAddressType.PickupAndDelivery, isDefault: false);
			AssertEquals("Should find PAD address");

			AddAddress(OrgAddressType.Pickup, isDefault: false);
			AssertEquals("Should find PIC address");

			AddAddress(OrgAddressType.Pickup, isDefault: false);
			AssertEquals("Should find PIC address");

			AddAddress(OrgAddressType.Pickup, isDefault: true);
			AssertEquals("Should find default PIC address");
		}

		public void TestCarrierProperty_GivenCarrierRatePartySpecified_ShouldBeDefined_SupportingOldClients()
		{
			// Arrange
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER1";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateParties = new[]
			{
				new OrganisationRole { Code = carrier.OH_Code, Role = OrganisationRole.Roles.CAR } // to support old clients
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);

			// Act
			var ratingAdapter = rateQueryBO.RatingAdapter;

			// Assert
			AssertNotNull(ratingAdapter.Carrier);
			AssertEquals("CARRIER1", ratingAdapter.Carrier.OH_Code);
		}

		public void TestCarrierProperty_GivenCarrierRatePartySpecified_ShouldBeDefined()
		{
			// Arrange
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER1";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Carriers = new[] { new Organisation { CWCode = carrier.OH_Code } };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);

			// Act
			var ratingAdapter = rateQueryBO.RatingAdapter;

			// Assert
			AssertNotNull(ratingAdapter.Carrier);
			AssertEquals("CARRIER1", ratingAdapter.Carrier.OH_Code);
		}

		public void TestCarrierProperty_GivenCarrierRatePartyNotSpecified_ShouldBeNull_SupportingOldClients()
		{
			// Arrange
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateParties = OrganisationRole.Roles.All.Where(role => role != OrganisationRole.Roles.CAR) // to support old clients
				.Select(role => new OrganisationRole { Code = Factory.NewWithValidTestData<OrgHeader>().OH_Code, Role = role })
				.ToArray();

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);

			// Act
			var ratingAdapter = rateQueryBO.RatingAdapter;

			// Assert
			AssertNull(ratingAdapter.Carrier);
		}

		public void TestCarrierProperty_GivenCarrierRatePartyNotSpecified_ShouldBeNull()
		{
			// Arrange
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();

			Assert(rateQuery.Carriers == null || !rateQuery.Carriers.Any());
			if (rateQuery.RateParties != null)
			{
				var nonCarRoles = rateQuery.RateParties.Where(x => x.Role != OrganisationRole.Roles.CAR);
				AssertEquals(0, nonCarRoles.Count());
			}

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);

			// Act
			var ratingAdapter = rateQueryBO.RatingAdapter;

			// Assert
			AssertNull(ratingAdapter.Carrier);
		}
	}
}
