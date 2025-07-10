using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.DistanceCalculation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingDistanceCalculationHelperTest : RatingTestCase
	{
		#region Get Distance from Postcodes Lat/Long

		public void TestGetDistanceForPostCodeLatLongCalculation_NonTransportRate()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var orgLine = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LRO, "AUSYD", "AUSYD").AddRateLine("ODOC", FlatCalculator.Code);
			var dstLine = rate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LRO, "AUSYD", "AUSYD").AddRateLine("DDOC", FlatCalculator.Code);

			var consignorAddress = Helper.NewOrgHeader().MainAddress;
			var consigneeAddress = Helper.NewOrgHeader().MainAddress;
			consigneeAddress.OA_PostCode = "2015";

			var criteria = new TestRatingCriteria("AUSYD", "AUSYD", FreightMode.LRO, 50M, 0.5M, null);
			criteria.PickupAddress = consignorAddress;
			criteria.DeliveryAddress = consigneeAddress;

			AssertEquals("2000", consignorAddress.OA_PostCode);
			AssertEquals("2015", consigneeAddress.OA_PostCode);
			AssertDistance(criteria, orgLine, 0);
			AssertDistance(criteria, dstLine, 0);

			var wharfAddress = Helper.NewOrgHeader().MainAddress;
			wharfAddress.OA_PostCode = "2018";
			criteria.WharfCTOAddress = wharfAddress;

			var expectedOrgLog = @"- Pickup Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: 6.115 (Consignor Pickup Address to CTO/Wharf)";

			var expectedDstLog = @"- Delivery Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: 2.264 (Consignee Delivery Address to CTO/Wharf)";

			AssertDistance(criteria, orgLine, 6.115, expectedOrgLog);
			AssertDistance(criteria, dstLine, 2.264, expectedDstLog);
		}

		public void TestGetDistanceForPostCodeLatLongCalculation_TransportRate()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry(RatingConstants.RateCategory.TRN, Core.Constants.RateMode.LRO, "AUSYD", "AUSYD").AddRateLine("ODOC", FlatCalculator.Code);

			var consigneeAddress = Helper.NewOrgHeader().Addresses.AddNew();
			consigneeAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			consigneeAddress.OA_Address1 = "58 Mentmore Ave";
			consigneeAddress.OA_City = "Rosebery";
			consigneeAddress.OA_PostCode = "2018";
			consigneeAddress.OA_State = "NSW";
			consigneeAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var consignorAddress = Helper.NewOrgHeader().Addresses.AddNew();
			consignorAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			consignorAddress.OA_Address1 = "241 Victoria St";
			consignorAddress.OA_City = "Darlinghurst";
			consignorAddress.OA_PostCode = "2010";
			consignorAddress.OA_State = "NSW";
			consignorAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var criteria = new TestRatingCriteria("AUSYD", "AUSYD", FreightMode.LRO, 50M, 0.5M, null);
			criteria.RateTypeToUse = RateType.LocalTransport;
			criteria.PickupAddress = consigneeAddress;
			criteria.DeliveryAddress = consignorAddress;

			var expectedLog = @"- Pickup Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: 4.927 (Consignor Pickup Address to Consignee Delivery Address)";

			AssertDistance(criteria, line, 4.927m, expectedLog);
		}

		public void TestLatLongPostCodeOnlyExistsForAustralia()
		{
			var query = new ZQuery(RefLatLongPostcodeSchema.RJ_RN_NKCountry, SQLComparisonOperator.NotEqual, Constants.CountryCodes.Australia);
			var results = Factory.LoadTop1<RefLatLongPostcode>(query);

			AssertNull("If this test fails please remove the Australian country check in GetDistancePostcodeToPostcode", results);
		}

		#endregion

		#region Forwarding Distance Calculation

		public void TestGetDistance()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine1 = clientRate.AddRateEntry("ORG", "LCL", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var rateLine2 = clientRate.AddRateEntry("DST", "LCL", "", "AUSYD").AddRateLine("DDOC", FlatCalculator.Code);

			var criteria = new TestRatingCriteria("", "", FreightMode.LCL, 50M, 0.5M, null);
			var log1 = @"- Pickup Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)";

			var log2 = @"- Delivery Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: empty (Consignee Delivery Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine1, 0m, log1);
			AssertDistance(criteria, rateLine2, 0m, log2);

			Env.Registry.Rating.UseDistanceCalculationService = true;

			log1 = @"- Pickup Distance: empty
- Distance Calculation Service: empty (Consignor Pickup Address to CTO/Wharf)
- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)";

			log2 = @"- Delivery Distance: empty
- Distance Calculation Service: empty (Consignee Delivery Address to CTO/Wharf)
- Lat/Long Postcode Distance: empty (Consignee Delivery Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine1, 0m, log1);
			AssertDistance(criteria, rateLine2, 0m, log2);

			Env.Registry.Rating.UseDistanceCalculationService = false;
			Env.Registry.Rating.DefaultCTOAddressSea = "2000";

			log1 = @"- Pickup Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)";

			log2 = @"- Delivery Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: empty (Consignee Delivery Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine1, 0m, log1);
			AssertDistance(criteria, rateLine2, 0m, log2);

			criteria.PickupAddress = Helper.NewOrgHeader().MainAddress;
			((OrgAddress)criteria.PickupAddress).OA_PostCode = "";
			criteria.DeliveryAddress = Helper.NewOrgHeader().MainAddress;
			((OrgAddress)criteria.DeliveryAddress).OA_PostCode = "";

			log1 = @"- Pickup Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)";

			log2 = @"- Delivery Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: empty (Consignee Delivery Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine1, 0m, log1);
			AssertDistance(criteria, rateLine2, 0m, log2);

			Env.Registry.Rating.UseDistanceCalculationService = true;
			var log1Result = @"- Pickup Distance: empty
- Distance Calculation Service: 46 (Consignor Pickup Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine1, 46m, log1Result);

			Env.Registry.Rating.UseDistanceCalculationService = false;

			((OrgAddress)criteria.PickupAddress).OA_PostCode = "88888";
			((OrgAddress)criteria.DeliveryAddress).OA_PostCode = "99999";

			AssertDistance(criteria, rateLine1, 0m, log1);
			AssertDistance(criteria, rateLine2, 0m, log2);

			Env.Registry.Rating.UseDistanceCalculationService = true;

			log1Result = @"- Pickup Distance: empty
- Distance Calculation Service: 51 (Consignor Pickup Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine1, 51m, log1Result);

			Env.Registry.Rating.UseDistanceCalculationService = false;

			((OrgAddress)criteria.PickupAddress).OA_PostCode = "2015";
			((OrgAddress)criteria.DeliveryAddress).OA_PostCode = "2229";

			log1Result = @"- Pickup Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: 4.381 (Consignor Pickup Address to CTO/Wharf)";

			var log2Result = @"- Delivery Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: 21.384 (Consignee Delivery Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine1, 4m, log1Result);
			AssertDistance(criteria, rateLine2, 21m, log2Result);

			Env.Registry.Rating.DefaultCTOAddressSea = "";
			criteria.WharfCTOAddress = Helper.NewOrgHeader().MainAddress;
			criteria.WharfCTOAddress.OA_PostCode = "2225";

			log1Result = @"- Pickup Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: 14.955 (Consignor Pickup Address to CTO/Wharf)";

			log2Result = @"- Delivery Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: 6.154 (Consignee Delivery Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine1, 15m, log1Result);
			AssertDistance(criteria, rateLine2, 6m, log2Result);

			rateLine1.TL_AC = Helper.ChargeCodes.New("TORIGBR", "Test Origin Brokerage", UnitCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerage).PK;
			rateLine2.TL_AC = Helper.ChargeCodes["CCLR"].PK;

			log1Result = @"- Job Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: 14.955 (Consignor Pickup Address to CTO/Wharf)";

			log2Result = @"- Job Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: 6.154 (Consignee Delivery Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine1, 15m, log1Result);
			AssertDistance(criteria, rateLine2, 6m, log2Result);
		}

		public void TestGetDistanceFromService_Sea()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var lineOrigin = rate.AddRateEntry("ORG", "SEA", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var lineDestination = rate.AddRateEntry("DST", "SEA", "", "AUSYD").AddRateLine("DDOC", FlatCalculator.Code);
			var criteria = new TestRatingCriteria("", "", FreightMode.SEA, 50M, 0.5M, null);
			criteria.PickupAddress = CreateOrgAddress(ZString.Empty, "1");
			criteria.DeliveryAddress = CreateOrgAddress(ZString.Empty, "22");

			Env.Registry.Rating.DefaultCTOAddressSea = "4444";
			Env.Registry.Rating.UseDistanceCalculationService = true;

			AssertDistance(criteria, lineOrigin, 47m);
			AssertDistance(criteria, lineDestination, 48m);
		}

		public void TestGetDistanceFromService_Rail()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var lineOrigin = rate.AddRateEntry("ORG", "RAI", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var lineDestination = rate.AddRateEntry("DST", "RAI", "", "AUSYD").AddRateLine("DDOC", FlatCalculator.Code);
			var criteria = new TestRatingCriteria("", "", FreightMode.RAI, 50M, 0.5M, null);
			criteria.PickupAddress = CreateOrgAddress(ZString.Empty, "1");
			criteria.DeliveryAddress = CreateOrgAddress(ZString.Empty, "22");

			Env.Registry.Rating.DefaultCTOAddressRail = "4444";
			Env.Registry.Rating.UseDistanceCalculationService = true;

			AssertDistance(criteria, lineOrigin, 47m);
			AssertDistance(criteria, lineDestination, 48m);
		}

		public void TestGetDistanceFromService_Air()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var lineOrigin = rate.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var lineDestination = rate.AddRateEntry("DST", "AIR", "", "AUSYD").AddRateLine("DDOC", FlatCalculator.Code);
			var criteria = new TestRatingCriteria("", "", FreightMode.AIR, 50M, 0.5M, null);
			criteria.PickupAddress = CreateOrgAddress(ZString.Empty, "1");
			criteria.DeliveryAddress = CreateOrgAddress(ZString.Empty, "22");

			Env.Registry.Rating.DefaultCTOAddressAir = "4444";
			Env.Registry.Rating.UseDistanceCalculationService = true;

			AssertDistance(criteria, lineOrigin, 47m);
			AssertDistance(criteria, lineDestination, 48m);
		}

		public void TestGetDistanceFromService_Road()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var lineOrigin = rate.AddRateEntry("ORG", "ROA", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var lineDestination = rate.AddRateEntry("DST", "ROA", "", "AUSYD").AddRateLine("DDOC", FlatCalculator.Code);
			var criteria = new TestRatingCriteria("", "", FreightMode.ROA, 50M, 0.5M, null);
			criteria.PickupAddress = CreateOrgAddress(ZString.Empty, "1");
			criteria.DeliveryAddress = CreateOrgAddress(ZString.Empty, "22");

			Env.Registry.Rating.DefaultCFSAddressRoad = "4444";
			Env.Registry.Rating.UseDistanceCalculationService = true;

			AssertDistance(criteria, lineOrigin, 47m);
			AssertDistance(criteria, lineDestination, 48m);
		}

		public void TestGetDistanceFromService_Cached()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var lineOrigin = rate.AddRateEntry("ORG", "SEA", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var lineDestination = rate.AddRateEntry("DST", "SEA", "", "AUSYD").AddRateLine("DDOC", FlatCalculator.Code);

			var criteria = new TestRatingCriteria("", "", FreightMode.SEA, 50M, 0.5M, null);
			criteria.PickupAddress = CreateOrgAddress(ZString.Empty, "1");
			criteria.DeliveryAddress = CreateOrgAddress(ZString.Empty, "22");

			Env.Registry.Rating.DefaultCTOAddressSea = "4444";
			Env.Registry.Rating.UseDistanceCalculationService = true;

			AssertEquals("Precondition - Should not have added any Distance Result", 0, DistanceCalculationManager.CallCountForTest);

			AssertDistance(criteria, lineOrigin, 47m);
			AssertDistance(criteria, lineDestination, 48m);

			AssertEquals("The distance result from Orgin should have been added", 2, DistanceCalculationManager.CallCountForTest);
			AssertEquals("The distance result from Destination should have been added", 2, DistanceCalculationManager.CallCountForTest);

			AssertDistance(criteria, lineOrigin, 47m);
			AssertDistance(criteria, lineDestination, 48m);

			AssertEquals("Should have been retrieved from the cache", 2, DistanceCalculationManager.CallCountForTest);
			AssertEquals("Should have been retrieved from the cache", 2, DistanceCalculationManager.CallCountForTest);
		}

		#endregion

		public void TestGetDistanceFromMeasures()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "USCHI", "USLAX").AddRateLine("ODOC", FlatCalculator.Code);

			var criteria = new TestRatingCriteria("", "", FreightMode.LCL, 50M, 0.5M, null);
			criteria.PickupAddress = Helper.NewOrgHeader().MainAddress;
			criteria.DeliveryAddress = Helper.NewOrgHeader().MainAddress;
			criteria.RateableMeasures.SetPickupDistance(10m, Constants.Length.Kilometres);
			criteria.RateableMeasures.SetDeliveryDistance(20m, Constants.Length.Kilometres);

			AssertDistance(criteria, line, 10m, "- Pickup Distance: 10");

			line = rate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LCL, "USCHI", "USLAX").AddRateLine("DDOC", FlatCalculator.Code);
			AssertDistance(criteria, line, 20m, "- Delivery Distance: 20");

			line = rate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LRO, "USCHI", "USLAX").AddRateLine("CCLR", FlatCalculator.Code);

			var log = @"- Job Distance: empty
- Distance Calculation Service: empty (registry disabled)";

			AssertDistance(criteria, line, 0, log); // none of the distance measures appear as mode isn't ORG/DST/TBC/TRN
		}

		public void TestGetDistanceFromService()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("ORG", "LCL", "USCHI", "USLAX").AddRateLine("ODOC", FlatCalculator.Code);

			var criteria = new TestRatingCriteria("USCHI", "", FreightMode.LCL, 50M, 0.5M, null);
			criteria.RateTypeToUse = RateType.TransportBookings;

			criteria.PickupAddress = Factory.New<OrgAddress>();
			criteria.DeliveryAddress = Factory.New<OrgAddress>();

			((OrgAddress)criteria.PickupAddress).OA_City = "Los Angeles";
			((OrgAddress)criteria.PickupAddress).OA_RL_NKRelatedPortCode = "USLAX";
			((OrgAddress)criteria.DeliveryAddress).OA_City = "Chicago";
			((OrgAddress)criteria.DeliveryAddress).OA_RL_NKRelatedPortCode = "USCHI";

			Env.Registry.Rating.UseDistanceCalculationService = true;

			var expectedLog = @"- Pickup Distance: empty
- Distance Calculation Service: 44 (Consignor Pickup Address to Consignee Delivery Address)";

			AssertDistance(criteria, rateLine, "ChicagoUnited StatesLos AngelesUnited States".Length, expectedLog);

			rateLine = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "AUMEL").AddRateLine("FRT", FlatCalculator.Code);

			expectedLog = @"- Pickup Distance: empty
- Distance Calculation Service: 44 (Consignor Pickup Address to Consignee Delivery Address)";

			AssertDistance(criteria, rateLine, 44m, expectedLog);

			Env.Registry.Rating.UseDistanceCalculationService = false;
			AssertDistance(criteria, rateLine, 0m);
		}

		public void TestGetDistanceFromUNLOCO()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "USCHI", "USLAX").AddRateLine("ODOC", FlatCalculator.Code);
			rateLine.ChargeCode.AC_ChargeGroup = "XXX";

			var criteria = new TestRatingCriteria("", "", FreightMode.LCL, 50M, 0.5M, null);

			Env.Registry.Rating.UseDistanceCalculationService = true;

			var expectedLog = @"- Job Distance: empty
- Distance Calculation Service: 48 (Rate Origin and Destination)";

			AssertDistance(criteria, rateLine, "ChicagoILUnited StatesLos AngelesCAUnited States".Length, expectedLog);

			Env.Registry.Rating.UseDistanceCalculationService = false;

			expectedLog = @"- Job Distance: empty
- Distance Calculation Service: empty (registry disabled)";

			AssertDistance(criteria, rateLine, 0m, expectedLog);

			clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			rateLine = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "US", "USLAX").AddRateLine("ODOC", FlatCalculator.Code);
			rateLine.ChargeCode.AC_ChargeGroup = "XXX";

			Env.Registry.Rating.UseDistanceCalculationService = true;

			expectedLog = @"- Job Distance: empty
- Distance Calculation Service: empty (no compatible locations)";

			AssertDistance(criteria, rateLine, 0m, expectedLog);
		}

		#region Warehouse Distance Calculation

		public void TestGetDistance_WarehouseOutwards()
		{
			Helper.ChargeCodes.New("WCART", "Warehouse Cartage", CartageCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("WHS", "ALL", "", "").AddRateLine("WCART", FlatCalculator.Code);

			var criteria = new TestRatingCriteria("", "", FreightMode.UKN, 50M, 0.5M, null)
			{
				RateTypeToUse = RateType.Warehouse,
				WharfCTOAddress = CreateOrgAddress(ZString.Empty, "2015"),
				DeliveryAddress = CreateOrgAddress(ZString.Empty, "2229")
			};

			AssertDistance(criteria, rateLine, 17.027m);

			var expectedLog = @"- Job Distance: empty
- Distance Calculation Service: empty (registry disabled)";

			((OrgAddress)criteria.DeliveryAddress).OA_RL_NKRelatedPortCode = "";

			AssertDistance(criteria, rateLine, 0m, expectedLog);

			Env.Registry.Rating.UseDistanceCalculationService = true;

			expectedLog = @"- Job Distance: empty
- Distance Calculation Service: empty (no compatible locations)";

			AssertDistance(criteria, rateLine, 0m, expectedLog);

			((OrgAddress)criteria.DeliveryAddress).OA_RL_NKRelatedPortCode = "AUSYD";

			expectedLog = @"- Job Distance: empty
- Distance Calculation Service: 50 (Consignee Delivery Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine, 50m, expectedLog);
		}

		public void TestGetDistance_WarehouseInwards()
		{
			Helper.ChargeCodes.New("WCART", "Warehouse Cartage", CartageCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("WHS", "ALL", "", "").AddRateLine("WCART", FlatCalculator.Code);

			var criteria = new TestRatingCriteria("", "", FreightMode.UKN, 50M, 0.5M, null)
			{
				RateTypeToUse = RateType.Warehouse,
				WharfCTOAddress = CreateOrgAddress(ZString.Empty, "2015"),
				PickupAddress = CreateOrgAddress(ZString.Empty, "2229")
			};

			var expectedLog = @"- Job Distance: empty
- Distance Calculation Service: empty (registry disabled)
- Lat/Long Postcode Distance: 17.027 (Consignor Pickup Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine, 17.027m, expectedLog);

			Env.Registry.Rating.UseDistanceCalculationService = true;

			expectedLog = @"- Job Distance: empty
- Distance Calculation Service: 50 (Consignor Pickup Address to CTO/Wharf)";

			AssertDistance(criteria, rateLine, 50m, expectedLog);
		}

		#endregion

		#region Implementation

		static void AssertDistance(RatingCriteria criteria, RateLine line, ZDecimal expectedValue, string log = "")
		{
			var helper = new RatingDistanceCalculationHelper(criteria, line);
			var description = new ZStringBuilder();

			AssertEquals(expectedValue, helper.GetDistance(description), 3);
			if (!string.IsNullOrEmpty(log))
			{
				AssertMultilineASCIIEquals("", log, description.ToString());
			}
		}

		OrgAddress CreateOrgAddress(ZString unloco, ZString postCode)
		{
			var orgHeader = Helper.NewOrgHeader();
			var address = orgHeader.MainAddress;
			if (!unloco.IsEmpty)
			{
				address.OA_RL_NKRelatedPortCode = unloco;
			}
			if (!postCode.IsEmpty)
			{
				address.OA_PostCode = postCode;
			}

			return address;
		}

		#endregion
	}
}
