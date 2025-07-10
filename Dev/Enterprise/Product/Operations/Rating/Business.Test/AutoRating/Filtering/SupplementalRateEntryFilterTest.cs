using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class SupplementalRateEntryFilterTest : RatingTestCase
	{
		#region Aircraft Type

		#region Cargo Aircraft

		public void TestAircraftType_CargoAircraftOnly_FreightCharges_ClientRate()
			=> TestAircraftType_CargoAircraftOnly_FreightCharges(Helper.NewClientRate(NewClient));

		public void TestAircraftType_CargoAircraftOnly_FreightCharges_Costing()
			=> TestAircraftType_CargoAircraftOnly_FreightCharges(Helper.NewCosting(NewClient));

		void TestAircraftType_CargoAircraftOnly_FreightCharges(RatingHeader ratingHeader)
		{
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "").TI_AircraftType = AircraftType.CAO;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "").TI_AircraftType = AircraftType.PAX;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "").TI_AircraftType = "";

			CreateCriteriaAndAssertFindRateEntries
			(
				"AUSYD",
				"USLAX",
				AircraftType.CAO,
				ratingHeader,
				expectedRateEntries: new[]
				{
					"{AIR}-{LSE}-{AUSYD}-{}-{CAO}",
					"{AIR}-{LSE}-{AUSYD}-{}-{}"
				}
			);
		}

		public void TestAircraftType_CargoAircraftOnly_OriginCharges_ClientRate()
			=> TestAircraftType_CargoAircraftOnly_OriginCharges(Helper.NewClientRate(NewClient));

		public void TestAircraftType_CargoAircraftOnly_OriginCharges_Costing()
			=> TestAircraftType_CargoAircraftOnly_OriginCharges(Helper.NewCosting(NewClient));

		void TestAircraftType_CargoAircraftOnly_OriginCharges(RatingHeader ratingHeader)
		{
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "").TI_AircraftType = AircraftType.CAO;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "").TI_AircraftType = AircraftType.PAX;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "").TI_AircraftType = "";

			CreateCriteriaAndAssertFindRateEntries
			(
				"AUSYD",
				"USLAX",
				AircraftType.CAO,
				ratingHeader,
				expectedRateEntries: new[]
				{
					"{ORG}-{AIR}-{AUSYD}-{}-{CAO}",
					"{ORG}-{AIR}-{AUSYD}-{}-{}"
				}
			);
		}

		public void TestAircraftType_CargoAircraftOnly_DestinationCharges_ClientRate()
			=> TestAircraftType_CargoAircraftOnly_DestinationCharges(Helper.NewClientRate(NewClient));

		public void TestAircraftType_CargoAircraftOnly_DestinationCharges_Costing()
			=> TestAircraftType_CargoAircraftOnly_DestinationCharges(Helper.NewCosting(NewClient));

		void TestAircraftType_CargoAircraftOnly_DestinationCharges(RatingHeader ratingHeader)
		{
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD").TI_AircraftType = AircraftType.CAO;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD").TI_AircraftType = AircraftType.PAX;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD").TI_AircraftType = "";

			CreateCriteriaAndAssertFindRateEntries
			(
				"USLAX",
				"AUSYD",
				AircraftType.CAO,
				ratingHeader,
				expectedRateEntries: new[]
				{
					"{DST}-{AIR}-{}-{AUSYD}-{CAO}",
					"{DST}-{AIR}-{}-{AUSYD}-{}"
				}
			);
		}

		#endregion

		#region Passenger and Cargo

		public void TestAircraftType_PassengerAndCargo_FreightCharges_ClientRate()
			=> TestAircraftType_PassengerAndCargo_FreightCharges(Helper.NewClientRate(NewClient));

		public void TestAircraftType_PassengerAndCargo_FreightCharges_Costing()
			=> TestAircraftType_PassengerAndCargo_FreightCharges(Helper.NewCosting(NewClient));

		void TestAircraftType_PassengerAndCargo_FreightCharges(RatingHeader ratingHeader)
		{
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "").TI_AircraftType = AircraftType.CAO;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "").TI_AircraftType = AircraftType.PAX;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "").TI_AircraftType = "";

			CreateCriteriaAndAssertFindRateEntries
			(
				"AUSYD",
				"USLAX",
				AircraftType.PAX,
				ratingHeader,
				expectedRateEntries: new[]
				{
					"{AIR}-{LSE}-{AUSYD}-{}-{PAX}",
					"{AIR}-{LSE}-{AUSYD}-{}-{}"
				}
			);
		}

		public void TestAircraftType_PassengerAndCargo_OriginCharges_ClientRate()
			=> TestAircraftType_PassengerAndCargo_OriginCharges(Helper.NewClientRate(NewClient));

		public void TestAircraftType_PassengerAndCargo_OriginCharges_Costing()
			=> TestAircraftType_PassengerAndCargo_OriginCharges(Helper.NewCosting(NewClient));

		void TestAircraftType_PassengerAndCargo_OriginCharges(RatingHeader ratingHeader)
		{
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "").TI_AircraftType = AircraftType.CAO;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "").TI_AircraftType = AircraftType.PAX;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "").TI_AircraftType = "";

			CreateCriteriaAndAssertFindRateEntries
			(
				"AUSYD",
				"USLAX",
				AircraftType.PAX,
				ratingHeader,
				expectedRateEntries: new[]
				{
					"{ORG}-{AIR}-{AUSYD}-{}-{PAX}",
					"{ORG}-{AIR}-{AUSYD}-{}-{}"
				}
			);
		}

		public void TestAircraftType_PassengerAndCargo_DestinationCharges_ClientRate()
			=> TestAircraftType_PassengerAndCargo_DestinationCharges(Helper.NewClientRate(NewClient));

		public void TestAircraftType_PassengerAndCargo_DestinationCharges_Costing()
			=> TestAircraftType_PassengerAndCargo_DestinationCharges(Helper.NewCosting(NewClient));

		void TestAircraftType_PassengerAndCargo_DestinationCharges(RatingHeader ratingHeader)
		{
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD").TI_AircraftType = AircraftType.CAO;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD").TI_AircraftType = AircraftType.PAX;
			ratingHeader.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD").TI_AircraftType = "";

			CreateCriteriaAndAssertFindRateEntries
			(
				"USLAX",
				"AUSYD",
				AircraftType.PAX,
				ratingHeader,
				expectedRateEntries: new[]
				{
					"{DST}-{AIR}-{}-{AUSYD}-{PAX}",
					"{DST}-{AIR}-{}-{AUSYD}-{}"
				}
			);
		}

		#endregion

		void CreateCriteriaAndAssertFindRateEntries(string origin, string destination, string aircraftType, IRatingHeader ratingHeader, IEnumerable<string> expectedRateEntries)
		{
			var ratingHeaders = new List<IRatingHeader>();
			ratingHeaders.Add(ratingHeader);

			var criteria = new TestRatingCriteria(origin, destination, FreightMode.LSE, 10m, 100m, NewClient);
			criteria.AircraftType = aircraftType;

			var results = Filter(ratingHeaders, criteria, false, Factory);
			AssertContainsExactElementsInAnyOrder
			(
				$"Criteria {aircraftType} should only match rate with {aircraftType}",
				expectedRateEntries,
				results.Select(result => GetDescriptionForAircraftTest((RateEntry)result))
			);
		}

		static string GetDescriptionForAircraftTest(RateEntry entry)
		{
			var sb = new ZStringBuilder();
			sb.Append(entry.TI_RateCategory);
			sb.Append(entry.TI_Mode);
			sb.Append(entry.TI_OriginLRC);
			sb.Append(entry.TI_DestinationLRC);
			sb.Append(entry.TI_AircraftType);
			return "{" + sb.ToStringWithDelimiterBetweenAppends("}-{") + "}";
		}

		#endregion

		#region Cross-Trade

		public void TestCrossTradeSupplementalCharges()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("INBOM", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "", "");
			entry1.TI_IsCrossTrade = true;

			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "INBOM", "");

			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");

			var entry4 = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");

			var entry5 = clientRate.AddRateEntry("DST", "AIR", "", "");
			entry5.TI_IsCrossTrade = true;

			var entry6 = clientRate.AddRateEntry("DST", "AIR", "", "AUMEL");

			var results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("4 items found - cross trade entries and standard entries", 4, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry4", true, results.Contains(entry4));
			AssertEquals("Entry5", true, results.Contains(entry5));
		}

		#endregion

		#region Consortium

		public void TestWithSupplierIsMemberOfConsortium()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var consortiumOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var consortiumOrgProxy2 = Factory.NewWithValidTestData<OrgHeader>();
			var unknownOrg = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Carrier = shippingLine;
			testCriteria.SetSupplier(shippingLine);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_OH_Supplier = consortiumOrgProxy.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry1, "ODOC", 20m);

			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry2.TI_OH_Supplier = unknownOrg.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry2, "ODOC", 25m);

			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry3.TI_OH_Supplier = consortiumOrgProxy2.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry3, "ODOC", 30m);

			var results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("No results found as ShippingLine is not part of the Consortium", 0, results.Count);

			var consortium = Factory.New<RefCarrierConsortium>();
			consortium.RG_Code = "ABC";
			consortium.RG_OH = consortiumOrgProxy.PK;
			consortium.OrgHeaders.Add(shippingLine);

			var consortium2 = Factory.New<RefCarrierConsortium>();
			consortium2.RG_Code = "XYZ";
			consortium2.RG_OH = consortiumOrgProxy2.PK;
			consortium2.OrgHeaders.Add(shippingLine);
			Factory.Save();

			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Carrier = shippingLine;
			testCriteria.SetSupplier(shippingLine);
			results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("2 items found as ShippingLine is now part of both consortiums", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry3", true, results.Contains(entry3));
		}

		public void TestWithTransportProviderIsMemberOfConsortium()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var consortiumOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var consortiumOrgProxy2 = Factory.NewWithValidTestData<OrgHeader>();
			var unknownOrg = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.SetSupplier(shippingLine);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_OH_Supplier = consortiumOrgProxy.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry1, "ODOC", 20m);

			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry2.TI_OH_Supplier = consortiumOrgProxy2.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry2, "ODOC", 25m);

			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry3.TI_OH_Supplier = unknownOrg.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry3, "ODOC", 30m);

			var results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("No results found as ShippingLine is not part of the Consortium", 0, results.Count);

			var consortium = Factory.New<RefCarrierConsortium>();
			consortium.RG_Code = "ABC";
			consortium.RG_OH = consortiumOrgProxy.PK;
			consortium.OrgHeaders.Add(shippingLine);

			var consortium2 = Factory.New<RefCarrierConsortium>();
			consortium2.RG_Code = "DEF";
			consortium2.RG_OH = consortiumOrgProxy2.PK;
			consortium2.OrgHeaders.Add(shippingLine);

			Factory.Save();

			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.SetSupplier(shippingLine);
			results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("2 items found as ShippingLine is now part of both consortiums", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
		}

		#endregion

		#region Consignee / Consignor Address

		public void TestConsignorWithoutAddress()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var collection = new[] { clientRate };

			var ratingCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			ratingCriteria.Consignor = Consignor;
			ratingCriteria.PickupAddress = Consignor.MainAddress;

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_OH_Consignor = Consignor.PK;
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry2.TI_OH_Consignor = Helper.NewOrgHeader().PK;
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");

			var results = Filter(collection, ratingCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Should match by Consignor org", true, results.Contains(entry1));
			AssertEquals("Has no consignor requirement", true, results.Contains(entry3));

			ratingCriteria.Consignor = null;
			results = Filter(collection, ratingCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Should still try to match by pickup address", true, results.Contains(entry1));
			AssertEquals("Has no consignor requirement", true, results.Contains(entry3));

			ratingCriteria.PickupAddress = null;
			results = Filter(collection, ratingCriteria, false, Factory);

			AssertEquals("Can no longer match this entry to any criteria fields", false, results.Contains(entry1));
			AssertEquals("Has no consignee requirement", true, results.Contains(entry3));
		}

		public void TestConsigneeWithoutAddress()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var collection = new[] { clientRate };

			var ratingCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			ratingCriteria.Consignee = Consignee;
			ratingCriteria.DeliveryAddress = Consignee.MainAddress;

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.AIR, "", "USLAX");
			entry1.TI_OH_Consignee = Consignee.PK;
			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.AIR, "", "USLAX");
			entry2.TI_OH_Consignee = Helper.NewOrgHeader().PK;
			var entry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.AIR, "", "USLAX");

			var results = Filter(collection, ratingCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Should match by Consignee org", true, results.Contains(entry1));
			AssertEquals("Has no consignee requirement", true, results.Contains(entry3));

			ratingCriteria.Consignee = null;
			results = Filter(collection, ratingCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Should still try to match by delivery address", true, results.Contains(entry1));
			AssertEquals("Has no consignee requirement", true, results.Contains(entry3));

			ratingCriteria.DeliveryAddress = null;
			results = Filter(collection, ratingCriteria, false, Factory);

			AssertEquals("Can no longer match this entry to any criteria fields", false, results.Contains(entry1));
			AssertEquals("Has no consignee requirement", true, results.Contains(entry3));
		}

		public void TestCartagePickupAddressPostcode()
		{
			Consignor.MainAddress.OA_PostCode = "2127";

			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Consignor = Consignor;
			testCriteria.PickupAddress = Consignor.MainAddress;
			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_CartagePickupAddressPostCode = "2127";
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry2.TI_CartagePickupAddressPostCode = "2000";
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry3", true, results.Contains(entry3));

			testCriteria.PickupAddress = null;
			results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 1, results.Count);
			AssertEquals("Entry3", true, results.Contains(entry3));

			Consignor.MainAddress.OA_PostCode = "2000";
			testCriteria.PickupAddress = Consignor.MainAddress;
			results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry3", true, results.Contains(entry3));
		}

		public void TestCartageDeliveryAddressPostcode()
		{
			Consignee.MainAddress.OA_PostCode = "2127";

			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Consignee = Consignee;
			testCriteria.DeliveryAddress = Consignee.MainAddress;
			var entry1 = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");
			entry1.TI_CartageDeliveryAddressPostCode = "2127";
			var entry2 = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");
			entry2.TI_CartageDeliveryAddressPostCode = "2000";
			var entry3 = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry3", true, results.Contains(entry3));

			testCriteria.DeliveryAddress = null;
			results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 1, results.Count);
			AssertEquals("Entry3", true, results.Contains(entry3));

			Consignee.MainAddress.OA_PostCode = "2000";
			testCriteria.DeliveryAddress = Consignee.MainAddress;
			results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry3", true, results.Contains(entry3));
		}

		#endregion

		#region Fallback

		public void TestAirModeFallBackToGenericAir()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "INBOM", "");
			var entry2 = clientRate.AddRateEntry("ORG", "LSE", "INBOM", "");
			var entry3 = clientRate.AddRateEntry("ORG", "ULD", "INBOM", "");

			var testCriteria = new TestRatingCriteria("INBOM", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("2 items found - generic AIR included but ULD not included", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));

			testCriteria = new TestRatingCriteria("INBOM", "USLAX", FreightMode.ULD, 0M, 0M, NewClient);

			results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("2 items found - generic AIR included but LSE not included", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry3", true, results.Contains(entry3));
		}

		public void TestWithGlobalFallbackButNoRegionCountryFallback()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var entry2 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			globalRate.AddRateEntry("ORG", "AIR", "AUMEL", "");
			var entry4 = globalRate.AddRateEntry("ORG", "ALL", "AU", "");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 3, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry4", true, results.Contains(entry4));
		}

		public void TestWithGlobalAndRegionCountryPriorityFallback()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var entry1 = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");
			var entry2 = clientRate.AddRateEntry("DST", "AIR", "", "USCA");
			var entry3 = clientRate.AddRateEntry("DST", "AIR", "", "US");
			clientRate.AddRateEntry("DST", "AIR", "", "USSFO");
			var entry5 = globalRate.AddRateEntry("DST", "AIR", "", "USLAX");
			globalRate.AddRateEntry("DST", "AIR", "", "USSFO");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 4, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry3", true, results.Contains(entry3));
			AssertEquals("Entry5", true, results.Contains(entry5));
		}

		public void TestWithPickupAddressAndPostCodeFallback()
		{
			var address = Consignor.MainAddress;
			address.OA_Address1 = "address1";
			address.OA_Code = "Hello";
			address.OA_PostCode = "2117";

			var address2 = Consignor.Addresses.AddNew();
			address2.OA_Address1 = "address2";
			address2.OA_Code = "AndAgain";
			address2.OA_PostCode = "2115";
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.PickupAndDelivery.Code);

			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Consignor = Consignor;
			testCriteria.PickupAddress = address;
			Factory.Save();
			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_OH_Consignor = Consignor.PK;
			entry1.TI_OA_CartagePickupAddressOverride = address.PK;
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry2.TI_OH_Consignor = Consignor.PK;
			entry2.TI_CartagePickupAddressPostCode = "2148";
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry3.TI_OH_Consignor = Consignor.PK;
			entry3.TI_CartagePickupAddressPostCode = "2117";
			var entry4 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry4.TI_OH_Consignor = Consignor.PK;
			entry4.TI_OA_CartagePickupAddressOverride = address2.PK;

			var results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry3", true, results.Contains(entry3));
		}

		public void TestWithDeliveryAddressAndPostCodeFallback()
		{
			var address = Consignee.MainAddress;
			address.OA_Address1 = "Address1";
			address.OA_Code = "Hello";
			address.OA_PostCode = "2117";

			var address2 = Consignee.Addresses.AddNew();
			address2.OA_Address1 = "address2";
			address2.OA_Code = "AndAgain";
			address2.OA_PostCode = "2115";
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.PickupAndDelivery.Code);

			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Consignee = Consignee;
			testCriteria.DeliveryAddress = address;
			Factory.Save();

			var entry1 = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");
			entry1.TI_OH_Consignee = Consignee.PK;
			entry1.TI_OA_CartageDeliveryAddressOverride = address.PK;

			var entry2 = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");
			entry2.TI_OH_Consignee = Consignee.PK;
			entry2.TI_CartageDeliveryAddressPostCode = "2148";

			var entry3 = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");
			entry3.TI_OH_Consignee = Consignee.PK;
			entry3.TI_CartageDeliveryAddressPostCode = "2117";

			var entry4 = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");
			entry4.TI_OH_Consignee = Consignee.PK;
			entry4.TI_OA_CartageDeliveryAddressOverride = address2.PK;

			var results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry3", true, results.Contains(entry3));
		}

		public void TestWithInvalidDate()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Consignor = Consignor;

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_RateStartDate = ZDate.Today.AddDays(-30);
			entry1.TI_RateEndDate = ZDate.Today.AddDays(-1);
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUEC", "");
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AU", "");
			clientRate.AddRateEntry("ORG", "AIR", "AUMEL", "");
			var entry5 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			globalRate.AddRateEntry("ORG", "AIR", "AUMEL", "");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 3, results.Count);
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry3", true, results.Contains(entry3));
			AssertEquals("Entry5", true, results.Contains(entry5));
		}

		public void TestWithNoSupplier()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Consignor = Consignor;

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUEC", "");
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AU", "");
			clientRate.AddRateEntry("ORG", "AIR", "AUMEL", "");
			var entry5 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			globalRate.AddRateEntry("ORG", "AIR", "AUMEL", "");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 4, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry3", true, results.Contains(entry3));
			AssertEquals("Entry5", true, results.Contains(entry5));
		}

		public void TestWithTranshipment()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.SetVia(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN"));
			testCriteria.Consignor = Consignor;

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_ViaLRC = "INBOM";
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUEC", "");
			entry2.TI_ViaLRC = "SGSIN";
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AU", "");
			var entry4 = clientRate.AddRateEntry("ORG", "AIR", "AUMEL", "");
			entry4.TI_ViaLRC = "SGSIN";
			var entry5 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry5.TI_ViaLRC = "SGSIN";
			globalRate.AddRateEntry("ORG", "AIR", "AUMEL", "");
			var entry7 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry7.TI_ViaLRC = "HKHKG";

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 3, results.Count);
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry3", true, results.Contains(entry3));
			AssertEquals("Entry5", true, results.Contains(entry5));
		}

		public void TestWithContainers()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP20, NewClient);
			testCriteria.Consignor = Consignor;

			var entry1 = clientRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			var entry2 = clientRate.AddRateEntry("ORG", "FCL", "AUEC", "", "", "20GP");
			var entry3 = clientRate.AddRateEntry("ORG", "FCL", "AU", "", "", "20GP");
			clientRate.AddRateEntry("ORG", "FCL", "AUMEL", "", "", "20GP");
			var entry5 = globalRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			globalRate.AddRateEntry("ORG", "FCL", "AUMEL", "", "", "20GP");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 4, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry3", true, results.Contains(entry3));
			AssertEquals("Entry5", true, results.Contains(entry5));
		}

		public void TestWithContainersInSameContainerClass()
		{
			var container1 = Factory.New<RefContainer>();
			container1.RC_HandlingRateClass = "ZUB";
			container1.RC_Code = "Z1";

			var container2 = Factory.New<RefContainer>();
			container2.RC_HandlingRateClass = "ZUB";
			container2.RC_Code = "Z2";

			var container3 = Factory.New<RefContainer>();
			container3.RC_Code = "Z3";

			Factory.Save();

			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, container1, NewClient);

			var entry1 = clientRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", container1.RC_Code);
			entry1.TI_MatchContainerRateClass = true;
			var entry2 = clientRate.AddRateEntry("ORG", "FCL", "AUEC", "", "", container2.RC_Code);
			entry2.TI_MatchContainerRateClass = true;
			var entry3 = clientRate.AddRateEntry("ORG", "FCL", "AU", "", "", container1.RC_Code);
			entry3.TI_MatchContainerRateClass = true;
			var entry4 = globalRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", container2.RC_Code);
			entry4.TI_MatchContainerRateClass = true;
			var entry5 = globalRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", container3.RC_Code);
			entry5.TI_MatchContainerRateClass = true;
			var entry6 = globalRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "");
			clientRate.SummaryRateEntries.Load();
			globalRate.SummaryRateEntries.Load();

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 5, results.Count);
			AssertEquals("Entry1 - Same container", true, results.Contains(entry1));
			AssertEquals("Entry2 - Fiff container, same class", true, results.Contains(entry2));
			AssertEquals("Entry3 - Same container", true, results.Contains(entry3));
			AssertEquals("Entry4 - Diff conatiner, same class", true, results.Contains(entry4));
			AssertEquals("Entry6 - No container (generic)", true, results.Contains(entry6));
		}

		public void TestWithFCLAndNoContainers()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP20, NewClient);
			testCriteria.Consignor = Consignor;

			var entry1 = clientRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			var entry2 = clientRate.AddRateEntry("ORG", "FCL", "AUEC", "", "", "20GP");
			var entry3 = clientRate.AddRateEntry("ORG", "FCL", "AU", "", "", "20GP");
			clientRate.AddRateEntry("ORG", "FCL", "AUMEL", "", "", "20GP");
			var entry5 = globalRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			globalRate.AddRateEntry("ORG", "FCL", "AUMEL", "", "", "20GP");
			var entry7 = globalRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 5, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry3", true, results.Contains(entry3));
			AssertEquals("Entry5", true, results.Contains(entry5));
			AssertEquals("Entry7", true, results.Contains(entry7));
		}

		public void TestWithOriginAndDestinationCharges()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP20, NewClient);

			var entry1 = clientRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			var entry2 = clientRate.AddRateEntry("ORG", "FCL", "AUEC", "", "", "20GP");
			var entry3 = clientRate.AddRateEntry("ORG", "FCL", "AU", "", "", "20GP");
			clientRate.AddRateEntry("ORG", "FCL", "AUMEL", "", "", "20GP");
			var entry5 = globalRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			globalRate.AddRateEntry("ORG", "FCL", "AUMEL", "", "", "20GP");
			var entry7 = clientRate.AddRateEntry("DST", "FCL", "", "USLAX", "", "20GP");
			var entry8 = clientRate.AddRateEntry("DST", "FCL", "", "USCA", "", "20GP");
			var entry9 = clientRate.AddRateEntry("DST", "FCL", "", "US", "", "20GP");
			clientRate.AddRateEntry("DST", "FCL", "", "USSFO", "", "20GP");
			var entry11 = globalRate.AddRateEntry("DST", "FCL", "", "USLAX", "", "20GP");
			globalRate.AddRateEntry("DST", "FCL", "", "USSFO", "", "20GP");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 8, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry3", true, results.Contains(entry3));
			AssertEquals("Entry5", true, results.Contains(entry5));
			AssertEquals("Entry7", true, results.Contains(entry7));
			AssertEquals("Entry8", true, results.Contains(entry8));
			AssertEquals("Entry9", true, results.Contains(entry9));
			AssertEquals("Entry11", true, results.Contains(entry11));
		}

		public void TestWithALLMode()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var entry2 = clientRate.AddRateEntry("ORG", "ALL", "AUSYD", "");
			clientRate.AddRateEntry("ORG", "AIR", "AUMEL", "");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
		}

		public void TestWithSEAMode()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LCL, 15M, 1M, NewClient);

			clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var entry2 = clientRate.AddRateEntry("ORG", "ALL", "AUSYD", "");
			var entry3 = clientRate.AddRateEntry("ORG", "SEA", "AUSYD", "");
			var entry4 = clientRate.AddRateEntry("ORG", "LCL", "AUSYD", "");
			clientRate.AddRateEntry("ORG", "AIR", "AUMEL", "");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 3, results.Count);
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry3", true, results.Contains(entry3));
			AssertEquals("Entry4", true, results.Contains(entry4));
		}

		public void TestDestinationChargeWithOrigin()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var entry1 = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");
			var entry2 = globalRate.AddRateEntry("DST", "AIR", "", "USLAX");
			var entry3 = globalRate.AddRateEntry("DST", "AIR", "", "USSFO");
			var entry4 = clientRate.AddRateEntry("DST", "AIR", "AUSYD", "USLAX");
			var entry5 = clientRate.AddRateEntry("DST", "AIR", "GBLON", "USLAX");
			var entry6 = globalRate.AddRateEntry("DST", "AIR", "AUEC", "USLAX");
			var entry7 = globalRate.AddRateEntry("DST", "AIR", "GBLON", "USLAX");
			var entry8 = clientRate.AddRateEntry("DST", "AIR", "AU", "USLAX");
			var entry9 = globalRate.AddRateEntry("DST", "AIR", "AU", "USLAX");
			var entry10 = globalRate.AddRateEntry("DST", "AIR", "NZ", "USLAX");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 6, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry4", true, results.Contains(entry4));
			AssertEquals("Entry6", true, results.Contains(entry6));
			AssertEquals("Entry8", true, results.Contains(entry8));
			AssertEquals("Entry9", true, results.Contains(entry9));
		}

		public void TestOriginChargeWithDestination()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var entry2 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var entry3 = globalRate.AddRateEntry("ORG", "AIR", "AUMEL", "");
			var entry4 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var entry5 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "GBLON");
			var entry6 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "USCA");
			var entry7 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "GBLON");
			var entry8 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "US");
			var entry9 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "GB");
			var entry10 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "NZ");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 5, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry4", true, results.Contains(entry4));
			AssertEquals("Entry6", true, results.Contains(entry6));
			AssertEquals("Entry8", true, results.Contains(entry8));
		}

		public void TestWithTransportProvider()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			var globalRate = Helper.NewCompanyTariff();
			collection.Add(clientRate);
			collection.Add(globalRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Carrier = TransportProvider1;

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_OH_TransportProvider = TransportProvider1.PK;
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUEC", "");
			entry2.TI_OH_TransportProvider = TransportProvider2.PK;
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AU", "");
			var entry4 = clientRate.AddRateEntry("ORG", "AIR", "AUMEL", "");

			var entry5 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var entry6 = globalRate.AddRateEntry("ORG", "AIR", "AUMEL", "");
			var entry7 = globalRate.AddRateEntry("ORG", "AIR", "AUEC", "");
			entry7.TI_OH_TransportProvider = TransportProvider1.PK;
			var entry8 = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry8.TI_OH_TransportProvider = TransportProvider2.PK;

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 4, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry3", true, results.Contains(entry3));
			AssertEquals("Entry5", true, results.Contains(entry5));
			AssertEquals("Entry7", true, results.Contains(entry7));
		}

		public void TestWithSupplier()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Carrier = TransportProvider1;
			testCriteria.SetSupplier(TransportProvider1);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_OH_Supplier = TransportProvider1.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry1, "ODOC", 20m);
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUEC", "");
			entry2.TI_OH_Supplier = TransportProvider1.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry2, "ODOC", 25m);
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AU", "");
			entry3.TI_OH_Supplier = TransportProvider2.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry3, "ODOC", 75m);
			var entry4 = clientRate.AddRateEntry("ORG", "AIR", "AU", "");
			entry4.TI_OH_Supplier = TransportProvider1.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry4, "ODOC", 45m);

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 3, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry4", true, results.Contains(entry4));
		}

		public void TestWithConsignorWithPickupAddress()
		{
			var consignor = Consignor;
			var pickupAddress = consignor.Addresses.AddNew();
			pickupAddress.OA_Code = "PICKUP";
			pickupAddress.OA_Address1 = "123 King St";

			var wrongAddress = consignor.Addresses.AddNew();
			wrongAddress.OA_Code = "WRONG";
			wrongAddress.OA_Address1 = "456 Coward St";

			var wrongConsignor = Helper.NewOrgHeader();
			wrongConsignor.OH_IsConsignor = true;
			Factory.Save();

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP40, NewClient);
			testCriteria.Consignor = consignor;
			testCriteria.PickupAddress = pickupAddress;

			#region Setup Client Rate

			var clientRate = Helper.NewClientRate(NewClient);

			var consignorRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "AU", "");
			consignorRateEntry.TI_OH_Consignor = consignor.PK;

			var wrongConsignorRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "AU", "");
			wrongConsignorRateEntry.TI_OH_Consignor = wrongConsignor.PK;

			var consignorAndPickupRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "AU", "");
			consignorAndPickupRateEntry.TI_OH_Consignor = consignor.PK;
			consignorAndPickupRateEntry.TI_OA_CartagePickupAddressOverride = pickupAddress.PK;

			var wrongPickupRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "AU", "");
			wrongPickupRateEntry.TI_OH_Consignor = Consignor.PK;
			wrongPickupRateEntry.TI_OA_CartagePickupAddressOverride = wrongAddress.PK;

			var wrongConsignorAndPickupRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "AU", "");
			wrongConsignorAndPickupRateEntry.TI_OH_Consignor = wrongConsignor.PK;
			wrongConsignorAndPickupRateEntry.TI_OA_CartagePickupAddressOverride = wrongConsignor.MainAddress.PK;

			#endregion

			#region Setup Tariff

			var companyTariff = Helper.NewCompanyTariff();

			var consignorTariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "AU", "");
			consignorTariffEntry.TI_OH_Consignor = consignor.PK;

			var wrongConsignorTariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "AU", "");
			wrongConsignorTariffEntry.TI_OH_Consignor = wrongConsignor.PK;

			var consignorAndPickupTariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "AU", "");
			consignorAndPickupTariffEntry.TI_OH_Consignor = consignor.PK;
			consignorAndPickupTariffEntry.TI_OA_CartagePickupAddressOverride = pickupAddress.PK;

			var wrongPickupTariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "AU", "");
			wrongPickupTariffEntry.TI_OH_Consignor = consignor.PK;
			wrongPickupTariffEntry.TI_OA_CartagePickupAddressOverride = wrongAddress.PK;

			var wrongConsignorAndPickupTariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "AU", "");
			wrongConsignorAndPickupTariffEntry.TI_OH_Consignor = wrongConsignor.PK;
			wrongConsignorAndPickupTariffEntry.TI_OA_CartagePickupAddressOverride = wrongConsignor.MainAddress.PK;

			#endregion

			var collection = new List<IRatingHeader> { clientRate, companyTariff };
			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Expected no wrong rate entries", 4, results.Count);
			AssertCollectionContains(consignorRateEntry, results);
			AssertCollectionContains(consignorAndPickupRateEntry, results);
			AssertCollectionContains(consignorTariffEntry, results);
			AssertCollectionContains(consignorAndPickupTariffEntry, results);
		}

		public void TestWithConsigneeWithDeliveryAddress()
		{
			var consignee = Consignee;
			var deliveryAddress = consignee.Addresses.AddNew();
			deliveryAddress.OA_Code = "DELIVER";
			deliveryAddress.OA_Address1 = "123 King St";

			var wrongAddress = consignee.Addresses.AddNew();
			wrongAddress.OA_Code = "WRONG";
			wrongAddress.OA_Address1 = "456 Coward St";

			var wrongConsignee = Helper.NewOrgHeader();
			wrongConsignee.OH_IsConsignee = true;
			Factory.Save();

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP40, NewClient);
			testCriteria.Consignee = consignee;
			testCriteria.DeliveryAddress = deliveryAddress;

			#region Setup Client Rate

			var clientRate = Helper.NewClientRate(NewClient);

			var consigneeRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "", "US");
			consigneeRateEntry.TI_OH_Consignee = consignee.PK;

			var wrongConsigneeRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "", "US");
			wrongConsigneeRateEntry.TI_OH_Consignee = wrongConsignee.PK;

			var consigneeAndDeliveryRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "", "US");
			consigneeAndDeliveryRateEntry.TI_OH_Consignee = consignee.PK;
			consigneeAndDeliveryRateEntry.TI_OA_CartageDeliveryAddressOverride = deliveryAddress.PK;

			var wrongDeliveryRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "", "US");
			wrongDeliveryRateEntry.TI_OH_Consignee = Consignee.PK;
			wrongDeliveryRateEntry.TI_OA_CartageDeliveryAddressOverride = wrongAddress.PK;

			var wrongConsigneeAndDeliveryRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "", "US");
			wrongConsigneeAndDeliveryRateEntry.TI_OH_Consignee = wrongConsignee.PK;
			wrongConsigneeAndDeliveryRateEntry.TI_OA_CartageDeliveryAddressOverride = wrongConsignee.MainAddress.PK;

			#endregion

			#region Setup Tariff

			var companyTariff = Helper.NewCompanyTariff();

			var consigneeTariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "", "US");
			consigneeTariffEntry.TI_OH_Consignee = consignee.PK;

			var wrongConsigneeTariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "", "US");
			wrongConsigneeTariffEntry.TI_OH_Consignee = wrongConsignee.PK;

			var consigneeAndDeliveryTariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "", "US");
			consigneeAndDeliveryTariffEntry.TI_OH_Consignee = consignee.PK;
			consigneeAndDeliveryTariffEntry.TI_OA_CartageDeliveryAddressOverride = deliveryAddress.PK;

			var wrongDeliveryTariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "", "US");
			wrongDeliveryTariffEntry.TI_OH_Consignee = consignee.PK;
			wrongDeliveryTariffEntry.TI_OA_CartageDeliveryAddressOverride = wrongAddress.PK;

			var wrongConsigneeAndDeliveryTariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "", "US");
			wrongConsigneeAndDeliveryTariffEntry.TI_OH_Consignee = wrongConsignee.PK;
			wrongConsigneeAndDeliveryTariffEntry.TI_OA_CartageDeliveryAddressOverride = wrongConsignee.MainAddress.PK;

			#endregion

			var collection = new List<IRatingHeader> { clientRate, companyTariff };
			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Expected no wrong rate entries", 4, results.Count);
			AssertCollectionContains(consigneeRateEntry, results);
			AssertCollectionContains(consigneeAndDeliveryRateEntry, results);
			AssertCollectionContains(consigneeTariffEntry, results);
			AssertCollectionContains(consigneeAndDeliveryTariffEntry, results);
		}

		public void TestWithCommodityCode()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			entry1.TI_RH_NKCommodityCode = ZString.Empty;
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			entry2.TI_RH_NKCommodityCode = "GEN";
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			entry3.TI_RH_NKCommodityCode = "HAZ";

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));

			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.SetCommodityCode("HAZ");
			results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry3", true, results.Contains(entry3));

			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.SetCommodityCode("REF");
			results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 1, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));

			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.SetCommodityCode("");
			results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
		}

		public void TestWithServiceLevel()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "", "STD", "");
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "", "URG", "");

			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));

			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.ServiceLevel = new ServiceLevelRatingInformation(new ServiceLevelInfo("URG", ServiceLevelType.Client));
			testCriteria.Consignor = Consignor;
			results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry3", true, results.Contains(entry3));

			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.ServiceLevel = new ServiceLevelRatingInformation(new ServiceLevelInfo("EXW", ServiceLevelType.Client));
			testCriteria.Consignor = Consignor;
			results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 1, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));

			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.ServiceLevel = new ServiceLevelRatingInformation(new ServiceLevelInfo("", ServiceLevelType.Client));
			results = Filter(collection, testCriteria, false, Factory);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry2", true, results.Contains(entry2));
		}

		public void TestWithRatesGoingToExpire()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_RateStartDate = ZDate.Today.AddDays(-5);
			entry1.TI_RateEndDate = ZDate.Today.AddDays(AutoRater.ExpiringRateNotificationDays + 1);
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry2.TI_RateStartDate = ZDate.Today.AddDays(-5);
			entry2.TI_RateEndDate = ZDate.Today.AddDays(AutoRater.ExpiringRateNotificationDays);
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry3.TI_RateStartDate = ZDate.Today.AddDays(-5);
			entry3.TI_RateEndDate = ZDate.Today.AddDays(AutoRater.ExpiringRateNotificationDays - 1);

			var results = Filter(collection, testCriteria, false, ratesToFind: FreightAutoRater.RatesToFindEnum.RatesGoingToExpire);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry2", true, results.Contains(entry2));
			AssertEquals("Entry3", true, results.Contains(entry3));
		}

		public void TestWithJustExpiredRates()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_RateStartDate = ZDate.Today.AddDays(-45);
			entry1.TI_RateEndDate = ZDate.Today.AddDays(-AutoRater.ExpiredRateNotificationDays);
			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry2.TI_RateStartDate = ZDate.Today.AddDays(-45);
			entry2.TI_RateEndDate = ZDate.Today.AddDays(-AutoRater.ExpiredRateNotificationDays - 1);
			var entry3 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry3.TI_RateStartDate = ZDate.Today.AddDays(-45);
			entry3.TI_RateEndDate = ZDate.Today.AddDays(-AutoRater.ExpiredRateNotificationDays + 1);
			var entry4 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry4.TI_RateStartDate = ZDate.Today.AddDays(-45);
			entry4.TI_RateEndDate = ZDate.Today.AddDays(-AutoRater.ExpiredRateNotificationDays - 1);

			var results = Filter(collection, testCriteria, false, ratesToFind: FreightAutoRater.RatesToFindEnum.JustExpiredRates);

			AssertEquals("Count", 2, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
			AssertEquals("Entry3", true, results.Contains(entry3));
		}

		public void TestOriginChargeWithRateOrigin()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var headers = new List<IRatingHeader> { clientRate };

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.RateOrigin = LocationHelper.GetLocationFromString("AUMEL", Factory);

			var entry1 = clientRate.AddRateEntry("ORG", "ALL", "", "");
			entry1.TI_RateOrigin = "AUMEL";

			var results = Filter(headers, testCriteria, false, Factory);
			AssertEquals("Count", 1, results.Count);
			Assert(results.Contains(entry1));

			entry1.TI_RateOrigin = "AU";

			results = Filter(headers, testCriteria, false, Factory);
			AssertEquals("Count", 1, results.Count);
			Assert(results.Contains(entry1));

			var entry2 = clientRate.AddRateEntry("ORG", "ALL", "AUSYD", "");
			entry2.TI_RateOrigin = "AUMEL";

			results = Filter(headers, testCriteria, false, Factory);
			AssertEquals("Count", 2, results.Count);
			Assert(results.Contains(entry1));
			Assert(results.Contains(entry2));
		}

		public void TestDestinationChargeWithRateDestination()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var headers = new List<IRatingHeader> { clientRate };

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.RateDestination = LocationHelper.GetLocationFromString("USLGB", Factory);

			var entry1 = clientRate.AddRateEntry("DST", "ALL", "", "");
			entry1.TI_RateDestination = "USLGB";

			var results = Filter(headers, testCriteria, false, Factory);
			AssertEquals("Count", 1, results.Count);
			AssertEquals("Entry1 matches job", true, results.Contains(entry1));

			entry1.TI_RateDestination = "US";

			results = Filter(headers, testCriteria, false, Factory);
			AssertEquals("Count", 1, results.Count);
			AssertEquals("Entry1 matches job", true, results.Contains(entry1));

			var entry2 = clientRate.AddRateEntry("DST", "ALL", "", "USLAX");

			results = Filter(headers, testCriteria, false, Factory);
			AssertEquals("Count", 2, results.Count);
			Assert(results.Contains(entry1));
			Assert(results.Contains(entry2));

			entry1.TI_RateDestination = "USLGB";
			results = Filter(headers, testCriteria, false, Factory);
			AssertEquals("Count", 2, results.Count);
			Assert(results.Contains(entry1));
			Assert(results.Contains(entry2));
		}

		#endregion

		#region Expiry Date

		[TestDate(2008, 1, 1)] // so that neither of the rates below are expired
		public void TestExpiryDate()
		{
			var collection = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			collection.Add(clientRate);

			var entrySep = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");
			entrySep.TI_RateStartDate = new ZDate(2008, 9, 1);
			entrySep.TI_RateEndDate = new ZDate(2008, 9, 30);

			var entryOct = clientRate.AddRateEntry("DST", "AIR", "", "USLAX");
			entryOct.TI_RateStartDate = new ZDate(2008, 10, 1);
			entryOct.TI_RateEndDate = new ZDate(2008, 10, 31);

			AssertCorrectRateForArrivalDate(new ZDate(2008, 9, 29), new List<IRateEntry> { entrySep }, collection);
			AssertCorrectRateForArrivalDate(new ZDate(2008, 9, 30), new List<IRateEntry> { entrySep }, collection);
			AssertCorrectRateForArrivalDate(new ZDate(2008, 9, 30), new List<IRateEntry> { entrySep }, collection);
			AssertCorrectRateForArrivalDate(new ZDate(2008, 9, 30), new List<IRateEntry> { entrySep }, collection);
			AssertCorrectRateForArrivalDate(new ZDate(2008, 10, 1), new List<IRateEntry> { entrySep, entryOct }, collection);
			AssertCorrectRateForArrivalDate(new ZDate(2008, 10, 1), new List<IRateEntry> { entrySep, entryOct }, collection);
		}

		void AssertCorrectRateForArrivalDate(ZDate arrivalDate, IEnumerable<IRateEntry> expectedEntryCollection, List<IRatingHeader> collection)
		{
			var criteria = new TestRatingCriteria("INBOM", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);

			var jobDatesProvider = new Mock<IJobDatesProvider>();
			criteria.JobDatesProvider = jobDatesProvider.Object;

			jobDatesProvider.Setup(m => m.EarliestPossibleDate).Returns(ZDate.Today);
			jobDatesProvider.Setup(m => m.LatestPossibleDate).Returns(arrivalDate);

			var results = Filter(collection, criteria, false, Factory);

			AssertContainsExactElementsInAnyOrder(expectedEntryCollection, results);
		}

		#endregion

		#region Forwarder Group

		public void TestWithSupplierIsMemberOfForwarderGroup()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var forwarderGroup = Factory.NewWithValidTestData<OrgHeader>();
			var unknownOrg = Factory.NewWithValidTestData<OrgHeader>();

			var ratingHeaders = new List<IRatingHeader>();
			var clientRate = Helper.NewClientRate(NewClient);
			ratingHeaders.Add(clientRate);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Carrier = org;
			testCriteria.SetSupplier(org);

			var entry1 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_OH_Supplier = forwarderGroup.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry1, "ODOC", 20m);

			var entry2 = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry2.TI_OH_Supplier = unknownOrg.PK;
			Helper.AddRateLineWithFlatCalculatorToRateEntry(entry2, "ODOC", 25m);

			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Carrier = org;
			testCriteria.SetSupplier(org);
			var results = Filter(ratingHeaders, testCriteria, false, Factory);
			AssertEquals("No results found as the Agent is not part of the Forwarder Group", 0, results.Count);

			var relatedParty = org.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderGroup;
			relatedParty.PR_OH_RelatedParty = forwarderGroup.PK;

			Factory.Save();

			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.Carrier = org;
			testCriteria.SetSupplier(org);
			results = Filter(ratingHeaders, testCriteria, false, Factory);
			AssertEquals("1 item is found as the Agent is now part of the Forwarder Group", 1, results.Count);
			AssertEquals("Entry1", true, results.Contains(entry1));
		}

		#endregion

		protected List<IRateEntry> Filter(
			IEnumerable<IRatingHeader> ratingHeaders,
			RatingCriteria criteria,
			bool isCosting,
			BusinessObjectFactory factory = null,
			ILogger logger = null,
			FreightAutoRater.RatesToFindEnum ratesToFind = FreightAutoRater.RatesToFindEnum.ActiveRates)
		{
			if (isCosting)
			{
				var loader = new CostRatesLoader(criteria.Factory, logger);
				var rates = loader.Load(criteria, ratingHeaders);
				return RateEntryFilter.Filter(criteria, true, rates, factory ?? Factory, logger ?? new DummyLogger(), ratesToFind).ToList();
			}
			else
			{
				var loader = new RevenueRatesLoader(criteria.Factory, logger);
				var rates = loader.Load(criteria, ratingHeaders);
				return RateEntryFilter.Filter(criteria, false, rates, factory ?? Factory, logger ?? new DummyLogger(), ratesToFind).ToList();
			}
		}
	}
}