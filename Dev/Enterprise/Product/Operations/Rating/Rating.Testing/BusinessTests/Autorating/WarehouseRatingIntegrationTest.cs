using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	class WarehouseRatingIntegrationTest : BaseRatingIntegrationTest
	{
		#region TestWarehouseReceive_CartageZoneCalculator

		[TestDate(2016, 01, 01)]
		public void TestWarehouseReceive_CartageZoneCalculator()
		{
			var client = Helper.NewOrgHeader();
			client.OH_Code = "CLIENT";

			var fromPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));
			var auZoneSet = Helper.CreateRateTransportZoneSet(client, CountryCodes.Australia);
			var auZone = auZoneSet.CreateRateTransportZoneForTest("AU Zone");
			auZone.CreateRateTransportZoneItemForTest(fromPostCode);

			var clientRate = Helper.NewClientRate(client);
			var whsEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			whsEntry.TI_WW_Warehouse = warehouse.PK;

			var inwardsCharge = Helper.ChargeCodes.NewConsolChargeCode("WHSINNY", "WHS1", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			var inwardsLine = whsEntry.AddRateLine(inwardsCharge, CartageZoneDistanceCalculator.Code, PkgUnit.Unit);
			var inwardsCalc = inwardsLine.GetCalculator<CartageZoneDistanceCalculator>();
			inwardsCalc.EquipmentType = EquipmentNeeded.Any;
			inwardsCalc.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 10m, auZone.PK);

			var pickUpAddress = Helper.NewOrgHeader().MainAddress;

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_DocketID = "JOB123";
			receive.WD_WW_Whs = warehouse.PK;
			receive.WD_OH_Client = client.PK;
			receive.PickUpAddressPK = pickUpAddress.PK;

			var part = Helper.NewOrgSupplierPart(client);
			var line = receive.Lines.AddNew();
			line.WE_OP = part.PK;
			line.WE_TransactionQuantity = 3;

			Factory.Save();

			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = inwardsCharge.AC_Code,
							JR_OSSellAmt = 30m,
							RevenueCalculationDescription = @"WHSINNY: 3 Unit(s) @ AUD 10.00/Unit
" + DescriptionHelpers.FormatWithTab("Zone:") + "CLIENT AU Zone"
					}
				};

			var expectedLogLines = @"Information: Matched 'AU Zone' for RateLine WHSINNY-CTZ-UNT-Client Rate CLIENT
	- Job Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'AU Zone' transport zone matched by Pick Up Address fallback";

			AutorateAndAssert(expected, receive, client, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(receive, "Log should contain expected lines", expectedLogLines);
		}

		[TestDate(2016, 01, 01)]
		public void TestWarehouseReceive_CartageZoneCalculatorMatchesGenericZone()
		{
			var client = Helper.NewOrgHeader();
			client.OH_Code = "CLIENT";

			var fromPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));
			var auZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia);
			var auZone = auZoneSet.CreateRateTransportZoneForTest("AU Zone");
			auZone.CreateRateTransportZoneItemForTest(fromPostCode);

			var clientRate = Helper.NewClientRate(client);
			var whsEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			whsEntry.TI_WW_Warehouse = warehouse.PK;

			var inwardsCharge = Helper.ChargeCodes.NewConsolChargeCode("WHSINNY", "WHS1", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			var inwardsLine = whsEntry.AddRateLine(inwardsCharge, CartageZoneDistanceCalculator.Code, PkgUnit.Unit);
			var inwardsCalc = inwardsLine.GetCalculator<CartageZoneDistanceCalculator>();
			inwardsCalc.EquipmentType = EquipmentNeeded.Any;
			inwardsCalc.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 10m, auZone.PK);

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_DocketID = "JOB123";
			receive.WD_WW_Whs = warehouse.PK;
			receive.WD_OH_Client = client.PK;
			receive.PickUpAddressPK = Helper.NewOrgHeader().MainAddress.PK;

			var part = Helper.NewOrgSupplierPart(client);
			var line = receive.Lines.AddNew();
			line.WE_OP = part.PK;
			line.WE_TransactionQuantity = 3;

			Factory.Save();

			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = inwardsCharge.AC_Code,
							JR_OSSellAmt = 30m,
							RevenueCalculationDescription = @"WHSINNY: 3 Unit(s) @ AUD 10.00/Unit
" + DescriptionHelpers.FormatWithTab("Zone:") + "AU Zone"
					}
				};

			AutorateAndAssert(expected, receive, client, autorateCosts: false);
		}

		[TestDate(2016, 01, 01)]
		public void TestWarehouseReceive_CartageZoneCalculatorMatchesRateZonesNotJobZones()
		{
			var client = Helper.NewOrgHeader();

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUMEL";

			//Create Transport Zone Sets
			var sydneyCityTown = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney"));
			AssertNotNull("Pre-condition", sydneyCityTown);

			var countryZoneSet = Helper.CreateRateTransportZoneSet(client, CountryCodes.Australia);
			var countryZone = countryZoneSet.CreateRateTransportZoneForTest("Gold Zone");
			countryZone.CreateRateTransportZoneItemForTest(sydneyCityTown);

			var cityZoneSet = Helper.CreateRateTransportZoneSet(client, "", sydneyCityTown);
			var silverZone = cityZoneSet.CreateRateTransportZoneForTest("Silver Zone");
			silverZone.CreateRateTransportZoneItemForTest(sydneyCityTown);

			//Create Client Rate
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;

			Helper.ChargeCodes.New("WCH1", "Warehouse Charge", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			var rateLine = rateEntry.AddRateLine("WCH1", CartageZoneDistanceCalculator.Code, PkgUnit.Unit);
			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;

			var expectedZones = new ZString[] { "Standard", "Gold Zone" };
			var actualZones = calculator.CartageZones.Cast<CartageZone>().Select(x => x.Description);
			AssertContainsExactElementsInAnyOrder("Pre-condition: calc should only contain standard and Gold zones", expectedZones, actualZones);

			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 10m, countryZone.PK);

			//Create Warehouse Receive
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_DocketID = "JOB123";
			receive.WD_WW_Whs = warehouse.PK;
			receive.WD_OH_Client = client.PK;
			receive.PickUpAddressPK = Helper.NewOrgHeader().MainAddress.PK;

			var part = Helper.NewOrgSupplierPart(client);
			var line = receive.Lines.AddNew();
			line.WE_OP = part.PK;
			line.WE_TransactionQuantity = 4;

			Factory.Save();

			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;

			var expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "WCH1",
						JR_OSSellAmt = 40m,
						RevenueCalculationDescription = @"4 Unit(s) @ AUD 10.00/Unit
" + DescriptionHelpers.FormatWithTab("Zone:") + "TESTORG1 Gold Zone"
					}
			};

			var expectedLogLines = @"Information: Matched 'Gold Zone' for RateLine WCH1-CTZ-UNT-Client Rate TESTORG1
	- Job Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Gold Zone' transport zone matched by Pick Up Address fallback";

			AutorateAndAssert(expected, receive, client, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(receive, "Log should contain expected lines", expectedLogLines);
		}

		public void TestWarehouseRating_CartageZoneCalculatorMatchesMostCorrectZoneSet()
		{
			var supplier = Helper.NewOrgHeader();
			supplier.OH_Code = "SUPORG";

			//Add Supplier's Zone Set for India
			var set1 = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.India, RatingConstants.RatingZoneTypes.Rating, RateMode.ALL);
			var zone1 = set1.CreateRateTransportZoneForTest("Zone1");
			var zone2 = set1.CreateRateTransportZoneForTest("Zone2");

			//Add generic Zone Set for India
			var genericZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.India, RatingConstants.RatingZoneTypes.Rating, RateMode.ALL);
			genericZoneSet.CreateRateTransportZoneForTest("Generic Zone");

			//Add Supplier's Zone Set for a City Town in Indian
			var cityTown = Factory.NewWithValidTestData<RefCityTown>();
			cityTown.R9_RN_NKCountry = CountryCodes.India;
			cityTown.R9_InternationalName = "Indian City Name";
			cityTown.R9_RW_NKState = "MH";

			var cityTownTransportZoneSet = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.India, RatingConstants.RatingZoneTypes.Rating, RateMode.ALL);
			cityTownTransportZoneSet.TP_R9_ZoneHubLocation = cityTown.PK;
			var cityTownZone = cityTownTransportZoneSet.CreateRateTransportZoneForTest("City Town Zone");

			//Add generic Zone Set for Australia
			var genericAUZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating);
			var genericAUZone = genericAUZoneSet.CreateRateTransportZoneForTest("Generic AU Zone");

			//Add Warehouse for Indian location
			var whsAddress = Helper.NewOrgHeader().MainAddress;
			whsAddress.OA_RL_NKRelatedPortCode = "INBOM";
			var whs = Factory.NewWithValidTestData<WhsWarehouse>();
			whs.WW_OA_WarehouseAddress = whsAddress.PK;

			//Create Rate and ChargeCode
			Helper.ChargeCodes.New("WCH1", "Warehouse Charge", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			var rate = Helper.NewClientRate(supplier);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry.TI_WW_Warehouse = whs.PK;
			var line = entry.AddRateLine("WCH1", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			Factory.Save();

			var rateKey = $"RateLinesLookups.Transport.SUPORG.{whsAddress.PK}.ALL";

			AssertEquals("Should only include the client specific zone set for India", 2, line.Lookups.Zones.Count);
			Assert(line.Lookups.Zones.ContainsCode(zone1.TZ_ZoneName));
			Assert(line.Lookups.Zones.ContainsCode(zone2.TZ_ZoneName));

			whsAddress.OA_RL_NKRelatedPortCode = "CNSHA";
			Factory.ClearCachedValue<CodeDescriptionPairList>(rateKey);

			AssertEquals(0, line.Lookups.Zones.Count);

			whsAddress.OA_RL_NKRelatedPortCode = "INBOM";
			whsAddress.OA_RN_NKCountryCode = CountryCodes.India;
			whsAddress.OA_City = cityTown.R9_InternationalName;
			Factory.Save();
			Factory.ClearCachedValue<CodeDescriptionPairList>(rateKey);

			AssertEquals("Warehouse address should consider city town to be the this city town", cityTown.PK, ((ILocation)whsAddress).CityTown.PK);
			AssertEquals("When city town is available we should prefer it ", true, line.Lookups.Zones.ContainsCode(cityTownZone.TZ_ZoneName));
			AssertEquals(false, line.Lookups.Zones.ContainsCode(zone2.TZ_ZoneName));

			entry.TI_WW_Warehouse = ZGuid.Empty;
			entry.AllWarehouses = true;
			Factory.ClearCachedValue<CodeDescriptionPairList>(rateKey);

			Assert("Should use current company country when there is no warehouse", line.Lookups.Zones.ContainsCode(genericAUZone.TZ_ZoneName));
		}

		#endregion

		#region TestWarehouseReceive_SellUsingCostBasedCalc

		public void TestWarehouseReceive_SellUsingCostBasedCalc()
		{
			var chargeCode = Helper.ChargeCodes.New("WCH", "Warehouse Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			var client = Helper.NewOrgHeader();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var costing = Helper.NewCosting(warehouse.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = warehouse.PK;

			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Unit);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1.5m;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;

			var rateLine = rateEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode, PkgUnit.Unit);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 100m;
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 100m;

			Factory.Save();

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WW_Whs = warehouse.PK;
			receive.WD_OH_Client = client.PK;

			var part1 = Helper.NewOrgSupplierPart(client);
			var part2 = Helper.NewOrgSupplierPart(client);

			var line1 = receive.Lines.AddNew();
			line1.WE_OP = part1.PK;
			line1.WE_TransactionQuantity = 10;

			var line2 = receive.Lines.AddNew();
			line2.WE_OP = part2.PK;
			line2.WE_TransactionQuantity = 13;

			Factory.Save();

			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 30m,
					JR_OSCostAmt = 15m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 10 Unit(s) @ AUD 3.00/Unit",
					CostCalculationDescription = chargeCode.AC_Code + ": 10 Unit(s) @ AUD 1.50/Unit"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 39m,
					JR_OSCostAmt = 19.5m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 13 Unit(s) @ AUD 3.00/Unit",
					CostCalculationDescription = chargeCode.AC_Code + ": 13 Unit(s) @ AUD 1.50/Unit"
				},
			};

			AutorateAndAssert(expected, receive, client);
		}

		#endregion

		#region TestWarehouseOrder_NoZeroLineWhenMinimumCalculatorDoesntApply

		[TestDate(2013, 4, 12)]
		public void TestWarehouseOrder_NoZeroLineWhenMinimumCalculatorDoesntApply()
		{
			var localClient = Helper.NewOrgHeader();
			var whsHelper = new WhsTestHelperFunctions(Factory);
			var warehouse = whsHelper.CreateWarehouse("WHS", "A");

			var chargeCode = Helper.ChargeCodes.New("WHSCHG", "Warehouse Charge", "UNT", "WOU");

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;

			var minRateLine = rateEntry.AddRateLine(chargeCode, MinimumCalculator.Code);
			minRateLine.GetCalculator<MinimumCalculator>().IsJobMinimum = false;
			minRateLine.GetCalculator<MinimumCalculator>().IsChargeCodeMinimum = true;
			minRateLine.GetCalculator<MinimumCalculator>().MinimumValue = 250;

			var untRateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Unit);
			untRateLine.GetCalculator<UnitCalculator>().PerUnit = 5;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PartNum";
			part.RelatedOrganisations.AddOwner(localClient);
			whsHelper.CreateWhsReceiveWithInventory(localClient, warehouse, "R1", part, 200);
			Factory.Save();

			var whsOrder = whsHelper.CreateWhsOrder(localClient, warehouse);
			var whsOrderLine = whsHelper.CreateWhsOrderLine(whsOrder, part, 200);
			whsHelper.CreatePickNew(whsOrder);

			AssertEquals(200m, whsOrderLine.SumOfUnitsMet);

			var expected = new[] { new AssertionCharge { ChargeCode = "WHSCHG", JR_OSSellAmt = 1000 } };

			AutorateAndAssert("200 units in total x5 per unit = 1000", expected, whsOrder, localClient);
		}

		#endregion

		#region TestWarehouseReceive_TwoUnitCalculators

		[TestDate(2016, 01, 14)]
		public void TestWarehouseReceive_TwoUnitCalculators()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;

			var localClient = data.Org1;
			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = data.Whs1.PK;
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-5);
			rateEntry.TI_RateEndDate = ZDate.Empty;

			var chargeCode = Helper.ChargeCodes.New("WPUT", "Warehouse Putaway Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			// Package rate
			var rateLinePackage = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Package);
			rateLinePackage.GetCalculator<UnitCalculator>().PerUnit = 15m;
			rateLinePackage.TL_IsWhsJobLevelCharge = true;
			rateLinePackage.UseOnlyActualWeightMeasure = true;
			rateLinePackage.TL_ActualPercentage = 100;

			// Pallet rate
			var rateLinePallet = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Pallet);
			rateLinePallet.GetCalculator<UnitCalculator>().PerUnit = 5m;
			rateLinePallet.TL_IsWhsJobLevelCharge = true;
			rateLinePallet.UseOnlyActualWeightMeasure = true;
			rateLinePallet.TL_ActualPercentage = 100;

			var receive = WarehouseTestHelper.CreateWhsReceive(localClient, data.Whs1, "R1");
			receive.WD_FinalisedDate = ZDateTimeOffset.Today.AddDays(-5);
			receive.WD_TotalPallets = 16;
			receive.WD_PackagesSent = 0;
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 80m
				}
			};

			AutorateAndAssert("5$ * 16 pallets + 15$ * 0 packages", expected, receive, localClient);
		}

		#endregion

		#region TestWarehouseReceive_OnlyLocalClientSellRatesAreApplied

		[TestDate(2016, 01, 14)]
		public void TestWarehouseReceive_OnlyLocalClientSellRatesAreApplied()
		{
			var chargeCode1 = Helper.ChargeCodes.New("WHSIN1", "LocalClient", FlatCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			var chargeCode2 = Helper.ChargeCodes.New("WHSIN2", "Consignee", FlatCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			var data = new TestDataSimpleEnvironment(Factory);

			var localClient = data.Org1;
			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode1, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 150m;

			var consignee = Helper.NewOrgHeader();
			rate = Helper.NewClientRate(consignee);
			rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateLine = rateEntry.AddRateLine(chargeCode2, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var receive = WarehouseTestHelper.CreateWhsReceiveWithInventory(localClient, data.Whs1, "R1", data.Part1, 1m);
			receive.WD_FinalisedDate = ZDateTimeOffset.Today;
			receive.WD_TotalPallets = 16;
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;

			var consigneeDocAddress = receive.DocAddresses.AddNew(DocAddressType.ConsigneeAddress);
			consigneeDocAddress.E2_OA_Address = consignee.MainAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode1.AC_Code,
					JR_OSSellAmt = 150m
				}
			};

			AutorateAndAssert("Expected only the local client sell rate to be found", expected, receive, localClient);
		}

		#endregion

		#region TestWarehouseReceive_TimeCalculatorPerWeekPerUnit

		[TestDate(2016, 01, 01)]
		public void TestWarehouseReceive_TimeCalculatorPerWeekPerUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory)
			{
				Part1 =
				{
					OP_Height = 2,
					OP_Width = 2,
					OP_Depth = 2,
					OP_MeasureUQ = Length.Metres,
					OP_Weight = 2,
					OP_WeightUQ = Weight.Kilograms
				}
			};

			var rate = Helper.NewClientRate(data.Org1);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = data.Whs1.PK;
			rateEntry.TI_RateStartDate = new ZDate(2015, 01, 01);
			rateEntry.TI_RateEndDate = ZDate.Empty;

			var chargeCode = Helper.ChargeCodes.New("WCH", "Warehouse Chargeable Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, PkgUnit.Unit);
			var rateLineItem1 = rateLine.RateLineItems.AddNew();
			rateLineItem1.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem1.TM_BreakWeightVolume = QuantityUnit.WK;
			rateLineItem1.TM_Value = 10;

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WW_Whs = data.Whs1.PK;
			receive.WD_OH_Client = data.Org1.PK;
			receive.WD_FinalisedDate = new ZDateTimeOffset(2015, 4, 20);
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;

			Factory.Save();

			var line = receive.Lines.AddNew();
			line.WE_OP = data.Part1.PK;
			line.WE_TransactionQuantity = 55;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 1650m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 165 Unit x Week (55 Unit(s) x 3 Week(s)) @ AUD 10.00/Unit x Week"
				}
			};

			AutorateAndAssert(expected, receive, data.Org1);
		}

		#endregion

		#region TestWarehouseReceive_ClientServiceLevel

		public void TestWarehouseReceive_ClientServiceLevel()
		{
			var localClient = Helper.NewOrgHeader();

			// create service Level and carrier Service Level
			var refServiceLevel = Factory.New<RefServiceLevel>();
			refServiceLevel.RS_Code = "EXP";
			refServiceLevel.RS_Description = "Express Service";

			// create the charge code
			var chargeCode = Helper.ChargeCodes.New("EXPFST", "EXP and FST", "WPK", "WIN");

			// create rate entries for the client
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var rate = Helper.NewClientRate(localClient);

			void CreateRateEntryAndLine(string serviceLevel, string carrierServiceLevel, decimal baseValue)
			{
				var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL);
				rateEntry.TI_PL_NKCarrierServiceLevel = carrierServiceLevel;
				rateEntry.TI_RS_NKServiceLevel_NI = serviceLevel;
				rateEntry.TI_RH_NKCommodityCode = "";
				rateEntry.RateLines.RemoveAndDeleteAll();

				var rateLine = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code);
				rateLine.GetCalculator<FlatCalculator>().BaseRate = baseValue;
			}

			CreateRateEntryAndLine("EXP", "FST", 25m);
			CreateRateEntryAndLine("", "FST", 20m);
			CreateRateEntryAndLine("EXP", "", 15m);
			CreateRateEntryAndLine("", "", 10m);

			// create receives with different service levels
			var receiveWithoutExpAndWithFST = CreateWhsReceiveWithLines("", "FST", "FAST SERVICE");
			var receiveWithExpAndFST = CreateWhsReceiveWithLines("EXP", "FST", "FAST SERVICE");
			var receiveWithExpAndWithoutFST = CreateWhsReceiveWithLines("EXP", "", "");
			var receiveWithoutServiceLevels = CreateWhsReceiveWithLines("", "", "");

			WhsReceive CreateWhsReceiveWithLines(string serviceLevel, string carrierServiceLevel, string carrierServiceLevelDescription)
			{
				var transportCo = Helper.NewOrgHeader();
				var transportCoServiceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
				transportCoServiceLevel.PL_Code = carrierServiceLevel;
				transportCoServiceLevel.PL_CarrierServiceLevelDescription = carrierServiceLevelDescription;

				var receive = Factory.NewWithValidTestData<WhsReceive>();
				receive.WD_WW_Whs = warehouse.PK;
				receive.WD_OH_Client = localClient.PK;
				receive.TransportCoPK = transportCo.PK;
				receive.WD_RS_NKServiceLevel = serviceLevel;
				receive.WD_PL_NKCarrierServiceLevel = carrierServiceLevel;

				return receive;
			}

			Factory.Save();

			AutorateAndAssertReceive("EXPFST", 25m, localClient, receiveWithExpAndFST);
			AutorateAndAssertReceive("EXPFST", 20m, localClient, receiveWithoutExpAndWithFST);
			AutorateAndAssertReceive("EXPFST", 15m, localClient, receiveWithExpAndWithoutFST);
			AutorateAndAssertReceive("EXPFST", 10m, localClient, receiveWithoutServiceLevels);
		}

		[TestDate(2013, 4, 12)]
		public void TestWarehouseReceive_AdditionalServices()
		{
			var localClient = Helper.NewOrgHeader();

			// create the charge codes
			var chargeCode = Helper.ChargeCodes.New("EXPFST", "EXP and FST", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, FreightServiceType.Codes.SteamCleaning);
			var chargeCodeWithNoService = Helper.ChargeCodes.New("WFUM", "Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, FreightServiceType.Codes.Fumigation);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var whsReceive = Factory.NewWithValidTestData<WhsReceive>();
			whsReceive.WD_WW_Whs = warehouse.PK;
			whsReceive.WD_OH_Client = localClient.PK;
			whsReceive.WD_DocketSubType = "REC";
			whsReceive.WD_BookingDate = new ZDateTimeOffset(2013, 11, 4);
			whsReceive.WD_TotalWeight = 23;
			whsReceive.WD_TotalCubic = 1;

			var service = whsReceive.Services.AddNew();
			service.ES_ServiceCode = FreightServiceType.Codes.SteamCleaning;
			service.ES_Completed = new ZDateTime(2013, 11, 4);
			service.ES_ServiceCount = 5;

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_RateStartDate = new ZDate(2013, 1, 1);
			rateEntry.TI_RateEndDate = new ZDate(2014, 1, 1);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.SV);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 3;

			rateEntry.AddRateLine(chargeCodeWithNoService, UnitCalculator.Code, QuantityUnit.SV)
				.GetCalculator<UnitCalculator>().PerUnit = 7;

			Factory.Save();

			AutorateAndAssertReceive("EXPFST", 15m, localClient, whsReceive);
		}

		void AutorateAndAssertReceive(string chargeCode, decimal sellAmount, OrgHeader localClient, WhsReceive receiveWithExpAndFST)
		{
			var expectedCharge = new AssertionCharge
			{
				ChargeCode = chargeCode,
				JR_OSSellAmt = sellAmount
			};

			AutorateAndAssert(new[] { expectedCharge }, receiveWithExpAndFST, localClient, autorateCosts: false);
		}

		#endregion

		#region TestWarehouseReceive_Chargeable_ContainerType

		public void TestWarehouseReceive_ChargeableContainer_NoContainerTypeSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;

			var chargeCode = Helper.ChargeCodes.New("WHSCHG", "Warehouse Receipt", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			var clientRate = Helper.NewClientRate(data.Org1);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = data.Whs1.PK;
			rateEntry.TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-5);
			rateEntry.TI_RateEndDate = ZDate.Empty;

			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Pallet);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 5m;
			rateLine.TL_IsWhsJobLevelCharge = true;
			rateLine.UseOnlyActualWeightMeasure = true;
			rateLine.TL_ActualPercentage = 100;

			var receive = WarehouseTestHelper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_FinalisedDate = ZDateTimeOffset.Today.AddDays(-5);
			receive.WD_TotalPallets = 16;
			receive.WD_PackagesSent = 0;
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;

			Factory.Save();

			var whsDocketContainer = Factory.NewWithValidTestData<WhsDocketContainer>();
			whsDocketContainer.WC_ContainerNum = "ABC123";
			whsDocketContainer.WC_IsPalletised = true;
			whsDocketContainer.WC_IsChargeable = true;
			whsDocketContainer.WC_ItemCount = 3;
			whsDocketContainer.WC_PalletCount = 3;
			whsDocketContainer.WC_WD = receive.PK;

			AutorateAndAssert(null, receive, data.Org1);
		}

		#endregion

		#region TestWarehouseReceive_Chargeable_WhsInwards

		public void TestWarehouseReceive_Chargeable_WhsInwards_CostLine()
		{
			SetWarehouseHandlingChargeableFactor();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;

			var chargeCode = Helper.ChargeCodes.New("WCH", "Warehouse Chargeable Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			var costing = Helper.NewCosting(data.Whs1.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = data.Whs1.PK;
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			costLine.TL_IsWhsJobLevelCharge = true;
			costLine.UseOnlyActualWeightMeasure = true;
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var localClient = data.Org1;
			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Unit);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WW_Whs = data.Whs1.PK;
			receive.WD_OH_Client = data.Org1.PK;

			var line = receive.Lines.AddNew();
			line.WE_OP = data.Part1.PK;
			line.WE_TransactionQuantity = 1;

			Factory.Save();

			var expectedChargeWhenUsingActualWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 2m,
					CostCalculationDescription = chargeCode.AC_Code + ": 2 Kilogram(s) @ AUD 1.00/KG"
				}
			};
			AutorateAndAssert(expectedChargeWhenUsingActualWeight, receive, data.Org1);

			costLine.UseOnlyActualWeightMeasure = false;
			Factory.Save();

			var expectedWhenUsingChargeableWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 2667m,
					CostCalculationDescription = chargeCode.AC_Code + ": 2667 Kilogram(s) @ AUD 1.00/KG"
				}
			};
			AutorateAndAssert(expectedWhenUsingChargeableWeight, receive, data.Org1);
		}

		public void TestWarehouseReceive_Chargeable_WhsInwards_RateLine()
		{
			SetWarehouseHandlingChargeableFactor();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;

			var chargeCode = Helper.ChargeCodes.New("WCH", "Warehouse Chargeable Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			var costing = Helper.NewCosting(data.Whs1.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = data.Whs1.PK;

			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Unit);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var localClient = data.Org1;
			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.TL_IsWhsJobLevelCharge = true;
			rateLine.UseOnlyActualWeightMeasure = true;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WW_Whs = data.Whs1.PK;
			receive.WD_OH_Client = localClient.PK;

			var line = receive.Lines.AddNew();
			line.WE_OP = data.Part1.PK;
			line.WE_TransactionQuantity = 1;

			Factory.Save();

			var expectedChargeWhenUsingActualWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Kilogram(s) @ AUD 1.00/KG"
				}
			};
			AutorateAndAssert(expectedChargeWhenUsingActualWeight, receive, localClient);

			rateLine.UseOnlyActualWeightMeasure = false;
			Factory.Save();

			var expectedWhenUsingChargeableWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2667m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2667 Kilogram(s) @ AUD 1.00/KG"
				}
			};
			AutorateAndAssert(expectedWhenUsingChargeableWeight, receive, localClient);
		}

		public void TestWarehousePeriodicBilling()
		{
			SetWarehouseHandlingChargeableFactor();
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;

			data.Part2.OP_Height = 3;
			data.Part2.OP_Width = 2;
			data.Part2.OP_Depth = 4;
			data.Part2.OP_MeasureUQ = Length.Metres;
			data.Part2.OP_Weight = 6;
			data.Part2.OP_WeightUQ = Weight.Kilograms;

			var chargeCode = Helper.ChargeCodes.New("WCS", "Warehouse Chargeable Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);

			var clientRate = Helper.NewClientRate(data.Org1);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.UseOnlyActualWeightMeasure = true;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var receive1 = WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", now.ToOffset().AddDays(-4), data.Part1, 1m);
			var receive2 = WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", now.ToOffset().AddDays(-2), data.Part2, 3m);
			var receiveJob1 = new JobHeader.Loader(receive1).TryLoadOrCreate() as Job;
			var receiveJob2 = new JobHeader.Loader(receive2).TryLoadOrCreate() as Job;

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = now.AddDays(-6);
			invoice.ET_StorageToDate = now;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2m,
					RevenueCalculationDescription = "WCS: 2 Kilogram(s) @ AUD 1.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 18m,
					RevenueCalculationDescription = "WCS: 18 Kilogram(s) @ AUD 1.00/KG"
				}
			};

			var interactor = new TestInteractor();
			AutorateAndAssert("", expected, invoice, data.Org1, testInteractor: interactor);
			AssertEquals("# of progressReports", 9, interactor.ProgressReports.Count);
		}

		public void TestWarehouseReceive_Chargeable_WhsInwards_SubChargeWarehouseStorage()
		{
			SetWarehouseHandlingChargeableFactor();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;

			var chargeCode = Helper.ChargeCodes.New("WCH", "Warehouse Chargeable Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);

			var costing = Helper.NewCosting(data.Whs1.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = data.Whs1.PK;

			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Unit);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var localClient = data.Org1;
			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.TL_IsWhsJobLevelCharge = true;
			rateLine.UseOnlyActualWeightMeasure = true;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WW_Whs = data.Whs1.PK;
			receive.WD_OH_Client = localClient.PK;

			var line = receive.Lines.AddNew();
			line.WE_OP = data.Part1.PK;
			line.WE_TransactionQuantity = 1;

			Factory.Save();

			var expectedChargeWhenUsingActualWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Kilogram(s) @ AUD 1.00/KG"
				}
			};
			AutorateAndAssert(expectedChargeWhenUsingActualWeight, receive, localClient);

			rateLine.UseOnlyActualWeightMeasure = false;
			Factory.Save();

			var expectedWhenUsingChargeableWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2667m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2667 Kilogram(s) @ AUD 1.00/KG"
				}
			};
			AutorateAndAssert(expectedWhenUsingChargeableWeight, receive, localClient);
		}

		public void TestWarehousePeriodicBilling_DoesNotClearExistingChargesFromReceipts()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;
			data.Org1.CompanyData.OB_IsDebtor = true;

			Factory.Save();

			var chargeCode = Helper.ChargeCodes.New("WCH", "Warehouse Chargeable Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			chargeCode.AC_ChargeType = ChargeType.Revenue;

			var receive = WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", now.ToOffset().AddDays(-4), data.Part1, 1m);
			var receiveJob = new JobHeader.Loader(receive).TryLoadOrCreate() as Job;

			var tax = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "CAPGST"));
			tax.SetRateNumerator_ForTestOnly(10);

			var existingCharge = receiveJob.Charges.AddNew();
			existingCharge.JR_SellRatingOverride = false;
			existingCharge.JR_CostRatingOverride = false;
			existingCharge.JR_AC = chargeCode.PK;
			existingCharge.JR_OH_SellAccount = data.Org1.PK;
			existingCharge.JR_AT_SellGSTRate = tax.PK;
			existingCharge.JR_RX_NKSellCurrency = "AUD";
			existingCharge.JR_OSSellAmt = 1000m;

			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = now.AddDays(-6);
			invoice.ET_StorageToDate = now;

			var invoiceJob = new JobHeader.Loader(invoice).TryLoadOrCreate() as Job;
			AssertEquals("Should have one charge from receipt", 1, invoiceJob.Charges.Count);
			AssertEquals("Should be the same charge", existingCharge.PK, invoiceJob.Charges[0].PK);

			Factory.Save();

			var expectedInvoiceCharge = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 1000m,
				}
			};

			AutorateAndAssert("Should not affecte the existing charge", expectedInvoiceCharge, invoice, data.Org1, job: invoiceJob);

			Factory.Save();
			var receiveCharge = (receive.JobHeader as Job).Charges.FirstOrDefault();
			AssertNotNull("Charge should not be removed from receive", receiveCharge);
			AssertEquals("Should be the same charge", existingCharge.PK, receiveCharge.PK);
		}

		[TestDate(2015, 10, 11)]
		public void TestWarehouseReceive_WarehousePackCalculatorDisplaysConversionRemainder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Nuka Cola";
			product.OP_StockKeepingUnit = Volume.Litre;
			var relatedOrg = product.RelatedOrganisations.AddNew();
			relatedOrg.OU_OH = data.Org1.PK;

			var productInCartons = product.PartUnits.AddNew();
			productInCartons.OF_QuantityInParent = 20m;
			productInCartons.OF_PackType = Volume.Litre;
			productInCartons.OF_ParentPackType = PkgUnit.Carton;

			var productInPallets = product.PartUnits.AddNew();
			productInPallets.OF_QuantityInParent = 250m;
			productInPallets.OF_PackType = Volume.Litre;
			productInPallets.OF_ParentPackType = PkgUnit.Pallet;

			Factory.Save();

			var chargeCode = Helper.ChargeCodes.New("WHS123", "Warehouse In", WarehousePackCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			var rateEntry = Helper.NewClientRate(data.Org1).AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_RateStartDate = ZDate.Today.AddDays(-5);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, PkgUnit.Unit);
			rateLine.Calculator.AddRateLineItem(PkgUnit.Carton, 0m, 1.25m);
			rateLine.Calculator.AddRateLineItem(PkgUnit.Pallet, 0m, 12m);

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WW_Whs = data.Whs1.PK;
			receive.WD_OH_Client = data.Org1.PK;
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.WD_BookingDate = ZDateTimeOffset.Now;
			receive.WD_TotalUnits = 400;

			var line = receive.Lines.AddNew();
			line.WE_OP = product.PK;
			line.WE_TransactionQuantity = 400;
			line.WE_StockOnHand = 400;
			line.ProductUQ = Volume.Litre;

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "WHS123",
							JR_OSSellAmt = 20.75m,
							RevenueCalculationDescription = @"WHS123: Product NUKA COLA - 1 Pallet @ AUD 12.00/PLT + 7 Carton @ AUD 1.25/CTN (There is a remainder of 10 L that could not be rated. No rates for product stock keeping unit (L) were found and no conversions were possible)"
						}
				};

			AutorateAndAssert(expected, receive, data.Org1);
		}

		public void TestWarehouseReceive_PerPackageCalculation_WithUnitConversion()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Humans";
			product.OP_StockKeepingUnit = PkgUnit.Box;
			var relatedOrg = product.RelatedOrganisations.AddNew();
			relatedOrg.OU_OH = data.Org1.PK;

			var productUnit = product.PartUnits.AddNew();
			productUnit.OF_QuantityInParent = 100m;
			productUnit.OF_PackType = PkgUnit.Box;
			productUnit.OF_ParentPackType = PkgUnit.Package;

			Factory.Save();

			var chargeCode = Helper.ChargeCodes.New("WOS", "Warehouse Outwards Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var rateEntry = Helper.NewClientRate(data.Org1).AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_RateStartDate = ZDate.Today.AddDays(-5);
			rateEntry.AddUnitRateLine(chargeCode.AC_Code, 10, PkgUnit.Package).TL_IsWhsJobLevelCharge = true;

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_UnitsSent = 400;
			order.WD_PackagesSent = 4;
			order.WD_F3_NKTotalPackType = PkgUnit.Package;

			helper.CreateWhsOrderLine(order, product, 400);

			Factory.Save();

			var expected = new[]
			{
				// The product is in boxes, 100 boxes per Package, since we have 400 boxes it results in 4 packages
				new AssertionCharge
				{
					ChargeCode = "WOS",
					JR_OSSellAmt = 40m,
					RevenueCalculationDescription = "WOS: 4 Package(s) @ AUD 10.00/Package"
				}
			};

			AutorateAndAssert(expected, order, data.Org1);
		}

		#region TestWarehouseReceive_ClientRatesSupplier

		#region TestWarehouseReceive_ClientRatesWithNoSupplier

		public void TestWarehouseReceive_ClientRatesWithNoSupplier()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var supplier = CreateSupplierAsForwarder(client);

			var clientRate = Helper.NewClientRate(client);
			var clientChargeCode = Helper.ChargeCodes.New("WHS123", "Warehouse Inwards", WarehousePackCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			CreateClientCharge(clientChargeCode, 2.00m, ZDate.Today.AddDays(-5), clientRate, consignor: supplier);
			CreateClientCharge(clientChargeCode, 3.00m, ZDate.Today.AddDays(-5), clientRate, consignor: null);

			var receive = StoreProductInWarehouse(client, "R1", "Chocolate", 200);
			Factory.Save();

			var expectedCharge = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "WHS123",
							JR_LocalSellAmt = 600.00m,
							RevenueCalculationDescription = @"WHS123: 200 Unit (Product CHOCOLATE) @ AUD 3.00/UNT"
						}
				};

			AutorateAndAssert("Receive does not have supplier it should match with charge with no supplier.", expectedCharge, receive, client);
		}

		#endregion

		#region TestWarehouseReceive_ClientRateWithSupplier

		public void TestWarehouseReceive_ClientRateWithSupplier()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var supplier = CreateSupplierAsForwarder(client);

			var clientRate = Helper.NewClientRate(client);
			var clientChargeCode = Helper.ChargeCodes.New("WHS123", "Warehouse Inwards", WarehousePackCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			CreateClientCharge(clientChargeCode, 2.00m, ZDate.Today.AddDays(-5), clientRate, consignor: supplier);
			CreateClientCharge(clientChargeCode, 3.00m, ZDate.Today.AddDays(-5), clientRate, consignor: null);

			var receive = StoreProductInWarehouse(client, "R1", "Chocolate", 200, supplier);
			Factory.Save();

			var expectedCharge = new[] { new AssertionCharge { RevenueCalculationDescription = "WHS123: 200 Unit (Product CHOCOLATE) @ AUD 2.00/UNT" } };

			AutorateAndAssert("Receive has supplier it should match with charge with supplier.", expectedCharge, receive, client);
		}

		public void TestWarehouseReceive_ClientRateWithMultipleSuppliers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var supplier1 = WarehouseTestHelper.CreateClient("Supplier1");
			var supplier2 = WarehouseTestHelper.CreateClient("Supplier2");
			var supplier3 = WarehouseTestHelper.CreateClient("Supplier3");

			var clientRate = Helper.NewClientRate(client);
			var clientChargeCode = Helper.ChargeCodes.New("WHS123", "Warehouse Inwards", WarehousePackCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			CreateClientCharge(clientChargeCode, 2.00m, ZDate.Today.AddDays(-5), clientRate, consignor: supplier1);
			CreateClientCharge(clientChargeCode, 3.00m, ZDate.Today.AddDays(-5), clientRate, consignor: supplier2);
			CreateClientCharge(clientChargeCode, 4.00m, ZDate.Today.AddDays(-5), clientRate, consignor: supplier3);
			CreateClientCharge(clientChargeCode, 5.00m, ZDate.Today.AddDays(-5), clientRate, consignor: null);

			var receive = StoreProductInWarehouse(client, "R1", "Chocolate", 200, supplier1);
			Factory.Save();

			var expectedCharge = new[] { new AssertionCharge { RevenueCalculationDescription = "WHS123: 200 Unit (Product CHOCOLATE) @ AUD 2.00/UNT" } };

			AutorateAndAssert("Receive has supplier it should match with charge with supplier.", expectedCharge, receive, client);
		}

		public void TestWarehouseReceive_DifferentClientRates()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var client = data.Org1;
			var supplier = CreateSupplierAsForwarder(client);

			var clientRate1 = Helper.NewClientRate(client);
			var clientChargeCode1 = Helper.ChargeCodes.New("WHS123", "Warehouse Inwards", WarehousePackCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			CreateClientCharge(clientChargeCode1, 3.00m, ZDate.Today.AddDays(-5), clientRate1, consignor: null);

			var clientRate2 = Helper.NewClientRate(supplier);
			var clientChargeCode2 = Helper.ChargeCodes.New("WHS456", "Warehouse Inwards2", WarehousePackCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			CreateClientCharge(clientChargeCode2, 3.00m, ZDate.Today.AddDays(-5), clientRate2, consignor: null);

			var receive = StoreProductInWarehouse(client, "R1", "Chocolate", 200, supplier);
			Factory.Save();

			var expectedCharge = new[] { new AssertionCharge { RevenueCalculationDescription = "WHS123: 200 Unit (Product CHOCOLATE) @ AUD 3.00/UNT" } };

			AutorateAndAssert("Receive should only create charges for local client, not supplier.", expectedCharge, receive, client);
		}

		#endregion

		#region TestWarehouseReceive_ClientRatesWithSupplier

		public void TestWarehouseReceive_MultipleClientRates()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var client2 = Helper.NewOrgHeader();
			var supplier = CreateSupplierAsForwarder(client1);
			SetSupplierAsForwarder(client2, supplier);

			var clientRateForClient1 = Helper.NewClientRate(client1);
			var clientRateForClient2 = Helper.NewClientRate(client2);
			var clientChargeCode = Helper.ChargeCodes.New("WHS123", "Warehouse Inwards", WarehousePackCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			CreateClientCharge(clientChargeCode, 3.00m, ZDate.Today.AddDays(-5), clientRateForClient2, consignor: supplier);
			CreateClientCharge(clientChargeCode, 6.00m, ZDate.Today.AddDays(-5), clientRateForClient1, consignor: null);

			var receive = StoreProductInWarehouse(client1, "R1", "Chocolate", 200, supplier);
			Factory.Save();

			var expectedCharge = new[] { new AssertionCharge { RevenueCalculationDescription = "WHS123: 200 Unit (Product CHOCOLATE) @ AUD 6.00/UNT" } };

			AutorateAndAssert("Although receive has supplier it should match with charge with no supplier since it's the best match.", expectedCharge, receive, client1);
		}

		#endregion

		#endregion

		[TestDate(2015, 10, 11)]
		public void TestWarehouseOrder_WarehousePackCalculatorRounding()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Chocolate";
			product.OP_StockKeepingUnit = PkgUnit.Unit;
			product.OP_CountDecimalPlaces = 2;

			var relatedOrg = product.RelatedOrganisations.AddNew();
			relatedOrg.OU_OH = data.Org1.PK;

			var chargeCode = Helper.ChargeCodes.New("WHS123", "Warehouse Outwards", WarehousePackCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);
			var rateEntry = Helper.NewClientRate(data.Org1).AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_RateStartDate = ZDate.Today.AddDays(-5);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, PkgUnit.Unit);
			rateLine.Calculator.AddRateLineItem(PkgUnit.Unit, 0m, 1.2345m);

			var whsHelper = new WhsTestHelperFunctions(Factory);
			var warehouse = whsHelper.CreateWarehouse("WHS", "A");
			whsHelper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R1", product, 200);

			Factory.Save();

			var whsOrder = whsHelper.CreateWhsOrder(data.Org1, warehouse);
			var whsOrderLine = whsHelper.CreateWhsOrderLine(whsOrder, product, 71.67m);
			var pick = whsHelper.CreatePickNew(whsOrder);

			AssertEquals(71.67m, whsOrderLine.SumOfUnitsMet);

			whsOrder.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			AssertEquals("Pre-condition", true, pick.IsFinalised);
			AssertEquals("Pre-condition", true, whsOrder.IsFinalised);

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "WHS123",
							JR_OSSellAmt = 88.48m,
							RevenueCalculationDescription = @"WHS123: 71.67 Unit (Product CHOCOLATE) @ AUD 1.2345/UNT"
						}
				};

			var roundings = new DefaultRoundingsCollection();
			var rounding = roundings.AddNew();
			rounding.Code = RatingConstants.RateCategory.WHS;
			rounding.RoundingType = RatingRoundingTypes.NoRounding;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				AutorateAndAssert(expected, whsOrder, data.Org1);
			}

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "WHS123",
							JR_OSSellAmt = 88.88m,
							RevenueCalculationDescription = @"WHS123: 72 Unit (Product CHOCOLATE) @ AUD 1.2345/UNT"
						}
				};

			roundings = new DefaultRoundingsCollection();
			rounding = roundings.AddNew();
			rounding.Code = RatingConstants.RateCategory.WHS;
			rounding.RoundingType = RatingRoundingTypes.UpTo1;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				AutorateAndAssert(expected, whsOrder, data.Org1);
			}
		}

		#endregion

		#region TestWarehouseOrder_Chargeable_WhsOutwards

		public void TestWarehouseOrder_Chargeable_WhsOutwards()
		{
			SetWarehouseHandlingChargeableFactor();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;

			var chargeCode = Helper.ChargeCodes.New("WCH", "Warehouse Chargeable Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);
			var costing = Helper.NewCosting(data.Whs1.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = data.Whs1.PK;

			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Unit);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var client = data.Org1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.TL_IsWhsJobLevelCharge = true;
			rateLine.UseOnlyActualWeightMeasure = true;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			WarehouseTestHelper.CreateWhsReceiveWithInventory(client, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var order = WarehouseTestHelper.CreateWhsOrderWithOrderLine(client, data.Whs1, "O1", data.Part1, 1m);
			var pick = WarehouseTestHelper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			AssertEquals("Pre-condition", true, pick.IsFinalised);
			AssertEquals("Pre-condition", true, order.IsFinalised);

			var expectedChargeWhenUsingActualWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Kilogram(s) @ AUD 1.00/KG"
				}
			};
			AutorateAndAssert(expectedChargeWhenUsingActualWeight, order, client);

			rateLine.UseOnlyActualWeightMeasure = false;
			Factory.Save();

			var expectedWhenUsingChargeableWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2667m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2667 Kilogram(s) @ AUD 1.00/KG"
				}
			};
			AutorateAndAssert(expectedWhenUsingChargeableWeight, order, client);
		}

		public void TestWarehouseOrder_Chargeable_WhsOutwards_SubChargeWarehouseStorage()
		{
			SetWarehouseHandlingChargeableFactor();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;

			var chargeCode = Helper.ChargeCodes.New("WCH", "Warehouse Chargeable Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);

			var costing = Helper.NewCosting(data.Whs1.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = data.Whs1.PK;

			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Unit);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var localClient = data.Org1;
			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.TL_IsWhsJobLevelCharge = true;
			rateLine.UseOnlyActualWeightMeasure = true;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var order = WarehouseTestHelper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = WarehouseTestHelper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			AssertEquals(true, pick.IsFinalised);
			AssertEquals(true, order.IsFinalised);

			var expectedChargeWhenUsingActualWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Kilogram(s) @ AUD 1.00/KG"
				}
			};
			AutorateAndAssert(expectedChargeWhenUsingActualWeight, order, data.Org1);

			rateLine.UseOnlyActualWeightMeasure = false;
			Factory.Save();

			var expectedWhenUsingChargeableWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2667m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2667 Kilogram(s) @ AUD 1.00/KG"
				}
			};
			AutorateAndAssert(expectedWhenUsingChargeableWeight, order, data.Org1);
		}

		#endregion

		#region TestWarehousePeriodicBilling_Chargeable_InvalidUnit

		[TestDate(2016, 7, 14)]
		public void TestWarehousePeriodicBilling_Chargeable_CalculateVolumeWhenWeightUnitInvalid()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AU2CO";

			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = "0";
			data.Part1.OP_StockKeepingUnit = "0";
			data.Part1.OP_Cubic = 2;
			data.Part1.OP_CubicUQ = Volume.CubicMetres;

			Factory.Save();

			var chargeCode = Helper.ChargeCodes.New("WCS", "Warehouse Chargeable Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);

			var costing = Helper.NewCosting(data.Whs1.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = data.Whs1.PK;
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var client = data.Org1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.M3);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			WarehouseTestHelper.CreateWhsReceiveWithInventory(client, data.Whs1, "R1", now.ToOffset().AddDays(-4), data.Part1, 1m);

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = now.AddDays(-6);
			invoice.ET_StorageToDate = now;

			Factory.Save();

			var expectedChargeWhenUsingInvalidWeightUnit = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Cubic Meter(s) @ AUD 1.00/M3"
				}
			};
			AutorateAndAssert(expectedChargeWhenUsingInvalidWeightUnit, invoice, client);

			var expectedLogLines = new[] { @"Information: RateLine Found WCS-UNT-KG-Costing 11112CO
Warning: RateLine Filtered WCS-UNT-KG-Costing 11112CO	reason:	Invalid Weight Unit in this Product Code: P1. Please use following valid unit types:
	DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN" };

			AssertAutoratingAuditLogNoteContainsLines(invoice, "Log should contain warning about invalid weight", expectedLogLines);
		}

		[TestDate(2016, 7, 14)]
		public void TestWarehousePeriodicBilling_Chargeable_CalculateWeightWhenVolumeUnitInvalid()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AU2CO";

			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;
			data.Part1.OP_StockKeepingUnit = "0";
			data.Part1.OP_Cubic = 2;
			data.Part1.OP_CubicUQ = "0";

			Factory.Save();

			var costing = Helper.NewCosting(data.Whs1.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = data.Whs1.PK;

			var chargeCode = Helper.ChargeCodes.New("WCS", "Warehouse Chargeable Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.M3);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var chargeCode2 = Helper.ChargeCodes.New("WC2", "Warehouse Chargeable 2", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			var costLine2 = costEntry.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.KG);
			costLine2.GetCalculator<UnitCalculator>().PerUnit = 3m;

			var client = data.Org1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var receive = WarehouseTestHelper.CreateWhsReceiveWithInventory(client, data.Whs1, "R1", now.ToOffset().AddDays(-4), data.Part1, 1m);

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = now.AddDays(-6);
			invoice.ET_StorageToDate = now;

			Factory.Save();

			var expectedChargeWhenUsingInvalidVolumeUnit = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Kilogram(s) @ AUD 1.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode2.AC_Code,
					JR_OSCostAmt = 6m,
					CostCalculationDescription = chargeCode2.AC_Code + ": 2 Kilogram(s) @ AUD 3.00/KG"
				}
			};
			AutorateAndAssert(expectedChargeWhenUsingInvalidVolumeUnit, invoice, client);

			var expectedLogLine = @"Warning: RateLine Filtered WCS-UNT-M3-Costing 11112CO	reason:	Invalid Volume Unit in this Product Code: P1. Please use following valid unit types:
	CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE";

			AssertAutoratingAuditLogNoteContainsLines(invoice, "Log should contain warning about invalid volume unit", expectedLogLine);
		}

		[TestDate(2016, 7, 14)]
		public void TestWarehousePeriodicBilling_Chargeable_CalculateWeightWhenWeightUnitInvalid_DoesNotShowError()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = "ZZ";
			data.Part1.OP_StockKeepingUnit = "UNT";
			data.Part1.OP_Cubic = 0;
			data.Part1.OP_CubicUQ = Volume.CubicMetres;

			Factory.Save();

			var chargeCode = Helper.ChargeCodes.New("WCS", "Warehouse Chargeable Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);

			var costing = Helper.NewCosting(data.Whs1.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = data.Whs1.PK;

			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var client = data.Org1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var receive = WarehouseTestHelper.CreateWhsReceiveWithInventory(client, data.Whs1, "R1", now.ToOffset().AddDays(-4), data.Part1, 1m);

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = now.AddDays(-6);
			invoice.ET_StorageToDate = now;

			Factory.Save();

			using (var form = new ZForm(invoice))
			{
				var runner = new AutoRatingStarter(invoice, new AutoRatingGUIInteractor(form));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				var expectedMessage = @"	Invalid Weight Unit in this Product Code: P1. Please use following valid unit types:
	DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN";
				var actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains(expectedMessage, actualMessage);

				Assert("an invalid unit should not stop autorating", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Autorating could not be completed."));
			}
		}

		[TestDate(2016, 7, 14)]
		public void TestWarehousePeriodicBilling_Chargeable_CalculateVolumeWhenVolumeUnitInvalid()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AU2CO";

			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;
			data.Part1.OP_StockKeepingUnit = "0";
			data.Part1.OP_Cubic = 2;
			data.Part1.OP_CubicUQ = "0";

			Factory.Save();

			var chargeCode = Helper.ChargeCodes.New("WCS", "Warehouse Chargeable Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);

			var costing = Helper.NewCosting(data.Whs1.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = data.Whs1.PK;

			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.M3);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var client = data.Org1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var receive = WarehouseTestHelper.CreateWhsReceiveWithInventory(client, data.Whs1, "R1", now.ToOffset().AddDays(-4), data.Part1, 1m);

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = now.AddDays(-6);
			invoice.ET_StorageToDate = now;

			Factory.Save();

			var expectedCharge = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Kilogram(s) @ AUD 1.00/KG"
				}
			};

			AutorateAndAssert("Should calculate revenue value", expectedCharge, invoice, client);

			var expectedLogLine = @"Warning: RateLine Filtered WCS-UNT-M3-Costing 11112CO	reason:	Invalid Volume Unit in this Product Code: P1. Please use following valid unit types:
	CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE";

			AssertAutoratingAuditLogNoteContainsLines(invoice, "Log should contain invalid volume unit warning", expectedLogLine);
		}

		#endregion

		#region TestWarehousePeriodicBilling_Chargeable_WhsStorage

		public void TestWarehousePeriodicBilling_Chargeable_WhsStorage()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;

			var chargeCode = Helper.ChargeCodes.New("WCS", "Warehouse Chargeable Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);

			var costing = Helper.NewCosting(data.Whs1.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = data.Whs1.PK;
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Unit);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var localClient = data.Org1;
			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.UseOnlyActualWeightMeasure = true;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			WarehouseDataRegistry.Instance.WarehouseChargeableFactorStorage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GetDefaultWarehouseChargeableFactor());

			WarehouseTestHelper.CreateWhsReceiveWithInventory(localClient, data.Whs1, "R1", now.ToOffset().AddDays(-4), data.Part1, 1m);

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = localClient.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = now.AddDays(-6);
			invoice.ET_StorageToDate = now;

			Factory.Save();

			var expectedChargeWhenUsingActualWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Kilogram(s) @ AUD 1.00/KG"
				}
			};

			AutorateAndAssert(expectedChargeWhenUsingActualWeight, invoice, localClient);

			rateLine.UseOnlyActualWeightMeasure = false;
			Factory.Save();

			var expectedWhenUsingChargeableWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2667m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2667 Kilogram(s) @ AUD 1.00/KG"
				}
			};

			AutorateAndAssert(expectedWhenUsingChargeableWeight, invoice, localClient);
		}

		[TestDate(2016, 11, 24)]
		public void TestWarehousePeriodicBilling_Chargeable_WhsStorage_PointMatchingLog()
		{
			SetWarehouseHandlingChargeableFactor();
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;

			data.Part2.OP_Height = 2;
			data.Part2.OP_Width = 2;
			data.Part2.OP_Depth = 2;
			data.Part2.OP_MeasureUQ = Length.Metres;
			data.Part2.OP_Weight = 2;
			data.Part2.OP_WeightUQ = Weight.Kilograms;
			data.Part2.OP_RH_NKCommodityCode = "HAZ";

			var chargeCode = Helper.ChargeCodes.New("WCS", "Warehouse Chargeable Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage, "");

			var costing = Helper.NewCosting(data.Whs1.WarehouseAddress.Header);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = data.Whs1.PK;
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Unit);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var localClient = data.Org1;
			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_RH_NKCommodityCode = "HAZ";
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.UseOnlyActualWeightMeasure = true;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			WarehouseTestHelper.CreateWhsReceiveWithInventory(localClient, data.Whs1, "R1", now.ToOffset().AddDays(-4), data.Part1, 1m);
			WarehouseTestHelper.CreateWhsReceiveWithInventory(localClient, data.Whs1, "R2", now.ToOffset().AddDays(-4), data.Part2, 1m);

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = localClient.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = now.AddDays(-6);
			invoice.ET_StorageToDate = now;

			Factory.Save();

			var expectedChargeWhenUsingActualWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 2m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Kilogram(s) @ AUD 1.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 1m,
				}
			};
			AutorateAndAssert(expectedChargeWhenUsingActualWeight, invoice, localClient);

			var expectedLog = @"Information: AUTORATING REVENUE FOR Warehouse Periodic Invoice I00000001
Information: RatingHeader Found Client Rate 111 Entries: 1
Information: RateLine Found WCS-UNT-KG-Client Rate 111
Information: Chargeable was added for
				RateLine WCS-UNT-KG-Client Rate 111
					Job's info:
					Part P2: 2.000 Weight
					Part P2: 8.000 Volume
	RateLine WCS-UNT-KG-Client Rate 111 will not rate Part P1 by Weight	reason: expected '' Commodity while rate is for 'HAZ' Commodity
Information: CHARGES CALCULATED:
	WCS: 2 Kilogram(s) @ AUD 1.00/KG";

			AssertAutoratingAuditLogNoteContainsLines(invoice, "Log should contain information about skipped lines", expectedLog);
		}

		[TestDate(2017, 09, 07)]
		public void TestWarehousePeriodicBilling_PalletID_PointMatchingLog()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "P01");
			WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.DefaultLocation, "P01");
			WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, data.Whs1.DefaultLocation, "P02");
			WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 10m, data.Whs1.DefaultLocation, "");

			var rate = Helper.NewClientRate(data.Org1);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = data.Whs1.PK;
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-5);
			rateEntry.TI_RateEndDate = ZDate.Empty;

			var chargeCode = Helper.ChargeCodes.New("WPS", "Warehouse PalletID Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			var rateLinePalletID = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.PI);
			rateLinePalletID.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = now.AddDays(-6);
			invoice.ET_StorageToDate = now;

			Factory.Save();

			var expectedCharge = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 10m,
					JR_OSCostAmt = 10m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Pallet ID(s) @ AUD 5.00/Pallet ID"
				}
			};
			AutorateAndAssert(expectedCharge, invoice, data.Org1);

			var expectedLog = @"Information: AUTORATING REVENUE FOR Warehouse Periodic Invoice I00000001
Information: RatingHeader Found Client Rate 111 Entries: 1
Information: RateLine Found WPS-UNT-PI-Client Rate 111
Information: Chargeable was added for
				RateLine WPS-UNT-PI-Client Rate 111
					Job's info:
					Pallet P01: 1 PalletID
					Pallet P02: 1 PalletID
Information: CHARGES CALCULATED:
	WPS: 2 Pallet ID(s) @ AUD 5.00/Pallet ID
Information: Warehouse Periodic Invoice I00000001 was auto-rated.
	The following rates were found:
	  • WPS charge from Client Rate 111
	Charges created: WPS";
			AssertAutoratingAuditLogNoteContainsLines(invoice, "Log should contain information about skipped lines", expectedLog);
		}

		[TestDate(2017, 09, 07)]
		public void TestWarehousePeriodicBilling_PalletID_RenamePalletByInternalAdjusment_FullyAdjust()
		{
			var expectedCharge = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "WPS",
					JR_OSSellAmt = 10m,
					JR_OSCostAmt = 10m,
					RevenueCalculationDescription = "WPS: 2 Pallet ID(s) @ AUD 5.00/Pallet ID"
				}
			};

			var expectedLog = $@"Information: AUTORATING REVENUE FOR Warehouse Periodic Invoice I00000001
Information: RatingHeader Found Client Rate 111 Entries: 1
Information: RateLine Found WPS-UNT-PI-Client Rate 111
Information: Chargeable was added for
				RateLine WPS-UNT-PI-Client Rate 111
					Job's info:
					Pallet ABC: 1 PalletID
					Pallet P02: 1 PalletID
Information: CHARGES CALCULATED:
	WPS: 2 Pallet ID(s) @ AUD 5.00/Pallet ID
Information: Warehouse Periodic Invoice I00000001 was auto-rated.
	The following rates were found:
	  • WPS charge from Client Rate 111
	Charges created: WPS";

			TestWarehousePeriodicBilling_PalletID_RenamePalletByInternalAdjusment_Core(fullyAdjust: true, expectedCharge, expectedLog);
		}

		[TestDate(2017, 09, 07)]
		public void TestWarehousePeriodicBilling_PalletID_RenamePalletByInternalAdjusment_PartiallyAdjust()
		{
			var expectedCharge = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "WPS",
					JR_OSSellAmt = 15m,
					JR_OSCostAmt = 15m,
					RevenueCalculationDescription = "WPS: 3 Pallet ID(s) @ AUD 5.00/Pallet ID"
				}
			};

			var expectedLog = $@"Information: AUTORATING REVENUE FOR Warehouse Periodic Invoice I00000001
Information: RatingHeader Found Client Rate 111 Entries: 1
Information: RateLine Found WPS-UNT-PI-Client Rate 111
Information: Chargeable was added for
				RateLine WPS-UNT-PI-Client Rate 111
					Job's info:
					Pallet ABC: 1 PalletID
					Pallet P01: 1 PalletID
					Pallet P02: 1 PalletID
Information: CHARGES CALCULATED:
	WPS: 3 Pallet ID(s) @ AUD 5.00/Pallet ID
Information: Warehouse Periodic Invoice I00000001 was auto-rated.
	The following rates were found:
	  • WPS charge from Client Rate 111
	Charges created: WPS";

			TestWarehousePeriodicBilling_PalletID_RenamePalletByInternalAdjusment_Core(fullyAdjust: false, expectedCharge, expectedLog);
		}

		void TestWarehousePeriodicBilling_PalletID_RenamePalletByInternalAdjusment_Core(bool fullyAdjust, IEnumerable<AssertionCharge> expectedCharge,string expectedLog)
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageMax;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var location = data.Whs1.DefaultLocation;
			var inventoryArrivalDate = now.AddMonths(-2);
			WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", inventoryArrivalDate, data.Part1, 10m, location, "P01");
			WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", inventoryArrivalDate, data.Part1, 10m, location, "P01");
			WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", inventoryArrivalDate, data.Part1, 10m, location, "P02");
			WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", inventoryArrivalDate, data.Part1, 10m, location, "");

			var rate = Helper.NewClientRate(data.Org1);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = data.Whs1.PK;
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-5);
			rateEntry.TI_RateEndDate = ZDate.Empty;

			var chargeCode = Helper.ChargeCodes.New("WPS", "Warehouse PalletID Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			var rateLinePalletID = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.PI);
			rateLinePalletID.GetCalculator<UnitCalculator>().PerUnit = 5m;
			Factory.Save();

			var adjustment = WarehouseTestHelper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1");
			adjustment.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			WarehouseTestHelper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 20m, data.Whs1.DefaultLocation.ToLocationString(), "ABC", inventoryArrivalDate);
			WarehouseTestHelper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, (fullyAdjust ? -20 : -15), data.Whs1.DefaultLocation.ToLocationString(), "P01", inventoryArrivalDate);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			Assert("Precondition", adjustment.Lines.All(l => l.WE_AdjustmentArrivalDate == inventoryArrivalDate));
			AssertNotEquals("Precondition", inventoryArrivalDate.Date, adjustment.WD_FinalisedDate.Date);

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = now.ToDateTime().AddDays(-6);
			invoice.ET_StorageToDate = now.ToDateTime();

			Factory.Save();

			AutorateAndAssert(expectedCharge, invoice, data.Org1);
			AssertAutoratingAuditLogNoteContainsLines(invoice, "Log should contain expected lines.", expectedLog);
		}

		#endregion

		#region TestWarehousePeriodicBilling_NoTransactionsWithinBillingPeriod()

		[TestDate(2016, 10, 10)]
		public void TestWarehousePeriodicBilling_NoTransactionsWithinBillingPeriod_AllWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;

			var flatChargeCode = Helper.ChargeCodes.New("FLTWHS", "Flat Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			var flatWarehouseLevelChargeCode = Helper.ChargeCodes.New("FLTWHL", "Flat Warehouse Level Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			var perUnitCharge = Helper.ChargeCodes.New("UNTWHS", "Per Unit Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);

			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);

			var costLine1 = costEntry.AddRateLine(flatChargeCode, FlatCalculator.Code);
			costLine1.GetCalculator<FlatCalculator>().BaseRate = 150m;

			var costLine2 = costEntry.AddRateLine(flatWarehouseLevelChargeCode, FlatCalculator.Code);
			costLine2.TL_IsWhsJobLevelCharge = true;
			costLine2.GetCalculator<FlatCalculator>().BaseRate = 80m;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);

			var rateLine1 = rateEntry.AddRateLine(flatChargeCode, FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 210m;

			var rateLine2 = rateEntry.AddRateLine(flatWarehouseLevelChargeCode, FlatCalculator.Code);
			rateLine2.TL_IsWhsJobLevelCharge = true;
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 90m;

			var rateLine3 = rateEntry.AddRateLine(perUnitCharge, UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var periodicBilling = Factory.New<WhsInvoice>();
			periodicBilling.ET_OH_Client = client.PK;
			periodicBilling.ET_WW = warehouse.PK;
			periodicBilling.ET_StorageFromDate = new ZDate(2016, 10, 04);
			periodicBilling.ET_StorageToDate = new ZDate(2016, 10, 10);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = flatChargeCode.AC_Code,
					JR_OSCostAmt = 150m,
					JR_OSSellAmt = 210m,
				},
				new AssertionCharge
				{
					ChargeCode = flatWarehouseLevelChargeCode.AC_Code,
					JR_OSCostAmt = 80m,
					JR_OSSellAmt = 90m,
				}
			};

			var message = "There are no transactions during the billing period so we can't apply the unit charge "
				+ "but we should apply the charges that don't require measures";

			AutorateAndAssert(message, expected, periodicBilling, client);

			var expectedLines = @"Information: RatingHeader Found Client Rate 111 Entries: 1
Information: RateLine Found FLTWHL-FLT-Client Rate 111
Information: RateLine Found FLTWHS-FLT-Client Rate 111
Information: RateLine Found UNTWHS-UNT-KG-Client Rate 111
Information: RateLine Filtered UNTWHS-UNT-KG-Client Rate 111	reason:	failed similarity check";

			AssertAutoratingAuditLogNoteContainsLines(periodicBilling, "", expectedLines);
		}

		[TestDate(2016, 10, 10)]
		public void TestWarehousePeriodicBilling_NoTransactionsWithinBillingPeriod_WarehouseSpecificRateEntry()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;

			var flatChargeCode = Helper.ChargeCodes.New("FLTWHS", "Flat Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			var flatWarehouseLevelChargeCode = Helper.ChargeCodes.New("FLTWHL", "Flat Warehouse Level Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			var minimumCharge = Helper.ChargeCodes.New("MINWHS", "Minimum Charge", MinimumCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			var perKiloCharge = Helper.ChargeCodes.New("UNTWHSKG", "Per Kilo Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			var perUnitCharge = Helper.ChargeCodes.New("UNTWHSUNT", "Per Unit Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);

			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = warehouse.PK;

			var costLine1 = costEntry.AddRateLine(flatChargeCode, FlatCalculator.Code);
			costLine1.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var costLine2 = costEntry.AddRateLine(flatWarehouseLevelChargeCode, FlatCalculator.Code);
			costLine2.TL_IsWhsJobLevelCharge = true;
			costLine2.GetCalculator<FlatCalculator>().BaseRate = 80m;

			var costLine3 = costEntry.AddRateLine(perKiloCharge, UnitCalculator.Code, QuantityUnit.KG);
			costLine3.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var costLine4 = costEntry.AddRateLine(perUnitCharge, UnitCalculator.Code, PkgUnit.Unit);
			costLine4.GetCalculator<UnitCalculator>().PerUnit = 11m;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;

			var rateLine1 = rateEntry.AddRateLine(flatChargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 20m;

			var rateLine2 = rateEntry.AddRateLine(flatWarehouseLevelChargeCode, FlatCalculator.Code);
			rateLine2.TL_IsWhsJobLevelCharge = true;
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 90m;

			var rateLine3 = rateEntry.AddRateLine(perKiloCharge, UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine4 = rateEntry.AddRateLine(perUnitCharge, UnitCalculator.Code, PkgUnit.Unit);
			rateLine4.GetCalculator<UnitCalculator>().PerUnit = 22m;

			var periodicBilling = Factory.New<WhsInvoice>();
			periodicBilling.ET_OH_Client = client.PK;
			periodicBilling.ET_WW = warehouse.PK;
			periodicBilling.ET_StorageFromDate = new ZDate(2016, 10, 04);
			periodicBilling.ET_StorageToDate = new ZDate(2016, 10, 10);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = flatChargeCode.AC_Code,
					JR_OSCostAmt = 100m,
					JR_OSSellAmt = 120m,
				},
				new AssertionCharge
				{
					ChargeCode = flatWarehouseLevelChargeCode.AC_Code,
					JR_OSCostAmt = 80m,
					JR_OSSellAmt = 90m,
				}
			};

			var message = "There are no units being stored in the warehouse during the billing period but we should still apply flat measures";
			AutorateAndAssert(message, expected, periodicBilling, client);

			var expectedLines = @"Information: AUTORATING REVENUE FOR Warehouse Periodic Invoice I00000001
Information: RatingHeader Found Client Rate 111 Entries: 1
Information: RateLine Found FLTWHL-FLT-Client Rate 111
Information: RateLine Found FLTWHS-CST-Client Rate 111
Information: RateLine Found UNTWHSKG-UNT-KG-Client Rate 111
Information: RateLine Found UNTWHSUNT-UNT-UNT-Client Rate 111
Information: RateLine Filtered UNTWHSKG-UNT-KG-Client Rate 111	reason:	failed similarity check
Information: RateLine Filtered UNTWHSUNT-UNT-UNT-Client Rate 111	reason:	failed similarity check";

			AssertAutoratingAuditLogNoteContainsLines(periodicBilling, "", expectedLines);
		}

		[TestDate(2016, 10, 10)]
		public void TestWarehousePeriodicBilling_NoTransactionsWithinBillingPeriod_WarehouseSpecificRateEntry_MinimumCharge()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;

			var minimumCharge = Helper.ChargeCodes.New("MINWHS", "Minimum Charge", MinimumCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);

			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			costEntry.TI_WW_Warehouse = warehouse.PK;
			var costLine3 = costEntry.AddRateLine(minimumCharge, MinimumCalculator.Code);
			costLine3.GetCalculator<MinimumCalculator>().MinimumValue = 1000m;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			var rateLine = rateEntry.AddRateLine(minimumCharge, MinimumCalculator.Code);
			rateLine.GetCalculator<MinimumCalculator>().MinimumValue = 2000m;

			var periodicBilling = Factory.New<WhsInvoice>();
			periodicBilling.ET_OH_Client = client.PK;
			periodicBilling.ET_WW = warehouse.PK;
			periodicBilling.ET_StorageFromDate = new ZDate(2016, 10, 04);
			periodicBilling.ET_StorageToDate = new ZDate(2016, 10, 10);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = minimumCharge.AC_Code,
					JR_OSCostAmt = 1000m,
					JR_OSSellAmt = 2000m,
				}
			};

			AutorateAndAssert("Should apply minimum charges despite no transactions in the warehouse", expected, periodicBilling, client);
		}

		#endregion

		#region TestWarehouseOrder_SplitMonthBilling_ZeroChargeForStockThatLeavesInTheSameMonthAsArrives

		[TestDate(2016, 1, 20)]
		public void TestWarehouseOrder_SplitMonthBilling_ZeroChargeForStockThatLeavesInTheSameMonthAsArrives()
		{
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			#region setup the charges

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var chargeCode = Helper.ChargeCodes.New("WOS", "Warehouse Outwards Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var rateLine = rateEntry.AddRateLine(chargeCode, SplitMonthBillingCalculator.Code, "UNT");
			rateLine.Calculator["-15"] = (ZDecimal)2m;
			rateLine.Calculator["+15"] = (ZDecimal)4m;
			#endregion

			#region create receive for 10 units at 01 Feb
			var product = Helper.NewOrgSupplierPart(client);
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WW_Whs = warehouse.PK;
			receive.WD_OH_Client = client.PK;
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 02, 01);
			receive.WD_BookingDate = ZDateTimeOffset.Now;

			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);
			receive.RunPreSaveValidation();
			receiveLine.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;
			Factory.Save();

			#endregion

			#region create order for 7 units at 14 Feb

			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.WD_RequiredDate = new ZDateTimeOffset(year, 02, 14);
			order.ConsigneePK = client.PK;
			order.ConsigneeAddressPK = client.MainAddress.PK;

			var orderLine = order.Lines.AddNew();
			orderLine.WE_OP = product.PK;
			orderLine.WE_TransactionQuantity = 7;

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocket();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			orderLine.WE_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			AssertEquals(7m, orderLine.SumOfUnitsMet);
			Assert("Precondition: order is finalised", order.IsFinalised);
			Factory.Save();

			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 0m,
					RevenueCalculationDescription = "WOS: 0 Unit(s) (Released Before 15 Day(s)) @ AUD 2.00/Unit",
				}
			};

			AutorateAndAssert(expected, order, client);
		}

		#endregion

		#region TestWarehouseOrder_SplitMonthBilling_JobServicesContainSTG

		[TestDate(2016, 1, 20)]
		public void TestWarehouseOrder_SplitMonthBilling_JobServicesContainSTG()
		{
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			#region setup the charges

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);
			rateEntry.TI_RX_NKCurrency = "AUD";

			var chargeCode = Helper.ChargeCodes.New("WOS", "Warehouse Outwards Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var rateLine = rateEntry.AddRateLine(chargeCode, SplitMonthBillingCalculator.Code, "UNT");
			rateLine.Calculator["-15"] = (ZDecimal)2m;
			rateLine.Calculator["+15"] = (ZDecimal)4m;
			#endregion

			#region create receive for 10 units at 01 Feb
			var product = Helper.NewOrgSupplierPart(client);
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_BookingDate = ZDateTimeOffset.Now;
			receive.WD_WW_Whs = warehouse.PK;
			receive.WD_OH_Client = client.PK;
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 02, 01);

			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);
			receive.RunPreSaveValidation();
			receiveLine.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;
			Factory.Save();

			#endregion

			#region create order for 7 units at 14 Feb

			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.WD_RequiredDate = new ZDateTimeOffset(year, 02, 14);
			order.ConsigneePK = client.PK;
			order.ConsigneeAddressPK = client.MainAddress.PK;

			var orderLine = order.Lines.AddNew();
			orderLine.WE_OP = product.PK;
			orderLine.WE_TransactionQuantity = 7;

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocket();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			orderLine.WE_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			AssertEquals(7m, orderLine.SumOfUnitsMet);
			Assert("Precondition: order is finalised", order.IsFinalised);
			Factory.Save();

			#endregion

			var collection = (SystemDefinableCodeDescriptionBoolCollection)WarehouseDataRegistry.Instance.JobServices.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (collection.ContainsCode(ChargeCodeSubGroupList.Storage))
			{
				collection.Remove(collection.FindByCode(ChargeCodeSubGroupList.Storage));
			}
			var element = collection.AddNew();
			element.Code = ChargeCodeSubGroupList.Storage;

			using (WarehouseDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var allServiceTypes = order.Services.AddNew().Lookups.JobServiceType_List;
				Assert("precondition", allServiceTypes.ContainsCode(ChargeCodeSubGroupList.Storage));

				var expected = new[]
				{
					new AssertionCharge
					{
						JR_OSSellAmt = 0m,
						RevenueCalculationDescription = "WOS: 0 Unit(s) (Released Before 15 Day(s)) @ AUD 2.00/Unit",
					}
				};
				AutorateAndAssert(expected, order, client);
			}
		}

		#endregion

		#region TestWarehouseOrder_SplitMonthBilling_PartialChargeForOutwardsWhsStorage

		[TestDate(2016, 1, 20)]
		public void TestWarehouseOrder_SplitMonthBilling_PartialChargeForOutwardsWhsStorage()
		{
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			#region setup the charges

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var chargeCode = Helper.ChargeCodes.New("WOS", "Warehouse Outwards Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var rateLine = rateEntry.AddRateLine(chargeCode, SplitMonthBillingCalculator.Code, "UNT");
			rateLine.Calculator["-15"] = (ZDecimal)2m;
			rateLine.Calculator["+15"] = (ZDecimal)4m;

			#endregion

			#region create receive for 10 units at 10 Jan
			var product = Helper.NewOrgSupplierPart(client);
			var receive1 = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive1.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine1 = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive1, product, 10m);
			receive1.RunPreSaveValidation();
			receiveLine1.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine1.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive1.WD_FinalisedDate = receive1.WD_ArrivalDate;
			#endregion

			#region create receive for 5 units at 3 Feb
			var receive2 = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R2");
			receive2.WD_ArrivalDate = new ZDateTimeOffset(year, 02, 03);

			var receiveLine2 = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive2, product, 5m);
			receive2.RunPreSaveValidation();
			receiveLine2.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine2.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = receive2.WD_ArrivalDate;
			Factory.Save();
			#endregion

			#region create order for 13 units at 14 Feb
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.WD_RequiredDate = new ZDateTimeOffset(year, 02, 14);
			order.ConsigneePK = client.PK;
			order.ConsigneeAddressPK = client.MainAddress.PK;

			var orderLine = order.Lines.AddNew();
			orderLine.WE_OP = product.PK;
			orderLine.WE_TransactionQuantity = 13;

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocket();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			orderLine.WE_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			AssertEquals(13m, orderLine.SumOfUnitsMet);
			Assert("Precondition: order is finalised", order.IsFinalised);
			Factory.Save();
			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 16m,
					RevenueCalculationDescription = "WOS: 8 Unit(s) (Released Before 15 Day(s)) @ AUD 2.00/Unit",
				}
			};

			AutorateAndAssert(expected, order, client);
		}

		[TestDate(2020, 1, 20)]
		public void TestWarehouseOrder_SplitMonthBilling_PartialChargeForOutwardsWhsStorage_StorageWeightMeasureType()
		{
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			#region setup the charges

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var chargeCode = Helper.ChargeCodes.New("WOS", "Warehouse Outwards Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var rateLine = rateEntry.AddRateLine(chargeCode, SplitMonthBillingCalculator.Code, Weight.Kilograms);
			rateLine.Calculator["-15"] = (ZDecimal)2m;
			rateLine.Calculator["+15"] = (ZDecimal)4m;

			#endregion

			#region create receive for 10 units at 10 Jan
			var product = Helper.NewOrgSupplierPart(client);
			product.OP_Weight = 1m;
			product.OP_WeightUQ = Weight.Kilograms;
			product.OP_Cubic = 4m;
			product.OP_CubicUQ = Volume.CubicCentimeters;
			Factory.Save();

			var receive1 = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive1.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine1 = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive1, product, 10m);
			receive1.RunPreSaveValidation();
			receiveLine1.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine1.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive1.WD_FinalisedDate = receive1.WD_ArrivalDate;
			#endregion

			#region create receive for 5 units at 3 Feb
			var receive2 = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R2");
			receive2.WD_ArrivalDate = new ZDateTimeOffset(year, 02, 03);

			var receiveLine2 = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive2, product, 5m);
			receive2.RunPreSaveValidation();
			receiveLine2.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine2.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = receive2.WD_ArrivalDate;
			Factory.Save();
			#endregion

			#region create order for 13 units at 14 Feb
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.WD_RequiredDate = new ZDateTimeOffset(year, 02, 14);
			order.ConsigneePK = client.PK;
			order.ConsigneeAddressPK = client.MainAddress.PK;

			var orderLine = order.Lines.AddNew();
			orderLine.WE_OP = product.PK;
			orderLine.WE_TransactionQuantity = 13;

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocket();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			orderLine.WE_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			AssertEquals(13m, orderLine.SumOfUnitsMet);
			Assert("Precondition: order is finalised", order.IsFinalised);
			Factory.Save();
			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 16m,
					RevenueCalculationDescription = "WOS: 8 Kilogram(s) (Released Before 15 Day(s)) @ AUD 2.00/KG",
				}
			};

			AutorateAndAssert(expected, order, client);
		}

		[TestDate(2020, 1, 20)]
		public void TestWarehouseOrder_SplitMonthBilling_PartialChargeForOutwardsWhsStorage_StorageVolumeMeasureType()
		{
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			#region setup the charges

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var chargeCode = Helper.ChargeCodes.New("WOS", "Warehouse Outwards Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var rateLine = rateEntry.AddRateLine(chargeCode, SplitMonthBillingCalculator.Code, Volume.CubicCentimeters);
			rateLine.Calculator["-15"] = (ZDecimal)2m;
			rateLine.Calculator["+15"] = (ZDecimal)4m;

			#endregion

			#region create receive for 10 units at 10 Jan
			var product = Helper.NewOrgSupplierPart(client);
			product.OP_Weight = 1m;
			product.OP_WeightUQ = Weight.Kilograms;
			product.OP_Cubic = 4m;
			product.OP_CubicUQ = Volume.CubicCentimeters;
			Factory.Save();

			var receive1 = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive1.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine1 = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive1, product, 10m);
			receive1.RunPreSaveValidation();
			receiveLine1.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine1.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive1.WD_FinalisedDate = receive1.WD_ArrivalDate;
			#endregion

			#region create receive for 5 units at 3 Feb
			var receive2 = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R2");
			receive2.WD_ArrivalDate = new ZDateTimeOffset(year, 02, 03);

			var receiveLine2 = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive2, product, 5m);
			receive2.RunPreSaveValidation();
			receiveLine2.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine2.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = receive2.WD_ArrivalDate;
			Factory.Save();
			#endregion

			#region create order for 13 units at 14 Feb
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.WD_RequiredDate = new ZDateTimeOffset(year, 02, 14);
			order.ConsigneePK = client.PK;
			order.ConsigneeAddressPK = client.MainAddress.PK;

			var orderLine = order.Lines.AddNew();
			orderLine.WE_OP = product.PK;
			orderLine.WE_TransactionQuantity = 13;

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocket();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			orderLine.WE_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			AssertEquals(13m, orderLine.SumOfUnitsMet);
			Assert("Precondition: order is finalised", order.IsFinalised);
			Factory.Save();
			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 64m,
					RevenueCalculationDescription = "WOS: 32 Cubic Centimeters(s) (Released Before 15 Day(s)) @ AUD 2.00/CC",
				}
			};

			AutorateAndAssert(expected, order, client);
		}

		#endregion

		#region TestWarehouseReceive_SplitMonthBilling_ReceiveChargesForWhsStorageIn

		[TestDate(2020, 1, 20)]
		public void TestWarehouseReceive_SplitMonthBilling_ReceiveChargesForWhsStorageIn()
		{
			#region Setup Charges

			var accStorageIn = Helper.ChargeCodes.New("STOOUT", "Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			#endregion

			#region Setup Rate Lines

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			Factory.Save();
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var rateLineStorageIn = rateEntry.AddRateLine(accStorageIn, SplitMonthBillingCalculator.Code, PkgUnit.Unit);
			rateLineStorageIn.Calculator["-15"] = (ZDecimal)2m;
			rateLineStorageIn.Calculator["+15"] = (ZDecimal)4m;

			#endregion

			#region Create Receive for 10 units on 10 Jan

			var product = Helper.NewOrgSupplierPart(client);
			var receive = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);
			receive.RunPreSaveValidation();
			receiveLine.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;

			#endregion

			#region create receive for 5 units at 3 Feb
			var receive2 = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R2");
			receive2.WD_ArrivalDate = new ZDateTimeOffset(year, 02, 03);

			var receiveLine2 = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive2, product, 5m);
			receive2.RunPreSaveValidation();
			receiveLine2.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine2.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = receive2.WD_ArrivalDate;
			Factory.Save();
			#endregion

			#region create order for 13 units at 14 Feb
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.WD_RequiredDate = new ZDateTimeOffset(year, 02, 14);
			order.ConsigneePK = client.PK;
			order.ConsigneeAddressPK = client.MainAddress.PK;

			var orderLine = order.Lines.AddNew();
			orderLine.WE_OP = product.PK;
			orderLine.WE_TransactionQuantity = 13;

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocket();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			orderLine.WE_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			AssertEquals(13m, orderLine.SumOfUnitsMet);
			Assert("Precondition: order is finalised", order.IsFinalised);
			Factory.Save();
			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = accStorageIn.AC_Code,
					JR_OSSellAmt = 20m,
					RevenueCalculationDescription = accStorageIn.AC_Code + ": 10 Unit(s) (Received Before 15 Day(s)) @ AUD 2.00/Unit",
				}
			};

			AutorateAndAssert("Expect correct charges for receive.", expected, receive, client);
		}

		[TestDate(2020, 1, 20)]
		public void TestWarehouseReceive_SplitMonthBilling_ReceiveChargesForWhsStorageIn_StorageWeightMeasureType()
		{
			#region Setup Charges

			var accStorageIn = Helper.ChargeCodes.New("STOOUT", "Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			#endregion

			#region Setup Rate Lines

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			Factory.Save();
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var rateLineStorageIn = rateEntry.AddRateLine(accStorageIn, SplitMonthBillingCalculator.Code, Weight.Kilograms);
			rateLineStorageIn.Calculator["-15"] = (ZDecimal)2m;
			rateLineStorageIn.Calculator["+15"] = (ZDecimal)4m;

			#endregion

			#region Create Receive for 10 units on 10 Jan

			var product = Helper.NewOrgSupplierPart(client);
			product.OP_Weight = 1m;
			product.OP_WeightUQ = Weight.Kilograms;
			product.OP_Cubic = 4m;
			product.OP_CubicUQ = Volume.CubicCentimeters;
			Factory.Save();

			var receive = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);
			receive.RunPreSaveValidation();
			receiveLine.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;

			#endregion

			#region create receive for 5 units at 3 Feb
			var receive2 = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R2");
			receive2.WD_ArrivalDate = new ZDateTimeOffset(year, 02, 03);

			var receiveLine2 = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive2, product, 5m);
			receive2.RunPreSaveValidation();
			receiveLine2.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine2.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = receive2.WD_ArrivalDate;
			Factory.Save();
			#endregion

			#region create order for 13 units at 14 Feb
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.WD_RequiredDate = new ZDateTimeOffset(year, 02, 14);
			order.ConsigneePK = client.PK;
			order.ConsigneeAddressPK = client.MainAddress.PK;

			var orderLine = order.Lines.AddNew();
			orderLine.WE_OP = product.PK;
			orderLine.WE_TransactionQuantity = 13;

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocket();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			orderLine.WE_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			AssertEquals(13m, orderLine.SumOfUnitsMet);
			Assert("Precondition: order is finalised", order.IsFinalised);
			Factory.Save();
			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = accStorageIn.AC_Code,
					JR_OSSellAmt = 20m,
					RevenueCalculationDescription = accStorageIn.AC_Code + ": 10 Kilogram(s) (Received Before 15 Day(s)) @ AUD 2.00/KG",
				}
			};

			AutorateAndAssert("Expect correct charges for receive.", expected, receive, client);
		}

		[TestDate(2020, 1, 20)]
		public void TestWarehouseReceive_SplitMonthBilling_ReceiveChargesForWhsStorageIn_StorageVolumeMeasureType()
		{
			#region Setup Charges

			var accStorageIn = Helper.ChargeCodes.New("STOOUT", "Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			#endregion

			#region Setup Rate Lines

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			Factory.Save();
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var rateLineStorageIn = rateEntry.AddRateLine(accStorageIn, SplitMonthBillingCalculator.Code, Volume.CubicCentimeters);
			rateLineStorageIn.Calculator["-15"] = (ZDecimal)2m;
			rateLineStorageIn.Calculator["+15"] = (ZDecimal)4m;

			#endregion

			#region Create Receive for 10 units on 10 Jan

			var product = Helper.NewOrgSupplierPart(client);
			product.OP_Weight = 1m;
			product.OP_WeightUQ = Weight.Kilograms;
			product.OP_Cubic = 4m;
			product.OP_CubicUQ = Volume.CubicCentimeters;
			Factory.Save();

			var receive = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);
			receive.RunPreSaveValidation();
			receiveLine.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;

			#endregion

			#region create receive for 5 units at 3 Feb
			var receive2 = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R2");
			receive2.WD_ArrivalDate = new ZDateTimeOffset(year, 02, 03);

			var receiveLine2 = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive2, product, 5m);
			receive2.RunPreSaveValidation();
			receiveLine2.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine2.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = receive2.WD_ArrivalDate;
			Factory.Save();
			#endregion

			#region create order for 13 units at 14 Feb
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.WD_RequiredDate = new ZDateTimeOffset(year, 02, 14);
			order.ConsigneePK = client.PK;
			order.ConsigneeAddressPK = client.MainAddress.PK;

			var orderLine = order.Lines.AddNew();
			orderLine.WE_OP = product.PK;
			orderLine.WE_TransactionQuantity = 13;

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocket();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			orderLine.WE_FinalisedDate = new ZDateTimeOffset(year, 02, 14);
			AssertEquals(13m, orderLine.SumOfUnitsMet);
			Assert("Precondition: order is finalised", order.IsFinalised);
			Factory.Save();
			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = accStorageIn.AC_Code,
					JR_OSSellAmt = 80m,
					RevenueCalculationDescription = accStorageIn.AC_Code + ": 40 Cubic Centimeters(s) (Received Before 15 Day(s)) @ AUD 2.00/CC",
				}
			};

			AutorateAndAssert("Expect correct charges for receive.", expected, receive, client);
		}

		#endregion

		#region TestWarehouseAdjustment_SplitMonthBilling_AdjustmentChargesForWhsStorage

		[TestDate(2016, 1, 20)]
		public void TestWarehouseAdjustment_SplitMonthBilling_AdjustmentChargesForWhsStorageIn()
		{
			#region Setup Charges

			var accStorageIn = Helper.ChargeCodes.New("STOOUT", "Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);
			var accHandlingIn = Helper.ChargeCodes.New("HANIN", "Handling", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			#endregion

			#region Setup Rate Lines

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			Factory.Save();
			var year = 2015;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var rateLineStorageIn = rateEntry.AddRateLine(accStorageIn, SplitMonthBillingCalculator.Code, PkgUnit.Unit);
			rateLineStorageIn.Calculator["-15"] = (ZDecimal)2m;
			rateLineStorageIn.Calculator["+15"] = (ZDecimal)4m;

			var rateLineHandlingIn = rateEntry.AddRateLine(accHandlingIn, UnitCalculator.Code, PkgUnit.Unit);
			rateLineHandlingIn.GetCalculator<UnitCalculator>().PerUnit = 6;

			#endregion

			#region Create Receive for 10 units on 10 Jan

			var product = Helper.NewOrgSupplierPart(client);
			var receive = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);
			receive.RunPreSaveValidation();
			receiveLine.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;

			#endregion

			#region Create Adjustment IN for 5 units on 2 Feb

			var adjustmentIn = WarehouseTestHelper.CreateWhsAdjustment(client, warehouse, "ADJ_IN");
			WarehouseTestHelper.CreateWhsAdjustmentLine(adjustmentIn, product, 5, warehouse.DefaultLocation);
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 3);
			Factory.Save();
			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = accStorageIn.AC_Code,
					JR_OSSellAmt = 10m,
					RevenueCalculationDescription = accStorageIn.AC_Code + ": 5 Unit(s) (Received Before 15 Day(s)) @ AUD 2.00/Unit",
				}
			};

			AutorateAndAssert("Expect no inwards handling charge", expected, adjustmentIn, client);
		}

		[TestDate(2016, 1, 22)]
		public void TestWarehouseAdjustment_SplitMonthBilling_AdjustmentChargesForWhsStorageOut()
		{
			#region Setup Charges

			var accStorageOut = Helper.ChargeCodes.New("STOOUT", "Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var accHandlingOut = Helper.ChargeCodes.New("HANOUT", "Handling", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			var year = 2015;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var rateLineStorageOut = rateEntry.AddRateLine(accStorageOut, SplitMonthBillingCalculator.Code, PkgUnit.Unit);
			rateLineStorageOut.Calculator["-15"] = (ZDecimal)3m;
			rateLineStorageOut.Calculator["+15"] = (ZDecimal)5m;

			var rateLineHandlingOut = rateEntry.AddRateLine(accHandlingOut, UnitCalculator.Code, PkgUnit.Unit);
			rateLineHandlingOut.GetCalculator<UnitCalculator>().PerUnit = 7;

			Factory.Save();

			#endregion

			#region Create Receive for 10 units on 10 Jan

			var product = Helper.NewOrgSupplierPart(client);
			var receive = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);
			receive.RunPreSaveValidation();
			receiveLine.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;

			#endregion

			#region Create Adjustment IN for 5 units on 2 Feb

			var adjustmentIn = WarehouseTestHelper.CreateWhsAdjustment(client, warehouse, "ADJ_IN");
			WarehouseTestHelper.CreateWhsAdjustmentLine(adjustmentIn, product, 5, warehouse.DefaultLocation);
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 3);

			#endregion

			#region Create Adjustment OUT for 13 units on 16 Feb

			var adjustmentOut = WarehouseTestHelper.CreateWhsAdjustment(client, warehouse, "ADJ_OUT");
			WarehouseTestHelper.CreateWhsAdjustmentLine(adjustmentOut, product, -13, warehouse.DefaultLocation);
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			adjustmentOut.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 16);

			Factory.Save();

			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = accStorageOut.AC_Code,
					JR_OSSellAmt = 40m,
					RevenueCalculationDescription = accStorageOut.AC_Code + ": 8 Unit(s) (Released After 15 Day(s)) @ AUD 5.00/Unit",
				}
			};

			AutorateAndAssert("Expect NO outwards handling charge", expected, adjustmentOut, client);
		}

		[TestDate(2020, 1, 20)]
		public void TestWarehouseAdjustment_SplitMonthBilling_AdjustmentChargesForWhsStorageIn_StorageWeightMeasureType()
		{
			#region Setup Charges

			var accStorageIn = Helper.ChargeCodes.New("STOOUT", "Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);
			var accHandlingIn = Helper.ChargeCodes.New("HANIN", "Handling", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			#endregion

			#region Setup Rate Lines

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			Factory.Save();
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var rateLineStorageIn = rateEntry.AddRateLine(accStorageIn, SplitMonthBillingCalculator.Code, Weight.Kilograms);
			rateLineStorageIn.Calculator["-15"] = (ZDecimal)2m;
			rateLineStorageIn.Calculator["+15"] = (ZDecimal)4m;

			var rateLineHandlingIn = rateEntry.AddRateLine(accHandlingIn, UnitCalculator.Code, PkgUnit.Unit);
			rateLineHandlingIn.GetCalculator<UnitCalculator>().PerUnit = 6;

			#endregion

			#region Create Receive for 10 units on 10 Jan

			var product = Helper.NewOrgSupplierPart(client);
			product.OP_Weight = 1m;
			product.OP_WeightUQ = Weight.Kilograms;
			product.OP_Cubic = 1m;
			product.OP_CubicUQ = Volume.CubicCentimeters;
			Factory.Save();

			var receive = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);
			receive.RunPreSaveValidation();
			receiveLine.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;

			#endregion

			#region Create Adjustment IN for 5 units on 2 Feb

			var adjustmentIn = WarehouseTestHelper.CreateWhsAdjustment(client, warehouse, "ADJ_IN");
			WarehouseTestHelper.CreateWhsAdjustmentLine(adjustmentIn, product, 5, warehouse.DefaultLocation);
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 3);
			Factory.Save();
			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = accStorageIn.AC_Code,
					JR_OSSellAmt = 10m,
					RevenueCalculationDescription = accStorageIn.AC_Code + ": 5 Kilogram(s) (Received Before 15 Day(s)) @ AUD 2.00/KG",
				}
			};

			AutorateAndAssert("Expect no inwards handling charge", expected, adjustmentIn, client);
		}

		[TestDate(2020, 1, 20)]
		public void TestWarehouseAdjustment_SplitMonthBilling_AdjustmentChargesForWhsStorageIn_StorageVolumeMeasureType()
		{
			#region Setup Charges

			var accStorageIn = Helper.ChargeCodes.New("STOOUT", "Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);
			var accHandlingIn = Helper.ChargeCodes.New("HANIN", "Handling", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			#endregion

			#region Setup Rate Lines

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			Factory.Save();
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var rateLineStorageIn = rateEntry.AddRateLine(accStorageIn, SplitMonthBillingCalculator.Code, Volume.CubicCentimeters);
			rateLineStorageIn.Calculator["-15"] = (ZDecimal)2m;
			rateLineStorageIn.Calculator["+15"] = (ZDecimal)4m;

			var rateLineHandlingIn = rateEntry.AddRateLine(accHandlingIn, UnitCalculator.Code, PkgUnit.Unit);
			rateLineHandlingIn.GetCalculator<UnitCalculator>().PerUnit = 6;

			#endregion

			#region Create Receive for 10 units on 10 Jan

			var product = Helper.NewOrgSupplierPart(client);
			product.OP_Weight = 1m;
			product.OP_WeightUQ = Weight.Kilograms;
			product.OP_Cubic = 4m;
			product.OP_CubicUQ = Volume.CubicCentimeters;
			Factory.Save();

			var receive = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);
			receive.RunPreSaveValidation();
			receiveLine.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;

			#endregion

			#region Create Adjustment IN for 5 units on 2 Feb

			var adjustmentIn = WarehouseTestHelper.CreateWhsAdjustment(client, warehouse, "ADJ_IN");
			WarehouseTestHelper.CreateWhsAdjustmentLine(adjustmentIn, product, 5, warehouse.DefaultLocation);
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 3);
			Factory.Save();
			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = accStorageIn.AC_Code,
					JR_OSSellAmt = 40m,
					RevenueCalculationDescription = accStorageIn.AC_Code + ": 20 Cubic Centimeters(s) (Received Before 15 Day(s)) @ AUD 2.00/CC",
				}
			};

			AutorateAndAssert("Expect no inwards handling charge", expected, adjustmentIn, client);
		}

		[TestDate(2020, 1, 22)]
		public void TestWarehouseAdjustment_SplitMonthBilling_AdjustmentChargesForWhsStorageOut_StorageWeightMeasureType()
		{
			#region Setup Charges

			var accStorageOut = Helper.ChargeCodes.New("STOOUT", "Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var accHandlingOut = Helper.ChargeCodes.New("HANOUT", "Handling", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var rateLineStorageOut = rateEntry.AddRateLine(accStorageOut, SplitMonthBillingCalculator.Code, Weight.Kilograms);
			rateLineStorageOut.Calculator["-15"] = (ZDecimal)3m;
			rateLineStorageOut.Calculator["+15"] = (ZDecimal)5m;

			var rateLineHandlingOut = rateEntry.AddRateLine(accHandlingOut, UnitCalculator.Code, PkgUnit.Unit);
			rateLineHandlingOut.GetCalculator<UnitCalculator>().PerUnit = 7;

			Factory.Save();

			#endregion

			#region Create Receive for 10 units on 10 Jan

			var product = Helper.NewOrgSupplierPart(client);
			product.OP_Weight = 1m;
			product.OP_WeightUQ = Weight.Kilograms;
			product.OP_Cubic = 1m;
			product.OP_CubicUQ = Volume.CubicCentimeters;
			Factory.Save();

			var receive = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);
			receive.RunPreSaveValidation();
			receiveLine.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;

			#endregion

			#region Create Adjustment IN for 5 units on 2 Feb

			var adjustmentIn = WarehouseTestHelper.CreateWhsAdjustment(client, warehouse, "ADJ_IN");
			WarehouseTestHelper.CreateWhsAdjustmentLine(adjustmentIn, product, 5, warehouse.DefaultLocation);
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 3);

			#endregion

			#region Create Adjustment OUT for 13 units on 16 Feb

			var adjustmentOut = WarehouseTestHelper.CreateWhsAdjustment(client, warehouse, "ADJ_OUT");
			WarehouseTestHelper.CreateWhsAdjustmentLine(adjustmentOut, product, -13, warehouse.DefaultLocation);
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			adjustmentOut.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 16);

			Factory.Save();

			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = accStorageOut.AC_Code,
					JR_OSSellAmt = 40m,
					RevenueCalculationDescription = accStorageOut.AC_Code + ": 8 Kilogram(s) (Released After 15 Day(s)) @ AUD 5.00/KG",
				}
			};

			AutorateAndAssert("Expect NO outwards handling charge", expected, adjustmentOut, client);
		}

		[TestDate(2020, 1, 22)]
		public void TestWarehouseAdjustment_SplitMonthBilling_AdjustmentChargesForWhsStorageOut_StorageVolumeMeasureType()
		{
			#region Setup Charges

			var accStorageOut = Helper.ChargeCodes.New("STOOUT", "Storage", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var accHandlingOut = Helper.ChargeCodes.New("HANOUT", "Handling", SplitMonthBillingCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Monthly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			var year = ZDateTime.Today.Year - 1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_WW_Warehouse = warehouse.PK;
			rateEntry.TI_OH_Supplier = warehouse.WarehouseAddress.Header.PK;
			rateEntry.TI_RateStartDate = new ZDate(year, 01, 01);
			rateEntry.TI_RateEndDate = new ZDate(year, 12, 31);

			var rateLineStorageOut = rateEntry.AddRateLine(accStorageOut, SplitMonthBillingCalculator.Code, Volume.CubicCentimeters);
			rateLineStorageOut.Calculator["-15"] = (ZDecimal)3m;
			rateLineStorageOut.Calculator["+15"] = (ZDecimal)5m;

			var rateLineHandlingOut = rateEntry.AddRateLine(accHandlingOut, UnitCalculator.Code, PkgUnit.Unit);
			rateLineHandlingOut.GetCalculator<UnitCalculator>().PerUnit = 7;

			Factory.Save();

			#endregion

			#region Create Receive for 10 units on 10 Jan

			var product = Helper.NewOrgSupplierPart(client);
			product.OP_Weight = 1m;
			product.OP_WeightUQ = Weight.Kilograms;
			product.OP_Cubic = 4m;
			product.OP_CubicUQ = Volume.CubicCentimeters;
			Factory.Save();

			var receive = WarehouseTestHelper.CreateWhsReceive(client, warehouse, "R1");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 01, 10);

			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);
			receive.RunPreSaveValidation();
			receiveLine.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			receiveLine.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = receive.WD_ArrivalDate;

			#endregion

			#region Create Adjustment IN for 5 units on 2 Feb

			var adjustmentIn = WarehouseTestHelper.CreateWhsAdjustment(client, warehouse, "ADJ_IN");
			WarehouseTestHelper.CreateWhsAdjustmentLine(adjustmentIn, product, 5, warehouse.DefaultLocation);
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 3);

			#endregion

			#region Create Adjustment OUT for 13 units on 16 Feb

			var adjustmentOut = WarehouseTestHelper.CreateWhsAdjustment(client, warehouse, "ADJ_OUT");
			WarehouseTestHelper.CreateWhsAdjustmentLine(adjustmentOut, product, -13, warehouse.DefaultLocation);
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			adjustmentOut.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 16);

			Factory.Save();

			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = accStorageOut.AC_Code,
					JR_OSSellAmt = 160m,
					RevenueCalculationDescription = accStorageOut.AC_Code + ": 32 Cubic Centimeters(s) (Released After 15 Day(s)) @ AUD 5.00/CC",
				}
			};

			AutorateAndAssert("Expect NO outwards handling charge", expected, adjustmentOut, client);
		}

		#endregion

		#region TestWarehousePeriodicBilling_PackageCountCalculator_OnlyAppliesBaseRateToPeriodicBilling

		[TestDate(2016, 1, 10)]
		public void TestWarehousePeriodicBilling_PackageCountCalculator_OnlyAppliesBaseRateToPeriodicBilling()
		{
			#region Setup Client, Warehouse and Periodic Billing

			var client = Helper.NewOrgHeader();
			client.OH_Code = "123RAT";
			client.CompanyData.OB_ARWarehouseRatingPeriod = StorageCalculationPeriods.Weekly;
			client.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			client.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A", 2, 2);
			var periodicBilling = WarehouseTestHelper.CreateWhsInvoice(warehouse.PK, client.PK, new ZDateTime(2016, 01, 9), new ZDateTime(2016, 01, 16)) as WhsInvoice;

			Factory.Save();

			#endregion

			#region Setup Rates

			var storageChargeCode = Helper.ChargeCodes.New("WHSSTOIAT", "Package Count Storage", PackageCountCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);

			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_RateStartDate = new ZDate(2016, 01, 01);
			rateEntry.TI_RateEndDate = ZDate.Empty;

			var rateLine = rateEntry.AddRateLine(storageChargeCode, PackageCountCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			rateLine.TL_IsWhsJobLevelCharge = true;

			var calculator = rateLine.GetCalculator<PackageCountCalculator>();
			calculator.BaseRate = 100m;
			calculator.FirstPackageRate = 10m;
			calculator.AddtionalPackageRate = 20m;

			Factory.Save();

			#endregion

			var message = "As periodic billing has no package measures, so this calculator isn't applicable";
			AutorateAndAssert(message, null, periodicBilling, client, autorateCosts: false);

			var expectedLine = @"Information: RateLine Filtered WHSSTOIAT-IAT-KG-Client Rate 123RAT	reason:	no JobWeight measure on the job";
			AssertAutoratingAuditLogNoteContainsLines(periodicBilling, "", expectedLine);
		}

		#endregion

		#region TestWarehouseOrder_HighestRateCalculator

		[TestDate(2015, 9, 16)]
		public void TestWarehouseReceive_HighestRateCalculator()
		{
			var localClient = Helper.NewOrgHeader();
			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS");
			var product = Helper.NewOrgSupplierPart(localClient);

			var receive = WarehouseTestHelper.CreateWhsReceive(localClient, warehouse, "R1");
			var receiveLine = WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, product, 10m);

			var chargeCode = Helper.ChargeCodes.New("WCS", "Warehouse Chargeable Order", HighestRateCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_RateStartDate = new ZDate(2015, 1, 1);
			rateEntry.TI_RateEndDate = new ZDate(2016, 1, 1);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine(chargeCode, HighestRateCalculator.Code);
			rateLine.TL_IsWhsJobLevelCharge = true;

			var rateLineItem11 = rateLine.RateLineItems.AddNew();
			rateLineItem11.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem11.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem11.TM_RelevantValue = 71.64m;
			rateLineItem11.TM_UnitMultiple = 1000;

			var rateLineItem12 = rateLine.RateLineItems.AddNew();
			rateLineItem12.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem12.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem12.TM_RelevantValue = 23.88;

			var rateLineItem13 = rateLine.RateLineItems.AddNew();
			rateLineItem13.TM_Type = Calculator.Items.Operator.MIN;
			rateLineItem13.TM_BreakWeightVolume = QuantityUnit.M3;
			rateLineItem13.TM_RelevantValue = 27.24m;

			var order = WarehouseTestHelper.CreateWhsOrderWithOrderLine(localClient, warehouse, product, 10m);
			var pick = WarehouseTestHelper.CreatePickNew(order);
			order.WD_WeightSentUserEntered = 2m;
			order.WD_TotalWeightUnit = "KG";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 27.24m,
					RevenueCalculationDescription = "WCS: Minimum AUD 27.24"
				}
			};

			AutorateAndAssert(expected, order, localClient);

			order.WD_WeightSentUserEntered = 2000m;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 143.28,
					RevenueCalculationDescription = "WCS: 2000 Kilogram(s) @ AUD 71.64/1000 KG"
				}
			};

			AutorateAndAssert(expected, order, localClient);

			order.WD_WeightSentUserEntered = 0m;
			order.WD_CubicSent = 1m;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 27.24m,
					RevenueCalculationDescription = "WCS: Minimum AUD 27.24"
				}
			};

			AutorateAndAssert(expected, order, localClient);

			order.WD_CubicSent = 20m;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 477.60m,
					RevenueCalculationDescription = "WCS: 20 Cubic Meter(s) @ AUD 23.88/M3"
				}
			};

			AutorateAndAssert(expected, order, localClient);
		}

		#endregion

		#region TestWarehouseReceive_LowerCaseProductPart

		[TestDate(2016, 01, 14)]
		public void TestWarehouseReceive_LowerCaseProductPart()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part = data.Part1;
			part.OP_PartNum = "Nifflers";
			part.OP_StockKeepingUnit = "ctn";

			var localClient = data.Org1;
			var clientRate = Helper.NewClientRate(localClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-5);

			var chargeCode = Helper.ChargeCodes.New("WHSPUT", "Warehouse Charge", WarehousePackCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, PkgUnit.Unit);
			rateLine.Calculator.AddRateLineItem(PkgUnit.Carton, 0m, 1.75m);

			var whsHelper = new WhsTestHelperFunctions(Factory);
			var receive = whsHelper.CreateWhsReceiveWithInventory(localClient, data.Whs1, "R1", part, 4);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 7m,
					RevenueCalculationDescription = "4 Carton (Product NIFFLERS) @ AUD 1.75/CTN"
				}
			};

			AutorateAndAssert(expected, receive, localClient);
		}

		#endregion

		#region TestWarehouseOrder_LowerCaseProductPart

		[TestDate(2016, 01, 14)]
		public void TestWarehouseOrder_LowerCaseProductPart()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part = data.Part1;
			part.OP_PartNum = "Nifflers";
			part.OP_StockKeepingUnit = "ctn";

			var localClient = data.Org1;
			var clientRate = Helper.NewClientRate(localClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-5);

			var chargeCode = Helper.ChargeCodes.New("WHSOUT", "WHS 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, PkgUnit.Unit);
			rateLine.Calculator.AddRateLineItem(PkgUnit.Carton, 0m, 1.75m);
			Factory.Save();

			var whsHelper = new WhsTestHelperFunctions(Factory);
			whsHelper.CreateWhsReceiveWithInventory(localClient, data.Whs1, "R1", part, 4);
			Factory.Save();

			var order = whsHelper.CreateWhsOrder(localClient, data.Whs1);
			whsHelper.CreateWhsOrderLine(order, part, 4);
			var pick = whsHelper.CreatePickNew(order);

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 7m,
					RevenueCalculationDescription = "4 Carton (Product NIFFLERS) @ AUD 1.75/CTN"
				}
			};

			AutorateAndAssert(expected, order, localClient);
		}

		#endregion

		public void TestCostBaseCalcShowsResultsFromDifferentWarehouses()
		{
			var warehouse1 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse1.WW_WarehouseCode = "WH1";
			warehouse1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var warehouse2 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse2.WW_WarehouseCode = "WH2";
			warehouse2.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var chargeCode = Helper.ChargeCodes.New("WFRT", "Warehouse Charge Code", FlatCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);

			var costing = Helper.NewCosting(NewClient);

			var wh1CostEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			wh1CostEntry.TI_WW_Warehouse = warehouse1.PK;
			var wh1CostLine = wh1CostEntry.AddRateLine(chargeCode);
			wh1CostLine.GetCalculator<FlatCalculator>().BaseRate = 100;

			var wh2CostEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS);
			wh2CostEntry.TI_WW_Warehouse = warehouse2.PK;
			var wh2CostLine = wh2CostEntry.AddRateLine(chargeCode);
			wh2CostLine.GetCalculator<FlatCalculator>().BaseRate = 200;

			Factory.Save();

			var clientRate = Helper.NewClientRate(NewClient2);
			var wh1ClientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			wh1ClientRateEntry.TI_WW_Warehouse = warehouse1.PK;
			wh1ClientRateEntry.TI_OH_Supplier = NewClient.PK;
			var wh1ClientRateLine = wh1ClientRateEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			wh1ClientRateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10;

			var wh2ClientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			wh2ClientRateEntry.TI_WW_Warehouse = warehouse2.PK;
			wh2ClientRateEntry.TI_OH_Supplier = NewClient.PK;
			var wh2ClientRateLine = wh2ClientRateEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			wh2ClientRateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10;

			wh1ClientRateLine.ViewResults = true;
			var wh1ClientRateLineResultRate = wh1ClientRateLine.ResultsRateLine.GetCalculator<FlatCalculator>().BaseRate;
			AssertEquals("Result should be calculated from cost line with Warehouse 1 + 10%", 110m, wh1ClientRateLineResultRate);

			wh2ClientRateLine.ViewResults = true;
			var wh2ClientRateLineResultRate = wh2ClientRateLine.ResultsRateLine.GetCalculator<FlatCalculator>().BaseRate;
			AssertEquals("Result should be calculated from cost line with Warehouse 2 + 10%", 220m, wh2ClientRateLineResultRate);
		}

		public void TestWarehouseOrder_Services()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part = data.Part1;
			part.OP_PartNum = "Nifflers";
			part.OP_StockKeepingUnit = "ctn";

			var localClient = data.Org1;
			var clientRate = Helper.NewClientRate(localClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-5);

			var chargeCode1 = Helper.ChargeCodes.New("WSTEAM", "WHS Steam Clean", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, FreightServiceType.Codes.SteamCleaning);
			rateEntry.AddRateLine(chargeCode1, UnitCalculator.Code, QuantityUnit.SV)
				.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var chargeCodeWithNoService = Helper.ChargeCodes.New("WFUM", "WHS Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, FreightServiceType.Codes.Fumigation);
			rateEntry.AddRateLine(chargeCodeWithNoService, UnitCalculator.Code, QuantityUnit.SV)
				.GetCalculator<UnitCalculator>().PerUnit = 7m;
			Factory.Save();

			var whsHelper = new WhsTestHelperFunctions(Factory);
			whsHelper.CreateWhsReceiveWithInventory(localClient, data.Whs1, "R1", part, 4);
			Factory.Save();

			var order = whsHelper.CreateWhsOrder(localClient, data.Whs1);
			whsHelper.CreateWhsOrderLine(order, part, 4);
			var pick = whsHelper.CreatePickNew(order);

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var service = order.Services.AddNew();
			service.ES_ServiceCode = FreightServiceType.Codes.SteamCleaning;
			service.ES_Completed = ZDate.Today.AddDays(-2);
			service.ES_ServiceCount = 3;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode1.AC_Code,
					JR_OSSellAmt = 15m,
					RevenueCalculationDescription = "WSTEAM: 3 Warehouse Orders Steam Cleaning @ AUD 5.00/Warehouse Orders Steam Cleaning"
				}
			};

			AutorateAndAssert(expected, order, localClient);
		}

		public void TestWarehousePeriodicBilling_WhenBothPalletIDRateAndPackageRateApply_ProducesCorrectCharges()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			AccChargeCode warehouseStorageCharge = WarehouseTestHelper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Helper will create a product with 12 units per carton
			var part3 = WarehouseTestHelper.CreateProduct(data.Org1, "P3");
			part3.OP_RH_NKCommodityCode = "HAZ";

			// charge by a package measure, for commodity HAZ
			var today = ZDateTime.UtcToday.Date;
			var entry1 = WarehouseTestHelper.CreateRateEntry(clientRate, today.AddMonths(-1), today.AddMonths(1));
			entry1.TI_RH_NKCommodityCode = "HAZ";
			var rateLine1 = WarehouseTestHelper.CreateRateLine(entry1, warehouseStorageCharge, PkgUnit.Carton, 4m);

			// charge by PalletID, for any commodity
			var entry2 = WarehouseTestHelper.CreateRateEntry(clientRate, today.AddMonths(-1), today.AddMonths(1));
			entry2.TI_RH_NKCommodityCode = "";
			var rateLine2 = WarehouseTestHelper.CreateRateLine(entry2, warehouseStorageCharge, QuantityUnit.PI, 7m);

			// Create Receive for Warehouse Storage charges.
			var receive = WarehouseTestHelper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = today.ToZDateTime().ToOffset();
			WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, null, "PALLETID1");
			WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, data.Part2, 11m, null, "PALLETID2");
			WarehouseTestHelper.CreateWhsReceiveInventoryLine(receive, part3, 24m, null, "PALLETID3");
			ReceiveAllocationHelper.AllocateLocations(receive, data.Whs1);
			receive.FinaliseDocket();
			AssertEquals("Precondition - receive was not finalised.", true, receive.IsFinalised);

			Factory.Save();

			// Create Periodic Billing. 
			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = today.AddDays(-2);
			invoice.ET_StorageToDate = today.AddDays(4);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "WSTO",
					JR_OSSellAmt = 8m,
					RevenueCalculationDescription = "WSTO: 2 Carton(s) @ AUD 4.00/Carton"
				},
				new AssertionCharge
				{
					ChargeCode = "WSTO",
					JR_OSSellAmt = 21m,
					RevenueCalculationDescription = "WSTO: 3 Pallet ID(s) @ AUD 7.00/Pallet ID"
				}
			};

			var interactor = new TestInteractor();
			AutorateAndAssert("", expected, invoice, data.Org1, testInteractor: interactor);
		}

		public void TestWarehouseOrder_FlatAndPerUnitChargesWithTheSameCodeWithAndWithoutProduct_FlatAndPerUnitShouldBeCalculatedSeparately()
		{
			var chargeCode = Helper.ChargeCodes.New("WPUT", "Warehouse Putaway Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = PkgUnit.Carton;

			WarehouseTestHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 500);

			Factory.Save();

			var order = WarehouseTestHelper.CreateWhsOrder(data.Org1, data.Whs1);
			WarehouseTestHelper.CreateWhsOrderLine(order, data.Part1, 360);
			var pick = WarehouseTestHelper.CreatePickNew(order);

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var clientRate = Helper.NewClientRate(data.Org1);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.AddUnitRateLine(chargeCode.AC_Code, 1, PkgUnit.Carton, warehouseProduct: data.Part1.PK);
			rateEntry.AddUnitRateLine(chargeCode.AC_Code, 2, PkgUnit.Carton);
			rateEntry.AddFlatRateLine(chargeCode.AC_Code, 100, warehouseProduct: data.Part1.PK);
			rateEntry.AddFlatRateLine(chargeCode.AC_Code, 200);

			Factory.Save();

			var expected = new[]
			{
				// Flat charges are not calculated per part and therefore are not comparable by attribute like Product, so, they both come through
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 300m,
					RevenueCalculationDescription = @"This charge is calculated from multiple rates
WPUT: Base Rate AUD 100.00
WPUT: Base Rate AUD 200.00"
				},
				// Per unit charges are calculated per part and thus the one which matches the part by product wins
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 360m,
					RevenueCalculationDescription = @"WPUT: 360 Carton(s) @ AUD 1.00/Carton"
				}
			};

			AutorateAndAssert(expected, order, data.Org1);
		}

		#region Percentage Calculator

		public void TestWarehouseOrder_PercentageCalculatorPerValue_ShouldIncludeReferenceInChargeDescription()
		{
			var chargeCode1 = Helper.ChargeCodes.New("WCH", "Percentage of goods value", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);
			var chargeCode2 = Helper.ChargeCodes.New("WCHPER", "Percentage of charge (WCH)", PercentageCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);

			Factory.Save();

			var data = new TestDataSimpleEnvironment(Factory)
			{
				Part1 =
				{
					OP_Height = 2,
					OP_Width = 2,
					OP_Depth = 2,
					OP_MeasureUQ = Length.Metres,
					OP_Weight = 2,
					OP_WeightUQ = Weight.Kilograms,
				}
			};

			var client = data.Org1;
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS);

			// WCH
			var rateLine1 = rateEntry.AddRateLine(chargeCode1, PercentageCalculator.Code);
			var calculator = rateLine1.GetCalculator<PercentageCalculator>();
			calculator.Percent = 20;
			calculator.AddApplyToItem(CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods);

			// WCHPER
			rateEntry.AddPercentageCharge(chargeCode2.AC_Code, chargeCode1, 20);

			var order = WarehouseTestHelper.CreateWhsOrderWithOrderLine(client, data.Whs1, "McLaren", data.Part1, 1m);
			order.WD_TotalOrderValue = 100;

			Factory.Save();

			var expectedChargeWhenUsingActualWeight = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode1.AC_Code,
					JR_OSSellAmt = 20m,
					JR_Desc = "Percentage of goods value McLaren",
					RevenueCalculationDescription = chargeCode1.AC_Code + ": 20.00% of (AUD 100.00 (Value of Goods))"
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode2.AC_Code,
					JR_OSSellAmt = 4m,
					JR_Desc = "Percentage of charge (WCH) McLaren",
					RevenueCalculationDescription = chargeCode2.AC_Code + ": 20.00% of (AUD 20.00 (WCH))"
				}
			};

			AutorateAndAssert(expectedChargeWhenUsingActualWeight, order, client);
		}

		#endregion

		#region Implementation

		void SetWarehouseHandlingChargeableFactor()
		{
			WarehouseDataRegistry.Instance.WarehouseChargeableFactorHandling.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GetDefaultWarehouseChargeableFactor());
		}

		ChargeableFactor GetDefaultWarehouseChargeableFactor()
		{
			var metricFactor = new ConversionFactor(3000m, Volume.CubicCentimeters, Weight.Kilograms);
			var imperialFactor = new ConversionFactor(97m, Volume.CubicInches, Weight.Pounds);
			var warehouseHandlingFactor = new ChargeableFactor(metricFactor, imperialFactor);

			return warehouseHandlingFactor;
		}

		WhsTestHelperFunctions WarehouseTestHelper
		{
			get { return warehouseTestHelper ?? (warehouseTestHelper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions warehouseTestHelper;

		#region CreateSupplierAsForwarder

		OrgHeader CreateSupplierAsForwarder(OrgHeader client, string code = "supplier")
		{
			var supplier = WarehouseTestHelper.CreateClient(code);
			supplier.OH_Code = code;
			return SetSupplierAsForwarder(client, supplier);
		}

		static OrgHeader SetSupplierAsForwarder(OrgHeader client, OrgHeader supplier)
		{
			var relatedParty = client.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderGroup;
			relatedParty.PR_OH_RelatedParty = supplier.PK;

			return supplier;
		}

		#endregion

		#region StoreProductInWarehouse

		WhsReceive StoreProductInWarehouse(OrgHeader client, ZString reference, ZString partNum, ZDecimal units, OrgHeader supplier = null)
		{
			var warehouse = WarehouseTestHelper.CreateWarehouse("WHS", "A");
			var product = WarehouseTestHelper.CreateProduct(client, partNum, 2);
			product.OP_StockKeepingUnit = PkgUnit.Unit;
			var receive = WarehouseTestHelper.CreateWhsReceiveWithInventory(client, warehouse, reference, product, units);
			if (supplier != null)
			{
				receive.SupplierDocAddress.OrganisationPK = supplier.PK;
			}
			return receive;
		}

		#endregion

		#region CreateClientCharge

		void CreateClientCharge(AccChargeCode clientChargeCode, decimal chargePerUnit, ZDate rateStartDate, ClientRate clientRate, OrgHeader consignor)
		{
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			rateEntry.TI_RateStartDate = rateStartDate;
			if (consignor != null)
			{
				rateEntry.TI_OH_Consignor = consignor.PK;
			}
			var rateLine = rateEntry.AddRateLine(clientChargeCode, WarehousePackCalculator.Code, PkgUnit.Unit);
			rateLine.Calculator.AddRateLineItem(PkgUnit.Unit, 0m, chargePerUnit);
		}

		#endregion

		#endregion
	}
}
