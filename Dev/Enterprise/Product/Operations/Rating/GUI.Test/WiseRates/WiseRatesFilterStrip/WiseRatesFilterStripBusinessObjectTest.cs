using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(WiseRatesFilterStripBusinessObject))]
	public class WiseRatesFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WiseRatesFilterStripBusinessObject();
		}

		[TestDate(2019, 01, 15)]
		public void TestDefaultValueOfEffectiveOnFilter()
		{
			var filterStripBizo = new WiseRatesFilterStripBusinessObject();

			var effectiveDateStrip = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.EffectiveOn);
			AssertEquals("Default value of Effective On filter", ZDateTime.Today, ((ModuleSingleDateFilter)effectiveDateStrip.CurrentModuleFilter).Property1);

			var (result, _) = filterStripBizo.BuildRatesQuery(new LoggerDecorator());
			AssertEquals("Effective Date in Rates Query", ZDateTime.Today, result.EffectiveDate);
		}

		public void TestFilter_CGReference()
		{
			var filterStripBizo = new WiseRatesFilterStripBusinessObject();

			var referenceTextFilter = filterStripBizo.AddFilterStrip<WiseRatesModuleTextFilter>(RateEntryFilterUtility.Constants.Codes.CGReference);
			AssertEquals("Default value of Effective On filter", string.Empty, referenceTextFilter.Property);

			var results = filterStripBizo.BuildRatesQueries(new LoggerDecorator());
			Assert(
				"All results should have no references in CargoGuideFilters by default.",
				results.All(result => (result.CargoGuideFilters?.References?.Count() ?? 0) == 0)
			);

			referenceTextFilter.Property = "Potato";
			results = filterStripBizo.BuildRatesQueries(new LoggerDecorator());
			AssertNotNull(
				"One result must have a reference matching 'Potato'.",
				results.SingleOrDefault(result => result.CargoGuideFilters?.References?.SingleOrDefault(reference => reference == "Potato") != null)
			);
		}

		public void Test_OrgListFilterIncludes_SCAC_C1C_IATA_ForCarrierOrg()
		{
			var scacCarrier = Factory.NewWithValidTestData<OrgHeader>();
			scacCarrier.OH_Code = "CARR_1";
			scacCarrier.OH_FullName = "Carrier SCAC test";
			scacCarrier.OH_IsShippingProvider = true;
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "CAR1";
			scacCarrier.OH_RSL_ShippingLine = shippingLine1.PK;

			var c1cCarrier = Factory.NewWithValidTestData<OrgHeader>();
			c1cCarrier.OH_Code = "CARR_2";
			c1cCarrier.OH_FullName = "Carrier C1C test";
			c1cCarrier.OH_IsShippingProvider = true;
			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "";
			shippingLine2.RSL_CargoWiseOneCode = "C1C2";
			c1cCarrier.OH_RSL_ShippingLine = shippingLine2.PK;

			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "AAA";
			airline.RM_ThreeLetterCode = "AAA";
			airline.RM_TwoCharacterCode = "AA";
			var iataCarrier = Factory.NewWithValidTestData<OrgHeader>();
			iataCarrier.OH_Code = "CARR_3";
			iataCarrier.OH_FullName = "Carrier IATA test";
			iataCarrier.OH_IsShippingProvider = true;
			iataCarrier.OH_IsAirLine = true;
			OrgMiscServ iataCarrierInfo = iataCarrier.MiscServ;
			iataCarrierInfo.OM_RM_Airline = airline.PK;

			var noneCarrier = Factory.NewWithValidTestData<OrgHeader>();
			noneCarrier.OH_Code = "CARR_4";
			noneCarrier.OH_FullName = "Carrier None test";
			noneCarrier.OH_IsShippingProvider = true;

			Factory.Save();

			var filterStripBizo = new WiseRatesFilterStripBusinessObject();
			var orgCollection = new OrgHeaderCollection(Factory, filterStripBizo.GetAdditionalCarrierOrgListFilter(typeof(OrgHeader), OrgHeaderSchema.PK));
			orgCollection.Load();

			Assert(orgCollection.Any(x => x.PK == scacCarrier.PK));
			Assert(orgCollection.Any(x => x.PK == c1cCarrier.PK));
			Assert(orgCollection.Any(x => x.PK == iataCarrier.PK));
			Assert(!orgCollection.Any(x => x.PK == noneCarrier.PK));
		}

		#region Build Filter

		public void TestBuildFilter_ShouldNotAllowEmptyIATACode()
		{
			var airlineA = Factory.NewWithValidTestData<RefAirline>();
			airlineA.RM_EagleAddedAirlinePrefixOrAccountingCode = "AAA";

			AssertNullOrEmpty("Precondition: airline with empty IATA-code", airlineA.RM_TwoCharacterCode);

			var filterStripBizo = new WiseRatesFilterStripBusinessObject();
			var iataCodeStrip = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.IATACode);

			AssertEquals
			(
				"IATA filter strip list should not include airline with empty IATA code",
				false,
				((ModuleGuidFilter)iataCodeStrip.CurrentModuleFilter).List.Contains(airlineA)
			);
		}

		public void TestBuildFilter()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "CARR_1";
			carrier1.OH_FullName = "Carrier 1";
			carrier1.OH_IsShippingProvider = true;
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "CAR1";
			shippingLine1.RSL_CargoWiseOneCode = "C1C1";
			carrier1.OH_RSL_ShippingLine = shippingLine1.PK;

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "CARR_2";
			carrier2.OH_FullName = "Carrier 2";
			carrier2.OH_IsShippingProvider = true;
			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "";
			shippingLine2.RSL_CargoWiseOneCode = "C1C2";
			shippingLine2.RSL_CarrierName = "CARR_2";
			carrier2.OH_RSL_ShippingLine = shippingLine2.PK;

			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier3.OH_Code = "CARR_3";
			carrier3.OH_FullName = "Carrier 3 NO SCAC";
			carrier3.OH_IsShippingProvider = true;

			var refContNoISO = Factory.NewWithValidTestData<MasterFiles.Business.RefContainer>();
			refContNoISO.RC_ISOType = string.Empty;
			refContNoISO.RC_Code = "00GP";

			var airlineA = Factory.NewWithValidTestData<RefAirline>();
			airlineA.RM_EagleAddedAirlinePrefixOrAccountingCode = "AAA";
			airlineA.RM_TwoCharacterCode = "AA";

			var airlineB = Factory.NewWithValidTestData<RefAirline>();
			airlineB.RM_EagleAddedAirlinePrefixOrAccountingCode = "BBB";
			airlineB.RM_TwoCharacterCode = "BB";

			Factory.Save();

			var filterStripBizo = new WiseRatesFilterStripBusinessObject();

			var effectiveDateStrip = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.EffectiveOn);
			((ModuleSingleDateFilter)effectiveDateStrip.CurrentModuleFilter).Property1 = new ZDateTime(2018, 3, 25);

			var originDestStrip1 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.OriginDestination);
			var originDestStrip2 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.OriginDestination);
			var originDestStrip3 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.OriginDestination);

			((ModuleLocationFilter)originDestStrip1.CurrentModuleFilter).Property1 = "AUSYD";
			((ModuleLocationFilter)originDestStrip1.CurrentModuleFilter).Property2 = "USLAX";
			((ModuleLocationFilter)originDestStrip2.CurrentModuleFilter).Property1 = "UA";
			((ModuleLocationFilter)originDestStrip2.CurrentModuleFilter).Property2 = "GB";
			((ModuleLocationFilter)originDestStrip3.CurrentModuleFilter).Property1 = "BLAG";
			((ModuleLocationFilter)originDestStrip3.CurrentModuleFilter).Property2 = "CAMG";

			var containerTypeStrip1 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.ContainerType);
			var containerTypeStrip2 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.ContainerType);

			((ModuleGuidFilter)containerTypeStrip1.CurrentModuleFilter).Property = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			((ModuleGuidFilter)containerTypeStrip2.CurrentModuleFilter).Property = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			var containerModeStrip1 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.ContainerMode);
			var containerModeStrip2 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.ContainerMode);

			((ModuleTextFilter)containerModeStrip1.CurrentModuleFilter).Property = "FCL";
			((ModuleTextFilter)containerModeStrip2.CurrentModuleFilter).Property = "LSE";

			var transportModeStrip1 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.TransportMode);
			var transportModeStrip2 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.TransportMode);

			((ModuleTextFilter)transportModeStrip1.CurrentModuleFilter).Property = "AIR";
			((ModuleTextFilter)transportModeStrip2.CurrentModuleFilter).Property = "SEA";

			var carrierStrip1 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider);
			var carrierStrip2 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider);
			var carrierStrip3 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider);

			((ModuleGuidFilter)carrierStrip1.CurrentModuleFilter).Property = carrier1.PK;
			((ModuleGuidFilter)carrierStrip2.CurrentModuleFilter).Property = carrier2.PK;
			((ModuleGuidFilter)carrierStrip3.CurrentModuleFilter).Property = carrier3.PK;

			var scacCodeStrip1 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.SCACCode);
			var scacCodeStrip2 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.SCACCode);

			((WiseRatesModuleTextFilter)scacCodeStrip1.CurrentModuleFilter).Property = "MAEU";
			((WiseRatesModuleTextFilter)scacCodeStrip2.CurrentModuleFilter).Property = "SCA2";

			var iataCodeStrip1 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.IATACode);
			var iataCodeStrip2 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.IATACode);

			((ModuleGuidFilter)iataCodeStrip1.CurrentModuleFilter).Property = airlineA.PK;
			((ModuleGuidFilter)iataCodeStrip2.CurrentModuleFilter).Property = airlineB.PK;

			var contractNumberStrip1 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);
			var contractNumberStrip2 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);

			((WiseRatesModuleTextFilter)contractNumberStrip1.CurrentModuleFilter).Property = "00001";
			((WiseRatesModuleTextFilter)contractNumberStrip2.CurrentModuleFilter).Property = "00002";

			var c1CodeStrip1 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.C1Code);
			var c1CodeStrip2 = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.C1Code);

			((WiseRatesModuleTextFilter)c1CodeStrip1.CurrentModuleFilter).Property = "C1Z1";
			((WiseRatesModuleTextFilter)c1CodeStrip2.CurrentModuleFilter).Property = "C1Z2";

			var (result, _) = filterStripBizo.BuildRatesQuery(new LoggerDecorator());
			AssertEquals("Effective Date", new DateTime(2018, 3, 25), result.EffectiveDate);
			AssertEquals("Transport Mode", null, result.TransportMode);
			AssertEquals("Container Mode", null, result.ContainerMode);

			AssertContainsExactElementsInAnyOrder("Origins", new[] { "", "AUSYD", "UA", "AU", "OCEG", "MEDG", "SYD" }, result.Origin);
			AssertContainsExactElementsInAnyOrder("Destination", new[] { "", "USLAX", "US", "GB", "NOEG", "USWG", "LAX" }, result.Destination);
			AssertContainsExactElementsInAnyOrder("Carrier Scacs", new[] { "CAR1", "MAEU", "SCA2" }, result.Carrier.Select(x => x.SCACCode).WhereNotNull());
			AssertContainsExactElementsInAnyOrder("Carrier IATA codes", new[] { "AA", "BB" }, result.Carrier.Select(x => x.IATACode).WhereNotNull());
			AssertContainsExactElementsInAnyOrder("Container ISO", new[] { "22G0", "42G0" }, result.Container.Select(x => x.ISOType));
			AssertContainsExactElementsInAnyOrder("Contract Numbers", new[] { "00001", "00002" }, result.Contract.Select(c => c.ContractNumber));
			AssertContainsExactElementsInAnyOrder("Carrier C1 codes", new[] { "C1C1", "C1C2", "C1Z1", "C1Z2" }, result.Carrier.Select(x => x.C1Code).WhereNotNull());
		}

		public void TestBuildRatesQueries_TransportModeAndContainerMode()
		{
			BuildRatesQueries_TransportModeAndContainerMode([], [],
				[
					"ContainerMode:FCL; TransportMode:SEA",
					"ContainerMode:LCL,FCL; TransportMode:AIR"
				],
				"No Transport Mode / Container Mode, Should build all possible queries");

			BuildRatesQueries_TransportModeAndContainerMode(["AIR"], [],
				[
					"ContainerMode:LCL,FCL; TransportMode:AIR"
				],
				"Setting Transport Mode AIR should build only AIR related query");

			BuildRatesQueries_TransportModeAndContainerMode(["AIR"], ["LSE"],
				[
					"ContainerMode:LCL; TransportMode:AIR"
				],
				"Setting Transport Mode and Container Mode for AIR should build only AIR related query");

			BuildRatesQueries_TransportModeAndContainerMode(["SEA"], [],
				[
					"ContainerMode:FCL; TransportMode:SEA"
				],
				"Setting Transport Mode SEA should build only SEA related query");

			BuildRatesQueries_TransportModeAndContainerMode(["SEA"], ["FCL"],
				[
					"ContainerMode:FCL; TransportMode:SEA"
				],
				"Setting Transport Mode and Container Mode for SEA should build only SEA related query");

			BuildRatesQueries_TransportModeAndContainerMode(["SEA"], ["LSE"], [],
				"Setting Transport Mode and invalid Container Mode for SEA should not build query");

			BuildRatesQueries_TransportModeAndContainerMode(["AIR"], ["FCL", "LCL"], [],
				"Setting Transport Mode and invalid Container Mode for AIR should not build query");

			BuildRatesQueries_TransportModeAndContainerMode(["AIR"], ["LSE", "ULD"],
				[
					"ContainerMode:LCL,FCL; TransportMode:AIR"
				],
				"Setting Transport Mode and one valid Container Mode for AIR should build only AIR related query");

			BuildRatesQueries_TransportModeAndContainerMode(["SEA"], ["FCL", "ULD"],
				[
					"ContainerMode:FCL; TransportMode:SEA"
				],
				"Setting Transport Mode and one valid Container Mode for SEA should build only SEA related query");

			BuildRatesQueries_TransportModeAndContainerMode(["AIR", "SEA"], ["FCL", "LSE"],
				[
					"ContainerMode:FCL; TransportMode:SEA",
					"ContainerMode:LCL; TransportMode:AIR"
				],
				"Should build query if valid combination exists");
			Assert(true);
		}

		public void TestBuildRatesQueries_ContainerTypeOnly_BuildsQuery()
		{
			BuildRatesQueries_TransportModeContainerModeAndContainerType([], [],
				[new RatesQueryContainer { Code = "DPE" }],
				[
					"Container:Code:DPE;ISOType:;ISOTypeGroups:;IATAULDRateClass:; ContainerMode:FCL; TransportMode:SEA",
					"Container:Code:DPE;ISOType:;ISOTypeGroups:;IATAULDRateClass:; ContainerMode:LCL,FCL; TransportMode:AIR"
				],
				"Specifying container type with no transport mode, container mode or iso code should build all queries");

			Assert(true);
		}

		public void TestBuildRatesQueries_ContainerTypeAndContainerMode_BuildsQuery()
		{
			BuildRatesQueries_TransportModeContainerModeAndContainerType([], ["ULD"],
				[new RatesQueryContainer { Code = "DPE" }],
				[
					"Container:Code:DPE;ISOType:;ISOTypeGroups:;IATAULDRateClass:; ContainerMode:FCL; TransportMode:AIR"
				],
				"Specifying container type and container mode with no transport mode and no iso code should still build query");

			Assert(true);
		}

		void BuildRatesQueries_TransportModeAndContainerMode(string[] transportModes, string[] containerModes, string[] expectedQueries, string because)
		{
			BuildRatesQueries_TransportModeContainerModeAndContainerType(transportModes, containerModes, [], expectedQueries, because);
		}

		void BuildRatesQueries_TransportModeContainerModeAndContainerType(string[] transportModes, string[] containerModes, RatesQueryContainer[] containerTypes, string[] expectedQueries, string because)
		{
			var settings = new RatesServiceRegistrySettingsCollection();

			settings.Add(new RatesServiceRegistrySettings(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL, true));
			settings.Add(new RatesServiceRegistrySettings(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Loose, true));
			settings.Add(new RatesServiceRegistrySettings(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.ULD, true));

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
			{
				var filterStripBizo = new WiseRatesFilterStripBusinessObject();

				foreach (var transportMode in transportModes)
				{
					var transportModeStrip = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.TransportMode);
					((ModuleTextFilter)transportModeStrip.CurrentModuleFilter).Property = transportMode;
				}

				foreach (var containerMode in containerModes)
				{
					var containerModeStrip = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.ContainerMode);
					((ModuleTextFilter)containerModeStrip.CurrentModuleFilter).Property = containerMode;
				}

				foreach (var containerType in containerTypes)
				{
					var containerTypeStrip = filterStripBizo.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.ContainerType);

					var container = Factory.NewWithValidTestData<MasterFiles.Business.RefContainer>();
					container.RC_Code = containerType.Code;
					container.RC_ISOType = containerType.ISOType;
					container.RC_IATARateClass = ZString.Empty;
					Factory.Save();
					((ModuleGuidFilter)containerTypeStrip.CurrentModuleFilter).Property = container.PK;
				}

				var ratesQueries = filterStripBizo.BuildRatesQueries(new LoggerDecorator());

				AssertContainsExactElementsInAnyOrder(
					because,
					expectedQueries,
					ratesQueries.Select(q => q.ToString())
				);
			}
		}

		#endregion
	}
}
