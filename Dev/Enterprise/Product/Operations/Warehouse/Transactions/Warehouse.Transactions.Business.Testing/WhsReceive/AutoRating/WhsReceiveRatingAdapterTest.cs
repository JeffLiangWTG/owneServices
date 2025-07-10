using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveRatingAdapterTest : WhsDocketRatingAdapterTest<WhsReceive>
	{
		#region WarehousePackageLine

		public void TestWarehousePackageLine_ClientRate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Unit);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 10m;
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 30m, data.Whs1.DefaultLocation);
			receiveLine1.WE_F3_NKPackType = PkgUnit.Unit;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 40m, data.Whs1.DefaultLocation);
			receiveLine2.WE_F3_NKPackType = PkgUnit.Pallet;

			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receive, CostSell.Revenue);
				AssertContainsExactElementsInAnyOrder
				(
					"PackageLine UnitFactor should not apply to Warehouse Receive",
					Array.Empty<string>(),
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		#endregion

		#region WarehouseProductLine

		public void TestWarehouseProductLine_ClientRate_CMBCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);

			var rateLine1 = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, PkgUnit.Unit);
			var calculator1 = rateLine1.GetCalculator<CombinedCalculator>();
			calculator1["-50"] = (ZDecimal)5m;
			calculator1["+50"] = (ZDecimal)10m;
			calculator1["+70"] = (ZDecimal)15m;
			rateLine1.TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			var rateLine2 = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, PkgUnit.Pallet);
			var calculator2 = rateLine2.GetCalculator<CombinedCalculator>();
			calculator2["-20"] = (ZDecimal)50m;
			calculator2["+20"] = (ZDecimal)100m;
			calculator2["+35"] = (ZDecimal)150m;
			rateLine2.TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 30m, data.Whs1.DefaultLocation);
			receiveLine1.WE_F3_NKPackType = PkgUnit.Unit;

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 40m, data.Whs1.DefaultLocation);
			receiveLine2.WE_F3_NKPackType = PkgUnit.Pallet;

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 50m, data.Whs1.DefaultLocation);
			receiveLine3.WE_F3_NKPackType = QuantityUnit.KG;

			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 60m, data.Whs1.DefaultLocation);
			receiveLine4.WE_F3_NKPackType = PkgUnit.Unit;

			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receive);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = ProductLine WHEN autorate THEN each warehouse line should be treated individually, even they have the same Product and PackUQ",
					new[] { "WHSCHG: 13 Pallet(s) @ AUD 50.00/Pallet (for warehouse line/s: P1(KG))\n\tWHSCHG: 15 Pallet(s) @ AUD 50.00/Pallet (for warehouse line/s: P1(UNT))\n\tWHSCHG: 25 Unit(s) @ AUD 5.00/Unit (for warehouse line/s: P1(KG))\n\tWHSCHG: 30 Pallet(s) @ AUD 100.00/Pallet (for warehouse line/s: P1(UNT))\n\tWHSCHG: 30 Unit(s) @ AUD 5.00/Unit (for warehouse line/s: P1(UNT))\n\tWHSCHG: 40 Pallet(s) @ AUD 150.00/Pallet (for warehouse line/s: P1(PLT))\n\tWHSCHG: 60 Unit(s) @ AUD 10.00/Unit (for warehouse line/s: P1(UNT))\n\tWHSCHG: 80 Unit(s) @ AUD 15.00/Unit (for warehouse line/s: P1(PLT)) => 12475.00" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestWarehouseProductLine_ClientRate_UnitCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);

			TestWarehouseProductLine_UnitCalculator(data, rateEntry, CostSell.Revenue);
		}

		public void TestWarehouseProductLine_CompanyTariff_UnitCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Org1.CompanyData.RateTariffLevels.SetLevel("DEF", 1);

			var rate = Factory.New<CompanyTariff>();
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL);

			TestWarehouseProductLine_UnitCalculator(data, rateEntry, CostSell.Revenue);
		}

		public void TestWarehouseProductLine_Costing_UnitCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var rate = Factory.New<Costing>();
			rate.TH_OH = data.Whs1.WarehouseAddress.Header.PK;
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL);

			TestWarehouseProductLine_UnitCalculator(data, rateEntry, CostSell.Cost);
		}

		void TestWarehouseProductLine_UnitCalculator(TestDataSimpleEnvironment data, RateEntry rateEntry, CostSell costSell)
		{
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rateLine1 = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Unit);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 10m;
			rateLine1.TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			var rateLine2 = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Pallet);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 20m;
			rateLine2.TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 30m, data.Whs1.DefaultLocation);
			receiveLine1.WE_F3_NKPackType = PkgUnit.Unit;

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 40m, data.Whs1.DefaultLocation);
			receiveLine2.WE_F3_NKPackType = PkgUnit.Pallet;

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 50m, data.Whs1.DefaultLocation);
			receiveLine3.WE_F3_NKPackType = QuantityUnit.KG;

			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 60m, data.Whs1.DefaultLocation);
			receiveLine4.WE_F3_NKPackType = PkgUnit.Unit;

			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receive, costSell);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = ProductLine WHEN autorate THEN each warehouse line should be treated individually, even they have the same Product and PackUQ",
					new[]
					{
						"WHSCHG: 13 Pallet(s) @ AUD 20.00/Pallet (for warehouse line/s: P1(KG))\n\tWHSCHG: 15 Pallet(s) @ AUD 20.00/Pallet (for warehouse line/s: P1(UNT))\n\tWHSCHG: 25 Unit(s) @ AUD 10.00/Unit (for warehouse line/s: P1(KG))\n\tWHSCHG: 30 Pallet(s) @ AUD 20.00/Pallet (for warehouse line/s: P1(UNT))\n\tWHSCHG: 30 Unit(s) @ AUD 10.00/Unit (for warehouse line/s: P1(UNT))\n\tWHSCHG: 40 Pallet(s) @ AUD 20.00/Pallet (for warehouse line/s: P1(PLT))\n\tWHSCHG: 60 Unit(s) @ AUD 10.00/Unit (for warehouse line/s: P1(UNT))\n\tWHSCHG: 80 Unit(s) @ AUD 10.00/Unit (for warehouse line/s: P1(PLT)) => 3910.00"
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		#endregion

		#region PacksWeight

		public void TestPacksWeight_ClientRate_CartageZoneDistanceCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var fromPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));
			var auZoneSet = CreateRateTransportZoneSet(data.Org1, CountryCodes.Australia);
			var auZone = auZoneSet.CreateRateTransportZoneForTest("AU Zone");
			auZone.CreateRateTransportZoneItemForTest(fromPostCode);

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 10m, auZone.PK);

			var whsReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "TEST", data.Part1, units: 30m, finalise: false);
			var receiveLine = (WhsReceiveLine)whsReceive.Lines.Single();
			receiveLine.WE_F3_NKPackType = PkgUnit.Pallet;
			whsReceive.FinaliseDocketWithoutUserConfirmation();

			data.Org1.MainAddress.OA_Address1 = ZString.Format("101 Fake Street");
			data.Org1.MainAddress.OA_City = "Sydney";
			data.Org1.MainAddress.OA_State = "NSW";
			data.Org1.MainAddress.OA_PostCode = "2000";
			whsReceive.PickUpAddressPK = data.Org1.MainAddress.PK;

			Factory.Save();

			// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
			var weightUQ = data.Part1.OP_WeightUQ;
			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receiveLine.Docket);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[] { "WHSCHG: 120 Kilogram(s) @ AUD 10.00/KG => 1200.00" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		RateTransportProvider CreateRateTransportZoneSet(OrgHeader supplier, string countryCode, string zoneType = RatingConstants.RatingZoneTypes.Rating, string zoneMode = RateMode.ALL)
		{
			var zoneSet = Factory.New<RateTransportProvider>();
			zoneSet.TP_ZoneType = zoneType;
			zoneSet.TP_ZoneMode = zoneMode;
			zoneSet.TP_RN_NKCountry = countryCode;
			if (supplier != null)
			{
				zoneSet.TP_OH_RelatedParty = supplier.PK;
			}

			return zoneSet;
		}

		public void TestPacksWeight_ClientRate_CartageCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CartageCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			var whsReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "TEST", data.Part1, units: 30m, finalise: false);
			var receiveLine = (WhsReceiveLine)whsReceive.Lines.Single();
			receiveLine.WE_F3_NKPackType = PkgUnit.Pallet;
			whsReceive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
			var weightUQ = data.Part1.OP_WeightUQ;
			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receiveLine.Docket);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[] { "WHSCHG: 120 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight) => 1200.00" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator_MultiplePacksWeightRateLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine1 = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine1.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator1 = rateLine1.GetCalculator<CombinedCalculator>();
			calculator1["-4"] = (ZDecimal)5m;
			calculator1["+4"] = (ZDecimal)10m;
			calculator1["+5"] = (ZDecimal)15m;
			var rateLine2 = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine2.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator2 = rateLine2.GetCalculator<CombinedCalculator>();
			calculator2["-4"] = (ZDecimal)50m;
			calculator2["+4"] = (ZDecimal)100m;
			calculator2["+5"] = (ZDecimal)150m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 30m, data.Whs1.DefaultLocation);
			receiveLine1.WE_F3_NKPackType = PkgUnit.Pallet;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 40m, data.Whs1.DefaultLocation);
			receiveLine2.WE_F3_NKPackType = PkgUnit.Pallet;
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
				var weightUQ = data.Part1.OP_WeightUQ;
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine1.WE_TransactionQuantity * data.Part1.OP_Weight);

				// 40 PLT x 2 PLT/UNT x 2 UNT/KG = 160 KG
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 160m, receiveLine2.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receive);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[] { "WHSCHG: 280 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight)\n\tWHSCHG: 280 Kilogram(s) @ AUD 100.00/KG (for 4 KG/PLT Packs Weight) => 30800.00" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator_SamePackType_SameProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 30m, data.Whs1.DefaultLocation);
			receiveLine1.WE_F3_NKPackType = PkgUnit.Pallet;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 40m, data.Whs1.DefaultLocation);
			receiveLine2.WE_F3_NKPackType = PkgUnit.Pallet;
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
				var weightUQ = data.Part1.OP_WeightUQ;
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine1.WE_TransactionQuantity * data.Part1.OP_Weight);

				// 40 PLT x 2 PLT/UNT x 2 UNT/KG = 160 KG
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 160m, receiveLine2.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receive);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[] { "WHSCHG: 280 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight) => 2800.00" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator_SamePackType_SameProducts_LessThan()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 1.9m);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 30m, data.Whs1.DefaultLocation);
			receiveLine1.WE_F3_NKPackType = PkgUnit.Pallet;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 40m, data.Whs1.DefaultLocation);
			receiveLine2.WE_F3_NKPackType = PkgUnit.Pallet;
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				// 30 PLT x 2 PLT/UNT x 1.9 UNT/KG = 114 KG
				var weightUQ = data.Part1.OP_WeightUQ;
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 3.8m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 114m, receiveLine1.WE_TransactionQuantity * data.Part1.OP_Weight);

				// 40 PLT x 2 PLT/UNT x 1.9 UNT/KG = 152 KG
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 3.8m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 152m, receiveLine2.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receive);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[] { "WHSCHG: 266 Kilogram(s) @ AUD 5.00/KG (for 3.8 KG/PLT Packs Weight) => 1330.00" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator_SamePackType_SameProducts_GreaterThan()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2.1m);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 30m, data.Whs1.DefaultLocation);
			receiveLine1.WE_F3_NKPackType = PkgUnit.Pallet;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 40m, data.Whs1.DefaultLocation);
			receiveLine2.WE_F3_NKPackType = PkgUnit.Pallet;
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				// 30 PLT x 2 PLT/UNT x 2.1 UNT/KG = 126 KG
				var weightUQ = data.Part1.OP_WeightUQ;
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4.2m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 126m, receiveLine1.WE_TransactionQuantity * data.Part1.OP_Weight);

				// 40 PLT x 2 PLT/UNT x 2.1 UNT/KG = 168 KG
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4.2m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 168m, receiveLine2.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receive);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[] { "WHSCHG: 294 Kilogram(s) @ AUD 10.00/KG (for 4.2 KG/PLT Packs Weight) => 2940.00" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator_SamePackType_DifferentProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit1 = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var partUnit2 = Helper.CreateProductUnit(data.Part2, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 3m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;
			calculator["+6"] = (ZDecimal)20m;
			calculator["+7"] = (ZDecimal)25m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 30m, data.Whs1.DefaultLocation);
			receiveLine1.WE_F3_NKPackType = PkgUnit.Pallet;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, units: 40m, data.Whs1.DefaultLocation);
			receiveLine2.WE_F3_NKPackType = PkgUnit.Pallet;
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
				var weightUQ = data.Part1.OP_WeightUQ;
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit1.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine1.WE_TransactionQuantity * data.Part1.OP_Weight);

				// 40 CRT x 3 CRT/UNT x 2 UNT/KG = 240 KG
				weightUQ = data.Part2.OP_WeightUQ;
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 6m, partUnit2.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 240m, receiveLine2.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receive);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[]
					{
					"WHSCHG: 120 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight) => 1200.00",
					"WHSCHG: 240 Kilogram(s) @ AUD 20.00/KG (for 6 KG/PLT Packs Weight) => 4800.00"
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator_DifferentPackType_SameProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit1 = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var partUnit2 = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Crate, partUnitSize: 3m);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;
			calculator["+6"] = (ZDecimal)20m;
			calculator["+7"] = (ZDecimal)25m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 30m, data.Whs1.DefaultLocation);
			receiveLine1.WE_F3_NKPackType = PkgUnit.Pallet;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 40m, data.Whs1.DefaultLocation);
			receiveLine2.WE_F3_NKPackType = PkgUnit.Crate;
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
				var weightUQ = data.Part1.OP_WeightUQ;
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit1.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine1.WE_TransactionQuantity * data.Part1.OP_Weight);

				// 40 CRT x 3 CRT/UNT x 2 UNT/KG = 240 KG
				AssertEquals($"PacksWeight {PkgUnit.Crate} (in {weightUQ})", 6m, partUnit2.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 240m, receiveLine2.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receive);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[]
					{
					"WHSCHG: 120 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight)\n\tWHSCHG: 240 Kilogram(s) @ AUD 20.00/KG (for 6 KG/CRT Packs Weight) => 6000.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator_DifferentPackType_DifferentProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit1 = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var partUnit2 = Helper.CreateProductUnit(data.Part2, PkgUnit.Unit, PkgUnit.Crate, partUnitSize: 3m);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;
			calculator["+6"] = (ZDecimal)20m;
			calculator["+7"] = (ZDecimal)25m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 30m, data.Whs1.DefaultLocation);
			receiveLine1.WE_F3_NKPackType = PkgUnit.Pallet;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, units: 40m, data.Whs1.DefaultLocation);
			receiveLine2.WE_F3_NKPackType = PkgUnit.Crate;
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
				var weightUQ = data.Part1.OP_WeightUQ;
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit1.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine1.WE_TransactionQuantity * data.Part1.OP_Weight);

				// 40 CRT x 3 CRT/UNT x 2 UNT/KG = 240 KG
				weightUQ = data.Part2.OP_WeightUQ;
				AssertEquals($"PacksWeight {PkgUnit.Crate} (in {weightUQ})", 6m, partUnit2.OF_QuantityInParent * data.Part2.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 240m, receiveLine2.WE_TransactionQuantity * data.Part2.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receive);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[]
					{
					"WHSCHG: 120 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight) => 1200.00",
					"WHSCHG: 240 Kilogram(s) @ AUD 20.00/KG (for 6 KG/CRT Packs Weight) => 4800.00"
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			var whsReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "TEST", data.Part1, units: 30m, finalise: false);
			var receiveLine = (WhsReceiveLine)whsReceive.Lines.Single();
			receiveLine.WE_F3_NKPackType = PkgUnit.Pallet;
			whsReceive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
			var weightUQ = data.Part1.OP_WeightUQ;
			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receiveLine.Docket);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[]
					{
					"WHSCHG: 120 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight) => 1200.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator_NonFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, units: 30m, data.Whs1.DefaultLocation);
			receiveLine.WE_F3_NKPackType = PkgUnit.Pallet;

			Factory.Save();

			// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
			var weightUQ = data.Part1.OP_WeightUQ;
			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receive);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[]
					{
					"WHSCHG: 0 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight) => 0.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_Costing_CombinedCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var costing = Factory.New<Costing>();
			costing.TH_OH = data.Whs1.WarehouseAddress.Header.PK;
			data.Whs1.WarehouseAddress.Header.OH_IsCreditor = true;
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL);
			var costLine = costEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			costLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			costLine.UseOnlyActualWeightMeasure = true;
			var calculator = costLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			var whsReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "TEST", data.Part1, units: 30m, finalise: false);
			var receiveLine = (WhsReceiveLine)whsReceive.Lines.Single();
			receiveLine.WE_F3_NKPackType = PkgUnit.Pallet;
			whsReceive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
			var weightUQ = data.Part1.OP_WeightUQ;
			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receiveLine.Docket, CostSell.Cost);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[]
					{
					"WHSCHG: 120 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight) => 1200.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_CompanyTariff_CombinedCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var localClient = data.Org1;
			localClient.OH_RL_NKClosestPort = "AUSYD";
			localClient.OH_IsDebtor = true;
			localClient.OH_IsCreditor = true;

			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var product = Helper.CreateProduct("Product1", localClient);
			product.OP_StockKeepingUnit = PkgUnit.Unit;
			var partUnit = Helper.CreateProductUnit(product, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			Helper.SetProductWeightAndVolume(product, weight: 2m, weightUQ: Weight.Kilograms, volume: 0.001m, volumeUQ: Volume.CubicMetres);

			var rate = Factory.New<CompanyTariff>();
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			Factory.Save();

			var whsReceive = Helper.CreateWhsReceiveWithInventory(localClient, data.Whs1, "TEST", product, units: 30m, finalise: false);
			var receiveLine = (WhsReceiveLine)whsReceive.Lines.Single();
			receiveLine.WE_F3_NKPackType = PkgUnit.Pallet;
			whsReceive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
			var weightUQ = product.OP_WeightUQ;
			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receiveLine.Docket);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[]
					{
					"WHSCHG: 120 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight) => 1200.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_Quote_CombinedCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSInwards, string.Empty);

			var quote = Factory.New<Quote>();
			quote.QuotationClientAddress.OrganisationPK = data.Org1.PK;
			quote.TH_QuoteNumber = "Q00009001";
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			quote.TryAcceptQuote(out _);

			var whsReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "TEST", data.Part1, units: 30m, finalise: false);
			var receiveLine = (WhsReceiveLine)whsReceive.Lines.Single();
			receiveLine.WE_F3_NKPackType = PkgUnit.Pallet;
			whsReceive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
			var weightUQ = data.Part1.OP_WeightUQ;
			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"PacksWeight {PkgUnit.Pallet} (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight (in {weightUQ})", 120m, receiveLine.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(receiveLine.Docket);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[]
					{
						"WHSCHG: 120 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight) => 1200.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_PalletID 

		#region TestIAutoRatingFreightInfo_Measures_PalletID

		public void TestIAutoRatingFreightInfo_Measures_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			receive.WD_TotalPallets = 10;

			var normalLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, string.Empty);
			var palletLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-123");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: Not saved to db, testing in memory branch.", true, receive.HasChanges);
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(receive).RateableMeasures;
			Assert("Should have pallet measure.", rateableMeasures.HasMeasureType(MeasureType.PalletID));
			AssertEquals(1m, rateableMeasures.GetActual(MeasureType.PalletID));
			AssertEquals("", rateableMeasures.GetUnit(MeasureType.PalletID));
			AssertContainsExactElementsInAnyOrder(new[] { data.Whs1.PK }, rateableMeasures.GetPalletIdWarehouses_ForTest().Distinct());
			AssertContainsExactElementsInAnyOrder(new[] { receive.WD_ExternalReference }, rateableMeasures.GetPalletIdDockets_ForTest().Distinct());
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123" }, rateableMeasures.GetPalletIds_ForTest());
			Assert("Should have pallet measure.", rateableMeasures.HasMeasureType(MeasureType.ChargeablePallet));
			AssertEquals(10m, rateableMeasures.GetActual(MeasureType.ChargeablePallet));
			AssertEquals("", rateableMeasures.GetUnit(MeasureType.ChargeablePallet));

			AssertEquals("Pallet PLT-123", PointMatchingLog.GetPalletName("PLT-123"));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_PalletID_NoPallets

		public void TestIAutoRatingFreightInfo_Measures_PalletID_NoPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			receive.WD_PalletsSent = 10;

			var normalLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, string.Empty);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(receive).RateableMeasures;
			Assert("Should have pallet measure.", rateableMeasures.HasMeasureType(MeasureType.PalletID));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.PalletID));
			AssertEquals("", rateableMeasures.GetUnit(MeasureType.PalletID));
			AssertEquals("Should have no pallet points.", 0, rateableMeasures.GetPartCount(MeasureType.PalletID));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_PalletID_PalletWithNoTransactionQuantity

		public void TestIAutoRatingFreightInfo_Measures_PalletID_PalletWithNoTransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			receive.WD_PalletsSent = 10;

			var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-123");
			line.WE_ClientOrderedUnits = 10m;
			line.WE_TransactionQuantity = 0m;

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: No Transaction Quantity.", 0m, line.WE_TransactionQuantity);
			AssertEquals("Precondition: Has Expected Quantity.", 10m, line.WE_ClientOrderedUnits);

			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(receive).RateableMeasures;
			Assert("Should have pallet measure.", rateableMeasures.HasMeasureType(MeasureType.PalletID));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.PalletID));
			AssertEquals("", rateableMeasures.GetUnit(MeasureType.PalletID));
			AssertEquals("Should have no pallet points.", 0, rateableMeasures.GetPartCount(MeasureType.PalletID));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_PalletID_MultiplePallets

		public void TestIAutoRatingFreightInfo_Measures_PalletID_MultiplePallets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");

			var palletLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-123");
			var palletLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-456");
			var palletLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-789");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(receive).RateableMeasures;
			Assert("Should have pallet measure.", rateableMeasures.HasMeasureType(MeasureType.PalletID));
			AssertEquals(3m, rateableMeasures.GetActual(MeasureType.PalletID));
			AssertEquals("", rateableMeasures.GetUnit(MeasureType.PalletID));
			AssertContainsExactElementsInAnyOrder(new[] { data.Whs1.PK }, rateableMeasures.GetPalletIdWarehouses_ForTest().Distinct());
			AssertContainsExactElementsInAnyOrder(new[] { receive.WD_ExternalReference }, rateableMeasures.GetPalletIdDockets_ForTest().Distinct());
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456", "PLT-789" }, rateableMeasures.GetPalletIds_ForTest());
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_PalletID_CountsUniquePallets

		public void TestIAutoRatingFreightInfo_Measures_PalletID_CountsUniquePallets() => TestIAutoRatingFreightInfo_Measures_PalletID_CountsUniquePallets(testCaseInsensitivity: false);
		public void TestIAutoRatingFreightInfo_Measures_PalletID_CountsUniquePallets_CaseInsensitive() => TestIAutoRatingFreightInfo_Measures_PalletID_CountsUniquePallets(testCaseInsensitivity: true);

		void TestIAutoRatingFreightInfo_Measures_PalletID_CountsUniquePallets(bool testCaseInsensitivity)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");

			var palletLine1_1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, testCaseInsensitivity ? "PLT-123" : "PLT-123");
			var palletLine1_2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, testCaseInsensitivity ? "plt-123" : "PLT-123");
			var palletLine1_3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, testCaseInsensitivity ? "pLt-123" : "PLT-123");

			var palletLine2_1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, testCaseInsensitivity ? "PLT-456" : "PLT-456");
			var palletLine2_2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, testCaseInsensitivity ? "plt-456" : "PLT-456");
			var palletLine2_3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, testCaseInsensitivity ? "pLt-456" : "PLT-456");

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(receive).RateableMeasures;
			Assert("Should have pallet measure.", rateableMeasures.HasMeasureType(MeasureType.PalletID));
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.PalletID));
			AssertEquals("", rateableMeasures.GetUnit(MeasureType.PalletID));
			AssertContainsExactElementsInAnyOrder(new[] { data.Whs1.PK }, rateableMeasures.GetPalletIdWarehouses_ForTest().Distinct());
			AssertContainsExactElementsInAnyOrder(new[] { receive.WD_ExternalReference }, rateableMeasures.GetPalletIdDockets_ForTest().Distinct());
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456", }, rateableMeasures.GetPalletIds_ForTest());
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_PalletID_FromDatabase

		public void TestIAutoRatingFreightInfo_Measures_PalletID_FromDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");

			var palletLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-123");
			var palletLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-456");
			var palletLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-789");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = factory2.Load<WhsReceive>(receive.PK);
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(receive).RateableMeasures;
			Assert("Should have pallet measure.", rateableMeasures.HasMeasureType(MeasureType.PalletID));
			AssertEquals(3m, rateableMeasures.GetActual(MeasureType.PalletID));
			AssertEquals("", rateableMeasures.GetUnit(MeasureType.PalletID));
			AssertContainsExactElementsInAnyOrder(new[] { data.Whs1.PK }, rateableMeasures.GetPalletIdWarehouses_ForTest().Distinct());
			AssertContainsExactElementsInAnyOrder(new[] { receive.WD_ExternalReference }, rateableMeasures.GetPalletIdDockets_ForTest().Distinct());
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456", "PLT-789" }, rateableMeasures.GetPalletIds_ForTest());
		}

		#endregion

		#endregion

		#region IAutoRating / IJobInvoicingPlugIn Members

		protected override void TestIAutoRatingConsumerTypeCore()
		{
			AssertEquals(JobInvoicingConsumerTypes.WarehouseInwards, GetIAutoRating(GetNewDocket()).ConsumerType);
		}

		protected override Dictionary<string, int> ExpectedHitsForPickupAddressCore => new Dictionary<string, int>();

		protected override Dictionary<string, int> ExpectedDbHitsForSplitPeriodBillingMeasuresCore => new Dictionary<string, int>
		{
			{ OrgMiscServSchema.Constants.TableName, 1 },
		};

		public void TestIJobInvoicingPlugIn_ConsumerType()
		{
			var jobInvPlugIn = (IJobInvoicingPlugIn)GetNewDocket();
			AssertEquals(JobInvoicingConsumerTypes.WarehouseInwards, jobInvPlugIn.InvoicingSupporter.ConsumerType);
		}

		public void TestAuditSecurity()
		{
			AssertEquals("AuditSecurity", Env.Security.WhsReceiveAuditBilling, ((IJobInvoicingPlugIn)GetNewDocket()).InvoicingSupporter.AuditSecurity);
		}

		public override void TestIAutoRatingPopulateChargeCodeGroups()
		{
			var iReceive = GetIAutoRating(GetNewDocket());
			AssertEquals(1, iReceive.ChargeCodeGroups.Count);
			AssertCollectionContains(ChargeCodeGroupList.Codes.WHSInwards, iReceive.ChargeCodeGroups);
		}

		public override void TestIAutoRatingAdapterTypeAndID()
		{
			var docket = GetNewDocket();
			var adapter = GetIAutoRating(docket);
			AssertEquals(AdapterType.WarehouseReceipt, adapter.AdapterType);
			AssertEquals(docket.WD_DocketID, adapter.OperationalJobCode);
		}

		public void TestWarehouseFallbackConsignorForFilterOnly_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = GetFinalisableHelper().GetNewFinalisableDocketWithOneLine(data, 10m, finalise: true);
			new JobHeader.Loader(receive).TryLoadOrCreateWithoutMutexForTestOnly();

			var supplier = Helper.CreateClient("SUP");
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;

			receive.Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsDocket>(receive.PK);
			var autoRating = GetIAutoRating(receiveInOtherFactory);
			AssertEquals(supplier.PK, ((IAutoRatingWarehouseInfo)autoRating).WarehouseFallbackConsignorForFilterOnly.PK);

			var expectedHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
			};

			AssertDbHits(expectedHits, otherFactory);
		}

		#region TestIAutoRatingFreightInfo_Measures_UnitInvalid

		public override void TestIAutoRatingFreightInfo_Measures_WhenWeightUnitIsInvalid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;

			data.Part1.OP_WeightUQ = "0";
			data.Part1.OP_StockKeepingUnit = "0";
			var line1 = docket.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line1.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 5m);
			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;

			var expectedErrorStr =
@"Invalid Weight Unit in this Product Code: P1. Please use following valid unit types:
DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN";
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageWeight));
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.Weight).Single());
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.StorageWeight).Single());

			data.Part1.OP_WeightUQ = "0";
			data.Part1.OP_StockKeepingUnit = Weight.Kilograms;
			rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.StorageWeight));
			AssertEquals(Weight.Kilograms, rateableMeasures.GetUnit(MeasureType.Weight));
			AssertEquals(Weight.Kilograms, rateableMeasures.GetUnit(MeasureType.StorageWeight));
			Assert(!rateableMeasures.GetMeasureErrors(MeasureType.Weight).Any());
			Assert(!rateableMeasures.GetMeasureErrors(MeasureType.StorageWeight).Any());
		}

		public override void TestIAutoRatingFreightInfo_Measures_WhenVolumeUnitIsInvalid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;

			data.Part1.OP_CubicUQ = "0";
			data.Part1.OP_StockKeepingUnit = "0";
			var line1 = docket.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line1.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 5m);
			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;

			var expectedErrorStr =
@"Invalid Volume Unit in this Product Code: P1. Please use following valid unit types:
CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE";
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageVolume));
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.Volume).Single());
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.StorageVolume).Single());

			data.Part1.OP_CubicUQ = "0";
			data.Part1.OP_StockKeepingUnit = Volume.CubicMetres;

			rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.StorageVolume));

			data.Part1.OP_CubicUQ = Volume.CubicMetres;
			data.Part1.OP_StockKeepingUnit = Volume.CubicMetres;

			rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.StorageVolume));
			AssertEquals(Volume.CubicMetres, rateableMeasures.GetUnit(MeasureType.Volume));
			AssertEquals(Volume.CubicMetres, rateableMeasures.GetUnit(MeasureType.StorageVolume));
			Assert(!rateableMeasures.GetMeasureErrors(MeasureType.Volume).Any());
			Assert(!rateableMeasures.GetMeasureErrors(MeasureType.StorageVolume).Any());
		}

		public override void TestIAutoRatingFreightInfo_Measures_WhenMultiFieldsAreInvalid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;

			data.Part1.OP_WeightUQ = "0";
			data.Part1.OP_CubicUQ = "0";
			data.Part1.OP_StockKeepingUnit = "0";
			var line1 = docket.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line1.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 5m);
			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;

			var expectedVolumeErrorStr =
 @"Invalid Volume Unit in this Product Code: P1. Please use following valid unit types:
CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE";
			var expectedWeightErrorStr =
@"Invalid Weight Unit in this Product Code: P1. Please use following valid unit types:
DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN";
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageWeight));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageVolume));
			AssertEquals(expectedVolumeErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.Volume).Single());
			AssertEquals(expectedVolumeErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.StorageVolume).Single());
			AssertEquals(expectedWeightErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.Weight).Single());
			AssertEquals(expectedWeightErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.StorageWeight).Single());
		}

		#endregion

		protected override bool SupportsSupplierAsFallbackConsignorFilter => true;

		#endregion

		#region Implementation

		protected override FinalisableDocketHelper<WhsReceive> GetFinalisableHelper() => new FinalisableReceiveHelper(Factory);

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsReceive>();
		}

		protected override IAutoRating GetIAutoRating(WhsDocket docket)
		{
			return new WhsReceiveRatingAdapter((WhsReceive)docket);
		}

		#endregion
	}
}
