using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WiseRates.Api.Model;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.Business.Test
{
	class DescriptionHelpersTest : RatingTestCase
	{
		public void TestCargoSphere_HandlingOfficeDescription()
		{
			var cw1ChargeFRT = Helper.ChargeCodes["FRT"];
			var criteria = new TestRatingCriteria();
			var rateServiceChargeFR = new Charge() { ChargeCode = "FRT", CustomCategory = "something" };
			var wiseHeader = new WiseHeader(Factory);
			var wiseEntry = new WiseEntry((Rate)null, Factory);
			wiseEntry.ParentRatingHeader = wiseHeader;
			var wiseLine = new WiseLine(Factory, rateServiceChargeFR);
			wiseLine.TL_AC = cw1ChargeFRT.PK;
			wiseLine.ParentRateEntry = wiseEntry;

			wiseLine.CustomFields = new[]
			{
				new CustomField()
				{
					 Code = Rate.CustomFields.CargoSphere.HandlingOffice,
					 Description = "potato",
					 Value = "It came from Outer Space!"
				}
			};

			AssertContains("Handling Office:\tIt came from Outer Space!", DescriptionHelpers.GetRateLineDescription(wiseLine, criteria));
		}

		public void TestServiceSpot_CostDescription()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var jobServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, "Destination Storage", 1m, TimeSpan.FromDays(5), Helper.NewOrgHeader(), 20m, JobServiceInfo.Constants.Codes.Day);
			jobServiceInfo.IsCostForSpotRate = true;

			var criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, refContainer, Helper.NewOrgHeader());
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.JobServices.Add(jobServiceInfo);

			var spotCostRateEntry = new SpotRateEntryCreator(criteria).GetAllJobServiceRates(true);
			var rateLine = spotCostRateEntry.FirstOrDefault().ChildRateLines.FirstOrDefault();

			var expected = @"Destination Storage Service Cost is applicable for record.

Mode:			ALL
Charge Code Group:	DST / STG
Origin:			AUSYD
Destination:		GBLON
Currency:		AUD
Autorated for:		record
Leg:			AUSYD-GBLON";

			AssertContains(expected, DescriptionHelpers.GetRateLineDescription(rateLine, criteria));
		}

		public void TestServiceSpot_SellDescription()
		{
			var jobServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, "Destination Storage", 1m, TimeSpan.FromDays(5), null, 20m, JobServiceInfo.Constants.Codes.Day);
			var criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, null, Helper.NewOrgHeader());
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.JobServices.Add(jobServiceInfo);

			var spotCostRateEntry = new SpotRateEntryCreator(criteria).GetAllJobServiceRates(false);
			var rateLine = spotCostRateEntry.FirstOrDefault().ChildRateLines.FirstOrDefault();

			var expected = @"Destination Storage / Warehousing

Destination Storage Service Charge is applicable for record.

Payment Term:		Prepaid
Mode:			ALL
Charge Code Group:	DST / STG
Origin:			AUSYD
Destination:		GBLON
Currency:		AUD
Autorated for:		record
Leg:			AUSYD-GBLON";

			AssertContains(expected, DescriptionHelpers.GetRateLineDescription(rateLine, criteria));
		}

		[TestDate(2023, 4, 4)]
		public void TestGetRateLineDescription_AutoratingDate()
		{
			var testDate = ZDate.Today;
			var consol = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingConsol>();

			var autoRating = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRating, Factory);

			var costing = Helper.NewCosting(null);
			var rateEntry = costing.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "US", "FRT", 100);
			var rateLine = rateEntry.ChildRateLines.First();

			consol.Transports.MostInterestingTransport.JW_ATD = testDate;
			var description = DescriptionHelpers.GetRateLineDescription(rateLine, criteria);
			AssertContains("Autorating Date:\t04-Apr-23", description);

			consol.AutoratingDate = testDate.AddDays(1);
			description = DescriptionHelpers.GetRateLineDescription(rateLine, criteria);
			AssertContains("Autorating Date:\t05-Apr-23 (Override)", description);
		}

		public void TestGetRateLineDescription_HBLDeliveryMode()
		{
			var orgHeader = Helper.NewOrgHeader();
			var shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();

			var autoRating = new AutoRatingProxy(shipment.RatingAdapter);
			var criteria = new RatingCriteria(autoRating, Factory);

			var clientRate = Helper.NewClientRate(orgHeader);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "US", "FRT", 100);
			var rateLine = rateEntry.ChildRateLines.First();
			rateEntry.TI_HBLDeliveryMode = "DOOR/DOOR";

			var description = DescriptionHelpers.GetRateLineDescription(rateLine, criteria);
			AssertContains("HBL Delivery Mode:\tDOOR/DOOR", description);

			var companyTariff = Helper.NewCompanyTariff();

			rateEntry = companyTariff.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "US", "FRT", 100);
			rateLine = rateEntry.ChildRateLines.First();
			rateEntry.TI_HBLDeliveryMode = "CY/CY";
			description = DescriptionHelpers.GetRateLineDescription(rateLine, criteria);
			AssertContains("HBL Delivery Mode:\tCY/CY", description);
		}

		public void TestGetRateLineDescription_WithAllMatchingLocations()
		{
			var orgHeader = Helper.NewOrgHeader();
			var shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();

			var autoRating = new AutoRatingProxy(shipment.RatingAdapter);
			var criteria = new RatingCriteria(autoRating, Factory);

			var clientRate = Helper.NewClientRate(orgHeader);

			var rateEntry = clientRate.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "US", "FRT", 100);
			var rateLine = rateEntry.ChildRateLines.First();
			rateEntry.TI_FirstLoadLRC = "SGSIN";
			rateEntry.TI_LastDischargeLRC = "USLGB";
			rateEntry.TI_FirstRouteSetLoadPortLRC = "AUBNE";
			rateEntry.TI_LastRouteSetDischargePortLRC = "CNSZX";

			var description = DescriptionHelpers.GetRateLineDescription(rateLine, criteria);
			AssertContains("Matching Locations:\tSGSIN (1LD), USLGB (LDC), AUBNE (MLD), CNSZX (MDC)", description);
		}

		public void TestGetRateLineDescription_WithMatchingLocations()
		{
			var orgHeader = Helper.NewOrgHeader();
			var shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();

			var autoRating = new AutoRatingProxy(shipment.RatingAdapter);
			var criteria = new RatingCriteria(autoRating, Factory);

			var clientRate = Helper.NewClientRate(orgHeader);

			var rateEntry = clientRate.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "US", "FRT", 100);
			var rateLine = rateEntry.ChildRateLines.First();
			rateEntry.TI_FirstRouteSetLoadPortLRC = "AUBNE";
			rateEntry.TI_LastRouteSetDischargePortLRC = "CNSZX";

			var description = DescriptionHelpers.GetRateLineDescription(rateLine, criteria);
			AssertContains("Matching Locations:\tAUBNE (MLD), CNSZX (MDC)", description);
		}

		public void TestGetRateLineDescription_NoMatchingLocations()
		{
			var orgHeader = Helper.NewOrgHeader();
			var shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();

			var autoRating = new AutoRatingProxy(shipment.RatingAdapter);
			var criteria = new RatingCriteria(autoRating, Factory);

			var clientRate = Helper.NewClientRate(orgHeader);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "US", "FRT", 100);
			var rateLine = rateEntry.ChildRateLines.First();

			var description = DescriptionHelpers.GetRateLineDescription(rateLine, criteria);
			AssertNotContains("Matching Locations:", description);
		}

		public void TestGetRateLineDescription_Frequency()
		{
			var orgHeader = Helper.NewOrgHeader();
			var shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();

			var autoRating = new AutoRatingProxy(shipment.RatingAdapter);
			var criteria = new RatingCriteria(autoRating, Factory);

			var clientRate = Helper.NewClientRate(orgHeader);

			var rateEntry = clientRate.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "US", "FRT", 100);
			var rateLine = rateEntry.ChildRateLines.First();

			var description = DescriptionHelpers.GetRateLineDescription(rateLine, criteria);
			AssertNotContains("Frequency:", description);
			AssertNotContains("Frequency Unit:", description);

			rateEntry.TI_FrequencyUnit = "Daily";
			description = DescriptionHelpers.GetRateLineDescription(rateLine, criteria);
			AssertNotContains("Frequency Unit:", description);

			rateEntry.TI_Frequency = 20;
			description = DescriptionHelpers.GetRateLineDescription(rateLine, criteria);
			AssertContains("Frequency:\t\t20", description);
			AssertContains("Frequency Unit:\t\tDaily", description);
		}

		public void TestGetRateLineDescription_AircraftType()
		{
			TestGetRateLineDescription("TI_AircraftType", "CA0");
		}

		public void TestGetRateLineDescription_CartageDeliveryAddressPostCode()
		{
			TestGetRateLineDescription("TI_CartageDeliveryAddressPostCode", "1");
		}

		public void TestGetRateLineDescription_CartagePickupAddressPostCode()
		{
			TestGetRateLineDescription("TI_CartagePickupAddressPostCode", "2");
		}

		public void TestGetRateLineDescription_RefContainer()
		{
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			TestGetRateLineDescription("TI_RC", gP20.PK);
		}

		public void TestGetRateLineDescription_GlobalCompanyPublisher()
		{
			TestGetRateLineDescription("TI_GC_Publisher", (ZGuid)Env.CurrentCompanyPK);
		}

		public void TestGetRateLineDescription_OriginSuburbPK()
		{
			var suburb = Factory.New<RefCityTown>();
			TestGetRateLineDescription("OriginSuburbPK", suburb.PK);
		}

		public void TestGetRateLineDescription_DestinationSuburbPK()
		{
			var suburb = Factory.New<RefCityTown>();
			TestGetRateLineDescription("DestinationSuburbPK", suburb.PK);
		}

		public void TestGetRateLineDescription_MatchContainerRateClass()
		{
			TestGetRateLineDescription("TI_MatchContainerRateClass", true, "Yes");
		}

		public void TestGetRateLineDescription_ContainerYardFacility()
		{
			var yard1 = Helper.NewWarehouse();
			TestGetRateLineDescription("TI_CYC_WW_Facility", yard1.PK);
		}

		public void TestGetRateLineDescription_GatewayServiceLevel()
		{
			TestGetRateLineDescription("TI_RS_NKGatewayServiceLevel", "STD");
		}

		public void TestGetRateLineDescription_CartageDeliveryAddressOverride()
		{
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			TestGetRateLineDescription("TI_OA_CartageDeliveryAddressOverride", address1.PK);
		}

		public void TestGetRateLineDescription_CartagePickupAddressOverride()
		{
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			TestGetRateLineDescription("TI_OA_CartagePickupAddressOverride", address1.PK);
		}

		public void TestGetRateLineDescription_IsTact()
		{
			TestGetRateLineDescription("TI_IsTact", true, "Yes");
		}

		public void TestGetRateLineDescription_ParentId()
		{
			TestGetRateLineDescription("TI_ParentID", ZGuid.NewZGuid());
		}

		public void TestGetRateLineDescription_ContractLinked()
		{
			TestGetRateLineDescription("TI_ContractNumberLinked", true, "Yes");
		}

		public void TestGetRateLineDescription_Currency()
		{
			TestGetRateLineDescription("TI_RX_NKCurrency", "AUD");
		}

		public void TestGetRateLineDescription_ParentTableCode()
		{
			TestGetRateLineDescription("TI_ParentTableCode", WhsWarehouseSchema.Constants.Prefix, shouldContainDescription: false);
		}

		public void TestGetRateLineDescription_PlannedLoadLRC()
		{
			TestGetRateLineDescription("TI_PlannedLoadLRC", "AUSYD");
		}

		public void TestGetRateLineDescription_PlannedDischargeLRC()
		{
			TestGetRateLineDescription("TI_PlannedDischargeLRC", "AUSYD");
		}

		public void TestGetRateLineDescription_RateOrigin()
		{
			TestGetRateLineDescription("TI_RateOrigin", "AUSYD");
		}

		public void TestGetRateLineDescription_RateDestination()
		{
			TestGetRateLineDescription("TI_RateDestination", "AUSYD");
		}

		public void TestGetRateLineDescription_IsExcludedFromAutoRating()
		{
			TestGetRateLineDescription("TI_IsExcludedFromAutoRating", true , "Yes");
		}

		public void TestGetRateLineDescription_ShipmentConsolidationStatus()
		{
			TestGetRateLineDescription("TI_ShipmentConsolidationStatus", "CNS");
		}

		static string GetDescriptionLinePrefix(ZString descriptionKey) =>
			(string)descriptionKey switch
			{
				"TI_AircraftType" => "Aircraft Type:\t\t",
				"TI_CartageDeliveryAddressPostCode" => "To Postcode:\t\t",
				"TI_CartagePickupAddressPostCode" => "From Postcode:\t\t",
				"TI_ContractNumberLinked" => "Contract Linked:\t",
				"TI_RX_NKCurrency" => "Currency:\t\t",
				"TI_RC" => "Container/Equipment Type:",
				"TI_GC_Publisher" => "Global Company Publisher:\r\n\t\t\t",
				"OriginSuburbPK" => "Origin Suburb PK:\t",
				"DestinationSuburbPK" => "Destination Suburb PK:\t",
				"TI_MatchContainerRateClass" => "Is Container Class Match:\r\n\t\t\t",
				"TI_CYC_WW_Facility" => "Container Yard Facility Charges:",
				"TI_RS_NKGatewayServiceLevel" => "Gateway Service Level:\t",
				"TI_OA_CartageDeliveryAddressOverride" => "Delivery/Consignee Address:",
				"TI_OA_CartagePickupAddressOverride" => "Pickup/Consignor Address:",
				"TI_IsTact" => "Is TACT Rate:\t\t",
				"TI_ParentID" => "Parent ID:\t\t",
				"TI_ParentTableCode" => "Parent Table Code:\t",
				"ReservedForJobIDs" => "Reserved For Job IDs:\t",
				"TI_PlannedLoadLRC" => "Planned Load:\t\t",
				"TI_PlannedDischargeLRC" => "Planned Discharge:\t",
				"TI_RateOrigin" => "Rate Origin:\t\t",
				"TI_RateDestination" => "Rate Destination:\t",
				"TI_IsExcludedFromAutoRating" => "Exclude from Autocosting:\r\n\t\t\t",
				"TI_ShipmentConsolidationStatus" => "Shipment Consolidation Status:\r\n\t\t\t",
				_ => null,
			};

		void TestGetRateLineDescription<T>(string descriptionKey, T value, string expectedValue = null, bool shouldContainDescription = true)
		{
			var orgHeader = Helper.NewOrgHeader();
			var shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();

			var autoRating = new AutoRatingProxy(shipment.RatingAdapter);
			var criteria = new RatingCriteria(autoRating, Factory);

			var clientRate = Helper.NewClientRate(orgHeader);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "US", "FRT", 100);

			var prefix = GetDescriptionLinePrefix(descriptionKey);
			AssertNotNull("Cannot find prefix based on the key", prefix);
			rateEntry[descriptionKey] = value;

			var rateLine = rateEntry.ChildRateLines.First();

			var description = DescriptionHelpers.GetRateLineDescription(rateLine, criteria);

			if (value is ZGuid || !shouldContainDescription)
			{
				AssertNotContains($"{prefix}{expectedValue ?? value.ToString()}", description);
			}
			else
			{
				AssertContains($"{prefix}{expectedValue ?? value.ToString()}", description);
			}
		}

		public void TestDescribeAllRateEntryFields_DoesNotContainManuallyAddedColumns() {
			var entryDescription = new ZStringBuilder();
			var orgHeader = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(orgHeader);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "US", "FRT", 100);

			DescriptionHelpers.DescribeAllRateEntryFields(entryDescription, rateEntry);

			foreach (var column in DescriptionHelpers.manuallyDescribedOrIgnoredColumns)
			{
				AssertNotContains(entryDescription.ToString(), column);
			}
		}

		public void TestFormatWithTab_AppendsLine_WhenLongDescription() =>
			AssertEquals(
				"Format with tab returns adds new line",
				DescriptionHelpers.FormatWithTab("hellothere", 2, 4),
				"hellothere" + System.Environment.NewLine + "\t\t");

		public void TestFormatWithTab_CorrectlyAddsTabs() =>
			CombineAssertions(() =>
			{
				AssertEquals(
					"Format with tab adds correct number of tabs",
					DescriptionHelpers.FormatWithTab("hello", 8, 24),
					"hello\t\t\t");

				AssertEquals(
					"Format with tab adds no tabs when descriptionPrefix is exactly maxWidth",
					DescriptionHelpers.FormatWithTab("hellothere", 8, 10),
					"hellothere");
			});
	}
}
