using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Rating.Business.Calculator;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOrderRatingAdapterTest : WhsDocketRatingAdapterTest<WhsOrder>
	{
		#region PackageLine

		public void TestPackageLine_ClientRate_CMBCalculator_UnitFactor_KM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.Postcode = "2015";
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator.AddRateLineItem(Items.Operator.Minus, 4m, 5m, QuantityUnit.KM);
			calculator["+4"] = (ZDecimal)10m;
			calculator["+10"] = (ZDecimal)15m;
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			whsOrder.ConsigneeDocAddress.E2_AddressOverride = true;
			whsOrder.ConsigneeDocAddress.E2_City = "Sydney";
			whsOrder.ConsigneeDocAddress.E2_Postcode = "2000";
			whsOrder.ConsigneeDocAddress.E2_State = "NSW";
			whsOrder.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, data.Part1.OP_Weight);
				AssertEquals("Package1: Weight", 6m, package1.KP_Weight);
				AssertEquals("Package2: Weight", 9m, package2.KP_Weight);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				// Distance: 2015-2000: 4km
				// Package1 = 10UNT @ 2KG + 2BOX @ 3KG = 26KG
				// Package2 = 40UNT @ 2KG + 3BSK @ 3KG = 89KG
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 26 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 2x BOX)\n\tWHSCHG: 89 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 3x BSK) => 1150.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		#region PackageLine UnitCalculator

		#region Rate Entry Filter

		public void TestPackageLine_ClientRate_UnitCalculator_RateLineWithCommodity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);
			data.Part1.OP_RH_NKCommodityCode = "HAZ";

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry1 = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			rateEntry1.TI_RH_NKCommodityCode = "HAZ";
			var rateLine1 = rateEntry1.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator.Decimal1 = 10m;

			var rateEntry2 = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			rateEntry2.TI_RH_NKCommodityCode = "GEN";
			var rateLine2 = rateEntry2.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.Calculator.Decimal1 = 20m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, data.Part1.OP_Weight);
				AssertEquals("Package1: Weight", 6m, package1.KP_Weight);
				AssertEquals("Package2: Weight", 9m, package2.KP_Weight);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine1.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 26 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 2x BOX)\n\tWHSCHG: 89 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 3x BSK) => 1150.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_RateLineWithContainerType()
		{
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var gp40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry1 = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			rateEntry1.TI_RC = gp20.PK;
			var rateLine1 = rateEntry1.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator.Decimal1 = 10m;

			var rateEntry2 = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			rateEntry2.TI_RC = gp40.PK;
			var rateLine2 = rateEntry2.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.Calculator.Decimal1 = 20m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, data.Part1.OP_Weight);
				AssertEquals("Package1: Weight", 6m, package1.KP_Weight);
				AssertEquals("Package2: Weight", 9m, package2.KP_Weight);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			var whsDocketContainer = Factory.NewWithValidTestData<WhsDocketContainer>();
			whsDocketContainer.WC_ContainerNum = "ABC123";
			whsDocketContainer.WC_RC = gp20.PK;
			whsDocketContainer.WC_IsPalletised = true;
			whsDocketContainer.WC_IsChargeable = true;
			whsDocketContainer.WC_ItemCount = 3;
			whsDocketContainer.WC_PalletCount = 3;
			whsDocketContainer.WC_WD = whsOrder.PK;

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine1.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 26 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 2x BOX)\n\tWHSCHG: 89 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 3x BSK) => 1150.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_RateLineWithSupplier()
		{
			var unrelatedSupplier = Factory.NewWithValidTestData<OrgHeader>();

			var client = Factory.NewWithValidTestData<OrgHeader>();

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry1 = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			rateEntry1.TI_OH_Supplier = data.Whs1.WarehouseAddress.Header.PK;
			var rateLine1 = rateEntry1.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator.Decimal1 = 10m;

			var rateEntry2 = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			rateEntry2.TI_OH_Supplier = unrelatedSupplier.PK;
			var rateLine2 = rateEntry2.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.Calculator.Decimal1 = 20m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, data.Part1.OP_Weight);
				AssertEquals("Package1: Weight", 6m, package1.KP_Weight);
				AssertEquals("Package2: Weight", 9m, package2.KP_Weight);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine1.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 26 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 2x BOX)\n\tWHSCHG: 89 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 3x BSK) => 1150.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_RateLineWithServiceLevel()
		{
			var refServiceLevel1 = Factory.New<RefServiceLevel>();
			refServiceLevel1.RS_Code = "BBB";

			var refServiceLevel2 = Factory.New<RefServiceLevel>();
			refServiceLevel2.RS_Code = "CCC";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry1 = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			rateEntry1.TI_RS_NKServiceLevel_NI = "BBB";
			var rateLine1 = rateEntry1.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator.Decimal1 = 10m;

			var rateEntry2 = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			rateEntry2.TI_RS_NKServiceLevel_NI = "CCC";
			var rateLine2 = rateEntry2.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.Calculator.Decimal1 = 20m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			whsOrder.WD_RS_NKServiceLevel = "BBB";
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, data.Part1.OP_Weight);
				AssertEquals("Package1: Weight", 6m, package1.KP_Weight);
				AssertEquals("Package2: Weight", 9m, package2.KP_Weight);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine1.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 26 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 2x BOX)\n\tWHSCHG: 89 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 3x BSK) => 1150.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_RateLineWithCarrierServiceLevel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var transportCo = data.Org1;
			var transportCoServiceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			transportCoServiceLevel.PL_Code = "AAA";
			transportCoServiceLevel.PL_CarrierServiceLevelDescription = "AAA Description";

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			rateEntry.TI_PL_NKCarrierServiceLevel = "AAA";
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.Calculator.Decimal1 = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			whsOrder.TransportCoPK = transportCo.PK;
			whsOrder.WD_PL_NKCarrierServiceLevel = "AAA";
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, data.Part1.OP_Weight);
				AssertEquals("Package1: Weight", 6m, package1.KP_Weight);
				AssertEquals("Package2: Weight", 9m, package2.KP_Weight);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 26 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 2x BOX)\n\tWHSCHG: 89 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 3x BSK) => 1150.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_RateLineWithWarehousePK()
		{
			var unrelatedWhs = Helper.CreateWarehouse("10", "Z", 1, 1);

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry1 = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			rateEntry1.TI_WW_Warehouse = data.Whs1.PK;
			var rateLine1 = rateEntry1.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator.Decimal1 = 10m;

			var rateEntry2 = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			rateEntry2.TI_WW_Warehouse = unrelatedWhs.PK;
			var rateLine2 = rateEntry2.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.Calculator.Decimal1 = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, data.Part1.OP_Weight);
				AssertEquals("Package1: Weight", 6m, package1.KP_Weight);
				AssertEquals("Package2: Weight", 9m, package2.KP_Weight);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine1.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 26 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 2x BOX)\n\tWHSCHG: 89 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 3x BSK) => 1150.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		#endregion

		public void TestPackageLine_ClientRate_UnitCalculator_ProductWeightUQInPound()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Product0";
			product.OP_StockKeepingUnit = PkgUnit.Unit;
			Helper.SetProductWeightAndVolume(product, weight: 2m, weightUQ: Weight.Pounds, volume: 0.001m, volumeUQ: Volume.Litre);
			product.RelatedOrganisations.AddOwner(data.Org1);
			Helper.CreateProductUnit(product, PkgUnit.Unit, PkgUnit.Package, partUnitSize: 2m);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, Weight.Pounds);
			rateLine.Calculator.Decimal1 = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", product, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, product, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package0 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			var package1 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, product.OP_Weight);
				AssertEquals("Package1: Weight", 6m, package0.KP_Weight);
				AssertEquals("Package2: Weight", 9m, package1.KP_Weight);
			});

			package0.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			package1.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				// PackageN = Product/s Weight + Package Weight
				// Package0 = 10UNT @ 2LB + 2BOX @ 3KG = 20LB(9KG) + 6KG = 15KG (34LB)
				// Package1 = 40UNT @ 2LB + 3BSK @ 3KG = 80LB(36KG) + 9KG = 45KG (100LB)
				AssertContainsExactElementsInAnyOrder
				(
					new[]
					{
						"WHSCHG: 100 Pound(s) @ AUD 10.00/LB (for warehouse package/s: 3x BSK)\n\tWHSCHG: 34 Pound(s) @ AUD 10.00/LB (for warehouse package/s: 2x BOX) => 1340.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_KG()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.Calculator.Decimal1 = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, data.Part1.OP_Weight);
				AssertEquals("Package1: Weight", 6m, package1.KP_Weight);
				AssertEquals("Package2: Weight", 9m, package2.KP_Weight);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				// Package1 = 10UNT @ 2KG + 2BOX @ 3KG
				// Package2 = 40UNT @ 2KG + 3BSK @ 3KG
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 26 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 2x BOX)\n\tWHSCHG: 89 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 3x BSK) => 1150.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_LB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, Weight.Pounds);
			rateLine.Calculator.Decimal1 = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, data.Part1.OP_Weight);
				AssertEquals("Package1: Weight", 6m, package1.KP_Weight);
				AssertEquals("Package2: Weight", 9m, package2.KP_Weight);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 197 Pound(s) @ AUD 10.00/LB (for warehouse package/s: 3x BSK)\n\tWHSCHG: 58 Pound(s) @ AUD 10.00/LB (for warehouse package/s: 2x BOX) => 2550.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_LB_Rounding()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, Weight.Pounds);
			rateLine.Calculator.Decimal1 = 10m;
			rateLine.TL_Rounding = RatingRoundingTypes.NoRounding;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, data.Part1.OP_Weight);
				AssertEquals("Package1: Weight", 6m, package1.KP_Weight);
				AssertEquals("Package2: Weight", 9m, package2.KP_Weight);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"Rounding should be based on RateLine TL_Rounding",
					new[]
					{
						"WHSCHG: 196.2114 Pound(s) @ AUD 10.00/LB (for warehouse package/s: 3x BSK)\n\tWHSCHG: 57.3202 Pound(s) @ AUD 10.00/LB (for warehouse package/s: 2x BOX) => 2535.32",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_M3()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.M3);
			rateLine.Calculator.Decimal1 = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 500m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 200, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 300, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Volume", 0.02m, data.Part1.OP_Cubic);
				AssertEquals("Package1: Volume", 10m, package1.KP_Volume);
				AssertEquals("Package2: Volume", 15m, package2.KP_Volume);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 100m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 400m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"Volume should not consider children packages volumes",
					new[]
					{
						"WHSCHG: 10 Cubic Meter(s) @ AUD 10.00/M3 (for warehouse package/s: 200x BOX)\n\tWHSCHG: 15 Cubic Meter(s) @ AUD 10.00/M3 (for warehouse package/s: 300x BSK) => 250.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_CF()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, Constants.Volume.CubicFeet);
			rateLine.Calculator.Decimal1 = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 500m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 200, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, 300, PkgUnit.Basket);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Volume", 0.02m, data.Part1.OP_Cubic);
				AssertEquals("Package1: Volume", 10m, package1.KP_Volume);
				AssertEquals("Package2: Volume", 15m, package2.KP_Volume);
			});

			package1.Pack(whsOrderLine1.ReleaseLines[0], 100m);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 400m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 354 Cubic Feet @ AUD 10.00/CF (for warehouse package/s: 200x BOX)\n\tWHSCHG: 530 Cubic Feet @ AUD 10.00/CF (for warehouse package/s: 300x BSK) => 8840.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_NoInnerPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Box);
			rateLine.Calculator.Decimal1 = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			var package2 = PackingHelper.CreatePackage(packageJob, 3, PkgUnit.Basket);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 40m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"Should only consider Inner Packages - in this case amount should be zero",
					new[]
					{
						"WHSCHG: 0 Box(s) @ AUD 10.00/Box (for warehouse package/s: 3x BSK)\n\tWHSCHG: 2 Box(s) @ AUD 10.00/Box (for warehouse package/s: 2x BOX) => 20.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_HasInnerPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Box);
			rateLine.Calculator.Decimal1 = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);

			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			var package11 = PackingHelper.CreatePackage(package1, 3, PkgUnit.Roll);
			package11.Pack(whsOrderLine1.ReleaseLines[0], 20m);

			var package2 = PackingHelper.CreatePackage(packageJob, 4, PkgUnit.Basket);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 5m);
			var package21 = PackingHelper.CreatePackage(package2, 5, PkgUnit.Box);
			package21.Pack(whsOrderLine1.ReleaseLines[0], 15m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"Should only consider inner packages",
					new[]
					{
						"WHSCHG: 0 Box(s) @ AUD 10.00/Box (for warehouse package/s: 4x BSK)\n\tWHSCHG: 2 Box(s) @ AUD 10.00/Box (for warehouse package/s: 2x BOX) => 20.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_NestedPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Box);
			rateLine.Calculator.Decimal1 = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);

			var package1 = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Bag);
			package1.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			var package11 = PackingHelper.CreatePackage(package1, 3, PkgUnit.Roll);
			package11.Pack(whsOrderLine1.ReleaseLines[0], 5m);
			var package111 = PackingHelper.CreatePackage(package11, 4, PkgUnit.Box);
			package111.Pack(whsOrderLine1.ReleaseLines[0], 10m);

			var package2 = PackingHelper.CreatePackage(packageJob, 5, PkgUnit.Basket);
			package2.Pack(whsOrderLine1.ReleaseLines[0], 5m);
			var package21 = PackingHelper.CreatePackage(package2, 6, PkgUnit.Box);
			package21.Pack(whsOrderLine1.ReleaseLines[0], 10m);
			var package22 = PackingHelper.CreatePackage(package2, 7, PkgUnit.Box);
			package22.Pack(whsOrderLine1.ReleaseLines[0], 5m);
			var package221 = PackingHelper.CreatePackage(package22, 8, PkgUnit.Bundle);
			package221.Pack(whsOrderLine1.ReleaseLines[0], 5m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 0 Box(s) @ AUD 10.00/Box (for warehouse package/s: 2x BAG)\n\tWHSCHG: 0 Box(s) @ AUD 10.00/Box (for warehouse package/s: 5x BSK) => 0.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPackageLine_ClientRate_UnitCalculator_MultipleProductsPacking()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var product0 = Factory.New<OrgSupplierPart>();
			product0.OP_PartNum = "Product0";
			product0.OP_StockKeepingUnit = PkgUnit.Unit;
			Helper.SetProductWeightAndVolume(product0, weight: 2m, weightUQ: Weight.Kilograms, volume: 0.001m, volumeUQ: Volume.Litre);
			product0.RelatedOrganisations.AddOwner(data.Org1);
			Helper.CreateProductUnit(product0, PkgUnit.Unit, PkgUnit.Package, partUnitSize: 2m);

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "Product1";
			product1.OP_StockKeepingUnit = PkgUnit.Unit;
			Helper.SetProductWeightAndVolume(product1, weight: 3m, weightUQ: Weight.Kilograms, volume: 0.001m, volumeUQ: Volume.Litre);
			product1.RelatedOrganisations.AddOwner(data.Org1);
			Helper.CreateProductUnit(product1, PkgUnit.Unit, PkgUnit.Package, partUnitSize: 3m);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, Weight.Kilograms);
			rateLine.Calculator.Decimal1 = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", product0, units: 1000);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", product1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, product0, units: 50m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Unit;
			var whsOrderLine2 = Helper.CreateWhsOrderLine(whsOrder, product1, units: 60m);
			whsOrderLine2.WE_F3_NKPackType = PkgUnit.Unit;

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);

			var package0 = PackingHelper.CreatePackage(packageJob, 5, PkgUnit.Bundle);
			var package1 = PackingHelper.CreatePackage(packageJob, 6, PkgUnit.Bag);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Package0: Weight", 15m, package0.KP_Weight);
				AssertEquals("Package1: Weight", 18m, package1.KP_Weight);
			});

			package0.Pack(whsOrderLine1.ReleaseLines[0], 50m);
			package1.Pack(whsOrderLine2.ReleaseLines[0], 60m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					// PackageN = Products/s Weight + Package Weight
					// Package0 = 50UNT Product0 => 50 X 2KG + 15KG = 115KG
					// Package1 = 60UNT Product1 => 60 X 3KG + 18KG = 198KG
					"GIVEN rateLine.UnitFactor = PackageLine WHEN autorate THEN should autorate per warehouse package",
					new[]
					{
						"WHSCHG: 115 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 5x BND)\n\tWHSCHG: 198 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 6x BAG) => 3130.00",
					},
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		#endregion

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion

		#region ProductLine

		public void TestWarehouseProductLine_ClientRate_CMBCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

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

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 30m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Unit;

			var whsOrderLine2 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 40m);
			whsOrderLine2.WE_F3_NKPackType = PkgUnit.Pallet;

			var whsOrderLine3 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine3.WE_F3_NKPackType = QuantityUnit.KG;

			var whsOrderLine4 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 60m);
			whsOrderLine4.WE_F3_NKPackType = PkgUnit.Unit;

			Helper.CreatePickNew(whsOrder);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(whsOrderLine1.Order);
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
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rateLine1 = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Unit);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 10m;
			rateLine1.TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			var rateLine2 = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Pallet);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 20m;
			rateLine2.TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 30m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Unit;

			var whsOrderLine2 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 40m);
			whsOrderLine2.WE_F3_NKPackType = PkgUnit.Pallet;

			var whsOrderLine3 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine3.WE_F3_NKPackType = QuantityUnit.KG;

			var whsOrderLine4 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 60m);
			whsOrderLine4.WE_F3_NKPackType = PkgUnit.Unit;

			Helper.CreatePickNew(whsOrder);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(whsOrderLine1.Order, costSell);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = ProductLine WHEN autorate THEN each warehouse line should be treated individually, even they have the same Product and PackUQ",
					new[] { "WHSCHG: 13 Pallet(s) @ AUD 20.00/Pallet (for warehouse line/s: P1(KG))\n\tWHSCHG: 15 Pallet(s) @ AUD 20.00/Pallet (for warehouse line/s: P1(UNT))\n\tWHSCHG: 25 Unit(s) @ AUD 10.00/Unit (for warehouse line/s: P1(KG))\n\tWHSCHG: 30 Pallet(s) @ AUD 20.00/Pallet (for warehouse line/s: P1(UNT))\n\tWHSCHG: 30 Unit(s) @ AUD 10.00/Unit (for warehouse line/s: P1(UNT))\n\tWHSCHG: 40 Pallet(s) @ AUD 20.00/Pallet (for warehouse line/s: P1(PLT))\n\tWHSCHG: 60 Unit(s) @ AUD 10.00/Unit (for warehouse line/s: P1(UNT))\n\tWHSCHG: 80 Unit(s) @ AUD 10.00/Unit (for warehouse line/s: P1(PLT)) => 3910.00" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestWarehouseProductLine_UnitCalculator_DifferentUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);

			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 4m); // 1PLT = 4UNT
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Bag, partUnitSize: 6m); // 1BAG = 6UNT
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pail, partUnitSize: 8m); // 1PAI = 8UNT

			Helper.CreateProductUnit(data.Part2, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 10m); // 1PLT = 10UNT
			Helper.CreateProductUnit(data.Part2, PkgUnit.Unit, PkgUnit.Bottle, partUnitSize: 12m); // 1BOT = 12UNT

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rateLine = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Pallet);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 20m;
			rateLine.TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 30m);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Bag;
			var whsOrderLine2 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 40m);
			whsOrderLine2.WE_F3_NKPackType = PkgUnit.Pail;
			var whsOrderLine3 = Helper.CreateWhsOrderLine(whsOrder, data.Part2, units: 50m);
			whsOrderLine3.WE_F3_NKPackType = PkgUnit.Bottle;

			Helper.CreatePickNew(whsOrder);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(whsOrderLine1.Order, CostSell.Revenue);
				// Part1: 30BAG = 180UNT = 45 PLT
				// Part1: 40PAI = 320UNT = 80 PLT
				// Part2: 50BOT = 600UNT = 60PLT
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = ProductLine WHEN autorate THEN each warehouse line should be treated individually, even they have the same Product and PackUQ",
					new[] { "WHSCHG: 45 Pallet(s) @ AUD 20.00/Pallet (for warehouse line/s: P1(BAG))\n\tWHSCHG: 60 Pallet(s) @ AUD 20.00/Pallet (for warehouse line/s: P2(BOT))\n\tWHSCHG: 80 Pallet(s) @ AUD 20.00/Pallet (for warehouse line/s: P1(PAI)) => 3700.00" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		#endregion

		#region PacksWeight

		public void TestPacksWeight_ClientRate_CombinedCalculator_Rounding()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, Weight.Pounds);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-8"] = (ZDecimal)5m;
			calculator["+8"] = (ZDecimal)10m;
			calculator["+9"] = (ZDecimal)15m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 30m);
			whsOrderLine.WE_F3_NKPackType = PkgUnit.Pallet;
			Helper.CreatePickNew(whsOrder);

			var weightUQ = data.Part1.OP_WeightUQ;
			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"PacksWeight (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight ({weightUQ})", 120m, whsOrderLine.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(whsOrderLine.Order);
				AssertContainsExactElementsInAnyOrder
				(
					"PacksWeight should be rounded to 4 decimal places",
					new[] { "WHSCHG: 265 Pound(s) @ AUD 10.00/LB (for 8.8185 LB/PLT Packs Weight) => 2650.00" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator_RateLineinPound()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, Weight.Pounds);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-8"] = (ZDecimal)5m;
			calculator["+8"] = (ZDecimal)10m;
			calculator["+9"] = (ZDecimal)15m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 30m);
			whsOrderLine.WE_F3_NKPackType = PkgUnit.Pallet;
			Helper.CreatePickNew(whsOrder);

			var weightUQ = data.Part1.OP_WeightUQ;
			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"PacksWeight (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight ({weightUQ})", 120m, whsOrderLine.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(whsOrderLine.Order);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[] { "WHSCHG: 265 Pound(s) @ AUD 10.00/LB (for 8.8185 LB/PLT Packs Weight) => 2650.00", }, // 120 KG = 264.56 LB
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator_RateLineHasNoWeightUnit()
		{
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(Helper.CreateClient());
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, PkgUnit.Pallet);
			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
				AssertHasError(rateLine.TL_UnitFactorInfo, "Packs weight requires weight unit.");
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator_ProductWeightUQInPound()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Product0";
			product.OP_StockKeepingUnit = PkgUnit.Unit;
			Helper.SetProductWeightAndVolume(product, weight: 2m, weightUQ: Weight.Pounds, volume: 0.001m, volumeUQ: Volume.Litre);
			product.RelatedOrganisations.AddOwner(data.Org1);
			var partUnit = Helper.CreateProductUnit(product, PkgUnit.Unit, PkgUnit.Package, partUnitSize: 2m);

			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-1"] = (ZDecimal)5m;
			calculator["+1"] = (ZDecimal)10m;
			calculator["+2"] = (ZDecimal)15m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", product, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine = Helper.CreateWhsOrderLine(whsOrder, product, units: 30m);
			whsOrderLine.WE_F3_NKPackType = PkgUnit.Package;
			Helper.CreatePickNew(whsOrder);

			var weightUQ = product.OP_WeightUQ;
			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"PacksWeight (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * product.OP_Weight); // 4 LB = 1.8 KG
				AssertEquals($"TotalPacksWeight ({weightUQ})", 120m, whsOrderLine.WE_TransactionQuantity * product.OP_Weight); // 120 LB = 54.43 KG
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(whsOrderLine.Order);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[] { "WHSCHG: 55 Kilogram(s) @ AUD 10.00/KG (for 1.8144 KG/PKG Packs Weight) => 550.00", },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_CombinedCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 30);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;
			var whsOrderLine2 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 40);
			whsOrderLine2.WE_F3_NKPackType = PkgUnit.Pallet;
			Helper.CreatePickNew(whsOrder);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("whsOrderLine1: Sum of Units Met (in SKU)", 60m, whsOrderLine1.SumOfUnitsMet);
				AssertEquals("whsOrderLine2: Sum of Units Met (in SKU)", 80m, whsOrderLine2.SumOfUnitsMet);

				// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
				var partUnit = data.Part1.PartUnits.Cast<OrgPartUnit>().Single(x => x.OF_ParentPackType == PkgUnit.Pallet);
				var weightUQ = data.Part1.OP_WeightUQ;
				AssertEquals($"PacksWeight (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight ({weightUQ})", 120m, whsOrderLine1.WE_TransactionQuantity * data.Part1.OP_Weight);

				// 40 PLT x 2 PLT/UNT x 2 UNT/KG = 160 KG
				AssertEquals($"TotalPacksWeight ({weightUQ})", 160m, whsOrderLine2.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = empty WHEN autorate THEN should not use PacksWeight and use TotalPacksWeight",
					new[] { "WHSCHG: 280 Kilogram(s) @ AUD 15.00/KG => 4200.00", },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);

				rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
				Factory.Save();

				rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[] { "WHSCHG: 280 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight) => 2800.00", },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_MultipleWhsOrderLines_SameProduct_DifferentPackTypeUnitOfQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 30);
			whsOrderLine1.WE_F3_NKPackType = PkgUnit.Pallet;
			var whsOrderLine2 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 40);
			whsOrderLine2.WE_F3_NKPackType = PkgUnit.Unit;
			Helper.CreatePickNew(whsOrder);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("whsOrderLine1: Sum of Units Met (in SKU)", 60m, whsOrderLine1.SumOfUnitsMet);
				AssertEquals("whsOrderLine2: Sum of Units Met (in SKU)", 40m, whsOrderLine2.SumOfUnitsMet);

				// 30 PLT x 2 PLT/UNT x 2 UNT/KG = 120 KG
				var partUnit = data.Part1.PartUnits.Cast<OrgPartUnit>().Single(x => x.OF_ParentPackType == PkgUnit.Pallet);
				var weightUQ = data.Part1.OP_WeightUQ;
				AssertEquals($"PacksWeight (in {weightUQ})", 4m, partUnit.OF_QuantityInParent * data.Part1.OP_Weight);
				AssertEquals($"TotalPacksWeight ({weightUQ})", 120m, whsOrderLine1.WE_TransactionQuantity * data.Part1.OP_Weight);

				// 40 UNT x 2 UNT/KG = 80 KG
				AssertEquals($"TotalPacksWeight ({weightUQ})", 80m, whsOrderLine2.WE_TransactionQuantity * data.Part1.OP_Weight);
			});

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
					new[] { "WHSCHG: 120 Kilogram(s) @ AUD 10.00/KG (for 4 KG/PLT Packs Weight)\n\tWHSCHG: 80 Kilogram(s) @ AUD 5.00/KG (for 2 KG/UNT Packs Weight) => 1600.00", },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_WhsRateLinePackTypeUnitOfQtyHasNoConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 30);
			whsOrderLine1.WE_F3_NKPackType = Weight.Kilotonnes;
			Helper.CreatePickNew(whsOrder);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					"GIVEN rateLine.UnitFactor = PacksWeight and WhsOrderLine PackType has no conversion WHEN autorate THEN should return error with zero amount",
					new[] { "WHSCHG: Calculation failed due to Cannot convert pack type 'KT' for packs weight break search. => 0" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		public void TestPacksWeight_ClientRate_WhsRateLinePackTypeUnitOfQtyLitre()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-4"] = (ZDecimal)5m;
			calculator["+4"] = (ZDecimal)10m;
			calculator["+5"] = (ZDecimal)15m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 30);
			whsOrderLine1.WE_F3_NKPackType = Volume.Litre;
			Helper.CreatePickNew(whsOrder);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertContainsExactElementsInAnyOrder
				(
					new[] { "WHSCHG: 4 Kilogram(s) @ AUD 5.00/KG (for 0.1 KG/L Packs Weight) => 20.00" },
					rateResults.Select(rateResult => $"{rateResult.SingleLineDescription} => {rateResult.Amount}")
				);
			}
		}

		#endregion

		#region TestAutoRating

		public void TestAutoRating_SameChargeWithDifferentUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var partUnit1 = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Spool, partUnitSize: 5m);
			var partUnit2 = Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine1 = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Spool);
			rateLine1.Calculator.Decimal1 = 10m;
			var rateLine2 = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Pallet);
			rateLine2.Calculator.Decimal1 = 20m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var whsOrderLine = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 10m);
			whsOrderLine.WE_F3_NKPackType = PkgUnit.Unit;
			Helper.CreatePickNew(whsOrder);

			var rateResults = Helper.AutoRateJob(whsOrderLine.Order);
			AssertContainsExactElementsInAnyOrder
			(
				"GIVEN rateLine.UnitFactor = PacksWeight WHEN autorate THEN break should use PacksWeight and not TotalPacksWeight",
				new[] { "2 Spool(s) @ AUD 10.00/Spool, 5 Pallet(s) @ AUD 20.00/Pallet => 120.00", },
				rateResults.Select(rateResult => $"{rateResult.CalculationDescription} => {rateResult.Amount}")
			);
		}

		#region TestAutoRating_Containers

		public void TestAutoRating_Containers()
		{
			#region Test Data Setup

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.New<ClientRate>();
			client.OH_IsDebtor = true;
			header.TH_OH = client.PK;

			var refContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR");
			var entry = header.WHSRateEntriesForBinding.AddNew();
			entry.TI_WW_Warehouse = warehouse.PK;
			entry.TI_Mode = "ALL";
			entry.TI_RateStartDate = ZDate.Today.AddDays(-1);
			entry.TI_RateEndDate = ZDate.Today.AddDays(1);
			entry.TI_MatchContainerRateClass = false;
			entry.TI_RC = refContainerType.PK;

			var chargeCode = Helper.CreateChargeCode("WHSCNPACK", "CNTR PACKING", "WOU", "PRC");

			var rateLine1 = entry.AddRateLine("WHSCNPACK", UnitCalculator.Code, QuantityUnit.CN);
			rateLine1.TL_IsOnPallets = true;
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var rateLine2 = entry.AddRateLine("WHSCNPACK", UnitCalculator.Code, QuantityUnit.CN);
			rateLine2.TL_IsOnPallets = false;
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 150m;

			var whsOrder = Helper.CreateWhsOrder(client, warehouse);
			var job = new JobHeader.Loader(whsOrder).TryCreateWithMutex() as Job;
			Helper.CreateWhsDocketContainer(whsOrder, "TRLU1234567", "20FR", true, false);
			Helper.CreateWhsDocketContainer(whsOrder, "ARLU1234567", "20FR", true, true);
			Helper.CreateWhsDocketContainer(whsOrder, "BRLU1234567", "20FR", false, false);
			Helper.CreateWhsDocketContainer(whsOrder, "CRLU1234567", "20GP", true, false);
			Helper.CreateWhsDocketContainer(whsOrder, "CRLU1234567", "40FR", true, false);

			Factory.Save();

			#endregion

			new AutoRatingStarter(whsOrder, null).ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue.With(billingType: BillingType.Invoicing));

			var charge = job.Charges[0];
			AssertEquals("charge.JR_AC", chargeCode.PK, charge.JR_AC);
			AssertEquals("charge.JR_Desc", "CNTR PACKING TEST", charge.JR_Desc);
			AssertEquals("charge.JR_EstimatedRevenue", 250m, charge.JR_EstimatedRevenue);
		}

		#endregion

		#region TestAutoRating_PackagesType

		public void TestAutoRating_PackagesType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Unit)).F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 10m);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var chargeCode = Helper.CreateChargeCode("OHAN", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			var clientRate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(clientRate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK); // Package

			var calculator = (WarehousePackCalculator)rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Box, 0m, 3.50m); // $3.50 per Box
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 5.00m); // $5.00 per Carton

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 25m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickCasesByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			AssertEquals("Precondition - should create 2x cases and 1x split case.", 3, order.PackageJob.Packages.Count);
			AssertEquals("Precondition", 2, order.PackageJob.Packages.Count(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box));
			AssertEquals("Precondition", 1, order.PackageJob.Packages.Count(p => p.KP_F3_NKPackType == Constants.PkgUnit.Carton));

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var result = Helper.AutoRateJob(order);
			CombineAssertions(() =>
			{
				AssertEquals("Should create only rate.", 1, result.Count);
				AssertEquals("Order Handling O1", result[0].InvoiceLineDescription);
				AssertEquals("OHAN: 1 Carton @ AUD 5.00/CTN + 2 Box @ AUD 3.50/BOX", result[0].SingleLineDescription);
				AssertEquals("Revenue", 12m, result[0].LocalAmount);
			});
		}

		public void TestAutoRating_PackagesType_WithPackageQuantityGreaterThan1()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var chargeCode = Helper.CreateChargeCode("OHAN", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			var clientRate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(clientRate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK); // Package

			var calculator = (WarehousePackCalculator)rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Box, 0m, 3.50m); // $3.50 per Box
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 5.00m); // $5.00 per Carton

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 25m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var packageJob = order.PackageJob;
			packageJob.Packages.AddNew(Constants.PkgUnit.Box, 4);
			packageJob.Packages.AddNew(Constants.PkgUnit.Carton, 1);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var result = Helper.AutoRateJob(order);
			CombineAssertions(() =>
			{
				AssertEquals("Should create only rate.", 1, result.Count);
				AssertEquals("Order Handling O1", result[0].InvoiceLineDescription);
				AssertEquals("OHAN: 1 Carton @ AUD 5.00/CTN + 4 Box @ AUD 3.50/BOX", result[0].SingleLineDescription);
				AssertEquals("Revenue", 19m, result[0].LocalAmount);
			});
		}

		public void TestAutoRating_PackagesType_IgnoresInnerPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var chargeCode = Helper.CreateChargeCode("OHAN", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			var clientRate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(clientRate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK); // Package

			var calculator = (WarehousePackCalculator)rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Box, 0m, 3.50m); // $3.50 per Box
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 5.00m); // $5.00 per Carton

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 25m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var packageJob = order.PackageJob;
			var boxPackage = packageJob.Packages.AddNew(Constants.PkgUnit.Box, "BOX123");
			boxPackage.Packages.AddNew(Constants.PkgUnit.Carton, 1);
			boxPackage.Packages.AddNew(Constants.PkgUnit.Bag, 2);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var result = Helper.AutoRateJob(order);
			CombineAssertions(() =>
			{
				AssertEquals("Should create only rate.", 1, result.Count);
				AssertEquals("Order Handling O1", result[0].InvoiceLineDescription);
				AssertEquals("OHAN: 1 Box @ AUD 3.50/BOX", result[0].SingleLineDescription);
				AssertEquals("Revenue", 3.5m, result[0].LocalAmount);
			});
		}

		public void TestAutoRating_PackagesType_UnitCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Unit)).F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 10m);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var chargeCode = Helper.CreateChargeCode("OHAN", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			var clientRate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(clientRate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = Helper.CreateRateLine(rateEntry, chargeCode, QuantityUnit.PK, 4m); // Package

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 25m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickCasesByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			AssertEquals("Precondition - should create 2x cases and 1x split case.", 3, order.PackageJob.Packages.Count);
			AssertEquals("Precondition", 2, order.PackageJob.Packages.Count(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box));
			AssertEquals("Precondition", 1, order.PackageJob.Packages.Count(p => p.KP_F3_NKPackType == Constants.PkgUnit.Carton));

			order.WD_PackagesSent = 7; // ensure real packages are used when exists
			order.WD_F3_NKTotalPackType = Constants.PkgUnit.Box;

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var result = Helper.AutoRateJob(order);
			CombineAssertions(() =>
			{
				AssertEquals("Should create only rate.", 1, result.Count);
				AssertEquals("Order Handling O1", result[0].InvoiceLineDescription);
				AssertEquals("OHAN: 3 Package(s) @ AUD 4.00/Package", result[0].SingleLineDescription);
				AssertEquals("Revenue", 12m, result[0].LocalAmount);
			});
		}

		public void TestAutoRating_PackagesType_NoPackageJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var chargeCode = Helper.CreateChargeCode("OHAN", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			var clientRate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(clientRate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK); // Package

			var calculator = (WarehousePackCalculator)rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Box, 0m, 3.50m); // $3.50 per Box
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 5.00m); // $5.00 per Carton

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 25m);
			order.WD_PackagesSent = 7;
			order.WD_F3_NKTotalPackType = Constants.PkgUnit.Box;
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - should have no packages.", 0, order.PackageJob.Packages.Count);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var result = Helper.AutoRateJob(order);
			CombineAssertions(() =>
			{
				AssertEquals("Should create only 1 rate.", 1, result.Count);
				AssertEquals("Order Handling O1", result[0].InvoiceLineDescription);
				AssertEquals("OHAN: 7 Box @ AUD 3.50/BOX", result[0].SingleLineDescription);
				AssertEquals("Revenue", 24.50m, result[0].LocalAmount);
			});
		}

		public void TestAutoRating_PackagesType_LoadedPackagesOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var chargeCode = Helper.CreateChargeCode("OHAN", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			var clientRate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(clientRate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK); // Package
			rateLine.TL_UnitFactor = UnitFactorList.Codes.LoadedPackagesOnly;

			var calculator = (WarehousePackCalculator)rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Box, 0m, 3.50m); // $3.50 per Box
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 5.00m); // $5.00 per Carton

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 25m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var packageJob = order.PackageJob;
			var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, 2);
			var package2 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, 2);
			var package3 = packageJob.Packages.AddNew(Constants.PkgUnit.Carton, 1);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot1.WLP_GS_NKLoadingUser = "E";
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			pivot2.WLP_LoadedTime = ZDateTimeOffset.Empty;
			var pivot3 = Helper.CreateLoadPkgPackagePivot(package3.PK, load);
			pivot3.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot3.WLP_GS_NKLoadingUser = "E";

			Factory.Save();

			var result = Helper.AutoRateJob(order);
			CombineAssertions(() =>
			{
				AssertEquals("Should create only rate.", 1, result.Count);
				AssertEquals("Order Handling O1", result[0].InvoiceLineDescription);
				AssertEquals("OHAN: 1 Carton @ AUD 5.00/CTN + 2 Box @ AUD 3.50/BOX", result[0].SingleLineDescription);
				AssertEquals("Revenue", 12m, result[0].LocalAmount);
			});
		}

		public void TestAutoRating_PackagesType_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var chargeCode = Helper.CreateChargeCode("OHAN", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			var clientRate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(clientRate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK); // Package
			rateLine.TL_UnitFactor = UnitFactorList.Codes.LoadedPackagesOnly;

			var calculator = (WarehousePackCalculator)rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Box, 0m, 3.50m); // $3.50 per Box
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 5.00m); // $5.00 per Carton

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 25m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var packageJob = order.PackageJob;
			for (var i = 0; i < 10; i++)
			{
				var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, 1);
				var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
				var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
				pivot1.WLP_GS_NKLoadingUser = "E";
				pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			}

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			Factory.Save();

			var expectedHits = new Dictionary<string, int>()
			{
				{ AccAllowedBranchDepartmentComboSchema.Constants.TableName, 1 },
				{ AccChargeBranchOverrideSchema.Constants.TableName, 1 },
				{ AccChargeCodeSchema.Constants.TableName, 1 },
				{ AccChargeRevRecOverrideSchema.Constants.TableName, 1 },
				{ AccChargeTypeOverrideSchema.Constants.TableName, 2 },
				{ AccExchangeRateConfigurationViewSchema.Constants.TableName, 1 },
				{ ExchangeRateCurrencyConfiguration.Schema.TableName, 1 },
				{ DtbConsignmentSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbDepartmentSchema.Constants.TableName, 1 },
				{ GlbDeptChargesSchema.Constants.TableName, 1 },
				{ JobCartageSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 1 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 4 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgInvoiceRollupOrGroupSchema.Constants.TableName, 1 },
				{ OrgInvoiceTypeSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgRateTariffLevelSchema.Constants.TableName, 1 },
				{ OrgRelatedPartySchema.Constants.TableName, 2 },
				{ OrgStaffAssignmentsSchema.Constants.TableName, 1 },
				{ RatingHeaderSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
				{ WhsPickByLabelLabelSchema.Constants.TableName, 0 },
				{ WhsPickTrolleySlotSchema.Constants.TableName, 0 },
			};
			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			var helper = new WhsTestHelperFunctions(otherFactory);
			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, otherFactory))
			{
				helper.AutoRateJob(orderInOtherFactory);
			}
		}

		#endregion

		#region TestAutoRating_BOMKits

		public void TestAutoRating_BOMKit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine1 = rateEntry.AddRateLine(whsChargeCode, UnitCalculator.Code, QuantityUnit.BOM);
			rateLine1.Calculator.Decimal1 = 10m;

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var kit = Helper.CreateProduct(data.Org1, "KIT");
			kit.OP_IsComponentPickedOnSalesOrder = true;
			var component = Helper.CreateProduct(data.Org1, "COMPONENT");
			Helper.CreateProductBOM(kit, component, 3m, "UNT");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", bike, 2m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", component, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, bike, 4m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, kit, 10m);
			var pick = Helper.CreatePickNew(order);

			var rateResults = Helper.AutoRateJob(order);
			AssertContainsExactElementsInAnyOrder
			(
				"7 Bikes and 10 Kits assembled.",
				new[] { "17 BOM Kit(s) @ AUD 10.00/BOM Kit => 170.00", },
				rateResults.Select(rateResult => $"{rateResult.CalculationDescription} => {rateResult.Amount}")
			);
		}

		#endregion

		#endregion

		#region IAutoRating / IJobInvoicingPlugIn Members

		#region TestMeasures_PickByBOM

		public void TestMeasures_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			bomComponentProduct1.OP_StockKeepingUnit = Constants.PkgUnit.Unit;
			Helper.CreateProductUnit(bomComponentProduct1, Constants.PkgUnit.Bundle, 2m);
			Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Bundle);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 6m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 15m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 15m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, mainProduct, 11m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			order.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(2);
			AssertEquals("Precondition", 5m, order.Lines[0].TotalPickLineQuantityFromComponents);
			Factory.Save();

			var adapter = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Should correctly calculate Kit + Built Kit amount.", 11m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals("Should correctly calculate Kit + Built Kit amount.", 11m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Should only be one line, since the Component Lines are not visible.", 1m, rateableMeasures.GetActual(MeasureType.Line));
		}

		public void TestMeasures_BOMKit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 6m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);

			var adapter = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Should be the number of assembled kits.", 3m, rateableMeasures.GetActual(MeasureType.BOMKit));
		}

		public void TestMeasures_BOMKit_MultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var kit = Helper.CreateProduct(data.Org1, "KIT");
			kit.OP_IsComponentPickedOnSalesOrder = true;
			var component = Helper.CreateProduct(data.Org1, "COMPONENT");
			Helper.CreateProductBOM(kit, component, 3m, "UNT");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", bike, 2m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", component, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, bike, 4m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, kit, 10m);
			var pick = Helper.CreatePickNew(order);

			var adapter = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Should be the number of assembled kits.", 17m, rateableMeasures.GetActual(MeasureType.BOMKit));
		}

		#endregion

		#region TestMeasures_ChargeablePallets

		public void TestMeasures_ChargeablePallets_OrderWithMixedPackageAndPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Helper.CreatePickNew(order);

			Factory.Save();

			var packageJob = order.PackageJob;
			var pallet1 = packageJob.Packages.AddNew();
			pallet1.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet1.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var pallet2 = packageJob.Packages.AddNew();
			pallet2.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet2.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var package1 = packageJob.Packages.AddNew();
			package1.Pack(order.Lines[0].ReleaseLines[0], 4m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Carton;

			var package2 = packageJob.Packages.AddNew();
			package2.Pack(order.Lines[0].ReleaseLines[0], 3m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Box;

			var package3 = packageJob.Packages.AddNew();
			package3.Pack(order.Lines[0].ReleaseLines[0], 3m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Bag;

			order.WD_PalletsSent = 3; // User Manually Entered Wrong Value

			Factory.Save();

			var adapter = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Should Fallback to WD_PalletSent Field.", 2m, rateableMeasures.GetActual(MeasureType.ChargeablePallet));
		}

		public void TestMeasures_ChargeablePallets_UsingPalletUOMType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Helper.CreatePickNew(order);

			Factory.Save();

			var palletRefType1 = PackingHelper.CreateRefPackType("PL1", "PL1", 1m, 2m, 3m, Constants.Length.Feet, 7,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.Pallet);

			var palletRefType2 = PackingHelper.CreateRefPackType("PL2", "PL2", 1m, 2m, 3m, Constants.Length.Feet, 7,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.Pallet);

			Factory.Save();

			var packageJob = order.PackageJob;
			var pallet1 = packageJob.Packages.AddNew();
			pallet1.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet1.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var pallet2 = packageJob.Packages.AddNew();
			pallet2.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet2.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var pallet3 = packageJob.Packages.AddNew();
			pallet3.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet3.KP_F3_NKPackType = palletRefType1.F3_Code;

			var pallet4 = packageJob.Packages.AddNew();
			pallet4.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet4.KP_F3_NKPackType = palletRefType2.F3_Code;

			order.WD_PalletsSent = 3; // User Manually Entered Wrong Value

			Factory.Save();

			var adapter = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Should Fallback to WD_PalletSent Field.", 4m, rateableMeasures.GetActual(MeasureType.ChargeablePallet));
		}

		public void TestMeasures_ChargeablePallets_WithNoPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Helper.CreatePickNew(order);

			Factory.Save();

			var palletRefType1 = PackingHelper.CreateRefPackType("PL1", "PL1", 1m, 2m, 3m, Constants.Length.Feet, 7,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.Pallet);

			var palletRefType2 = PackingHelper.CreateRefPackType("PL2", "PL2", 1m, 2m, 3m, Constants.Length.Feet, 7,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.Pallet);

			Factory.Save();

			var packageJob = order.PackageJob;
			var package1 = packageJob.Packages.AddNew();
			package1.Pack(order.Lines[0].ReleaseLines[0], 4m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Carton;

			var package2 = packageJob.Packages.AddNew();
			package2.Pack(order.Lines[0].ReleaseLines[0], 3m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Box;

			var package3 = packageJob.Packages.AddNew();
			package3.Pack(order.Lines[0].ReleaseLines[0], 3m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Bag;

			order.WD_PalletsSent = 3; // User Manually Entered Wrong Value

			Factory.Save();

			var adapter = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Should return 0 if no pallets.", 0m, rateableMeasures.GetActual(MeasureType.ChargeablePallet));
		}

		public void TestMeasures_ChargeablePallets_FallbackToPalletsSentValue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			order.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(2);

			order.WD_PalletsSent = 2;

			Factory.Save();

			var adapter = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Should Fallback to WD_PalletSent Field.", 2m, rateableMeasures.GetActual(MeasureType.ChargeablePallet));
		}

		#endregion

		#region TestMeasures_ChargeablePackages

		public void TestMeasures_ChargeablePackages_OrderWithMixedPackageAndPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Helper.CreatePickNew(order);

			Factory.Save();

			var packageJob = order.PackageJob;
			var pallet1 = packageJob.Packages.AddNew();
			pallet1.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet1.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var pallet2 = packageJob.Packages.AddNew();
			pallet2.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet2.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var package1 = packageJob.Packages.AddNew();
			package1.Pack(order.Lines[0].ReleaseLines[0], 4m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Carton;

			var package2 = packageJob.Packages.AddNew();
			package2.Pack(order.Lines[0].ReleaseLines[0], 3m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Box;

			var package3 = packageJob.Packages.AddNew();
			package3.Pack(order.Lines[0].ReleaseLines[0], 3m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Bag;

			order.WD_PackagesSent = 1; // User Manually Entered Wrong Value

			Factory.Save();

			var adapter = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Should use correct package count.", 3m, rateableMeasures.GetActual(MeasureType.Package));
		}

		public void TestMeasures_ChargeablePackages_OrderWithMixedPackageAndPallets_IncludingUsingPalletUOMType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Helper.CreatePickNew(order);

			Factory.Save();

			var palletRefType = PackingHelper.CreateRefPackType("PL1", "PL1", 1m, 2m, 3m, Constants.Length.Feet, 7,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.Pallet);

			Factory.Save();

			var packageJob = order.PackageJob;
			var pallet1 = packageJob.Packages.AddNew();
			pallet1.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet1.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var pallet2 = packageJob.Packages.AddNew();
			pallet2.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet2.KP_F3_NKPackType = palletRefType.F3_Code;

			var package1 = packageJob.Packages.AddNew();
			package1.Pack(order.Lines[0].ReleaseLines[0], 4m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Carton;

			var package2 = packageJob.Packages.AddNew();
			package2.Pack(order.Lines[0].ReleaseLines[0], 3m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Bag;

			var package3 = packageJob.Packages.AddNew();
			package3.Pack(order.Lines[0].ReleaseLines[0], 3m);
			package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Box;

			order.WD_PackagesSent = 1; // User Manually Entered Wrong Value

			Factory.Save();

			var adapter = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Should use correct package count.", 3m, rateableMeasures.GetActual(MeasureType.Package));
		}

		public void TestMeasures_ChargeablePackages_WithNoPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Helper.CreatePickNew(order);

			Factory.Save();

			var packageJob = order.PackageJob;
			var pallet1 = packageJob.Packages.AddNew();
			pallet1.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet1.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var pallet2 = packageJob.Packages.AddNew();
			pallet2.Pack(order.Lines[0].ReleaseLines[0], 5m);
			pallet2.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			order.WD_PackagesSent = 1; // User Manually Entered Wrong Value

			Factory.Save();

			var adapter = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Should return 0 if no packages.", 0m, rateableMeasures.GetActual(MeasureType.Package));
		}

		public void TestMeasures_ChargeablePackages_FallbackToPackagesSentValue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			order.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(2);

			order.WD_PackagesSent = 2;

			Factory.Save();

			var adapter = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Should Fallback to WD_PackagesSent Field.", 2m, rateableMeasures.GetActual(MeasureType.Package));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_UnitInvalid

		public override void TestIAutoRatingFreightInfo_Measures_WhenWeightUnitIsInvalid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var line1 = docket.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line1.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 5m);

			data.Part1.OP_WeightUQ = "0";
			data.Part1.OP_StockKeepingUnit = "0";

			docket.FinaliseDocketWithoutUserConfirmation();

			var expectedErrorStr =
@"Invalid Weight Unit in this Product Code: P1. Please use following valid unit types:
DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN";
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageWeight));
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.Weight).Single());
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.StorageWeight).Single());

			data.Part1.OP_WeightUQ = "0";
			data.Part1.OP_StockKeepingUnit = Constants.Weight.Kilograms;
			rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageWeight));
			AssertEquals(Constants.Weight.Kilograms, rateableMeasures.GetUnit(MeasureType.Weight));
			AssertEquals(Constants.Weight.Kilograms, rateableMeasures.GetUnit(MeasureType.StorageWeight));
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
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var line1 = docket.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line1.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 5m);

			data.Part1.OP_CubicUQ = "0";
			data.Part1.OP_StockKeepingUnit = "0";

			docket.FinaliseDocketWithoutUserConfirmation();

			var expectedErrorStr =
@"Invalid Volume Unit in this Product Code: P1. Please use following valid unit types:
CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE";
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageVolume));
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.Volume).Single());
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.StorageVolume).Single());

			data.Part1.OP_CubicUQ = "0";
			data.Part1.OP_StockKeepingUnit = Constants.Volume.CubicMetres;

			rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals("Volume should be correct.", 5m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals("StorageVolume should be correct.", 0m, rateableMeasures.GetActual(MeasureType.StorageVolume));

			data.Part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			data.Part1.OP_StockKeepingUnit = Constants.Volume.CubicMetres;

			rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageVolume));
			AssertEquals(Constants.Volume.CubicMetres, rateableMeasures.GetUnit(MeasureType.Volume));
			AssertEquals(Constants.Volume.CubicMetres, rateableMeasures.GetUnit(MeasureType.StorageVolume));
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
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var line1 = docket.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line1.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 5m);

			data.Part1.OP_WeightUQ = "0";
			data.Part1.OP_CubicUQ = "0";
			data.Part1.OP_StockKeepingUnit = "0";

			docket.FinaliseDocketWithoutUserConfirmation();

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

		protected override void TestIAutoRatingFreightInfo_Measures_WithProductAttributes_SerialNumberIsKeyCore(bool isFactorySaved)
		{
			using (WhsTestHelperFunctions.SuspendTrigger(WhsValidationHelper.TG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder, WhsDocketLineSchema.Constants.TableName))
			{
				base.TestIAutoRatingFreightInfo_Measures_WithProductAttributes_SerialNumberIsKeyCore(isFactorySaved);
			}
		}

		#endregion

		public void TestIAutoRatingFreightInfo_FreightMode_IsContainerisedWithContainers()
		{
			var docket = GetNewDocket();
			var adapter = GetIAutoRating(docket);
			var container = Helper.CreateWhsDocketContainer(docket, "CRLU1234567", "20GP", true, false);
			AssertEquals(FreightMode.Containerised, adapter.FreightMode);

			container.WC_IsChargeable = false;
			AssertEquals(FreightMode.UKN, adapter.FreightMode);

			Helper.CreateWhsDocketContainer(docket, "ARLU1234567", "20FR", true, true);
			AssertEquals(FreightMode.Containerised, adapter.FreightMode);
		}

		public override void TestIAutoRatingAdapterTypeAndID()
		{
			var docket = GetNewDocket();
			var adapter = GetIAutoRating(docket);
			AssertEquals(AdapterType.WarehouseOrder, adapter.AdapterType);
			AssertEquals(docket.WD_DocketID, adapter.OperationalJobCode);
		}

		protected override Dictionary<string, int> ExpectedHitsForPickupAddressCore => new Dictionary<string, int>
		{
			{ WhsWarehouseSchema.Constants.TableName, 1 },
			{ OrgAddressSchema.Constants.TableName, 2 },
		};

		#region IAutoRatingOrganisations Members

		#region TestIAutoRatingOrganisationsDeliveryAddress

		public void TestIAutoRatingOrganisationsDeliveryAddress()
		{
			var order = (WhsOrder)GetNewDocket();
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(order);
			var consignee = Factory.New<OrgHeader>();
			order.ConsigneeDocAddress.E2_OA_Address = consignee.Addresses.MainAddress.PK;
			AssertEquals(order.ConsigneeDocAddress, autoRating.DeliveryAddress);
		}

		public void TestIAutoRatingOrganisationsDeliveryAddress_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = GetFinalisableHelper().GetNewFinalisableDocketWithOneLine(data, 10m, finalise: true);
			new JobHeader.Loader(order).TryLoadOrCreateWithoutMutexForTestOnly();

			AfterDocketFinalised(order);

			order.Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsDocket>(order.PK);
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(orderInOtherFactory);
			AssertEquals(order.ConsigneeAddress.PK, autoRating.DeliveryAddress.E2_OA_Address);

			var expectedHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			AssertDbHits(expectedHits, otherFactory);
		}

		#endregion

		#endregion

		#region TestMonthSplitBilling_ChargeStorageInAdvanceCancelsOrderStorage

		public void TestMonthSplitBilling_ChargeStorageInAdvanceCancelsOrderStorage()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = true;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 5), data.Part1, 5m);

			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
			order.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8);

			var iOrder = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)iOrder.RateableMeasures;
			AssertEquals("There should be no storage charges for order", 0m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units must equal 8", 8m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_SplitPeriodBilling_DbHits

		protected override Dictionary<string, int> ExpectedDbHitsForSplitPeriodBillingMeasuresCore => new Dictionary<string, int>
		{
			{ JobStorageSchema.Constants.TableName, 2 },
			{ OrgCompanyDataSchema.Constants.TableName, 1 },
			{ PkgPackageSchema.Constants.TableName, 1 },
			{ PkgPackageJobSchema.Constants.TableName, 1 },
		};

		#endregion

		#region TestMonthSplitBilling_OrdersPartialCharge

		public void TestMonthSplitBilling_OrdersPartialCharge()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//    +100 (incoming)
			//     |   +5 (RCV)                        |
			//-----|----┴---┬--------------------------|
			//     |       -8 (ORD)                    |
			// 2   |                                   |
			// Mth |          1 Mth ago                |
			// ago |                                   |

			// We must charge order storage only for 3 units in this case, because 5 units were charged in receive

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 5), data.Part1, 5m);

			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
			order.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8);

			var iOrder = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)iOrder.RateableMeasures;
			AssertEquals("Charged storage units must equal 8-5 = 3", 3m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units must equal 8", 8m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestSplitBillingNoChargeForUnfinalisedOrders

		public void TestSplitBillingNoChargeForUnfinalisedOrders()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//    +100 (incoming)
			//     |   +5 (RCV)                        |
			//-----|----┴---┬--------------------------|
			//     |       -8 (ORD)                    |
			// 2   |                                   |
			// Mth |          1 Mth ago                |
			// ago |                                   |

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 5), data.Part1, 5m);

			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			// no order finalisation!
			Helper.CreatePickNew(finaliseOrders: false, finalisePick: false, pickableDockets: order);

			var iOrder = GetIAutoRating(order);

			var rateableMeasures = (RateableMeasureSet)iOrder.RateableMeasures;
			AssertEquals("Charged Storage units must equal 0, because order is not finalised", 0m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units must equal 8, because normal charges ignore finalisation status", 8m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestWeekSplitBilling_OrdersPartialCharge

		public void TestWeekSplitBilling_OrdersPartialCharge()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Weekly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//
			// +10   |   +5 (RCV)                        |
			//--┴----|----┴---┬--------------------------|
			//       |       -8 (ORD)                    |
			// prev  |                                   |
			// week  |     invoice week                  |
			//       |                                   |

			var receiveFinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 3));
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", receiveFinalisedDate, data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", receiveFinalisedDate.AddDays(2), data.Part1, 5m);

			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
			order.WD_FinalisedDate = receiveFinalisedDate.AddDays(8); // more than a week from prev week

			var iOrder = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)iOrder.RateableMeasures;
			AssertEquals("Charged storage units must equal 8-5 = 3", 3m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units must equal 8", 8m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestMonthSplitBilling_OrdersMultipleLines

		public void TestMonthSplitBilling_OrdersMultipleLines()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//    +100 (incoming)
			//     |   +5 (RCV)                        |
			//-----|----┴---┬--------------------------|
			//     |       -4 (ORD)                    |
			// 2   |       -4                          |
			// Mth |           1 Mth ago               |
			// ago |                                   |

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 5), data.Part1, 5m);

			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			// 2 lines
			Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
			order.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8);

			var iOrder = GetIAutoRating(order);

			// order lines will be joined for the product (unlike before, when we charge per docket line)
			var rateableMeasures = (RateableMeasureSet)iOrder.RateableMeasures;
			AssertEquals("Charged storage units must equal 8-5 = 3", 3m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units must equal 8", 8m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestMonthSplitBilling_OrdersNoCharge

		public void TestMonthSplitBilling_OrdersNoCharge()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//    +100 (incoming)
			//       |   +5 +3 (RCV)                 |
			//-------|----┴--┴-┬---------------------|
			//       |        -8 (ORD)               |
			// 2     |         1                     |
			// Month |         Month                 |
			// ago   |         ago                   |

			// We must NOT charge order storage, because 5+3 units were charged in receives

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 25), data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 5), data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 7), data.Part1, 3m);

			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
			order.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8);

			var iOrder = GetIAutoRating(order);

			var rateableMeasures = (RateableMeasureSet)iOrder.RateableMeasures;
			AssertEquals("Charged storage units must equal 8-5-3 = 0", 0m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units must equal 8", 8m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestMonthSplitBilling_OrdersFullCharge

		public void TestMonthSplitBilling_OrdersFullCharge()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//    +100 (incoming)
			//     |        +5 (RCV)                  |
			//-----|----┬---┴-------------------------|
			//     |   -8 (ORD)                       |
			// 2   |                                  |
			// Mth |          1 Mth ago               |
			// ago |                                  |

			// We must charge order storage for all 8 units in this case, because there are no receives before order in this month

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8), data.Part1, 5m);

			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
			order.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 2);

			var iOrder = GetIAutoRating(order);

			var rateableMeasures = (RateableMeasureSet)iOrder.RateableMeasures;
			AssertEquals("Charged storage units must equal 8", 8m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units must equal 8", 8m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestMonthSplitBilling_OrdersMixedCase

		public void TestMonthSplitBilling_OrdersMixedCase()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			// here comes complex case

			//    +100 (incoming)
			//     |   +5 +2   +7      (RCV)            |
			//-----|----┴--┴-┬--┴--┬--------------------|
			//     |        -8    -9  (ORD)             |
			// 2   |                                    |
			// Mth |            1 Mth ago               |
			// ago |                                    |

			// First order should be charged for 8-2-5= 1 unit
			// Second order should be charged for 9-7 = 2 units. We ignore previous docket lines, because previous order used all stock from receives before it.

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 2), data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 4), data.Part1, 2m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8), data.Part1, 7m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order1, data.Part1, 8m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order1);
			order1.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 6);
			Factory.Save();

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 9m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order2);
			order2.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 10);
			Factory.Save();

			var iOrder1 = GetIAutoRating(order1);
			var iOrder2 = GetIAutoRating(order2);
			var rateableMeasures1 = (RateableMeasureSet)iOrder1.RateableMeasures;
			var rateableMeasures2 = (RateableMeasureSet)iOrder2.RateableMeasures;
			AssertEquals("Charged storage units for order1 must be 8-2-5 = 1", 1m, rateableMeasures1.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged storage units for order1 must be 8", 8m, rateableMeasures1.GetActual(MeasureType.Unit));
			AssertEquals("Charged storage units for order2 must be 9-7 = 2", 2m, rateableMeasures2.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged storage units for order2 must be 9", 9m, rateableMeasures2.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestMonthSplitBilling_OrdersMixedCaseWhenPreviousOrderDidNotUseAllReceivedStock

		public void TestMonthSplitBilling_OrdersMixedCaseWhenPreviousOrderDidNotUseAllReceivedStock()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			// here comes complex case

			//    +100 (incoming)
			//     |   +5 +2   +1      (RCV)            |
			//-----|----┴--┴-┬--┴--┬--------------------|
			//     |        -4    -9  (ORD)             |
			// 2   |                                    |
			// Mth |            1 Mth ago               |
			// ago |                                    |

			// First order should NOT be charged, because it used stock from receives:  4-2-5 = -3
			// Second order should be charged for 9-1+4-2-5 = 5 units. We DO NOT ignore previous docket lines,
			// because previous order did NOT use all stock from receives before it.

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 2), data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 4), data.Part1, 2m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8), data.Part1, 1m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order1, data.Part1, 4m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order1);
			order1.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 6);
			Factory.Save();

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 9m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order2);
			order2.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 10);
			Factory.Save();

			var iOrder1 = GetIAutoRating(order1);
			var iOrder2 = GetIAutoRating(order2);
			var rateableMeasures1 = (RateableMeasureSet)iOrder1.RateableMeasures;
			var rateableMeasures2 = (RateableMeasureSet)iOrder2.RateableMeasures;
			AssertEquals("Charged storage units for order1 must be 0", 0m, rateableMeasures1.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units for order1 must be 4", 4m, rateableMeasures1.GetActual(MeasureType.Unit));
			AssertEquals("Charged units for order2 must be 9-1+4-2-5 = 5", 5m, rateableMeasures2.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units for order2 must be 9", 9m, rateableMeasures2.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestPeriodSplitBillingTakesCurrentInvoiceIfPresent

		public void TestPeriodSplitBillingTakesCurrentInvoiceIfPresent()
		{
			var now = ZDateTime.Now;
			var oneMonthAgoDate = now.AddMonths(-1);
			var twoMonthAgoDate = now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			var invoice = Factory.New<IWhsInvoice>();
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 5);
			invoice.ET_StorageToDate = new ZDateTime(now.Year, now.Month, 4);

			//
			//                  |   +10 +5    (RCV)             |
			//------------------|----┴--┴-┬---------------------|--------------------
			//                  |      ^ -8   (ORD)             |
			//      2 Mths ago  |      |    1 Mth ago           |   Now
			//                  |      |                        |
			//  |<------------------Invoice------------------------>|
			//  |  1st month (invoice) |  2nd month (invoice)       |

			// The main thing of this test is that day from which running total will be calculated for order
			// is 5th of August because current invoice takes more than 1 month.
			var receiveFinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 4));

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", receiveFinalisedDate, data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", receiveFinalisedDate.AddDays(1), data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			order.WD_FinalisedDate = receiveFinalisedDate.AddDays(6);
			Factory.Save();

			var iOrder = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)iOrder.RateableMeasures;
			AssertEquals("Charged units for order1 must be 8-5 = 3", 3m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units for order1 must be 8", 8m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestPeriodSplitBillingTakesPreviousInvoiceIfNoCurrentPresent

		public void TestPeriodSplitBillingTakesPreviousInvoiceIfNoCurrentPresent()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			var invoice = Factory.New<IWhsInvoice>();
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 5);
			invoice.ET_StorageToDate = new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 4);

			//
			//                  |   +10 +5    (RCV)             |
			//------------------|----┴--┴-┬---------------------|--------------------
			//                  |      ^ -8   (ORD)             |
			//      2 Mth ago   |      |    1 Mth Ago           |   Now
			//                  |      |                        |
			//  |<-------Invoice------>|
			var receiveFinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 4));
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", receiveFinalisedDate, data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", receiveFinalisedDate.AddDays(1), data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			order.WD_FinalisedDate = receiveFinalisedDate.AddDays(6);
			Factory.Save();

			var iOrder = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)iOrder.RateableMeasures;
			AssertEquals("Charged units for order1 must be 8-5 = 3", 3m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units for order1 must be 8", 8m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestPeriodSplitBillingTakesDefaultPeriodIfNoInvoicePresent

		public void TestPeriodSplitBillingTakesDefaultPeriodIfNoInvoicePresent()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Weekly; // Weekly!
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//
			//      | +10  +5    (RCV)
			//------|--┴---┴-┬--------------------
			//      |   ^   -8   (ORD)
			// 2    |   |    ^      1 Month Ago
			// Mth  |   |week|
			// ago  |   |    |

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 4), data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8), data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			order.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 12);
			Factory.Save();

			var iOrder = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)iOrder.RateableMeasures;
			AssertEquals("Charged units for order1 must be 8-5 = 3", 3m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units for order1 must be 8", 8m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestPeriodSplitBillingIgnoresOrderTimePart

		public void TestPeriodSplitBillingIgnoresOrderTimePart()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Weekly; // Weekly!
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//
			//      | +10  +5    (RCV)
			//------|--┴---┴-┬--------------------
			// 2    |   ^   -8   (ORD)
			// Mth  |   |    ^      1 Mth ago
			// ago      |week|
			//          |    |

			var invoice = Factory.New<IWhsInvoice>();
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 6);
			invoice.ET_StorageToDate = new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 12);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 4), data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8), data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			order.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 12, 11, 30, 55);
			Factory.Save();

			var iOrder = GetIAutoRating(order);
			var rateableMeasures = (RateableMeasureSet)iOrder.RateableMeasures;
			AssertEquals("Charged units for order1 must be 8-5 = 3", 3m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged units for order1 must be 8", 8m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestIAutoRating_ConsumerType

		protected override void TestIAutoRatingConsumerTypeCore()
		{
			var jobInvPlugIn = GetIAutoRating(GetNewDocket());
			AssertEquals(JobInvoicingConsumerTypes.WarehouseOutwards, jobInvPlugIn.InvoicingSupporter.ConsumerType);
		}

		protected override SchemaColumn AutoratingChargeablePalletsColumn
		{
			get { return WhsDocketSchema.WD_PalletsSent; }
		}

		#endregion

		#region TestIAutoRatingPopulateChargeCodeGroups

		public override void TestIAutoRatingPopulateChargeCodeGroups()
		{
			var iOrder = GetIAutoRating(GetNewDocket());
			AssertEquals(1, iOrder.ChargeCodeGroups.Count);
			AssertCollectionContains(ChargeCodeGroupList.Codes.WHSOutwards, iOrder.ChargeCodeGroups);
		}

		#endregion

		public void TestPackageLineUnitFactor_InvoiceLineDescriptionHasDocketReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.Postcode = "2015";
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator.AddRateLineItem(Items.Operator.Minus, 4m, 5m, QuantityUnit.KM);
			calculator["+4"] = (ZDecimal)10m;
			calculator["+10"] = (ZDecimal)15m;
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var whsOrderLine = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine.WE_F3_NKPackType = PkgUnit.Pallet;

			whsOrder.ConsigneeDocAddress.E2_AddressOverride = true;
			whsOrder.ConsigneeDocAddress.E2_City = "Sydney";
			whsOrder.ConsigneeDocAddress.E2_Postcode = "2000";
			whsOrder.ConsigneeDocAddress.E2_State = "NSW";
			whsOrder.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Part1: Weight", 2m, data.Part1.OP_Weight);
				AssertEquals("Package: Weight", 6m, package.KP_Weight);
			});

			package.Pack(whsOrderLine.ReleaseLines[0], 10m);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				Factory.Save();

				var rateResults = Helper.AutoRateJob(whsOrder);
				AssertEquals("Warehouse Charge O1", rateResults[0].InvoiceLineDescription);
			}
		}

		public void TestPackageLineUnitFactor_PackageRateableMeasureIncludesDocketReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.Postcode = "2015";
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator.AddRateLineItem(Items.Operator.Minus, 4m, 5m, QuantityUnit.KM);
			calculator["+4"] = (ZDecimal)10m;
			calculator["+10"] = (ZDecimal)15m;
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var whsOrderLine = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine.WE_F3_NKPackType = PkgUnit.Pallet;

			whsOrder.ConsigneeDocAddress.E2_AddressOverride = true;
			whsOrder.ConsigneeDocAddress.E2_City = "Sydney";
			whsOrder.ConsigneeDocAddress.E2_Postcode = "2000";
			whsOrder.ConsigneeDocAddress.E2_State = "NSW";
			whsOrder.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var package = PackingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			package.Pack(whsOrderLine.ReleaseLines[0], 10m);
			Factory.Save();

			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var invoice = Factory.New<IWhsInvoice>();
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 6);
			invoice.ET_StorageToDate = new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 12);
			Factory.Save();

			var iOrder = GetIAutoRating(whsOrder);
			var partList = ((RateableMeasureSet)iOrder.RateableMeasures).GetPartList(MeasureType.WarehousePackage);
			AssertEquals(1, partList.Count);
			AssertEquals("O1", partList[0].DocketReference);
		}

		#endregion

		#region Implementation

		protected override void SetFinalisedDocketFields(WhsDocket docket, ZDateTime finalisedDate) => docket.WD_FinalisedDate = ZDateTimeOffset.Now;

		protected override FinalisableDocketHelper<WhsOrder> GetFinalisableHelper() => new FinalisableOrderHelper(Factory);

		protected override void AfterDocketFinalised(WhsOrder docket)
		{
			base.AfterDocketFinalised(docket);
			docket.Pick.FinalisePick();
			AssertIsFinalisedPrecondition(docket.Pick);
		}

		protected override void SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(WhsDocketLine line, ZDecimal units)
		{
			var pickableLine = line as WhsPickableDocketLine;
			if (pickableLine != null)
			{
				if (pickableLine.WE_TransactionQuantity < units)
				{
					pickableLine.WE_TransactionQuantity = units; //prevent validation error in WhsDocketLineAttributesValidation
				}

				var receiveReference = line.PK.ToString().Substring(0, 20); // ensuring we will get unique receive references
				var order = (WhsPickableDocket)pickableLine.Docket;
				if (order.WD_WP.IsEmpty)
				{
					var receive = Helper.CreateWhsReceiveWithInventory(order.Client, order.Warehouse, receiveReference, pickableLine.SupplierPart, units);
					receive.FinaliseDocket();
					Factory.Save();

					order.WD_RequiredDate = ZDateTimeOffset.Today;
					order.ConsigneeNameOrPK = Helper.CreateClient("CNE").PK.ToString();
					Helper.CreatePickNew(order);
				}
				else if (!(pickableLine.PickLines.Count > 0))
				{
					Helper.CreateWhsReceiveWithInventory(order.Client, order.Warehouse, receiveReference, pickableLine.SupplierPart, units);
					Factory.Save();

					order.Pick.ClearAllInventoriesCache();
					order.Pick.ClearOrderedInventoriesCache();
					order.Pick.AutoAllocateItemsWithMock();
				}
				order.Pick.IsAlterPick = true;
				pickableLine.ReleaseLines[0].Quantity = units;
			}
			else
			{
				base.SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line, units);
			}
		}

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsOrder>();
		}

		protected override IAutoRating GetIAutoRating(WhsDocket docket)
		{
			return new WhsOrderRatingAdapter((WhsOrder)docket);
		}

		#endregion
	}
}
