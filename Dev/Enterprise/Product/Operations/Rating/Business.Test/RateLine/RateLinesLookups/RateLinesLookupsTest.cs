using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class RateLinesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRateLineConditions_NonIntercompanyTariffs()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			var line = entry.RateLines.AddNew();
			var lookups = new RateLinesLookups(line);

			entry.TI_RateCategory = RatingConstants.RateCategory.AIR;
			var codes = lookups.RateLineConditions.Cast<ICodeDescription>().Select(x => x.Code);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				RateLineConditions.DangerousGoods,
				RateLineConditions.OwnBrokerage,
				RateLineConditions.HandOver,
				RateLineConditions.ForwardingAndBrokerage,
				RateLineConditions.OwnCFS,
				RateLineConditions.OwnGateway,
				RateLineConditions.OwnControllingAgent,
				RateLineConditions.UserDefined
			}, codes);

			entry.TI_RateCategory = RatingConstants.RateCategory.SCO;
			codes = lookups.RateLineConditions.Cast<ICodeDescription>().Select(x => x.Code);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				RateLineConditions.DangerousGoods,
				RateLineConditions.UserDefined
			}, codes);
		}

		public void TestRateLineConditions_IntercompanyTariffs()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			var entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR);
			var line = entry.RateLines.AddNew();
			var lookups = new RateLinesLookups(line);

			var codes = lookups.RateLineConditions.Cast<ICodeDescription>().Select(x => x.Code);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				RateLineConditions.DangerousGoods,
				RateLineConditions.OwnBrokerage,
				RateLineConditions.HandOver,
				RateLineConditions.ForwardingAndBrokerage,
				RateLineConditions.OwnCFS,
				RateLineConditions.OwnControllingAgent,
				RateLineConditions.UserDefined
			}, codes);

			entry.TI_RateCategory = RatingConstants.RateCategory.SCO;
			codes = lookups.RateLineConditions.Cast<ICodeDescription>().Select(x => x.Code);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				RateLineConditions.DangerousGoods,
				RateLineConditions.UserDefined
			}, codes);
		}

		public void TestForwardingChargeGroups()
		{
			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.AIR,
				new string[] {
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.FCL,
				new string[] {
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.LCL,
				new string[] {
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.ORG,
				new string[] {
					ChargeCodeGroupList.Codes.Origin,
					ChargeCodeGroupList.Codes.Loading,
					ChargeCodeGroupList.Codes.OriginBrokerage,
					ChargeCodeGroupList.Codes.OriginBrokerageOnly,
					ChargeCodeGroupList.Codes.CustomsDuty,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.DST,
				new string[] {
					ChargeCodeGroupList.Codes.Destination,
					ChargeCodeGroupList.Codes.Unloading,
					ChargeCodeGroupList.Codes.Brokerage,
					ChargeCodeGroupList.Codes.BrokerageOnly,
					ChargeCodeGroupList.Codes.CustomsDuty,
				});
		}

		public void TestShippingDetentionChargeGroups()
		{
			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.SID, new[] { ChargeCodeGroupList.Codes.Destination });
			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.SED, new[] { ChargeCodeGroupList.Codes.Origin });
		}

		public void TestShippingChargeGroups()
		{
			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.SCO,
				new string[] {
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.SNC,
				new string[] {
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.SOR,
				new string[] {
					ChargeCodeGroupList.Codes.Origin,
					ChargeCodeGroupList.Codes.Loading,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.SDE,
				new string[] {
					ChargeCodeGroupList.Codes.Destination,
					ChargeCodeGroupList.Codes.Unloading,
				});
		}

		public void TestContainerYardChargeGroup()
		{
			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.CYD,
				new string[] {
					ChargeCodeGroupList.Codes.YardGateIn,
					ChargeCodeGroupList.Codes.YardGateOut,
					ChargeCodeGroupList.Codes.YardStorage,
				});
		}

		public void TestCustomsChargeGroups()
		{
			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.CAI,
				new string[] {
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.CFC,
				new string[] {
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.CLC,
				new string[] {
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.COR,
				new string[] {
					ChargeCodeGroupList.Codes.Origin,
					ChargeCodeGroupList.Codes.Loading,
					ChargeCodeGroupList.Codes.OriginBrokerage,
					ChargeCodeGroupList.Codes.OriginBrokerageOnly,
					ChargeCodeGroupList.Codes.CustomsDuty,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.CDS,
				new string[] {
					ChargeCodeGroupList.Codes.Destination,
					ChargeCodeGroupList.Codes.Unloading,
					ChargeCodeGroupList.Codes.Brokerage,
					ChargeCodeGroupList.Codes.BrokerageOnly,
					ChargeCodeGroupList.Codes.CustomsDuty,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.WHS,
				new string[] {
					ChargeCodeGroupList.Codes.WHSInwards,
					ChargeCodeGroupList.Codes.WHSOutwards,
					ChargeCodeGroupList.Codes.WHSStorage,
					ChargeCodeGroupList.Codes.WHSAdHocServiceJob,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.TRW,
				new string[] {
					ChargeCodeGroupList.Codes.TRWReceive,
					ChargeCodeGroupList.Codes.TRWDispatch,
				});

			AssertChargeGroupValidForRateMode(RatingConstants.RateCategory.TWU,
				new string[] {
					ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit,
					ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit,
				});
		}

		void AssertChargeGroupValidForRateMode(string mode, IEnumerable<string> chargeGroups)
		{
			var rate = Factory.New<ClientRate>();
			var item = rate.AddRateEntry(mode).RateLines.AddNew();

			foreach (var chargeGroupToCheck in chargeGroups)
			{
				var message = ZString.Format("{0} should be a valid charge group for {1} rate lines", chargeGroupToCheck, mode);
				Assert(message, item.Lookups.IsChargeGroupApplicableToRateLine(chargeGroupToCheck));
			}
		}

		public void TestDefaultChargeGroupFilterOnChargeCodeCollection()
		{
			var clientRate = Factory.New<ClientRate>();

			// Forwarding
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, clientRate, RatingConstants.RateCategory.AIR, "", expectedChargeCodeGroup: ChargeCodeGroupList.Codes.Freight);
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, clientRate, RatingConstants.RateCategory.FCL, RateMode.SEA, expectedChargeCodeGroup: ChargeCodeGroupList.Codes.Freight);
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, clientRate, RatingConstants.RateCategory.LCL, RateMode.LCL, expectedChargeCodeGroup: ChargeCodeGroupList.Codes.Freight);
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, clientRate, RatingConstants.RateCategory.ORG, RateMode.AIR, expectedChargeCodeGroup: AccChargeCodeLookups.OriginAndLoadingGroupFilterCode);
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, clientRate, RatingConstants.RateCategory.DST, RateMode.AIR, expectedChargeCodeGroup: AccChargeCodeLookups.DestinationAndUnloadingGroupFilterCode);

			// Customs Forwarding
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, clientRate, RatingConstants.RateCategory.CAI, "", expectedChargeCodeGroup: ChargeCodeGroupList.Codes.Freight);
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, clientRate, RatingConstants.RateCategory.CFC, RateMode.SEA, expectedChargeCodeGroup: ChargeCodeGroupList.Codes.Freight);
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, clientRate, RatingConstants.RateCategory.CLC, RateMode.LCL, expectedChargeCodeGroup: ChargeCodeGroupList.Codes.Freight);
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, clientRate, RatingConstants.RateCategory.COR, RateMode.AIR, expectedChargeCodeGroup: AccChargeCodeLookups.OriginAndLoadingGroupFilterCode);
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, clientRate, RatingConstants.RateCategory.CDS, RateMode.AIR, expectedChargeCodeGroup: AccChargeCodeLookups.DestinationAndUnloadingGroupFilterCode);

			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, null, RatingConstants.RateCategory.PAC, RateMode.AIR, expectedChargeCodeGroup: AccChargeCodeLookups.CFSGroupFilterCode);
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, null, RatingConstants.RateCategory.CYD, RateMode.ALL, expectedChargeCodeGroup: AccChargeCodeLookups.CYDGroupFilterCode);

			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, null, RatingConstants.RateCategory.WHS, RateMode.ALL, expectedChargeCodeGroup: AccChargeCodeLookups.WHSGroupFilterCode);
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, null, RatingConstants.RateCategory.TRW, RateMode.ALL, expectedChargeCodeGroup: AccChargeCodeLookups.TRWGroupFilterCode);
			AssertDefaultChargeGroupFilterOnChargeCodeCollection(Factory, null, RatingConstants.RateCategory.TWU, RateMode.ALL, expectedChargeCodeGroup: AccChargeCodeLookups.TWUGroupFilterCode);

			void AssertDefaultChargeGroupFilterOnChargeCodeCollection(BusinessObjectFactory factory, RatingHeader existingRatingHeader, string category, string mode, string expectedChargeCodeGroup)
			{
				var ratingHeader = existingRatingHeader ?? factory.New<ClientRate>();

				var rateEntry = ratingHeader.AddRateEntry(category, mode, "", "");
				var rateLine = rateEntry.RateLines.AddNew();
				AssertEquals
				(
					$"Category: {category}, Mode: {mode}",
					expectedChargeCodeGroup,
					rateLine.Lookups.ChargeCodes.FilterBusinessObjectDefaults["Charge Group" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value
				);
			}
		}

		#region Zones

		public void TestLookupZone_ClientNotSet()
		{
			var destinationRateLine = SetupClientRateLine();
			clientRateTransportProvider.TP_IsActive = false;

			AssertEquals(1, destinationRateLine.Lookups.Zones.Count);
			Assert(destinationRateLine.Lookups.Zones.ContainsCode("generic zone"));
		}

		public void TestLookupZone_ClientSetWithInactiveZone()
		{
			var destinationRateLine = SetupClientRateLine();

			clientRateTransportProvider.TP_IsActive = false;
			var clientZone = clientRateTransportProvider.CreateRateTransportZoneForTest("client zone");
			clientZone.TZ_IsActive = false;
			AssertEquals(1, destinationRateLine.Lookups.Zones.Count);
			Assert(destinationRateLine.Lookups.Zones.ContainsCode("generic zone"));
		}

		public void TestLookupZone_ClientSetWithActiveZone()
		{
			var destinationRateLine = SetupClientRateLine();

			var clientZone = clientRateTransportProvider.CreateRateTransportZoneForTest("client zone");
			clientZone.TZ_IsActive = true;
			AssertEquals(1, destinationRateLine.Lookups.Zones.Count);
			Assert(destinationRateLine.Lookups.Zones.ContainsCode("client zone"));
		}

		public void TestLookupZone_SupplierSetWithInactiveZone()
		{
			var destinationRateLine = SetupClientRateLine(true);

			var clientZone = clientRateTransportProvider.CreateRateTransportZoneForTest("client zone");
			clientZone.TZ_IsActive = true;

			supplierRateTransportProvider.TP_IsActive = false;
			var supplierZone = supplierRateTransportProvider.CreateRateTransportZoneForTest("supplier zone");
			supplierZone.TZ_IsActive = false;

			AssertEquals(1, destinationRateLine.Lookups.Zones.Count);
			Assert(destinationRateLine.Lookups.Zones.ContainsCode("client zone"));
		}

		public void TestLookupZone_SupplierSetWithActiveZone()
		{
			var rateLine = SetupClientRateLine(true);

			var clientZone = clientRateTransportProvider.CreateRateTransportZoneForTest("client zone");
			clientZone.TZ_IsActive = true;

			var supplierZone = supplierRateTransportProvider.CreateRateTransportZoneForTest("supplier zone");
			supplierZone.TZ_IsActive = true;

			Assert(rateLine.Lookups.Zones.ContainsCode("supplier zone"));
		}

		public void TestLookupZone_SupplierNotSet_StandardCosting()
		{
			SetupRateTransportProviders(null, null);

			var costing = Helper.NewCosting(null);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, string.Empty, "AUSYD");
			var rateLine = rateEntry.AddRateLine("DDOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			Assert("Costs never use supplier", rateLine.Lookups.Zones.ContainsCode("generic zone"));
		}

		public void TestLookupZone_SupplierSet_StandardCosting()
		{
			var supplier = Helper.NewOrgHeader();
			SetupRateTransportProviders(null, supplier);

			var costing = Helper.NewCosting(supplier);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, string.Empty, "AUSYD");
			var rateLine = rateEntry.AddRateLine("DDOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var supplierZone = supplierRateTransportProvider.CreateRateTransportZoneForTest("supplier zone");
			supplierZone.TZ_IsActive = true;

			Assert("Costs for supplier should return suppliers zones", rateLine.Lookups.Zones.ContainsCode("supplier zone"));
		}

		RateLine SetupClientRateLine(bool setSupplier = false)
		{
			var client = Helper.NewOrgHeader();
			var supplier = Helper.NewOrgHeader();

			SetupRateTransportProviders(client, supplier);

			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, string.Empty, "AUSYD");
			var rateLine = rateEntry.AddRateLine("DDOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			if (setSupplier)
			{
				rateEntry.TI_OH_Supplier = supplier.PK;
			}

			return rateLine;
		}

		void SetupRateTransportProviders(OrgHeader client, OrgHeader supplier)
		{
			var genericRateTransportProvider = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.All);
			var genericZone = genericRateTransportProvider.CreateRateTransportZoneForTest("generic zone");
			genericZone.TZ_IsActive = true;

			if (client != null)
			{
				clientRateTransportProvider = Helper.CreateRateTransportZoneSet(client, CountryCodes.Australia, RatingConstants.RatingZoneTypes.All);
			}

			if (supplier != null)
			{
				supplierRateTransportProvider = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.Australia, RatingConstants.RatingZoneTypes.All);
			}

			Factory.Save();
		}

		public void TestGetCartageLocationForZones()
		{
			var unmatchedUnloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unmatchedUnloco.RL_RN_NKCountryCode = CountryCodes.Australia;
			unmatchedUnloco.RL_Code = "AUNOP";
			unmatchedUnloco.RL_PortName = "Nope nope";

			var suburbA = Helper.GetCityTown("Arncliffe", "NSW");
			var suburbB = Helper.GetCityTown("Bathurst", "NSW");
			var suburbC = Helper.GetCityTown("Chullora", "NSW");

			Helper.CreateRateTransportZoneSet(null, "", suburbA);
			Helper.CreateRateTransportZoneSet(null, "", suburbB);
			Helper.CreateRateTransportZoneSet(null, "", suburbC);

			var client = Helper.NewOrgHeader();
			Helper.CreateRateTransportZoneSet(client, "", suburbA, "Client Zone A");
			Helper.CreateRateTransportZoneSet(client, "", suburbB, "Client Zone B");

			var supplier = Helper.NewOrgHeader();
			Helper.CreateRateTransportZoneSet(supplier, "", suburbB, "Supplier Zone B");
			Helper.CreateRateTransportZoneSet(supplier, "", suburbC, "Supplier Zone C");

			var consignee = Helper.NewOrgHeader();
			var consigneeAddress = consignee.MainAddress;
			consigneeAddress.OA_Address1 = "Dodgey Back St";
			consigneeAddress.OA_City = "Chullora";
			consigneeAddress.OA_State = "NSW";
			consigneeAddress.OA_RL_NKRelatedPortCode = "AUCFS";

			Factory.Save();

			var clientRate = Helper.NewClientRate(client);
			var originEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, unmatchedUnloco.RL_Code, "AUSYD");
			var originLine = originEntry.AddRateLine("ODOC", FlatCalculator.Code);

			var location = originLine.GetCartageLocationForZones();

			AssertEquals("Expected to use the origin port for origin rate entries", unmatchedUnloco.RL_Code, location.Code);
			AssertEquals("Expected unloco not to be matched to any city town", null, location.CityTown);

			originEntry.TI_OriginLRC = "AUARC";
			location = originLine.GetCartageLocationForZones();

			AssertEquals("With this particular unloco, there's enough information to match a city town", suburbA, location.CityTown);
			AssertEquals("Should have matched client specific zone set rather than generic zone set for same location", "Client Zone A", originLine.Lookups.Zones[0].Code);

			originEntry.TI_OriginLRC = "AUBHS";
			location = originLine.GetCartageLocationForZones();

			AssertEquals("AUBHS", location.Code);
			AssertEquals(suburbB, location.CityTown);
			AssertEquals("Zones should update to new location zone", "Client Zone B", originLine.Lookups.Zones[0].Code);

			originEntry.TI_OH_Supplier = supplier.PK;
			location = originLine.GetCartageLocationForZones();

			AssertEquals("Location should not change", suburbB, location.CityTown);
			AssertEquals("Zones should belong to new provider which is prefered to RatingHeader client", "Supplier Zone B", originLine.Lookups.Zones[0].Code);

			originEntry.TI_OH_Consignee = consignee.PK;
			originEntry.TI_OA_CartagePickupAddressOverride = consigneeAddress.PK;
			location = originLine.GetCartageLocationForZones();

			AssertEquals("Pre-condition", consigneeAddress.PK, originLine.ParentRateEntry.TI_OA_CartagePickupAddressOverride);
			AssertEquals(suburbC, location.CityTown);
			AssertEquals("Supplier Zone C", originLine.Lookups.Zones[0].Code);
		}

		public void TestZonesListRateEntryNoParent()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var line = entry.RateLines.AddNew();
			AssertEquals(0, line.Lookups.Zones.Count);
		}

		public void TestZonesListCorrectlyFiltered()
		{
			SetUpZones();

			var rate = Factory.New<ClientRate>();

			var orgRateEntry = rate.AddRateEntry("ORG", "AIR", "INBOM", "USLAX");
			orgRateEntry.TI_OH_Supplier = supplier1.PK;
			var orgRateLine = orgRateEntry.AddRateLine("ODOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var dstRateEntry = rate.AddRateEntry("DST", "AIR", "INBOM", "USLAX");
			dstRateEntry.TI_OH_Supplier = supplier2.PK;
			var dstRateLine = dstRateEntry.AddRateLine("DDOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertEquals("2 zones present for origin line", 2, orgRateLine.Lookups.Zones.Count);
			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone1.TZ_ZoneName));
			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone2.TZ_ZoneName));

			AssertEquals("2 zones present for dest line", 2, dstRateLine.Lookups.Zones.Count);
			Assert("Correct zones present", dstRateLine.Lookups.Zones.ContainsCode(zone5.TZ_ZoneName));
			Assert("Correct zones present", dstRateLine.Lookups.Zones.ContainsCode(zone6.TZ_ZoneName));
		}

		public void TestZonesListCorrectlyFiltered_Costings()
		{
			SetUpZones();

			var cost = Factory.New<Costing>();
			cost.TH_OH = supplier1.PK;

			var orgRateEntry = cost.AddRateEntry("ORG", "AIR", "INBOM", "USLAX");
			var orgRateLine = orgRateEntry.AddRateLine("ODOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var dstRateEntry = cost.AddRateEntry("DST", "AIR", "INBOM", "USLAX");
			var dstRateLine = dstRateEntry.AddRateLine("DDOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertEquals("2 zones present for origin line", 2, orgRateLine.Lookups.Zones.Count);
			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone1.TZ_ZoneName));
			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone2.TZ_ZoneName));

			AssertEquals("2 zones present for dest line", 2, dstRateLine.Lookups.Zones.Count);
			Assert("Correct zones present", dstRateLine.Lookups.Zones.ContainsCode(zone3.TZ_ZoneName));
			Assert("Correct zones present", dstRateLine.Lookups.Zones.ContainsCode(zone4.TZ_ZoneName));
		}

		public void TestZonesListCorrectlyFiltered_Costings_FallBack()
		{
			SetUpZones();

			var cost = new BusinessObjectFactory().New<Costing>();
			cost.TH_OH = supplier1.PK;

			var orgRateEntry = cost.AddRateEntry("ORG", "AIR", "INBOM", "DEHAM");
			var orgRateLine = orgRateEntry.AddRateLine("ODOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertEquals("2 zones present for origin line", 2, orgRateLine.Lookups.Zones.Count);
			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone1.TZ_ZoneName));
			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone2.TZ_ZoneName));

			zone1.TransportProvider.Delete();

			Factory.Save();

			cost = new BusinessObjectFactory().New<Costing>();
			cost.TH_OH = supplier1.PK;

			orgRateEntry = cost.AddRateEntry("ORG", "AIR", "INBOM", "DEHAM");
			orgRateLine = orgRateEntry.AddRateLine("ODOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertEquals("2 zones present for origin line", 2, orgRateLine.Lookups.Zones.Count);
			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone7.TZ_ZoneName));
			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone8.TZ_ZoneName));
		}

		public void TestZonesListCorrectlyFilteredForGenericZone()
		{
			SetUpZones();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var orgRateEntry = rate.AddRateEntry("ORG", "AIR", "INBOM", "USLAX");
			var orgRateLine = orgRateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertEquals("2 zones present for origin line", 2, orgRateLine.Lookups.Zones.Count);

			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone7.TZ_ZoneName));
			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone8.TZ_ZoneName));
		}

		public void TestZonesListFilterZoneTypes()
		{
			SetUpZones();
			zone7.TransportProvider.TP_ZoneType = RatingConstants.RatingZoneTypes.Operations;

			var cost = Factory.New<Costing>();
			var orgRateEntry = cost.AddRateEntry("ORG", "AIR", "INBOM", "USLAX");
			var orgRateLine = orgRateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertEquals("Expected no zones present as the matching zone by location has the wrong type", 0, orgRateLine.Lookups.Zones.Count);

			zone7.TransportProvider.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			cost = newFactory.New<Costing>();
			orgRateEntry = cost.AddRateEntry("ORG", "AIR", "INBOM", "USLAX");
			orgRateLine = orgRateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertEquals("2 zones present as the zone type is now rating", 2, orgRateLine.Lookups.Zones.Count);
			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone7.TZ_ZoneName));
			Assert("Correct zones present", orgRateLine.Lookups.Zones.ContainsCode(zone8.TZ_ZoneName));
		}

		public void TestZonesListFilterZoneTypes_AllowRatingTypesByMode()
		{
			var client = Helper.NewOrgHeader();

			var allZoneSet = Helper.CreateRateTransportZoneSet(client, CountryCodes.Australia, RatingConstants.RatingZoneTypes.All, RateMode.ALL);
			allZoneSet.CreateRateTransportZoneForTest("ALL Zone");

			var ratZoneSet = Helper.CreateRateTransportZoneSet(client, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.ALL);
			ratZoneSet.CreateRateTransportZoneForTest("RATE Zone");

			var racZoneSet = Helper.CreateRateTransportZoneSet(client, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.ULD);
			racZoneSet.CreateRateTransportZoneForTest("RATE Zone");

			var rateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "");
			var rateLine = rateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertContainsExactElementsInAnyOrder("Should load a zone set even though ALL is not a valid mode for CTZ calc", new[] { "RATE Zone" }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = RateMode.ULD;
			AssertContainsExactElementsInAnyOrder(new[] { "RATE Zone" }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = RateMode.ULD;
			AssertContainsExactElementsInAnyOrder(new[] { "RATE Zone" }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = RateMode.LSE;
			AssertContainsExactElementsInAnyOrder("There is no Air Uncontainerised mode so should fall back to Rating only type", new[] { "RATE Zone" }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = RateMode.AIR;
			AssertContainsExactElementsInAnyOrder("Should contain both air elements", new[] { "RATE Zone" }, rateLine.Lookups.Zones.GetAllCodes());

			var rauZone = Helper.CreateRateTransportZoneSet(client, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.ULD);
			Factory.Save();
			Factory.ClearCachedValue<CodeDescriptionPairList>("RateLinesLookups.Transport.TESTORG1.AUSYD.LSE");
			Factory.ClearCachedValue<CodeDescriptionPairList>("RateLinesLookups.Transport.TESTORG1.AUSYD.AIR");

			rateEntry.TI_Mode = RateMode.LSE;
			AssertContainsExactElementsInAnyOrder("Now there is a more specific zone set, it should be prefered instead", new[] { "RATE Zone" }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = RateMode.AIR;
			AssertContainsExactElementsInAnyOrder("Should contain both only RATE Zone", new[] { "RATE Zone" }, rateLine.Lookups.Zones.GetAllCodes());
		}

		public void TestZonesListFilterZoneTypes_ByMode_NoException()
		{
			var client = Helper.NewOrgHeader();
			var rateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "");
			var rateLine = rateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertEquals("When there are no zone sets, should return empty", 0, rateLine.Lookups.Zones.Count);

			rateEntry.TI_Mode = RateMode.ULD;
			AssertEquals("When there are no zone sets, should return empty", 0, rateLine.Lookups.Zones.Count);

			rateEntry.TI_Mode = RateMode.AIR;
			AssertEquals("When there are no zone sets, should return empty", 0, rateLine.Lookups.Zones.Count);
		}

		public void TestZonesForACIZones()
		{
			CreateZone("1234", "A", "USLAX");
			CreateZone("2222", "B", "USLAX");
			CreateZone("3333", "C", "USLAX");
			CreateZone("1235", "D", "USMEM");
			CreateZone("45643", "E", "USMEM");
			CreateZone("1236", "A", "USXXX");
			CreateZone("9999", "", "USXXX");
			Factory.Save();

			var rate = Factory.New<ClientRate>();
			var orgRateEntry = rate.AddRateEntry("ORG", "AIR", "USLAX", "USLAX");
			var orgRateLine = orgRateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			orgRateLine.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones = false;
			AssertEquals(0, orgRateLine.Lookups.Zones.Count);

			orgRateLine.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones = true;
			AssertEquals(3, orgRateLine.Lookups.Zones.Count);
			Assert("A", orgRateLine.Lookups.Zones.ContainsCode("A"));
			Assert("B", orgRateLine.Lookups.Zones.ContainsCode("B"));
			Assert("C", orgRateLine.Lookups.Zones.ContainsCode("C"));

			orgRateEntry.TI_OriginLRC = "US";
			orgRateLine.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones = false;
			AssertEquals(0, orgRateLine.Lookups.Zones.Count);

			orgRateLine.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones = true;

			var resultZones = orgRateLine.Lookups.Zones.GetAllCodes();
			var expectedZones = new[] { "A", "B", "C", "D", "E" };

			AssertContainsExactElementsInAnyOrder("No repeated zones", expectedZones, resultZones);
		}

		public void TestZonesForACIZonesWithBreaks()
		{
			CreateZone("0001", "zone1", "USLAX");
			CreateZone("0002", "zone2", "USLAX");
			CreateZone("0003", "zone3", "USLAX");
			Factory.Save();

			var rate = Factory.New<ClientRate>();
			var orgRateEntry = rate.AddRateEntry("ORG", "AIR", "USLAX", "AUSYD");
			var orgRateLine = orgRateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var calculator = orgRateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.UseACIZones = true;

			calculator.AddRateLineItemWithZone("-", 10, 100, "zone1");
			calculator.AddRateLineItemWithZone("+", 10, 110, "zone1");
			var restricted = calculator.AddRateLineItemWithZone("+", 20, 0, "zone1");
			restricted.TM_CallForPricing = true;
			restricted.TM_Text = "Potato";
			calculator.AddRateLineItemWithZone("-", 10, 200, "zone2");
			calculator.AddRateLineItemWithZone("+", 10, 220, "zone2");
			restricted = calculator.AddRateLineItemWithZone("+", 20, 0, "zone2");
			restricted.TM_CallForPricing = true;
			restricted.TM_Text = "Carrot";
			calculator.AddRateLineItemWithZone("-", 10, 300, "zone3");
			calculator.AddRateLineItemWithZone("+", 10, 330, "zone3");
			restricted = calculator.AddRateLineItemWithZone("+", 20, 0, "zone3");
			restricted.TM_CallForPricing = true;
			restricted.TM_Text = "Tomato";
			calculator.AddRateLineItemWithZone("-", 10, 400, string.Empty);
			calculator.AddRateLineItemWithZone("+", 10, 440, string.Empty);
			restricted = calculator.AddRateLineItemWithZone("+", 20, 0, string.Empty);
			restricted.TM_CallForPricing = true;
			restricted.TM_Text = "Canola";

			var current = orgRateLine.Lookups.Zones.ToArray().Select(i => i.Code).ToArray();
			var expectedZones = new[] { "zone1", "zone2", "zone3" };
			AssertContainsExactElementsInAnyOrder("Checking the zones available to select is correct", expectedZones, current);

			var itemsZone1 = calculator.CartageZones.FindZone("zone1").ZoneRateLineItems
				.Cast<RateLineItem>()
				.Select(i => $"{i.TM_Type}|{i.TM_Break}|{i.TM_RelevantValue}|{i.TM_Text}|{i.TM_F1Zone}")
				.ToArray();
			var expectedItemsZone1 = new[]
			{
				"-|10|100||zone1",
				"+|10|110||zone1",
				"+|20|0|Potato|zone1"
			};
			AssertContainsExactElementsInAnyOrder("Expected items in zone1 did not match", expectedItemsZone1, itemsZone1);

			var itemsZone2 = calculator.CartageZones.FindZone("zone2").ZoneRateLineItems
				.Cast<RateLineItem>()
				.Select(i => $"{i.TM_Type}|{i.TM_Break}|{i.TM_RelevantValue}|{i.TM_Text}|{i.TM_F1Zone}")
				.ToArray();
			var expectedItemsZone2 = new[]
			{
				"-|10|200||zone2",
				"+|10|220||zone2",
				"+|20|0|Carrot|zone2"
			};
			AssertContainsExactElementsInAnyOrder("Expected items in zone2 did not match", expectedItemsZone2, itemsZone2);

			var itemsZone3 = calculator.CartageZones.FindZone("zone3").ZoneRateLineItems
				.Cast<RateLineItem>()
				.Select(i => $"{i.TM_Type}|{i.TM_Break}|{i.TM_RelevantValue}|{i.TM_Text}|{i.TM_F1Zone}")
				.ToArray();
			var expectedItemsZone3 = new[]
			{
				"-|10|300||zone3",
				"+|10|330||zone3",
				"+|20|0|Tomato|zone3"
			};
			AssertContainsExactElementsInAnyOrder("Expected items in zone3 did not match", expectedItemsZone3, itemsZone3);

			var itemsGeneral = calculator.CartageZones.FindZone(string.Empty).ZoneRateLineItems
				.Cast<RateLineItem>()
				.Select(i => $"{i.TM_Type}|{i.TM_Break}|{i.TM_RelevantValue}|{i.TM_Text}|{i.TM_F1Zone}")
				.ToArray();
			var expectedItemsGeneral = new[]
			{
				"-|10|400||",
				"+|10|440||",
				"+|20|0|Canola|"
			};
			AssertContainsExactElementsInAnyOrder("Expected items in general zone did not match", expectedItemsGeneral, itemsGeneral);
		}

		public void TestGetTransportProviderLocationQueryForInternationalZones()
		{
			var clientRate = Factory.New<ClientRate>();

			var zoneSet1 = Helper.CreateRateTransportZoneSet(null, CountryCodes.Angola);
			zoneSet1.CreateRateTransportZoneForTest("Zone AO");

			var zoneSet2 = Helper.CreateRateTransportZoneSet(null, CountryCodes.Colombia);
			zoneSet2.CreateRateTransportZoneForTest("Zone CO");

			var rateEntryCountry = clientRate.AddRateEntry("ORG", "AIR", "AFOR", "");
			var rateLineCountry = rateEntryCountry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			Assert("Should contain Zone AO, as AO is a country included in AFOR.", rateLineCountry.Lookups.Zones.ContainsCode("Zone AO"));
			Assert("Should not contain Zone CO, as it is not a country included in AFOR.", !rateLineCountry.Lookups.Zones.ContainsCode("Zone CO"));
		}

		public void TestInactiveTransportZonesNotIncludedOnRateLineLookups()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var rateTransportProvider = Helper.CreateRateTransportZoneSet(orgHeader, CountryCodes.Australia, RatingConstants.RatingZoneTypes.All);
			var inactiveZone = rateTransportProvider.CreateRateTransportZoneForTest("zone1");
			inactiveZone.TZ_IsActive = false;
			var activeZone = rateTransportProvider.CreateRateTransportZoneForTest("zone2");
			activeZone.TZ_IsActive = true;

			Factory.Save();

			var rate = Helper.NewClientRate(orgHeader);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "CNSHA", "AUSYD");
			entry.TI_OH_TransportProvider = orgHeader.PK;
			var originRateLine = entry.AddRateLine("DDOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertEquals(1, originRateLine.Lookups.Zones.Count);
			Assert(originRateLine.Lookups.Zones.ContainsCode("zone2"));
		}

		public void TestZonesUpdateWhenPortTransportAddressIsChangedForOriginRates()
		{
			var client = Helper.NewOrgHeader();

			var portTransportOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			portTransportOrgHeader.OH_IsConsignor = true;

			var melbourneAddress = portTransportOrgHeader.MainAddress;
			melbourneAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			melbourneAddress.OA_City = "Melbourne";
			melbourneAddress.OA_State = "VIC";
			melbourneAddress.Address1 = "123 King St";

			var canberraAddress = portTransportOrgHeader.Addresses.AddNew();
			canberraAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			canberraAddress.OA_City = "Canberra";
			canberraAddress.OA_State = "ACT";
			canberraAddress.Address1 = "25 Edinburgh Ave";

			Helper.CreateRateTransportZoneSet(client, "", Helper.GetCityTown("Sydney", "NSW"), "Sydney Zone");
			Helper.CreateRateTransportZoneSet(client, "", Helper.GetCityTown("Canberra", "ACT"), "Canberra Zone");
			Helper.CreateRateTransportZoneSet(client, CountryCodes.Australia, zoneNames: new ZString[] { "AU Zone" });

			Factory.Save();

			var orgRateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUSYD", "");
			var orgRateLine = orgRateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var message = "Expected the zone to be matched with CityTown(sydney) from the Origin (AUSYD)";
			Assert(message, orgRateLine.Calculator.CartageZones.ContainsZone("Sydney Zone"));
			Assert(message, !orgRateLine.Calculator.CartageZones.ContainsZone("AU Zone"));
			Assert(message, !orgRateLine.Calculator.CartageZones.ContainsZone("Canberra Zone"));

			orgRateEntry.TI_OH_Consignor = portTransportOrgHeader.PK;
			orgRateEntry.TI_OA_CartagePickupAddressOverride = melbourneAddress.PK;

			message = "Since Melbourne was added as pickup address to rate entry then Sydney/Canberra Zones no longer matches";
			Assert(message, orgRateLine.Calculator.CartageZones.ContainsZone("AU Zone"));
			Assert(message, !orgRateLine.Calculator.CartageZones.ContainsZone("Sydney Zone"));
			Assert(message, !orgRateLine.Calculator.CartageZones.ContainsZone("Canberra Zone"));

			orgRateEntry.TI_OA_CartagePickupAddressOverride = canberraAddress.PK;

			message = "Since Canberra was added as pickup address to rate entry then Sydney/AU Zones no longer matches";
			Assert(message, orgRateLine.Calculator.CartageZones.ContainsZone("Canberra Zone"));
			Assert(message, !orgRateLine.Calculator.CartageZones.ContainsZone("AU Zone"));
			Assert(message, !orgRateLine.Calculator.CartageZones.ContainsZone("Sydney Zone"));
		}

		public void TestZonesUpdateWhenPortTransportAddressIsChangedForDestinationRates()
		{
			var client = Helper.NewOrgHeader();

			var portTransportOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			portTransportOrgHeader.OH_IsConsignee = true;

			var melbourneAddress = portTransportOrgHeader.MainAddress;
			melbourneAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			melbourneAddress.OA_City = "Melbourne";
			melbourneAddress.OA_State = "VIC";
			melbourneAddress.Address1 = "123 King St";

			var canberraAddress = portTransportOrgHeader.Addresses.AddNew();
			canberraAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			canberraAddress.OA_City = "Canberra";
			canberraAddress.OA_State = "ACT";
			canberraAddress.Address1 = "25 Edinburgh Ave";

			Helper.CreateRateTransportZoneSet(client, "", Helper.GetCityTown("Sydney", "NSW"), "Sydney Zone");
			Helper.CreateRateTransportZoneSet(client, "", Helper.GetCityTown("Melbourne", "VIC"), "Melbourne Zone");
			Helper.CreateRateTransportZoneSet(client, "", Helper.GetCityTown("Canberra", "ACT"), "Canberra Zone");

			Factory.Save();

			var dstRateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "", "AUSYD");
			var dstRateLine = dstRateEntry.AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var message = "Expected the zone to be matched with CityTown(sydney) from the Destination (AUSYD)";
			Assert(message, dstRateLine.Calculator.CartageZones.ContainsZone("Sydney Zone"));
			Assert(message, !dstRateLine.Calculator.CartageZones.ContainsZone("Canberra Zone"));
			Assert(message, !dstRateLine.Calculator.CartageZones.ContainsZone("Melbourne Zone"));

			dstRateEntry.TI_OH_Consignee = portTransportOrgHeader.PK;
			dstRateEntry.TI_OA_CartageDeliveryAddressOverride = canberraAddress.PK;

			message = "Since Canberra was added as delivery address to rate entry then Sydney/Melbourne Zone no longer matches";
			Assert(message, dstRateLine.Calculator.CartageZones.ContainsZone("Canberra Zone"));
			Assert(message, !dstRateLine.Calculator.CartageZones.ContainsZone("Sydney Zone"));
			Assert(message, !dstRateLine.Calculator.CartageZones.ContainsZone("Melbourne Zone"));

			dstRateEntry.TI_OA_CartageDeliveryAddressOverride = melbourneAddress.PK;
			message = "Since Melbourne was added as delivery address to rate entry then Sydney/Canberra Zones no longer matches";
			Assert(message, dstRateLine.Calculator.CartageZones.ContainsZone("Melbourne Zone"));
			Assert(message, !dstRateLine.Calculator.CartageZones.ContainsZone("Canberra Zone"));
			Assert(message, !dstRateLine.Calculator.CartageZones.ContainsZone("Sydney Zone"));
		}

		void CreateZone(string postCode, string zoneName, string unloco)
		{
			var zone = Factory.New<RefDomesticCartageZone>();
			zone.F1_CityTownPostCode = postCode;
			zone.F1_Zone = zoneName;
			zone.F1_RL_NKLoco = unloco;
		}

		#endregion

		public void TestRevenueChargeCodesVisibleForRates()
		{
			var revCharge = Factory.New<AccChargeCode>();
			revCharge.AC_Code = "REV1";
			revCharge.AC_ChargeType = "REV";
			revCharge.AC_RateCalculator = "UNT";
			revCharge.AC_ChargeGroup = "FRT";

			var marginCharge = Factory.New<AccChargeCode>();
			marginCharge.AC_Code = "MRG1";
			marginCharge.AC_ChargeType = "MRG";
			marginCharge.AC_RateCalculator = "UNT";
			marginCharge.AC_ChargeGroup = "FRT";

			var rate = Factory.New<ClientRate>();
			var rateEntry = rate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();

			rateLine.Lookups.ChargeCodes.Load();

			Assert("Revenue charge code is available", rateLine.Lookups.ChargeCodes.Contains(revCharge));
			Assert("Margin charge code is available", rateLine.Lookups.ChargeCodes.Contains(marginCharge));
		}

		public void TestRevenueChargeCodesNotVisibleForCosts()
		{
			var revCharge = Factory.New<AccChargeCode>();
			revCharge.AC_Code = "REV1";
			revCharge.AC_ChargeType = "REV";
			revCharge.AC_RateCalculator = "UNT";
			revCharge.AC_ChargeGroup = "FRT";

			var marginCharge = Factory.New<AccChargeCode>();
			marginCharge.AC_Code = "MRG1";
			marginCharge.AC_ChargeType = "MRG";
			marginCharge.AC_RateCalculator = "UNT";
			marginCharge.AC_ChargeGroup = "FRT";

			var cost = Factory.New<Costing>();
			var costEntry = cost.AddRateEntry("AIR");
			var costLine = costEntry.RateLines.AddNew();

			costLine.Lookups.ChargeCodes.Load();

			Assert("Revenue charge code is NOT available for costing", !costLine.Lookups.ChargeCodes.Contains(revCharge));
			Assert("Margin charge code is available", costLine.Lookups.ChargeCodes.Contains(marginCharge));
		}

		public void TestCommentChargeCodesNotVisibleForQuotes()
		{
			var commentCharge = Factory.New<AccChargeCode>();
			commentCharge.AC_Code = "COM1";
			commentCharge.AC_ChargeType = "CMT";
			commentCharge.AC_RateCalculator = "NTE";
			commentCharge.AC_ChargeGroup = "FRT";

			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry("AIR");
			var rateLine = entry.RateLines.AddNew();

			rateLine.Lookups.ChargeCodes.Load();

			Assert("Comment charge code is NOT available for quote", !rateLine.Lookups.ChargeCodes.Contains(commentCharge));
		}

		#region ChargeCodes

		public void TestChargeCodes()
		{
			var globalOrgChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBORG", chargeGroup: ChargeCodeGroupList.Codes.Origin);
			var globalFrtChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT");
			Factory.Save();

			var localOrgChargeCode = globalOrgChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);
			var localFrtChargeCode = globalFrtChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);

			var localClientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var chargeCodesCollection = localClientRate.AddRateEntry(RatingConstants.RateCategory.ORG).RateLines.AddNew().Lookups.ChargeCodes;
			chargeCodesCollection.Load();
			var chargeCodePKs = chargeCodesCollection.Select(x => x.PK).ToArray();

			AssertCollectionContains("Should only include the Local ORG charge code", localOrgChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(localFrtChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(globalOrgChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(globalFrtChargeCode.PK, chargeCodePKs);

			chargeCodesCollection = localClientRate.AddRateEntry(RatingConstants.RateCategory.AIR).RateLines[0].Lookups.ChargeCodes;
			chargeCodesCollection.Load();
			chargeCodePKs = chargeCodesCollection.Select(x => x.PK).ToArray();

			AssertCollectionContains("Should only include the Local FRT charge code", localFrtChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(localOrgChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(globalFrtChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(globalOrgChargeCode.PK, chargeCodePKs);

			var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());

			chargeCodesCollection = globalClientRate.AddRateEntry(RatingConstants.RateCategory.ORG).RateLines.AddNew().Lookups.ChargeCodes;
			chargeCodesCollection.Load();
			chargeCodePKs = chargeCodesCollection.Select(x => x.PK).ToArray();

			AssertCollectionContains("Should only include the Global ORG charge code", globalOrgChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(localOrgChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(localFrtChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(globalFrtChargeCode.PK, chargeCodePKs);

			chargeCodesCollection = globalClientRate.AddRateEntry(RatingConstants.RateCategory.AIR).RateLines[0].Lookups.ChargeCodes;
			chargeCodesCollection.Load();
			chargeCodePKs = chargeCodesCollection.Select(x => x.PK).ToArray();

			AssertCollectionContains("Should only include the Global FRT charge code", globalFrtChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(localOrgChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(localFrtChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(globalOrgChargeCode.PK, chargeCodePKs);
		}

		public void TestResetChargeCodes()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR");
			var lookups = entry.RateLines[0].Lookups;
			lookups.ChargeCodes.Load();
			AssertEquals(true, lookups.ChargeCodes.Count > 0);

			entry.RateLines[0].IsBulkRateUpdateActionLine = true;
			AssertEquals("Charge code list has been reset", 0, lookups.ChargeCodes.Count);
		}

		public void TestChargeCodesForFreightInclusive_FCL()
			=> TestChargeCodesForFreightInclusive(RatingConstants.RateCategory.FCL);

		public void TestChargeCodesForFreightInclusive_CFC()
			=> TestChargeCodesForFreightInclusive(RatingConstants.RateCategory.CFC);

		void TestChargeCodesForFreightInclusive(string category)
		{
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var cafChargeCode = Helper.ChargeCodes["CAF"];
			var fscChargeCode = Helper.ChargeCodes["FSC"];

			var localClientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = localClientRate.AddRateEntry(category);
			entry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = entry.AddRateLine(bafChargeCode, FreightInclusiveCalculator.Code);
			var perCarriageOnCarriageRateLineItem1 = rateLine1.RateLineItems.AddNew();
			perCarriageOnCarriageRateLineItem1.TM_Type = FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType;
			perCarriageOnCarriageRateLineItem1.TM_AC = cafChargeCode.PK;

			var rateLine2 = entry.AddRateLine(fscChargeCode, FreightInclusiveCalculator.Code);

			var chargeCodesCollection = rateLine2.Lookups.GetChargeCodes(isFromCalculator: true);
			chargeCodesCollection.Load();
			var chargeCodePKs = chargeCodesCollection.Select(x => x.PK).ToArray();

			CombineAssertions("Charge code lists for TM_AC", () =>
			{
				AssertCollectionContains("BAF does not include a charge code.", bafChargeCode.PK, chargeCodePKs);
				AssertCollectionContains("CAF includes BAF. Leave it in the list but there should be an error in TM_AC if it is chosen", cafChargeCode.PK, chargeCodePKs);
				AssertCollectionNotContains("FSC is on parent rate line. Should not have it in the list", fscChargeCode.PK, chargeCodePKs);
			});

			chargeCodesCollection = rateLine2.Lookups.GetChargeCodes(isFromCalculator: false);
			chargeCodesCollection.Load();
			chargeCodePKs = chargeCodesCollection.Select(x => x.PK).ToArray();

			CombineAssertions("Charge code lists for TL_AC", () =>
			{
				AssertCollectionContains(bafChargeCode.PK, chargeCodePKs);
				AssertCollectionNotContains(cafChargeCode.PK, chargeCodePKs);
				AssertCollectionContains(fscChargeCode.PK, chargeCodePKs);
			});
		}

		public void TestChargeCodesForFreightInclusive_DuplicateChargeWithFreightInclusiveCalculator_Allowed()
		{
			var localClientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var cafChargeCode = Helper.ChargeCodes["CAF"];
			var fscChargeCode = Helper.ChargeCodes["FSC"];

			var entry = localClientRate.AddRateEntry(RatingConstants.RateCategory.FCL);
			entry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = entry.AddRateLine(bafChargeCode, FreightInclusiveCalculator.Code);
			var perCarriageOnCarriageRateLineItem1 = rateLine1.RateLineItems.AddNew();
			perCarriageOnCarriageRateLineItem1.TM_Type = FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType;
			perCarriageOnCarriageRateLineItem1.TM_AC = cafChargeCode.PK;

			var rateLine2 = entry.AddRateLine(bafChargeCode, FreightInclusiveCalculator.Code);

			Assert(!rateLine1.TL_ACInfo.HasErrors());
			Assert(!rateLine2.TL_ACInfo.HasErrors());

			var rateLine3 = entry.AddRateLine(bafChargeCode, FreightInclusiveCalculator.Code);
			var perCarriageOnCarriageRateLineItem3 = rateLine1.RateLineItems.AddNew();
			perCarriageOnCarriageRateLineItem3.TM_Type = FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType;
			perCarriageOnCarriageRateLineItem3.TM_AC = fscChargeCode.PK;

			Assert(!rateLine1.TL_ACInfo.HasErrors());
			Assert(!rateLine2.TL_ACInfo.HasErrors());
			Assert(!rateLine3.TL_ACInfo.HasErrors());
		}

		public void TestChargeCodesForFreightInclusive_ShouldNotExcludeChargeCodesUsedInOtherCalculators()
		{
			var localClientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var chargeCodeBAF = Helper.ChargeCodes["BAF"];
			var chargeCodeCAF = Helper.ChargeCodes["CAF"];

			var entry = localClientRate.AddRateEntry(RatingConstants.RateCategory.FCL);
			var rateLine1 = entry.AddRateLine(chargeCodeBAF, FreightInclusiveCalculator.Code);
			var perCarriageOnCarriageRateLineItem1 = rateLine1.RateLineItems.AddNew();
			perCarriageOnCarriageRateLineItem1.TM_Type = FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType;
			perCarriageOnCarriageRateLineItem1.TM_AC = chargeCodeCAF.PK;

			var rateLine2 = entry.AddRateLine(chargeCodeBAF, FlatCalculator.Code);
			var rateLine3 = entry.AddRateLine(chargeCodeCAF, FlatCalculator.Code);

			rateLine1.Validation.ValidateAll();
			rateLine2.Validation.ValidateAll();
			rateLine3.Validation.ValidateAll();

			Assert("BAF charge code can be used in line 1", !rateLine1.TL_ACInfo.HasErrors());
			Assert("BAF charge code can also be used in line 2", !rateLine2.TL_ACInfo.HasErrors());
			Assert("CAF charge code can also be used in line 3", !rateLine3.TL_ACInfo.HasErrors());
		}

		public void TestChargeCodesForFreightInclusive_NotSameEntry_ShouldNotExcludeChargeCodesUsedInFreightInclusiveCalculators()
		{
			var localClientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var chargeCodeBAF = Helper.ChargeCodes["BAF"];
			var chargeCodeCAF = Helper.ChargeCodes["CAF"];

			var entry1 = localClientRate.AddRateEntry(RatingConstants.RateCategory.FCL);
			var rateLine1 = entry1.AddRateLine(chargeCodeBAF, FreightInclusiveCalculator.Code);
			var perCarriageOnCarriageRateLineItem1 = rateLine1.RateLineItems.AddNew();
			perCarriageOnCarriageRateLineItem1.TM_Type = FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType;
			perCarriageOnCarriageRateLineItem1.TM_AC = chargeCodeCAF.PK;

			Factory.Save();

			var entry2 = localClientRate.AddRateEntry(RatingConstants.RateCategory.FCL);
			var rateLine2 = entry2.AddRateLine(chargeCodeBAF, FreightInclusiveCalculator.Code);
			var chargeCodesCollection = rateLine2.Lookups.GetChargeCodes(false);
			chargeCodesCollection.Load();
			var chargeCodePKs = chargeCodesCollection.Select(x => x.PK).ToArray();

			CombineAssertions("Should not exclude charge codes being used in FRT calculators in other entries.", () =>
			{
				AssertCollectionContains(chargeCodeBAF.PK, chargeCodePKs);
				AssertCollectionContains(chargeCodeCAF.PK, chargeCodePKs);
			});
		}

		#endregion

		public void TestRounding()
		{
			var forwardingEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.AIR);
			Assert("Chargeable rounding type is available for forwarding", forwardingEntry.RateLines[0].Lookups.Roundings.ContainsCode(RatingRoundingTypes.Chargeable));

			var landTransportChargeCode = Helper.ChargeCodes.New("TBK1", "Transport Booking 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			var landTransportEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.TBC);
			var landTransportLine = landTransportEntry.AddRateLine(landTransportChargeCode);
			Assert("Chargeable rounding type is available for land transport", landTransportLine.Lookups.Roundings.ContainsCode(RatingRoundingTypes.Chargeable));

			var cFSEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.PAC, "FCL", "", "");
			Assert("Chargeable rounding type is not available for CFS", !cFSEntry.AddRateLine("CFSSTOR", FlatCalculator.Code).Lookups.Roundings.ContainsCode(RatingRoundingTypes.Chargeable));

			var shippingEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SOR);
			Assert("Chargeable rounding type is not available for shipping", !shippingEntry.AddRateLine("CCLR", FlatCalculator.Code).Lookups.Roundings.ContainsCode(RatingRoundingTypes.Chargeable));
		}

		public void TestMessageTypeSubTypeLists()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");
				AssertMessageTypeSubTypeLists(8, 6);

				GlbCompany.CurrentCompany.SetCountry("NZ");
				AssertMessageTypeSubTypeLists(5, 8);

				GlbCompany.CurrentCompany.SetCountry("US");
				AssertMessageTypeSubTypeLists(7, 17);

				GlbCompany.CurrentCompany.SetCountry("CA");
				AssertMessageTypeSubTypeLists(8, 17);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
				{
					GlbCompany.CurrentCompany.SetCountry("CA");
					AssertMessageTypeSubTypeLists(8, 25);
				}

				GlbCompany.CurrentCompany.SetCountry("ER");
				AssertMessageTypeSubTypeLists(6, 0);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		void AssertMessageTypeSubTypeLists(int messageTypeCount, int messageSubtypeCount)
		{
			var factory = new BusinessObjectFactory();
			var helper = new TestHelper(factory);
			var line = helper.NewClientRate(helper.NewOrgHeader()).AddRateEntry("DST", "AIR", "", "").AddRateLine("CCLR", AgencyCalculator.Code);
			AssertEquals("Number of Message Types in " + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, messageTypeCount, line.Lookups.MessageTypeList.Count);

			var lineTypeItem = line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageType) ?? line.RateLineItems.AddNew();
			lineTypeItem.TM_Type = AgencyCalculator.Items.MessageType;
			lineTypeItem.TM_Text = SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Number of Message Subtypes for IMP message type in " + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, messageSubtypeCount, line.Lookups.MessageSubTypeList.Count);
		}

		public void TestWeightBreaks()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("DST", "AIR", "", "").AddRateLine("DDOC", FlatCalculator.Code);

			line.TL_RateCalculator = CombinedCalculator.Code;
			line.Calculator.UseInclusiveBreaks = true;
			AssertEquals("Less Than Or Equal To", line.Lookups.WeightBreaks.GetDescriptionFromCode("-"));
			AssertEquals("Greater Than", line.Lookups.WeightBreaks.GetDescriptionFromCode("+"));

			line.Calculator.UseInclusiveBreaks = false;
			AssertEquals("Less Than", line.Lookups.WeightBreaks.GetDescriptionFromCode("-"));
			AssertEquals("Greater Than Or Equal To", line.Lookups.WeightBreaks.GetDescriptionFromCode("+"));

			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			AssertEquals(2, line.Lookups.WeightBreaks.Count);

			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			AssertEquals(2, line.Lookups.WeightBreaks.Count);

			line.TL_RateCalculator = HighestRateCalculator.Code;
			AssertEquals(2, line.Lookups.WeightBreaks.Count);
			AssertEquals("WeightBreak list for HighestRateCalculator contains only MIN and UNT types", true, line.Lookups.WeightBreaks.ContainsOnly("MIN", "UNT"));

			line.TL_RateCalculator = ValueRangeCalculator.Code;
			AssertEquals(3, line.Lookups.WeightBreaks.Count);
			AssertEquals("WeightBreak list for ValueRangeCalculator contains only MIN and Plus and Minus types", true, line.Lookups.WeightBreaks.ContainsOnly("MIN", "+", "-"));
		}

		public void TestBreaksPer()
		{
			var rateEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR");
			var rateLine = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);

			AssertEquals("Container Type/Class", rateLine.Lookups.BreaksPer.GetDescriptionFromCode("CTT"));
			AssertEquals("Container", rateLine.Lookups.BreaksPer.GetDescriptionFromCode("CTN"));
		}

		public void TestWeightVolumes()
		{
			var rate = Factory.New<ClientRate>();
			var rateLine = rate.AddRateEntry(RatingConstants.RateCategory.FCL).RateLines.AddNew();

			AssertEquals("Cubic Inches", rateLine.Lookups.WeightVolumes.GetDescriptionFromCode("CI"));
			AssertEquals("Cubic Yards", rateLine.Lookups.WeightVolumes.GetDescriptionFromCode("CY"));
			AssertEquals("Milligrams", rateLine.Lookups.WeightVolumes.GetDescriptionFromCode("MG"));
		}

		public void TestEquipmentTypes()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var fclEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL);
			var fclLine = fclEntry.RateLines.AddNew();
			var fclLookups = new RateLinesLookups(fclLine);
			var fclEquipmentTypes = fclLookups.EquipmentTypes;
			AssertEquals("ANY, LOF, SDL, TRL, WUP", fclEquipmentTypes.CodesAsString);

			var airEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			var airLine = airEntry.RateLines.AddNew();
			var airLookups = new RateLinesLookups(airLine);
			var airEquipmentTypes = airLookups.EquipmentTypes;
			AssertEquals("ANY, HSL, HUL, HWL, PSL", airEquipmentTypes.CodesAsString);

			var bcnEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.BCN);
			var bcnLine = bcnEntry.RateLines.AddNew();
			var bcnLookups = new RateLinesLookups(bcnLine);
			var bcnEquipmentTypes = bcnLookups.EquipmentTypes;
			AssertEquals("ANY, HSL, HUL, HWL, LOF, PSL, SDL, TRL, WUP", bcnEquipmentTypes.CodesAsString);

			var dstBcnEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.BCN);
			var dstBcnLine = dstBcnEntry.RateLines.AddNew();
			var dstBcnLookups = new RateLinesLookups(dstBcnLine);
			var dstBcnEquipmentTypes = dstBcnLookups.EquipmentTypes;
			AssertEquals("ANY, HSL, HUL, HWL, LOF, PSL, SDL, TRL, WUP", dstBcnEquipmentTypes.CodesAsString);
		}

		public void TestBCNEquipmentTypesWhenRegistryIsOverriddenWithDuplicateValues()
		{
			CodeDescriptionPairList registryList = new CodeDescriptionPairList();
			registryList.AddPair("ABC", "Test ABC");
			EnvProxy.Instance.Registry.FCLEquipmentNeededList = registryList;
			registryList.AddPair("BCD", "Test BCD");
			EnvProxy.Instance.Registry.LCLAIREquipmentNeededList = registryList;
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var bcnEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.BCN);
			var bcnLine = bcnEntry.RateLines.AddNew();
			var bcnLookups = new RateLinesLookups(bcnLine);
			var bcnEquipmentTypes = bcnLookups.EquipmentTypes;
			AssertEquals("ABC, ANY, BCD, HSL, HUL, HWL, LOF, PSL, SDL, TRL, WUP", bcnEquipmentTypes.CodesAsString);
		}

		public void TestRateCalculators_CombinedWithIncrementCalculator_ShouldOnlyBeVisibleInYardEntries()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var yardCategories = new[] { "CYD", "CYM" };
			foreach (var category in RatingConstants.RateCategory.RateCategories)
			{
				var entry = clientRate.AddRateEntry(category);
				var rateLine = entry.RateLines.AddNew();
				var lookups = new RateLinesLookups(rateLine);
				var calculatorCodes = lookups.RateCalculators.Cast<CodeDescriptionPair>().Select(c => c.Code);
				if (yardCategories.Contains(category))
				{
					Assert($"{category} | Only Yard entries should have CBI calculator", calculatorCodes.Contains(RatingCalculatorCodes.CombinedWithIncrement));
					continue;
				}
				Assert($"{category} | Non-Yard entries should not have CBI calculator", !calculatorCodes.Contains(RatingCalculatorCodes.CombinedWithIncrement));
			}
		}

		#region Fees and Charges Types

		public void TestFeeChargeTypes()
		{
			var rateLine = Helper.NewCompanyTariff().AddRateEntry("DST", "AIR", "", "").AddRateLine("DDOC", FlatCalculator.Code);
			rateLine.TL_CompanyTariffLevel = 1;
			var types = rateLine.Lookups.FeeChargeTypes;

			AssertNotNull(types);
			AssertEquals("Expected to default to 3 default registry types", 3, types.Count);
			AssertNotNull("Expected the look up collection to contain item from registry", types[ServiceTypeFromRegistry.Code]);
			AssertEquals(ServiceTypeFromRegistry.Description, types[ServiceTypeFromRegistry.Code].Description);

			var registrySection = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value;
			var newType = registrySection.FeeChargeTypes.AddNew();
			newType.Code = "NEW";
			newType.EnglishDescription = "New Type";

			OrganisationsDataRegistry.Instance.RateFeeChargeLevels.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySection);

			types = rateLine.Lookups.FeeChargeTypes;
			AssertEquals("Expected to include newly added type", 4, types.Count);
			AssertNotNull("Expected the look up collection to contain the new item", types[newType.Code]);
			AssertEquals(newType.Description, types[newType.Code].Description);
		}

		public void TestFeeChargeLevels()
		{
			var rateLine = Helper.NewCompanyTariff().AddRateEntry("DST", "AIR", "", "").AddRateLine("DDOC", FlatCalculator.Code);
			rateLine.TL_CompanyTariffLevel = 1;
			var types = rateLine.Lookups.FeeChargeLevels;

			AssertNotNull(types);

			AssertEquals("Expected no level as type has not been set", 0, rateLine.Lookups.FeeChargeLevels.Count);

			rateLine.TL_FeeChargeType = ServiceTypeFromRegistry.Code;

			AssertEquals("Expected default level", 1, rateLine.Lookups.FeeChargeLevels.Count);

			rateLine.TL_FeeChargeType = "XXX";

			AssertEquals("Expected no level as type is not valid", 0, rateLine.Lookups.FeeChargeLevels.Count);

			var registrySection = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value;
			var type = registrySection.FeeChargeTypes.AddNew();
			type.Code = "ZZZ";
			type.EnglishDescription = "ZZZ Type";
			var xLevel = type.FeeChargeLevels.AddNew();
			xLevel.Code = "XXX";
			xLevel.EnglishDescription = "XXX Level";
			xLevel.Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.None;
			xLevel.Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.None;
			var yLevel = type.FeeChargeLevels.AddNew();
			yLevel.Code = "YYY";
			yLevel.EnglishDescription = "YYY Level";
			yLevel.Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.None;
			yLevel.Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.None;

			OrganisationsDataRegistry.Instance.RateFeeChargeLevels.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySection);

			rateLine.TL_FeeChargeType = "ZZZ";

			AssertEquals("Expected x and y levels to be added the new ZZZ type", 2, rateLine.Lookups.FeeChargeLevels.Count);
			AssertEquals("XXX", rateLine.Lookups.FeeChargeLevels[0].Code);
			AssertEquals("XXX Level", rateLine.Lookups.FeeChargeLevels[0].Description);
			AssertEquals("YYY", rateLine.Lookups.FeeChargeLevels[1].Code);
			AssertEquals("YYY Level", rateLine.Lookups.FeeChargeLevels[1].Description);
		}

		static FeeChargeType ServiceTypeFromRegistry
		{
			get
			{
				var registrySetting = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (registrySetting != null)
				{
					return registrySetting.FeeChargeTypes.FirstOrDefault() as FeeChargeType;
				}

				return null;
			}
		}

		#endregion

		#region Implementation

		RateTransportProvider supplierRateTransportProvider;
		RateTransportProvider clientRateTransportProvider;
		RateTransportZone zone1;
		RateTransportZone zone2;
		RateTransportZone zone3;
		RateTransportZone zone4;
		RateTransportZone zone5;
		RateTransportZone zone6;
		RateTransportZone zone7;
		RateTransportZone zone8;
		OrgHeader supplier1;
		OrgHeader supplier2;

		void SetUpZones()
		{
			supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "TEST1";
			supplier1.OH_FullName = "TEST ORG 1";
			supplier1.MainAddress.OA_Address1 = "Some Street";

			supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_Code = "TEST2";
			supplier2.OH_FullName = "TEST ORG 2";
			supplier2.MainAddress.OA_Address1 = "Some Other Street";

			var set1 = Helper.CreateRateTransportZoneSet(supplier1, CountryCodes.India);
			zone1 = set1.CreateRateTransportZoneForTest("Zone1");
			zone2 = set1.CreateRateTransportZoneForTest("Zone2");
			var set2 = Helper.CreateRateTransportZoneSet(supplier1, CountryCodes.UnitedStates);
			zone3 = set2.CreateRateTransportZoneForTest("Zone3");
			zone4 = set2.CreateRateTransportZoneForTest("Zone4");
			var set3 = Helper.CreateRateTransportZoneSet(supplier2, CountryCodes.UnitedStates);
			zone5 = set3.CreateRateTransportZoneForTest("Zone5");
			zone6 = set3.CreateRateTransportZoneForTest("Zone6");
			var set4 = Helper.CreateRateTransportZoneSet(null, CountryCodes.India);
			zone7 = set4.CreateRateTransportZoneForTest("Zone7");
			zone8 = set4.CreateRateTransportZoneForTest("Zone8");

			Factory.Save();
		}

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}

	[TestedType(typeof(GlobalChargeCodesCollection))]
	public class GlobalChargeCodesCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlobalChargeCodesCollection(Factory);
		}
	}
}
