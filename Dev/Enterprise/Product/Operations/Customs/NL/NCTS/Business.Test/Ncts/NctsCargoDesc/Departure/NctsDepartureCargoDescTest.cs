using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureCargoDesc))]
sealed class NctsDepartureCargoDescTest : EU.NCTS.Business.Testing.NctsDepartureCargoDescAbstractTest<NctsHeader>
{
	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateNctsDepartureCargoDesc();

	protected override BusinessObject GetNewBusinessObject() => CreateNctsDepartureCargoDesc();

	protected override ZString CountryCode => Core.Constants.CountryCodes.Netherlands;

	public void TestNotifyChangeOfCAL() => CombineAssertions(() =>
	{
		var cargoDesc = CreateNctsDepartureCargoDescForTest();

		cargoDesc.NotifyChangeOfCAL("WGT", "WGT");
		AssertEquals("change from WGT to WGT", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());

		var dtyPercent = CreateFee(cargoDesc, "DTY", "%");
		var vatWeight = CreateFee(cargoDesc, "VAT", "WGT");
		cargoDesc.NotifyChangeOfCAL("WGT", "X");

		AssertEquals("change from X to WGT", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		AssertEquals("change from X to WGT, dtyPercent fee deleted", false, cargoDesc.Fees.Contains(dtyPercent));
		AssertEquals("change from X to WGT, vatWeight fee deleted", false, cargoDesc.Fees.Contains(vatWeight));

		var dtyWeight = CreateFee(cargoDesc, "DTY", "WGT");
		cargoDesc.NotifyChangeOfCAL("X", "WGT");
		AssertEquals("change from WGT to X", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		AssertEquals("change from WGT to X, dtyWeight fee deleted", false, cargoDesc.Fees.Contains(dtyWeight));
	});

	public void TestNotifyChangeOfExportTransportMode() => CombineAssertions(() =>
	{
		var cargoDesc = CreateNctsDepartureCargoDescForTest();

		cargoDesc.Header.CALCalculationMethod = "WGT";
		cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled();
		cargoDesc.NotifyChangeOfExportTransportModeFallbackOnInlandTransportMode("4", "4");
		AssertEquals("WGT, changed from 4 to 4", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.NotifyChangeOfExportTransportModeFallbackOnInlandTransportMode("4", "X");
		AssertEquals("WGT, changed from X to 4", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.NotifyChangeOfExportTransportModeFallbackOnInlandTransportMode("X", "4");
		AssertEquals("WGT, changed from 4 to X", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.NotifyChangeOfExportTransportModeFallbackOnInlandTransportMode("1", "X");
		AssertEquals("WGT, changed from X to 1", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.NotifyChangeOfExportTransportModeFallbackOnInlandTransportMode("X", "1");
		AssertEquals("WGT, changed from 1 to X", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());

		cargoDesc.Header.CALCalculationMethod = "X";
		cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled();
		cargoDesc.NotifyChangeOfExportTransportModeFallbackOnInlandTransportMode("X", "4");
		AssertEquals("not WGT, changed from 4 to X", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
	});

	public void TestUpdateAllFeesFromTariffRatesCalledOnWeightChanges() => CombineAssertions(() =>
	{
		var cargoDesc = CreateNctsDepartureCargoDescForTest();

		cargoDesc.BY_NetWeight = 20;
		AssertEquals("net weight changed, CAl is not WGT", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.BY_NetWeight = ZDecimal.Zero;
		AssertEquals("net weight changed to zero, CAl is not WGT", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.BY_NetWeightUnit = "KG";
		AssertEquals("net weight unit changed, CAl is not WGT", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.BY_GrossWeight = 20;
		AssertEquals("gross weight changed, CAl is not WGT", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.BY_GrossWeightUnit = "KG";
		AssertEquals("gross weight unit changed, CAl is not WGT", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());

		cargoDesc.Header.CALCalculationMethod = "WGT";
		cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled();
		cargoDesc.BY_NetWeight = 40;
		AssertEquals("net weight changed", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.BY_NetWeight = ZDecimal.Zero;
		AssertEquals("net weight changed to zero", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.BY_NetWeightUnit = "G";
		AssertEquals("net weight unit changed", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.BY_GrossWeight = 40;
		AssertEquals("gross weight changed", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.BY_GrossWeightUnit = "G";
		AssertEquals("gross weight unit changed", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
	});

	public void TestNotifiedOnNctsHeaderCALChange()
	{
		var cargoDesc = CreateNctsDepartureCargoDescForTest();
		cargoDesc.Header.CALCalculationMethod = "WGT";
		AssertEquals(true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
	}

	public void TestNotifiedOnMovementHeaderExportTransportModeChange() => CombineAssertions(() =>
	{
		var cargoDesc = CreateNctsDepartureCargoDescForTest();
		cargoDesc.Header.CALCalculationMethod = "WGT";
		cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled();
		cargoDesc.Header.MovementHeader.BM_ExportTransportMode = "4";
		AssertEquals("export: '' -> 4, inland: ''. With fallback '' -> 4", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.Header.MovementHeader.BM_InlandTransportMode = "4";
		AssertEquals("export: 4, inland: '' -> 4. With fallback 4 -> 4", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.Header.MovementHeader.BM_ExportTransportMode = "";
		AssertEquals("export: 4 -> '', inland: 4. With fallback 4 -> 4", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
	});

	public void TestNotifiedOnMovementHeaderInlandTransportModeChange() => CombineAssertions(() =>
	{
		var cargoDesc = CreateNctsDepartureCargoDescForTest();
		cargoDesc.Header.CALCalculationMethod = "WGT";
		cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled();
		cargoDesc.Header.MovementHeader.BM_InlandTransportMode = "4";
		AssertEquals("export: '', inland: '' -> 4. With fallback '' -> 4", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.Header.MovementHeader.BM_ExportTransportMode = "4";
		AssertEquals("export: '' -> 4, inland: 4. With fallback 4 -> 4", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		cargoDesc.Header.MovementHeader.BM_InlandTransportMode = "";
		AssertEquals("export: 4, inland: 4 -> ''. With fallback 4 -> 4", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
	});

	public void TestNotifiedOnNctsDepartureHeaderContainerContainerModeChange()
	{
		var cargoDesc = CreateNctsDepartureCargoDescForTest();
		cargoDesc.Header.CALCalculationMethod = "WGT";
		cargoDesc.Header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled();
		var container = cargoDesc.Header.DepartureHeaderContainers.AddNew();
		AssertEquals("Create of container, no container mode set", false, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		container.BC_Mode = Core.Constants.ContainerModes.Containerised;
		AssertEquals("Container Mode is changed to 'Containerized'", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
		container.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
		AssertEquals("Container Mode is changed away from 'Non Containerized", true, cargoDesc.GetAndResetUpdateAllFeesFromTariffRatesWasCalled());
	}

	public void TestUpdateAllFeesFromTariffRates_WGT_AIR() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands);
		var c400 = helper.CreateTaxOrFee(NLNctsConstants.TaxOrFeeCodes.C400, 12, Core.Constants.CountryCodes.Netherlands);
		var c414 = helper.CreateTaxOrFee(NLNctsConstants.TaxOrFeeCodes.C414, 1.3, Core.Constants.CountryCodes.Netherlands);
		var c480 = helper.CreateTaxOrFee(NLNctsConstants.TaxOrFeeCodes.C480, 40, Core.Constants.CountryCodes.Netherlands);

		var cargoDesc = CreateNctsDepartureCargoDesc();
		var header = cargoDesc.Header;
		header.CALCalculationMethod = "WGT";
		header.MovementHeader.BM_ExportTransportMode = "4";
		cargoDesc.BY_NetWeight = 10_000;
		cargoDesc.BY_NetWeightUnit = "G";
		cargoDesc.BY_FormattedHarmonisedTariff = "01";
		var fee = cargoDesc.Fees.First();
		AssertEquals("tariff 01, charge type", "DTY", fee.BFE_ChargeType);
		AssertEquals("tariff 01, method", "WGT", fee.BFE_MethodOfCalculation);
		AssertEquals("tariff 01, base", 10M, fee.BFE_BaseValue);
		AssertEquals("tariff 01, rate", 1.3M, fee.BFE_Rate);
		AssertEquals("tariff 01, amount", 13M, fee.BFE_ChargeAmount);
		cargoDesc.BY_NetWeight = ZDecimal.Zero;
		cargoDesc.BY_GrossWeight = 20;
		cargoDesc.BY_GrossWeightUnit = "KG";
		fee = cargoDesc.Fees.First();
		AssertEquals("tariff 01, base (taking gross weight)", 20M, fee.BFE_BaseValue);
		AssertEquals("tariff 01, (gross weight) amount", 26M, fee.BFE_ChargeAmount);
		header.MovementHeader.BM_ExportTransportMode = "";
		header.MovementHeader.BM_InlandTransportMode = "4";
		cargoDesc.BY_FormattedHarmonisedTariff = "84";
		fee = cargoDesc.Fees.First();
		AssertEquals("tariff 84, rate", 40M, fee.BFE_Rate);
		cargoDesc.BY_FormattedHarmonisedTariff = "77";
		fee = cargoDesc.Fees.First();
		AssertEquals("tariff 77 (filled, but not listed), rate", 12M, fee.BFE_Rate);
	});

	public void TestUpdateAllFeesFromTariffRates_WGT_SEA() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands);
		var c114 = helper.CreateTaxOrFee(NLNctsConstants.TaxOrFeeCodes.C114, 0.65, Core.Constants.CountryCodes.Netherlands);
		var c180 = helper.CreateTaxOrFee(NLNctsConstants.TaxOrFeeCodes.C180, 2.8, Core.Constants.CountryCodes.Netherlands);
		var c100 = helper.CreateTaxOrFee(NLNctsConstants.TaxOrFeeCodes.C100, 0.4, Core.Constants.CountryCodes.Netherlands);

		var cargoDesc = CreateNctsDepartureCargoDesc();
		var header = cargoDesc.Header;
		var container = header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = "CNT";
		container.BC_ContainerNum = "MSCU1234566";

		header.CALCalculationMethod = NL.Business.CalculationMethodList.Codes.WGT;
		header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		cargoDesc.BY_NetWeight = 10;
		cargoDesc.BY_GrossWeight = 20;
		cargoDesc.BY_HarmonisedTariff = "0304798000";

		var fee = cargoDesc.Fees.FirstOrDefault();
		AssertEquals("Tariff 01-14, Number of fees", 1, cargoDesc.Fees.Count);
		AssertEquals("Tariff 01-14, Charge Type", "DTY", fee.BFE_ChargeType);
		AssertEquals("Tariff 01-14, Calculation Method", "WGT", fee.BFE_MethodOfCalculation);
		AssertEquals("Tariff 01-14, Base Amount", 10M, fee.BFE_BaseValue);
		AssertEquals("Tariff 01-14, Rate Amount", 0.65M, fee.BFE_Rate);
		AssertEquals("Tariff 01-14, Charge Amount", 6.5M, fee.BFE_ChargeAmount);

		cargoDesc.BY_NetWeight = ZDecimal.Zero;
		header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		fee = cargoDesc.Fees.FirstOrDefault();
		AssertEquals("Tariff 01-14, Number of fees (based on gross weight)", 1, cargoDesc.Fees.Count);
		AssertEquals("Tariff 01-14, Base Amount (based on gross weight)", 20M, fee.BFE_BaseValue);
		AssertEquals("Tariff 01-14, Charge Amount (based on gross weight)", 13.0M, fee.BFE_ChargeAmount);

		cargoDesc.BY_HarmonisedTariff = "84";
		fee = cargoDesc.Fees.FirstOrDefault();
		AssertEquals("Tariff 84-85, Number of fees", 1, cargoDesc.Fees.Count);
		AssertEquals("Tariff 84-85, Rate", 2.8M, fee.BFE_Rate);

		cargoDesc.BY_HarmonisedTariff = "8001100000";
		fee = cargoDesc.Fees.FirstOrDefault();
		AssertEquals("Tariff 80 (not listed), Number of fees", 1, cargoDesc.Fees.Count);
		AssertEquals("Tariff 80 (not listed), Rate", 0.4M, fee.BFE_Rate);

		cargoDesc.BY_HarmonisedTariff = ZString.Empty;
		fee = cargoDesc.Fees.FirstOrDefault();
		AssertEquals("Empty tariff, Number of fees", 0, cargoDesc.Fees.Count);

		header.DepartureHeaderContainers.RemoveAndDeleteAll();
		cargoDesc.BY_HarmonisedTariff = "01";
		AssertEquals("Tariff 01-14, No container, Number of fees", 0, cargoDesc.Fees.Count);
		cargoDesc.BY_HarmonisedTariff = "84";
		AssertEquals("Tariff 84-85, No container, Number of fees", 0, cargoDesc.Fees.Count);
		cargoDesc.BY_HarmonisedTariff = "77";
		AssertEquals("Tariff 77 (not listed), No container, Number of fees", 0, cargoDesc.Fees.Count);
		cargoDesc.BY_HarmonisedTariff = string.Empty;
		AssertEquals("Empty tariff, No container, Number of fees", 0, cargoDesc.Fees.Count);
	});

	public void TestUpdateAllFeesFromTariffRates_WGT_NotAirOrSea() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands);
		var c000 = helper.CreateTaxOrFee(NLNctsConstants.TaxOrFeeCodes.C000, 0.36, Core.Constants.CountryCodes.Netherlands);

		var cargoDesc = CreateNctsDepartureCargoDesc();
		var header = cargoDesc.Header;
		header.CALCalculationMethod = "WGT";
		header.MovementHeader.BM_ExportTransportMode = "2";
		cargoDesc.BY_NetWeight = 10_000;
		cargoDesc.BY_NetWeightUnit = "G";
		cargoDesc.BY_FormattedHarmonisedTariff = "01";
		var fee = cargoDesc.Fees.First();
		AssertEquals("tariff 01, charge type", "DTY", fee.BFE_ChargeType);
		AssertEquals("tariff 01, method", "WGT", fee.BFE_MethodOfCalculation);
		AssertEquals("tariff 01, base", 10M, fee.BFE_BaseValue);
		AssertEquals("tariff 01, rate", 0.36M, fee.BFE_Rate);
		AssertEquals("tariff 01, amount", 3.6M, fee.BFE_ChargeAmount);
		cargoDesc.BY_NetWeight = ZDecimal.Zero;
		cargoDesc.BY_GrossWeight = 20;
		cargoDesc.BY_GrossWeightUnit = "KG";
		fee = cargoDesc.Fees.First();
		AssertEquals("tariff 01, base (taking gross weight)", 20M, fee.BFE_BaseValue);
		AssertEquals("tariff 01, (gross weight) amount", 7.2M, fee.BFE_ChargeAmount);
		header.MovementHeader.BM_ExportTransportMode = "";
		header.MovementHeader.BM_InlandTransportMode = "5";
		fee = cargoDesc.Fees.First();
		AssertEquals("tariff 01, rate", 0.36M, fee.BFE_Rate);
	});

	NctsDepartureCargoDescForTest CreateNctsDepartureCargoDescForTest()
	{
		var cargoDesc = Factory.New<NctsDepartureCargoDescForTest>();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var bill = nctsHeader.Bills.AddNew();
		cargoDesc.BY_ParentID = bill.PK;
		cargoDesc.BY_ParentTableCode = bill.TablePrefix;

		return cargoDesc;
	}

	NctsDepartureCargoDesc CreateNctsDepartureCargoDesc()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var bill = nctsHeader.Bills.AddNew();
		return bill.GoodsItems.AddNew();
	}

	NctsCargoDescFee CreateFee(NctsDepartureCargoDesc cargoDesc, string chargeType, string methodOfCalculation)
	{
		var fee = cargoDesc.Fees.AddNew();
		fee.BFE_ChargeType = chargeType;
		fee.BFE_MethodOfCalculation = methodOfCalculation;
		return fee;
	}

	class NctsDepartureCargoDescForTest : NctsDepartureCargoDesc
	{
		public NctsDepartureCargoDescForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool GetAndResetUpdateAllFeesFromTariffRatesWasCalled()
		{
			var wasCalled = updateAllFeesFromTariffRatesCalled;
			updateAllFeesFromTariffRatesCalled = false;
			return wasCalled;
		}

		bool updateAllFeesFromTariffRatesCalled;

		public override void UpdateAllFeesFromTariffRates()
		{
			base.UpdateAllFeesFromTariffRates();
			updateAllFeesFromTariffRatesCalled = true;
		}
	}
}
