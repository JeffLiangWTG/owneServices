using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing
{
	public class DutyCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateDuty()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
				var rateType = helper.CreateNewOrGetExistingRateType("NZ", "DTY");
				var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", rateType.PK);
				var preference1 = helper.CreatePreferenceForCountry("Q", "Qualify", "NZ");
				var preference2 = helper.CreatePreferenceForCountry("N", "UnQualify", "NZ");
				var tariff = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
				var rate1 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "(5.000000*(VFD/100))+(0.700240*[LMS])", preference1.PK);
				var rate2 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "(2.000000*(VFD/100))+(0.500240*[LMS])", preference1.PK);
				var rate3 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "10.000000*(VFD/100)", preference2.PK);
				var tradeGroup1 = helper.LoadOrCreateTradeGroup("NZ", "AU", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				var tradeGroup2 = helper.LoadOrCreateTradeGroup("NZ", "TPP", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				var tradeGroup3 = helper.LoadOrCreateTradeGroup("NZ", "STD", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				helper.AddCountry(tradeGroup1, "AU");
				helper.AddCountry(tradeGroup2, "AU");
				helper.CreateCusApplicability(rate1, tradeGroup1, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				helper.CreateCusApplicability(rate2, tradeGroup2, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				helper.CreateCusApplicability(rate3, tradeGroup3, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

				var testObjects = new DutyCalculatorTestObjects(Factory);
				testObjects.Declaration.JE_DateOfArrival = ZDateTime.Today;
				testObjects.InvoiceLine.JI_Tariff = "123456789";
				testObjects.InvoiceLine.JI_CountryOfOrigin = "AU";
				testObjects.InvoiceLine.JI_LinePrice = 150m;
				testObjects.InvoiceLine.JI_CustomsQuantity = 10m;
				testObjects.InvoiceLine.JI_CustomsUnitQty = "LMS";
				testObjects.InvoiceLine.JI_QualifiesForPreferentialDuty = "Q";
				testObjects.InvoiceLine.JI_PreferentialCountryGroup = "AU";
				var dutyCalc = new DutyCalculator(testObjects.InvoiceLine);

				CombineAssertions(() =>
				{
					AssertEquals("ActualClassification", "123456789", dutyCalc.ActualClassification);
					AssertEquals("DutyRatePercent", 5.00m, dutyCalc.DutyRatePercent);
					AssertEquals("DutyRateFlatRate", 0.700240m, dutyCalc.DutyRateFlatRate);
					AssertEquals("DutyRateFlatUQ", "LMS", dutyCalc.DutyRateFlatUQ);
					AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
					AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
					AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
					AssertEquals("PFMLFuelLevyAmount", 0.00m, dutyCalc.PFMLFuelLevyAmount);
					AssertEquals("SyntheticGreenhouseGasesLevyAmount", 0.00m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
					AssertEquals("DutyAmount", 14.50m, dutyCalc.DutyAmount);
					AssertEquals("DutyRateComplete", "123456789 @ AU = 5.00% $0.7002/LMS", dutyCalc.DutyRateComplete);
				});

				testObjects.InvoiceLine.JI_QualifiesForPreferentialDuty = "N";
				dutyCalc.Update();
				CombineAssertions(() =>
				{
					AssertEquals("ActualClassification", "123456789", dutyCalc.ActualClassification);
					AssertEquals("DutyRatePercent", 10.00m, dutyCalc.DutyRatePercent);
					AssertEquals("DutyRateFlatRate", 0.00m, dutyCalc.DutyRateFlatRate);
					AssertEquals("DutyRateFlatUQ", "", dutyCalc.DutyRateFlatUQ);
					AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
					AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
					AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
					AssertEquals("PFMLFuelLevyAmount", 0.00m, dutyCalc.PFMLFuelLevyAmount);
					AssertEquals("SyntheticGreenhouseGasesLevyAmount", 0.00m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
					AssertEquals("DutyAmount", 15.00m, dutyCalc.DutyAmount);
					AssertEquals("DutyRateComplete", "123456789 @ STD = 10.00%", dutyCalc.DutyRateComplete);
				});
				testObjects.InvoiceLine.JI_QualifiesForPreferentialDuty = "Q";

				using (NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					testObjects.InvoiceLine.JI_PreferentialCountryGroup = "";
					dutyCalc.Update();
					CombineAssertions(() =>
					{
						AssertEquals("ActualClassification", "123456789", dutyCalc.ActualClassification);
						AssertEquals("DutyRatePercent", 2.00m, dutyCalc.DutyRatePercent);
						AssertEquals("DutyRateFlatRate", 0.500240m, dutyCalc.DutyRateFlatRate);
						AssertEquals("DutyRateFlatUQ", "LMS", dutyCalc.DutyRateFlatUQ);
						AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
						AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
						AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
						AssertEquals("PFMLFuelLevyAmount", 0.00m, dutyCalc.PFMLFuelLevyAmount);
						AssertEquals("SyntheticGreenhouseGasesLevyAmount", 0.00m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
						AssertEquals("DutyAmount", 8.00m, dutyCalc.DutyAmount);
						AssertEquals("DutyRateComplete", "123456789 @ TPP = 2.00% $0.5002/LMS", dutyCalc.DutyRateComplete);
					});
				}
				testObjects.InvoiceLine.JI_PreferentialCountryGroup = "AU";

				testObjects.InvoiceLine.JI_IsZeroRatedDuty = "Y";
				dutyCalc.Update();
				CombineAssertions(() =>
				{
					AssertEquals("ActualClassification", "123456789", dutyCalc.ActualClassification);
					AssertEquals("DutyRatePercent", 0.00m, dutyCalc.DutyRatePercent);
					AssertEquals("DutyRateFlatRate", 0.700240m, dutyCalc.DutyRateFlatRate);
					AssertEquals("DutyRateFlatUQ", "LMS", dutyCalc.DutyRateFlatUQ);
					AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
					AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
					AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
					AssertEquals("PFMLFuelLevyAmount", 0.00m, dutyCalc.PFMLFuelLevyAmount);
					AssertEquals("SyntheticGreenhouseGasesLevyAmount", 0.00m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
					AssertEquals("DutyAmount", 7.00m, dutyCalc.DutyAmount);
					AssertEquals("DutyRateComplete", "123456789 @ AU = $0.7002/LMS", dutyCalc.DutyRateComplete);
				});
				testObjects.InvoiceLine.JI_IsZeroRatedDuty = "N";

				testObjects.InvoiceLine.JI_IsZeroRatedExcise = "Y";
				dutyCalc.Update();
				CombineAssertions(() =>
				{
					AssertEquals("ActualClassification", "123456789", dutyCalc.ActualClassification);
					AssertEquals("DutyRatePercent", 5.00m, dutyCalc.DutyRatePercent);
					AssertEquals("DutyRateFlatRate", 0.00m, dutyCalc.DutyRateFlatRate);
					AssertEquals("DutyRateFlatUQ", "", dutyCalc.DutyRateFlatUQ);
					AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
					AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
					AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
					AssertEquals("PFMLFuelLevyAmount", 0.00m, dutyCalc.PFMLFuelLevyAmount);
					AssertEquals("SyntheticGreenhouseGasesLevyAmount", 0.00m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
					AssertEquals("DutyAmount", 7.50m, dutyCalc.DutyAmount);
					AssertEquals("DutyRateComplete", "123456789 @ AU = 5.00%", dutyCalc.DutyRateComplete);
				});
			}
		}

		public void TestCalculateDuty_ConcessionApplied()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
				var rateType = helper.CreateNewOrGetExistingRateType("NZ", "DTY");
				var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", rateType.PK);
				var preference1 = helper.CreatePreferenceForCountry("Q", "Qualify", "NZ");
				var tariff = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
				var rate1 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "(5.000000*(VFD/100))+(0.700240*[LMS])", preference1.PK);
				var rate2 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "10.000000*(VFD/100)", preference1.PK);
				var tradeGroup1 = helper.LoadOrCreateTradeGroup("NZ", "AU", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				var tradeGroup2 = helper.LoadOrCreateTradeGroup("NZ", "TPP", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				var tradeGroup3 = helper.LoadOrCreateTradeGroup("NZ", "STD", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				helper.AddCountry(tradeGroup1, "AU");
				helper.AddCountry(tradeGroup2, "AU");
				helper.CreateCusApplicability(rate1, tradeGroup1, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "", "100001");
				helper.CreateCusApplicability(rate1, tradeGroup2, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "", "100002");
				helper.CreateCusApplicability(rate2, tradeGroup3, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "", "100001");

				var testObjects = new DutyCalculatorTestObjects(Factory);
				testObjects.Declaration.JE_DateOfArrival = ZDateTime.Today;
				testObjects.InvoiceLine.JI_Tariff = "123456789";
				testObjects.InvoiceLine.JI_ConcessionCode = "100002";
				testObjects.InvoiceLine.JI_CountryOfOrigin = "AU";
				testObjects.InvoiceLine.JI_LinePrice = 150m;
				testObjects.InvoiceLine.JI_CustomsQuantity = 10m;
				testObjects.InvoiceLine.JI_CustomsUnitQty = "LMS";
				testObjects.InvoiceLine.JI_QualifiesForPreferentialDuty = "Q";
				testObjects.InvoiceLine.JI_PreferentialCountryGroup = "TPP";
				var dutyCalc = new DutyCalculator(testObjects.InvoiceLine);

				CombineAssertions(() =>
				{
					AssertEquals("ActualClassification", "123456789", dutyCalc.ActualClassification);
					AssertEquals("DutyRatePercent", 5.00m, dutyCalc.DutyRatePercent);
					AssertEquals("DutyRateFlatRate", 0.700240m, dutyCalc.DutyRateFlatRate);
					AssertEquals("DutyRateFlatUQ", "LMS", dutyCalc.DutyRateFlatUQ);
					AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
					AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
					AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
					AssertEquals("PFMLFuelLevyAmount", 0.00m, dutyCalc.PFMLFuelLevyAmount);
					AssertEquals("SyntheticGreenhouseGasesLevyAmount", 0.00m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
					AssertEquals("DutyAmount", 14.50m, dutyCalc.DutyAmount);
					AssertEquals("DutyRateComplete", "123456789-100002 @ TPP = 5.00% $0.7002/LMS", dutyCalc.DutyRateComplete);
				});

				testObjects.InvoiceLine.JI_ConcessionCode = "100001";
				testObjects.InvoiceLine.JI_PreferentialCountryGroup = "";
				dutyCalc.Update();
				CombineAssertions(() =>
				{
					AssertEquals("ActualClassification", "123456789", dutyCalc.ActualClassification);
					AssertEquals("DutyRatePercent", 10.00m, dutyCalc.DutyRatePercent);
					AssertEquals("DutyRateFlatRate", 0.00m, dutyCalc.DutyRateFlatRate);
					AssertEquals("DutyRateFlatUQ", "", dutyCalc.DutyRateFlatUQ);
					AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
					AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
					AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
					AssertEquals("PFMLFuelLevyAmount", 0.00m, dutyCalc.PFMLFuelLevyAmount);
					AssertEquals("SyntheticGreenhouseGasesLevyAmount", 0.00m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
					AssertEquals("DutyAmount", 15.00m, dutyCalc.DutyAmount);
					AssertEquals("DutyRateComplete", "123456789-100001 @ STD = 10.00%", dutyCalc.DutyRateComplete);
				});
			}
		}

		public void TestCalculateDuty_CU1IsReservedWordForCustomsQuantity()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
				var rateType = helper.CreateNewOrGetExistingRateType("NZ", "DTY");
				var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", rateType.PK);
				var preference = helper.CreatePreferenceForCountry("Q", "Qualify", "NZ");
				var tariff = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
				var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "(5*(CU1/100))", preference.PK);
				var tradeGroup = helper.LoadOrCreateTradeGroup("NZ", "AU", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "", "100001");

				var testObjects = new DutyCalculatorTestObjects(Factory)
				{
					Declaration = { JE_DateOfArrival = ZDateTime.Today }, InvoiceLine =
					{
						JI_Tariff = "123456789", JI_ConcessionCode = "100001", JI_CountryOfOrigin = "AU",
						JI_CustomsQuantity = 10m,
						JI_CustomsUnitQty = "LMS",
						JI_QualifiesForPreferentialDuty = "Q",
						JI_PreferentialCountryGroup = "AU"
					}
				};
				var dutyCalc = new DutyCalculator(testObjects.InvoiceLine);

				AssertEquals("DutyAmount", 0.50m, dutyCalc.DutyAmount);
			}
		}

		public void TestCalculateDuty_ConcessionIsFree()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
				var rateType = helper.CreateNewOrGetExistingRateType("NZ", "DTY");
				var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", rateType.PK);
				var preference1 = helper.CreatePreferenceForCountry("Q", "Qualify", "NZ");
				var tariff = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
				var rate1 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "0", preference1.PK);
				var rate2 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "(5.000000*(VFD/100))+(0.700240*[LMS])", preference1.PK);
				var tradeGroup1 = helper.LoadOrCreateTradeGroup("NZ", "AU", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				helper.AddCountry(tradeGroup1, "AU");
				helper.CreateCusApplicability(rate1, tradeGroup1, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "", "100001");
				helper.CreateCusApplicability(rate2, tradeGroup1, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

				var testObjects = new DutyCalculatorTestObjects(Factory);
				testObjects.Declaration.JE_DateOfArrival = ZDateTime.Today;
				testObjects.InvoiceLine.JI_Tariff = "123456789";
				testObjects.InvoiceLine.JI_ConcessionCode = "100001";
				testObjects.InvoiceLine.JI_CountryOfOrigin = "AU";
				testObjects.InvoiceLine.JI_LinePrice = 150m;
				testObjects.InvoiceLine.JI_CustomsQuantity = 10m;
				testObjects.InvoiceLine.JI_CustomsUnitQty = "LMS";
				testObjects.InvoiceLine.JI_QualifiesForPreferentialDuty = "Q";
				testObjects.InvoiceLine.JI_PreferentialCountryGroup = "AU";
				var dutyCalc = new DutyCalculator(testObjects.InvoiceLine);

				CombineAssertions(() =>
				{
					AssertEquals("ActualClassification", "123456789", dutyCalc.ActualClassification);
					AssertEquals("DutyRatePercent", 0.00m, dutyCalc.DutyRatePercent);
					AssertEquals("DutyRateFlatRate", 0.700240m, dutyCalc.DutyRateFlatRate);
					AssertEquals("DutyRateFlatUQ", "LMS", dutyCalc.DutyRateFlatUQ);
					AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
					AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
					AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
					AssertEquals("PFMLFuelLevyAmount", 0.00m, dutyCalc.PFMLFuelLevyAmount);
					AssertEquals("SyntheticGreenhouseGasesLevyAmount", 0.00m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
					AssertEquals("DutyAmount", 7.00m, dutyCalc.DutyAmount);
					AssertEquals("DutyRateComplete", "123456789-100001 @ AU = $0.7002/LMS", dutyCalc.DutyRateComplete);
				});

				rate2.ZZ2_RateFormula = "0.700240*[LMS]";
				testObjects.InvoiceLine.JI_CustomsQuantity = 15m;
				dutyCalc.Update();
				CombineAssertions(() =>
				{
					AssertEquals("ActualClassification", "123456789", dutyCalc.ActualClassification);
					AssertEquals("DutyRatePercent", 0.00m, dutyCalc.DutyRatePercent);
					AssertEquals("DutyRateFlatRate", 0.00m, dutyCalc.DutyRateFlatRate);
					AssertEquals("DutyRateFlatUQ", "", dutyCalc.DutyRateFlatUQ);
					AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
					AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
					AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
					AssertEquals("PFMLFuelLevyAmount", 0.00m, dutyCalc.PFMLFuelLevyAmount);
					AssertEquals("SyntheticGreenhouseGasesLevyAmount", 0.00m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
					AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
					AssertEquals("DutyRateComplete", "123456789-100001 @ AU = FREE", dutyCalc.DutyRateComplete);
				});
			}
		}

		public void TestCalculateLevy()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
				var rateType = helper.CreateNewOrGetExistingRateType("NZ", "LVY");
				var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "AL", rateType.PK);
				var rateCode2 = helper.LoadOrCreateNewCusRateCode(Factory, "SL", rateType.PK);
				var rateCode3 = helper.LoadOrCreateNewCusRateCode(Factory, "AC", rateType.PK);
				var rateCode4 = helper.LoadOrCreateNewCusRateCode(Factory, "PF", rateType.PK);
				var rateCode5 = helper.LoadOrCreateNewCusRateCode(Factory, "GG", rateType.PK);
				var tariff = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
				helper.CreateRate(tariff, rateCode1.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "0.700240*[LTR]");
				helper.CreateRate(tariff, rateCode2.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "1.700240*[LTR]");
				helper.CreateRate(tariff, rateCode3.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "2.700240*[LTR]");
				helper.CreateRate(tariff, rateCode4.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "3.700240*[LTR]");
				helper.CreateRate(tariff, rateCode5.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "4.700240*[LTR]");

				var testObjects = new DutyCalculatorTestObjects(Factory);
				testObjects.Declaration.JE_DateOfArrival = ZDateTime.Today;
				testObjects.InvoiceLine.JI_Tariff = "123456789";
				testObjects.InvoiceLine.JI_LinePrice = 150m;
				testObjects.InvoiceLine.JI_SupplementaryQty = 10m;
				testObjects.InvoiceLine.JI_SupplementaryUQ = "LTR";
				var dutyCalc = new DutyCalculator(testObjects.InvoiceLine);

				CombineAssertions(() =>
				{
					AssertEquals("ActualClassification", "123456789", dutyCalc.ActualClassification);
					AssertEquals("DutyRatePercent", 0.00m, dutyCalc.DutyRatePercent);
					AssertEquals("DutyRateFlatRate", 0.00m, dutyCalc.DutyRateFlatRate);
					AssertEquals("DutyRateFlatUQ", "", dutyCalc.DutyRateFlatUQ);
					AssertEquals("ACCFuelLevyAmount", 27.00m, dutyCalc.ACCFuelLevyAmount);
					AssertEquals("ALACLevyAmount", 7.00m, dutyCalc.ALACLevyAmount);
					AssertEquals("HERALevyAmount", 17.00m, dutyCalc.HERALevyAmount);
					AssertEquals("PFMLFuelLevyAmount", 37.00m, dutyCalc.PFMLFuelLevyAmount);
					AssertEquals("SyntheticGreenhouseGasesLevyAmount", 47.00m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
					AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
					AssertContains("DutyRateComplete", "HERA:$1.7002/LTR", dutyCalc.DutyRateComplete);
					AssertContains("DutyRateComplete", "SGG:$4.70024/LTR", dutyCalc.DutyRateComplete);
					AssertContains("DutyRateComplete", "ALAC:$0.7002/LTR", dutyCalc.DutyRateComplete);
					AssertContains("DutyRateComplete", "PFML:$3.70024/LTR", dutyCalc.DutyRateComplete);
					AssertContains("DutyRateComplete", "ACC:$2.7002/LTR", dutyCalc.DutyRateComplete);
				});

				testObjects.InvoiceLine.JI_IsZeroRatedLevies = "Y";
				dutyCalc.Update();

				CombineAssertions(() =>
				{
					AssertEquals("ActualClassification", "123456789", dutyCalc.ActualClassification);
					AssertEquals("DutyRatePercent", 0.00m, dutyCalc.DutyRatePercent);
					AssertEquals("DutyRateFlatRate", 0.00m, dutyCalc.DutyRateFlatRate);
					AssertEquals("DutyRateFlatUQ", "", dutyCalc.DutyRateFlatUQ);
					AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
					AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
					AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
					AssertEquals("PFMLFuelLevyAmount", 0.00m, dutyCalc.PFMLFuelLevyAmount);
					AssertEquals("SyntheticGreenhouseGasesLevyAmount", 0.00m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
					AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
					AssertEquals("DutyRateComplete", "123456789 @ STD = FREE", dutyCalc.DutyRateComplete);
				});
			}
		}

		[TestDate(2014, 5, 25)]
		public void TestDutySourceWhenPrefGroupCNIsUsed()
		{
			var classification = NZCClassification.GetClassForCompleteCode(Factory, "3824.90.87.00G", ZDateTime.Today);
			AssertNotNull("PreCondition:NZCClassification exists for 3824.90.87.00G", classification);

			var concessionFilter = new ZQuery(NZCConcessionSchema.U2_Code, "303322J");
			var concessions = Factory.Load<NZCConcession>(concessionFilter);
			var concession = concessions.FirstOrDefault(x => x.U2_DateActiveFrom <= ZDateTime.Today);
			if (concession == null)
			{
				concession = Factory.New<NZCConcession>();
				concession.U2_Code = "303322J";
				concession.U2_DateActiveFrom = ZDateTime.BrettsBirthday;
			}

			var concessionDutyRateForCN = concession.DutyRates.Cast<NZCConcessionDutyRate>().FirstOrDefault(x => x.U5_PreferentialCountryGroup == "CN");
			if (concessionDutyRateForCN == null)
			{
				concessionDutyRateForCN = concession.DutyRates.AddNew();
				concessionDutyRateForCN.U5_PreferentialCountryGroup = "CN";
			}
			concessionDutyRateForCN.U5_DutyRatePercent = 10m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "NZD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3824.90.87.00G";
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_ConcessionCode = "303322J";
			invoiceLine.JI_QualifiesForPreferentialDuty = "Q";
			invoiceLine.JI_PreferentialCountryGroup = "CN";

			AssertContains("3824.90.87.00G-303322J @ CN", invoiceLine.JI_DutyRateComplete);
		}

		public void TestZeroRatingFlagWorksOnUsingJobComInvoiceLines()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			testObjects.Declaration.JE_IsZeroRatedAll = YesNoList.Codes.Yes;

			DutyCalculator dutyCalc = new DutyCalculator(testObjects.InvoiceLine);

			AssertEquals("ActualClassification", "7007.21.02.01K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRatePercent", 0.00m, dutyCalc.DutyRatePercent);
		}

		public void TestZeroRatingFlagWorksOnDeclarationUsingCusEntryLines()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			testObjects.Declaration.JE_IsZeroRatedAll = YesNoList.Codes.Yes;

			DutyCalculator dutyCalc = new DutyCalculator(testObjects.EntryLine);

			AssertEquals("ActualClassification", "7007.21.02.01K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRatePercent", 0.00m, dutyCalc.DutyRatePercent);
		}

		public void TestPublicInterfaceUsingCusEntryLineWhereTariffAlreadyFilledInOnEntryLine()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			testObjects.EntryLine.CL_AdValoremTariff = "8421.99.09.19E";

			DutyCalculator dutyCalc = new DutyCalculator(testObjects.EntryLine);

			AssertEquals("ActualClassification", "8421.99.09.19E", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 200.00m, dutyCalc.DutyAmount);
		}

		public void TestPublicInterfaceUsingCusEntryLine()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);

			DutyCalculator dutyCalc = new DutyCalculator(testObjects.EntryLine);

			AssertEquals("ActualClassification", "7007.21.02.01K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 350.00m, dutyCalc.DutyAmount);
		}

		public void TestPublicInterfaceUsingJobComInvoiceLine()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);

			DutyCalculator dutyCalc = new DutyCalculator(testObjects.InvoiceLine);

			AssertEquals("ActualClassification", "7007.21.02.01K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 350.00m, dutyCalc.DutyAmount);
		}

		public void TestUpdateUsingCusEntryLine()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);

			DutyCalculator dutyCalc = new DutyCalculator(testObjects.EntryLine);

			AssertEquals("ActualClassification", "7007.21.02.01K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 350.00m, dutyCalc.DutyAmount);

			testObjects.EntryLine.CL_AdValoremTariff = "7007.21.03.01F";
			dutyCalc.Update();

			AssertEquals("ActualClassification", "7007.21.03.01F", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 140.00m, dutyCalc.DutyAmount);
		}

		public void TestUpdateUsingJobComInvoiceLine()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);

			DutyCalculator dutyCalc = new DutyCalculator(testObjects.InvoiceLine);

			AssertEquals("ActualClassification", "7007.21.02.01K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 350.00m, dutyCalc.DutyAmount);

			testObjects.InvoiceLine.JI_Tariff = "7007.21.03.01F";
			dutyCalc.Update();

			AssertEquals("ActualClassification", "7007.21.03.01F", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 140.00m, dutyCalc.DutyAmount);
		}

		public void TestDutyRateFlagsIncludingFlatRate()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);

			DutyCalculator dutyCalc = new DutyCalculator(testObjects.InvoiceLine);
			AssertEquals("DutyRateOnly", "17.50%", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "7007.21.02.01K @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "7007.21.02.01K @ NML = 17.50%", dutyCalc.DutyRateComplete);
			AssertEquals("DutyRatePercent", 17.50m, dutyCalc.DutyRatePercent);
			AssertEquals("DutyRateFlatRate", 0.00m, dutyCalc.DutyRateFlatRate);
			AssertEquals("DutyRateFlatUQ", "", dutyCalc.DutyRateFlatUQ);

			testObjects.InvoiceLine.JI_Tariff = "7007.21.03.01F";
			dutyCalc.Update();

			AssertEquals("DutyRateOnly", "7.00%", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "7007.21.03.01F @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "7007.21.03.01F @ NML = 7.00%", dutyCalc.DutyRateComplete);
			AssertEquals("DutyRatePercent", 7.00m, dutyCalc.DutyRatePercent);
			AssertEquals("DutyRateFlatRate", 0.00m, dutyCalc.DutyRateFlatRate);
			AssertEquals("DutyRateFlatUQ", "", dutyCalc.DutyRateFlatUQ);

			testObjects.InvoiceLine.JI_ConcessionCode = "100101M";
			dutyCalc.Update();
			AssertEquals("DutyRateOnly", DutyCalculator.FreeDutyRateDescription, dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "7007.21.03.01F-100101M @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "7007.21.03.01F-100101M @ NML = FREE", dutyCalc.DutyRateComplete);
			AssertEquals("DutyRatePercent", 0.00m, dutyCalc.DutyRatePercent);
			AssertEquals("DutyRateFlatRate", 0.00m, dutyCalc.DutyRateFlatRate);
			AssertEquals("DutyRateFlatUQ", "", dutyCalc.DutyRateFlatUQ);

			testObjects.InvoiceLine.JI_Tariff = "2208.60.19.09J";
			testObjects.InvoiceLine.JI_ConcessionCode = "";
			testObjects.InvoiceLine.JI_CustomsQuantity = 3;
			testObjects.EntryLine.RandomLine.JI_CustomsUnitQty = "LPA";
			dutyCalc.Update();
			AssertEquals("DutyRateOnly", "6.50% $40.0350/LPA ALAC:$0.3851/LPA", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2208.60.19.09J @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2208.60.19.09J @ NML = 6.50% $40.0350/LPA ALAC:$0.3851/LPA", dutyCalc.DutyRateComplete);
			AssertEquals("DutyRatePercent", 6.50m, dutyCalc.DutyRatePercent);
			AssertEquals("DutyRateFlatRate", 40.0350m, dutyCalc.DutyRateFlatRate);
			AssertEquals("DutyRateFlatUQ", "LPA", dutyCalc.DutyRateFlatUQ);
		}

		public void TestCountryOfOriginAndQualifiesForPreferentialDutyDefaultThroughFromInvoiceHeader()
		{
			NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var testObjects = new DutyCalculatorTestObjects(Factory);

			testObjects.InvoiceLine.JI_Tariff = "6204.63.19.00H";
			testObjects.InvoiceLine.JI_CountryOfOrigin = "";
			testObjects.InvoiceLine.JI_QualifiesForPreferentialDuty = "";

			testObjects.InvoiceHeader.JZ_RN_NKDefaultOrigin = "TH";
			testObjects.InvoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;

			testObjects.Declaration.JE_DateOfArrival = new ZDateTime(2005, 10, 12);

			DutyCalculator dutyCalc = new DutyCalculator(testObjects.InvoiceLine);

			AssertEquals("DutyRateComplete", "6204.63.19.00H @ TH = 17.00%", dutyCalc.DutyRateComplete);
		}

		public void TestApplyLevyRate_CS00213070()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "NZD";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			var tariff = Factory.LoadTop1<NZCClassification>(new ZQuery(NZCClassificationSchema.U0_Tariff, "2710.12.17.00G"));
			if (tariff == null)
			{
				tariff = Factory.New<NZCClassification>();
				tariff.U0_Tariff = "2710.12.17.00G";
				tariff.U0_DateActiveFrom = new ZDateTime(2012, 01, 01);
				tariff.U0_Description = "Research Octane No. (RON) less than 95 (regular grade) blended with ethyl alcohol and that can be used as a fuel for engines {per l ms}";
				tariff.U0_StatisticalUnit = "LMS";

				var levy = Factory.New<NZCClassificationLevyRate>();
				levy.L0_DateActiveFrom = tariff.U0_DateActiveFrom;
				levy.L0_LevyUnit = tariff.U0_StatisticalUnit;
				levy.L0_LevyRate = 0.099000m;
				levy.L0_U0_Classification = tariff.PK;
				levy.L0_LevyCode = "AU";

				var levy2 = Factory.New<NZCClassificationLevyRate>();
				levy2.L0_DateActiveFrom = tariff.U0_DateActiveFrom;
				levy2.L0_LevyUnit = tariff.U0_StatisticalUnit;
				levy2.L0_LevyRate = 0.000450m;
				levy2.L0_U0_Classification = tariff.PK;
				levy2.L0_LevyCode = "PS";
			}
			invoiceLine.JI_Tariff = tariff.U0_Tariff;
			invoiceLine.JI_CustomsUnitQty = tariff.U0_StatisticalUnit;
			invoiceLine.JI_CustomsQuantity = 100m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("2710.12.17.00G @ NML = ACC:$0.0990/LMS PFML:$0.00045/LMS", invoiceLine.JI_DutyRateComplete);
		}
		public void TestDutyRatePercentTextFormatting()
		{
			var dutyRate = GetNMLDutyRate();
			dutyRate.U1_DutyRatePercent = 123456.8767861m;
			dutyRate.U1_DutyRatePerUnit1 = 0m;
			dutyRate.U1_DutyRatePerUnit2 = 0m;
			Factory.Save();

			var dutyCalc = GetDuty("0101.99.99.99#", "", qualifiesForPreferentialDuty: true, "TH", "TH", 0m, "LTR", 0m, "LTR", 1000m, ZDateTime.Today, "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete displays to 6 places", "0101.99.99.99# @ NML = 123456.876786%", dutyCalc.DutyRateComplete);

			var newFactory = new BusinessObjectFactory();
			var dutyRateInOtherFactory = newFactory.Load<NZCClassificationDutyRate>(dutyRate.PK);
			AssertEquals("No loss of information in DB round trip.", 123456.876786m, dutyRateInOtherFactory.U1_DutyRatePercent);

			dutyRate.U1_DutyRatePercent = 123456m;
			var dutyCalc2 = GetDuty("0101.99.99.99#", "", qualifiesForPreferentialDuty: true, "TH", "TH", 0m, "", 0m, "", 1000m, ZDateTime.Today, "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete displays to 2 places", "0101.99.99.99# @ NML = 123456.00%", dutyCalc2.DutyRateComplete);
		}

		public void TestDutyRatePerUnit1TextFormatting()
		{
			var dutyRate = GetNMLDutyRate();
			dutyRate.U1_DutyRatePercent = 0m;
			dutyRate.U1_DutyRatePerUnit1 = 123456.8767861m;
			dutyRate.U1_DutyRatePerUnit2 = 0m;
			Factory.Save();

			var dutyCalc = GetDuty("0101.99.99.99#", "", qualifiesForPreferentialDuty: true, "TH", "TH", 0m, "LTR", 0m, "LTR", 1000m, ZDateTime.Today, "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete displays to 6 places", "0101.99.99.99# @ NML = $123456.876786/LTR", dutyCalc.DutyRateComplete);

			var newFactory = new BusinessObjectFactory();
			var dutyRateInOtherFactory = newFactory.Load<NZCClassificationDutyRate>(dutyRate.PK);
			AssertEquals("No loss of information in DB round trip.", 123456.876786m, dutyRateInOtherFactory.U1_DutyRatePerUnit1);

			dutyRate.U1_DutyRatePerUnit1 = 123456m;
			var dutyCalc2 = GetDuty("0101.99.99.99#", "", qualifiesForPreferentialDuty: true, "TH", "TH", 0m, "", 0m, "", 1000m, ZDateTime.Today, "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete displays to 4 places", "0101.99.99.99# @ NML = $123456.0000/", dutyCalc2.DutyRateComplete);
		}

		public void TestDutyRatePerUnit2TextFormatting()
		{
			var dutyRate = GetNMLDutyRate();
			dutyRate.U1_DutyRatePercent = 0m;
			dutyRate.U1_DutyRatePerUnit1 = 0m;
			dutyRate.U1_DutyRatePerUnit2 = 123456.8767861m;
			Factory.Save();

			var dutyCalc = GetDuty("0101.99.99.99#", "", qualifiesForPreferentialDuty: true, "TH", "TH", 0m, "LTR", 0m, "LTR", 1000m, ZDateTime.Today, "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete displays to 6 places", "0101.99.99.99# @ NML = $123456.876786/LTR", dutyCalc.DutyRateComplete);

			var newFactory = new BusinessObjectFactory();
			var dutyRateInOtherFactory = newFactory.Load<NZCClassificationDutyRate>(dutyRate.PK);
			AssertEquals("No loss of information in DB round trip.", 123456.876786m, dutyRateInOtherFactory.U1_DutyRatePerUnit2);

			dutyRate.U1_DutyRatePerUnit2 = 123456m;
			var dutyCalc2 = GetDuty("0101.99.99.99#", "", qualifiesForPreferentialDuty: true, "TH", "TH", 0m, "", 0m, "", 1000m, ZDateTime.Today, "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete displays to 4 places", "0101.99.99.99# @ NML = $123456.0000/", dutyCalc2.DutyRateComplete);
		}

		public void TestFuturePreferentialRatesDoNotGetFactoredIn()
		{
			var classification = Factory.New<NZCClassification>();
			classification.U0_Tariff = "0101.99.99.99#";
			classification.U0_DateActiveFrom = new ZDateTime(1900, 1, 1);
			classification.U0_DateActiveTo = new ZDateTime(2009, 12, 12);
			classification.U0_Description = "Fred";
			classification.U0_ComputerDumpDescr = "Fred";

			var dutyRateFuturePreferential = Factory.New<NZCClassificationDutyRate>();
			dutyRateFuturePreferential.U1_U0_Classification = classification.PK;
			dutyRateFuturePreferential.U1_PreferentialCountryGroup = "TH";
			dutyRateFuturePreferential.U1_DateActiveFrom = new ZDateTime(2009, 11, 12);
			dutyRateFuturePreferential.U1_DateActiveTo = new ZDateTime(2009, 12, 12);
			dutyRateFuturePreferential.U1_DutyRatePercent = 5.45m;

			var dutyRateNormal = Factory.New<NZCClassificationDutyRate>();
			dutyRateNormal.U1_U0_Classification = classification.PK;
			dutyRateNormal.U1_PreferentialCountryGroup = "NML";
			dutyRateNormal.U1_DateActiveTo = new ZDateTime(2009, 12, 12);
			dutyRateNormal.U1_DutyRatePercent = 15.8m;

			var dutyCalc = GetDuty("0101.99.99.99#", "", qualifiesForPreferentialDuty: true, "TH", "TH", 0m, "", 0m, "", 1000m, new ZDateTime(2008, 1, 1), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete", "0101.99.99.99# @ NML = 15.80%", dutyCalc.DutyRateComplete);

			var dutyCalc2 = GetDuty("0101.99.99.99#", "", qualifiesForPreferentialDuty: true, "TH", "TH", 0m, "", 0m, "", 1000m, new ZDateTime(2009, 11, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete", "0101.99.99.99# @ TH = 5.45%", dutyCalc2.DutyRateComplete);
		}

		public void TestPFMLLevyWorksWithACC()
		{
			var dutyCalc = GetDuty("2710.19.21.10C", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 20000m, "LTR", 0m, "", 20000m, new ZDateTime(2008, 10, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2710.19.21.10C", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 1868.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("PFMLFuelLevyAmount", 9.00m, dutyCalc.PFMLFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 8504.80m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "$0.42524/LTR ACC:$0.0934/LTR PFML:$0.00045/LTR", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2710.19.21.10C @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2710.19.21.10C @ NML = $0.42524/LTR ACC:$0.0934/LTR PFML:$0.00045/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestTHRatesOn_9401_61_00_01J()
		{
			var dutyCalc = GetDuty("9401.61.00.01J", "", qualifiesForPreferentialDuty: true, "TH", "TH", 0m, "", 0m, "", 2000m, new ZDateTime(2006, 1, 19), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete", "9401.61.00.01J @ TH = 4.00%", dutyCalc.DutyRateComplete);
		}

		public void TestFutureRatesAreIncludedInTariffTill2006()
		{
			var dutyCalc = GetDuty("8421.99.09.19E", "", qualifiesForPreferentialDuty: true, "TH", "TH", 0m, "", 0m, "", 2000m, new ZDateTime(2006, 1, 1), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete", "8421.99.09.19E @ TH = 7.00%", dutyCalc.DutyRateComplete);
		}

		public void TestFutureRatesAreIncludedInTariffTill2007TH()
		{
			var dutyCalc = GetDuty("8421.99.09.19E", "", qualifiesForPreferentialDuty: true, "TH", "TH", 0m, "", 0m, "", 2000m, new ZDateTime(2007, 1, 1), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete", "8421.99.09.19E @ TH = 6.00%", dutyCalc.DutyRateComplete);
		}

		public void TestNonGroupCountrySpecificRatesTakePrecedence1()
		{
			NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var dutyCalc = GetDuty("6204.63.19.00H", "", qualifiesForPreferentialDuty: true, "TH", ZString.Empty, 1000m, "LTR", 0m, "", 2000m, new ZDateTime(2005, 10, 17), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateOnly", "17.00%", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateComplete", "6204.63.19.00H @ TH = 17.00%", dutyCalc.DutyRateComplete);
		}

		public void TestNonGroupCountrySpecificRatesTakePrecedence2()
		{
			NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var dutyCalc = GetDuty("6205.20.12.00C", "", qualifiesForPreferentialDuty: true, "TH", ZString.Empty, 1000m, "LTR", 0m, "", 2000m, new ZDateTime(2005, 10, 17), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateOnly", "17.00%", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateComplete", "6205.20.12.00C @ TH = 17.00%", dutyCalc.DutyRateComplete);
		}

		[TestDate(2009, 09, 12)]
		public void TestCacheReturnsStraightAway()
		{
			var dutyCalc = GetDuty("2711.13.00.09C", "", qualifiesForPreferentialDuty: true, "AU", ZString.Empty, 1000m, "LTR", 0m, "", 2000m, new ZDateTime(2009, 09, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateOnly", "$0.1040/LTR", dutyCalc.DutyRateOnly);
			var dutyCalc2 = GetDuty("2711.13.00.09C", "", qualifiesForPreferentialDuty: true, "AU", ZString.Empty, 1000m, "LTR", 0m, "", 2000m, new ZDateTime(2009, 09, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateOnly", "$0.1040/LTR", dutyCalc2.DutyRateOnly);
		}

		[TestDate(2009, 09, 12)]
		public void TestRegistryOptionWhenPrefRateExpired()
		{
			NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var dutyCalc = GetDuty("2711.13.00.09C", "", qualifiesForPreferentialDuty: true, "AU", ZString.Empty, 1000m, "LTR", 0m, "", 2000m, new ZDateTime(2009, 09, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("The system should be able to detect that the preference is no longer current.", "$0.1040/LTR", dutyCalc.DutyRateOnly);
		}

		public void TestTranslateKilosToTonnesForRequiredStatQty()
		{
			var dutyCalc = GetDuty("7208.40.10.01F", "", qualifiesForPreferentialDuty: false, "AU", ZString.Empty, 10000m, StatisticalUQList.Codes.Kilograms, 0m, "", 2000m, new ZDateTime(2004, 12, 25), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("HERALevyAmount", 50.00m, dutyCalc.HERALevyAmount);
		}

		public void TestCountrySpecificConcessionalDutyRate()
		{
			SetupForConcessionalDutyRateTesting();
			var dutyCalc = GetDuty("2711.13.00.09C", TestConcessionCode, qualifiesForPreferentialDuty: true, "CA", ZString.Empty, 0m, "", 0m, "", 1000m, new ZDateTime(2004, 01, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateOnly", "6.50%", dutyCalc.DutyRateOnly);
		}

		public void TestNonCountrySpecificConcessionalDutyRate()
		{
			SetupForConcessionalDutyRateTesting();
			var dutyCalc = GetDuty("2711.13.00.09C", TestConcessionCode, qualifiesForPreferentialDuty: false, "CA", ZString.Empty, 0m, "", 0m, "", 1000m, new ZDateTime(2004, 01, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateOnly", "19.00%", dutyCalc.DutyRateOnly);
		}

		public void TestAUGetsNormalDutyRateWhenNotOtherwiseIndicatedAndPrefIndicatorOFF()
		{
			var dutyCalc = GetDuty("2711.13.00.09C", "", qualifiesForPreferentialDuty: false, "AU", ZString.Empty, 1000m, "LTR", 0m, "", 2000m, new ZDateTime(2004, 01, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateOnly", "$0.1040/LTR", dutyCalc.DutyRateOnly);
		}

		public void TestDutyRateGetsCorrectHistoricalValues()
		{
			var dutyCalc = GetDuty("2204.10.01.00H", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 1000m, "LTR", 0m, "", 2000m, new ZDateTime(2004, 01, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateOnly", "$2.1647/LTR ALAC:$0.0319/LTR", dutyCalc.DutyRateOnly);
		}

		public void TestStraightDutyAndGST()
		{
			var dutyCalc = GetDuty("7007.21.02.01K", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 10000m, "NMB", 0m, "", 2000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "7007.21.02.01K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 350.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "17.50%", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "7007.21.02.01K @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "7007.21.02.01K @ NML = 17.50%", dutyCalc.DutyRateComplete);
		}

		public void TestConcessionDropsDuty()
		{
			var dutyCalc = GetDuty("7007.21.02.01K", "DUMMYCONC", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 10000m, "NMB", 0m, "", 2000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "7007.21.02.01K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", DutyCalculator.FreeDutyRateDescription, dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "7007.21.02.01K-DUMMYCONC @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "7007.21.02.01K-DUMMYCONC @ NML = FREE", dutyCalc.DutyRateComplete);
		}

		public void TestDutyAndGSTWithALAC()
		{
			var dutyCalc = GetDuty("2203.00.39.02K", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 100m, "LPA", 2000m, "LTR", 20000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2203.00.39.02K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 23.40m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 2198.20m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "$21.9820/LPA ALAC:$0.0117/LTR", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2203.00.39.02K @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2203.00.39.02K @ NML = $21.9820/LPA ALAC:$0.0117/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestDutyAndGSTWithSGG()
		{
			var tariff = Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing.NZCClassificationTest.CreateSGGTariff(Factory);
			var dutyCalc = GetDuty(tariff.U0_Tariff, "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 100m, "NMB", 0m, "", 20000m, new ZDateTime(2012, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", tariff.U0_Tariff, dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("SGGLevyAmount", 855m, dutyCalc.SyntheticGreenhouseGasesLevyAmount);
			AssertEquals("DutyRateOnly", "SGG:$8.55000/NMB", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "0000.00.00.00E @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "0000.00.00.00E @ NML = SGG:$8.55000/NMB", dutyCalc.DutyRateComplete);
		}

		public void TestACCFuelLevy()
		{
			var dutyCalc = GetDuty("2710.19.11.11F", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 20000m, "LTR", 0m, "", 20000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2710.19.11.11F", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 1016.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 7240.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "$0.3620/LTR $0.0800/GPB ACC:$0.0508/LTR", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2710.19.11.11F @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2710.19.11.11F @ NML = $0.3620/LTR $0.0800/GPB ACC:$0.0508/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestACCFuelLevy2()
		{
			var dutyCalc = GetDuty("2710.19.11.11F", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 20000m, "LTR", 0m, "", 20000m, new ZDateTime(2006, 3, 20), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("DutyRateComplete", "2710.19.11.11F @ NML = $0.4120/LTR $0.0800/GPB ACC:$0.0578/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestACCFuelLevyZeroRated()
		{
			var dutyCalc = GetDuty("2710.19.11.11F", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 20000m, "LTR", 0m, "", 20000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: true, isZeroRatedExcise: true, isZeroRatedLevies: true);
			AssertEquals("ActualClassification", "2710.19.11.11F", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "FREE", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2710.19.11.11F @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2710.19.11.11F @ NML = FREE", dutyCalc.DutyRateComplete);
		}

		public void TestHERALevyAt5DollarsPerTonne()
		{
			var dutyCalc = GetDuty("7210.70.01.01K", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 20000m, StatisticalUQList.Codes.Kilograms, 0m, "", 20000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "7210.70.01.01K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 100.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "HERA:$5.0000/TNE", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "7210.70.01.01K @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "7210.70.01.01K @ NML = HERA:$5.0000/TNE", dutyCalc.DutyRateComplete);

			var dutyCalc2 = GetDuty("7210.70.01.01K", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 20000m, StatisticalUQList.Codes.Kilograms, 0m, "", 20000m, new ZDateTime(2005, 7, 7), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("HERALevyAmount", 100.00m, dutyCalc2.HERALevyAmount);
			AssertEquals("DutyRateOnly", "HERA:$5.0000/TNE", dutyCalc2.DutyRateOnly);
		}

		public void TestHERALevyAt5CentsPerKilo()
		{
			var dutyCalc = GetDuty("7601.20.01.01H", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 2000m, StatisticalUQList.Codes.Kilograms, 0m, "", 20000m, new ZDateTime(2005, 7, 7), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "7601.20.01.01H", dutyCalc.ActualClassification);
			AssertEquals("HERALevyAmount", 100.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyRateOnly", "HERA:$0.0500/KGM", dutyCalc.DutyRateOnly);
		}

		public void TestHERALevyAt2CentsPerKilo()
		{
			var dutyCalc = GetDuty("7601.20.01.01H", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 5000m, StatisticalUQList.Codes.Kilograms, 0m, "", 20000m, new ZDateTime(2005, 1, 1), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "7601.20.01.01H", dutyCalc.ActualClassification);
			AssertEquals("HERALevyAmount", 100.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyRateOnly", "HERA:$0.0200/KGM", dutyCalc.DutyRateOnly);
		}

		public void TestAdValorem()
		{
			var dutyCalc = GetDuty("6207.11.01.00J", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 10000m, "NMB", 0m, "", 20000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "6207.11.01.00J", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 3800.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "19.00%", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "6207.11.01.00J @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "6207.11.01.00J @ NML = 19.00%", dutyCalc.DutyRateComplete);

			var dutyCalc2 = GetDuty("6207.11.01.00J", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 10000m, "NMB", 0m, "", 2000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "6207.11.09.00K", dutyCalc2.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc2.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc2.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc2.HERALevyAmount);
			AssertEquals("DutyAmount", 2500.00m, dutyCalc2.DutyAmount);
			AssertEquals("DutyRateOnly", "$0.2500/NMB", dutyCalc2.DutyRateOnly);
			AssertEquals("DutyRateSource", "6207.11.09.00K @ NML", dutyCalc2.DutyRateSource);
			AssertEquals("DutyRateComplete", "6207.11.09.00K @ NML = $0.2500/NMB", dutyCalc2.DutyRateComplete);
		}

		public void TestConcessionDutyRate()
		{
			SetupForConcessionalDutyRateTesting();
			var dutyCalc = GetDuty("3926.20.69.02B", TestConcessionCode, qualifiesForPreferentialDuty: false, "US", ZString.Empty, 30000m, "NMB", 0m, "", 2000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "3926.20.69.02B", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 380.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "19.00%", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "3926.20.69.02B-" + TestConcessionCode + " @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "3926.20.69.02B-" + TestConcessionCode + " @ NML = 19.00%", dutyCalc.DutyRateComplete);
		}

		public void TestConcessionDutyRate_WhenConcessionIsEmpty()
		{
			var dutyCalc = GetDuty("2204.21.18.09K", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 30000m, "NMB", 0m, "", 2000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2204.21.18.09K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 66086.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "7.00% $2.1982/NMB ALAC:$0.0432/LTR", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2204.21.18.09K @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2204.21.18.09K @ NML = 7.00% $2.1982/NMB ALAC:$0.0432/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestConcessionDutyRate_WhenConcessionIsFree_AndClassificationHasDutyPercent()
		{
			var dutyCalc = GetDuty("2204.21.18.09K", "989924D", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 30000m, "NMB", 0m, "", 2000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2204.21.18.09K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 65946.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "$2.1982/NMB ALAC:$0.0432/LTR", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2204.21.18.09K-989924D @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2204.21.18.09K-989924D @ NML = $2.1982/NMB ALAC:$0.0432/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestConcessionDutyRate_WhenConcessionIsFree_AndClassificationHasNoDutyPercent()
		{
			var dutyCalc = GetDuty("2207.10.29.00D", "989924D", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 30000m, "NMB", 0m, "", 2000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2207.10.29.00D", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "FREE", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2207.10.29.00D-989924D @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2207.10.29.00D-989924D @ NML = FREE", dutyCalc.DutyRateComplete);
		}

		public void TestConcessionDutyRate_WhenConcessionIsNotFree()
		{
			var dutyCalc = GetDuty("3926.20.69.02B", "300382F", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 30000m, "NMB", 0m, "", 2000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "3926.20.69.02B", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 200.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "10.00%", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "3926.20.69.02B-300382F @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "3926.20.69.02B-300382F @ NML = 10.00%", dutyCalc.DutyRateComplete);
		}

		public void TestPartsOfClassification()
		{
			var dutyCalc = GetDuty("8431.49.19.00J", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 10000m, "NMB", 0m, "", 2000m, new ZDateTime(2004, 12, 12), "8426.99.00.09A", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "8431.49.19.00J", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 140.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "7.00%", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "8431.49.19.00J @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "8431.49.19.00J @ NML = 7.00%", dutyCalc.DutyRateComplete);
		}

		public void TestSpirits()
		{
			var dutyCalc = GetDuty("2206.00.78.09G", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 400m, "LPA", 2000m, "LTR", 40000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2206.00.78.09G", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 141.20m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 16014.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "$40.0350/LPA ALAC:$0.0706/LTR", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2206.00.78.09G @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2206.00.78.09G @ NML = $40.0350/LPA ALAC:$0.0706/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestBeer()
		{
			var dutyCalc = GetDuty("2203.00.31.08H", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 100m, "LPA", 2000m, "LTR", 4000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2203.00.31.08H", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 23.40m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 2198.20m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "$21.9820/LPA ALAC:$0.0117/LTR", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2203.00.31.08H @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2203.00.31.08H @ NML = $21.9820/LPA ALAC:$0.0117/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestWine()
		{
			var dutyCalc = GetDuty("2204.21.18.19G", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 2000m, "LTR", 0m, "", 4000m, new ZDateTime(2005, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2204.21.18.19G", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 98.60m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 4798.40m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "7.00% $2.2592/LTR ALAC:$0.0493/LTR", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2204.21.18.19G @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2204.21.18.19G @ NML = 7.00% $2.2592/LTR ALAC:$0.0493/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestWineZeroRatedDuty()
		{
			var dutyCalc = GetDuty("2204.21.18.19G", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 2000m, "LTR", 0m, "", 4000m, new ZDateTime(2005, 12, 12), "", isZeroRatedDuty: true, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2204.21.18.19G", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 98.60m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 4518.40m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "$2.2592/LTR ALAC:$0.0493/LTR", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2204.21.18.19G @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2204.21.18.19G @ NML = $2.2592/LTR ALAC:$0.0493/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestWineZeroRatedDutyAndExcise()
		{
			var dutyCalc = GetDuty("2204.21.18.19G", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 2000m, "LTR", 0m, "", 4000m, new ZDateTime(2005, 12, 12), "", isZeroRatedDuty: true, isZeroRatedExcise: true, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2204.21.18.19G", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 98.60m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "ALAC:$0.0493/LTR", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2204.21.18.19G @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2204.21.18.19G @ NML = ALAC:$0.0493/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestWineZeroRatedDutyExciseAndALAC()
		{
			var dutyCalc = GetDuty("2204.21.18.19G", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 2000m, "LTR", 0m, "", 4000m, new ZDateTime(2005, 12, 12), "", isZeroRatedDuty: true, isZeroRatedExcise: true, isZeroRatedLevies: true);
			AssertEquals("ActualClassification", "2204.21.18.19G", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "FREE", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2204.21.18.19G @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2204.21.18.19G @ NML = FREE", dutyCalc.DutyRateComplete);
		}

		public void TestZeroRated()
		{
			var dutyCalc = GetDuty("7007.21.02.01K", "", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 10000m, "NMB", 0m, "", 2000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: true, isZeroRatedExcise: true, isZeroRatedLevies: true);
			AssertEquals("ActualClassification", "7007.21.02.01K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", DutyCalculator.FreeDutyRateDescription, dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "7007.21.02.01K @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "7007.21.02.01K @ NML = FREE", dutyCalc.DutyRateComplete);
		}

		public void TestDutyAndGSTWithALACDoesntDropALACWithConcessionCode()
		{
			var dutyCalc = GetDuty("2203.00.39.02K", "801351K", qualifiesForPreferentialDuty: false, "US", ZString.Empty, 100m, "LPA", 2000m, "LTR", 20000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ActualClassification", "2203.00.39.02K", dutyCalc.ActualClassification);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 23.40m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 0.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "ALAC:$0.0117/LTR", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "2203.00.39.02K-801351K @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "2203.00.39.02K-801351K @ NML = ALAC:$0.0117/LTR", dutyCalc.DutyRateComplete);
		}

		public void TestHERALevyDoesntDropHERAWithConcessionCode()
		{
			var dutyCalc = GetDuty("7208.52.90.00D", "801351K", qualifiesForPreferentialDuty: true, "ID", ZString.Empty, 20000m, StatisticalUQList.Codes.Kilograms, 0m, "", 20000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 100.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 0.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "HERA:$5.0000/TNE", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "7208.52.90.00D-801351K @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "7208.52.90.00D-801351K @ NML = HERA:$5.0000/TNE", dutyCalc.DutyRateComplete);
		}

		public void TestHERALevyWorksWithDutyRateAndHERA()
		{
			var dutyCalc = GetDuty("7208.52.90.00D", "", qualifiesForPreferentialDuty: true, "ID", ZString.Empty, 20000m, StatisticalUQList.Codes.Kilograms, 0m, "", 20000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals("ACCFuelLevyAmount", 0.00m, dutyCalc.ACCFuelLevyAmount);
			AssertEquals("ALACLevyAmount", 0.00m, dutyCalc.ALACLevyAmount);
			AssertEquals("HERALevyAmount", 100.00m, dutyCalc.HERALevyAmount);
			AssertEquals("DutyAmount", 1000.00m, dutyCalc.DutyAmount);
			AssertEquals("DutyRateOnly", "5.00% HERA:$5.0000/TNE", dutyCalc.DutyRateOnly);
			AssertEquals("DutyRateSource", "7208.52.90.00D @ NML", dutyCalc.DutyRateSource);
			AssertEquals("DutyRateComplete", "7208.52.90.00D @ NML = 5.00% HERA:$5.0000/TNE", dutyCalc.DutyRateComplete);
		}

		public void TestDutyCalculationRoundsStatQtyAndSuppQtyTo3Decimals()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			var dutyRateParameters = new DutyCalculator.CusEntryLineWrapper(testObjects.EntryLine);

			testObjects.InvoiceLine.JI_CustomsQuantity = 1.5m;
			testObjects.InvoiceLine.JI_SupplementaryQty = 2.75m;
			AssertEquals("StatQty = 1.5", 1.5m, dutyRateParameters.StatQty);
			AssertEquals("SuppQty = 2.75", 2.75m, dutyRateParameters.SuppQty);

			testObjects.InvoiceLine.JI_CustomsQuantity = 1.1234m;
			testObjects.InvoiceLine.JI_SupplementaryQty = 2.4321m;
			AssertEquals("StatQty = 1.1234 -> 1.123", 1.123m, dutyRateParameters.StatQty);
			AssertEquals("SuppQty = 2.4321 -> 2.432", 2.432m, dutyRateParameters.SuppQty);

			testObjects.InvoiceLine.JI_CustomsQuantity = 1.6789m;
			testObjects.InvoiceLine.JI_SupplementaryQty = 2.9876m;
			AssertEquals("StatQty = 1.6789 -> 1.679", 1.679m, dutyRateParameters.StatQty);
			AssertEquals("SuppQty = 2.9876 -> 2.988", 2.988m, dutyRateParameters.SuppQty);
		}

		#region Implementation

		#region SetupForConcessionalDutyRateTesting
		const string TestConcessionCode = "000000L";
		void SetupForConcessionalDutyRateTesting()
		{
			NZCConcession concession = Factory.New<NZCConcession>();
			concession.U2_Code = TestConcessionCode;
			concession.U2_DateActiveFrom = new ZDateTime(2000, 1, 1);
			concession.U2_Description = "TEST CONCESSION CODE";
			NZCConcessionDutyRate concessionDutyRate1 = concession.DutyRates.AddNew();
			concessionDutyRate1.U5_DutyRatePercent = 6.50m;
			concessionDutyRate1.U5_PreferentialCountryGroup = "CA";
			NZCConcessionDutyRate concessionDutyRate2 = concession.DutyRates.AddNew();
			concessionDutyRate2.U5_DutyRatePercent = 19.00m;
			concessionDutyRate2.U5_PreferentialCountryGroup = "NML";
		}
		#endregion

		NZCClassificationDutyRate GetNMLDutyRate()
		{
			var classification = Factory.New<NZCClassification>();
			classification.U0_Tariff = "0101.99.99.99#";
			classification.U0_DateActiveFrom = ZDateTime.MinSmallDateTimeValue;
			classification.U0_DateActiveTo = ZDateTime.MaxSmallDateTimeValue;
			classification.U0_Description = "Dummy Data";
			classification.U0_ComputerDumpDescr = "Dummy Data";

			var dutyRate = Factory.New<NZCClassificationDutyRate>();
			dutyRate.U1_U0_Classification = classification.PK;
			dutyRate.U1_PreferentialCountryGroup = "NML";
			dutyRate.U1_DateActiveTo = classification.U0_DateActiveTo;

			return dutyRate;
		}

		DutyCalculator GetDuty(
	   ZString classificationCode,
	   ZString concessionCode,
	   ZBool qualifiesForPreferentialDuty,
	   ZString countryOfOrigin,
	   ZString preferentialCountryGroup,
	   ZDecimal statQty,
	   ZString statUQ,
	   ZDecimal suppQty,
	   ZString suppUQ,
	   ZDecimal valueForDuty,
	   ZDateTime dateForDutyRate,
	   ZString partsOfClassification,
	   ZBool isZeroRatedDuty,
	   ZBool isZeroRatedExcise,
	   ZBool isZeroRatedLevies)
		{
			return new DutyCalculator(new DutyParameters_ForTesting(Factory)
			{
				ClassificationCode = classificationCode,
				ConcessionCode = concessionCode,
				QualifiesForPreferentialDuty = qualifiesForPreferentialDuty,
				CountryOfOrigin = countryOfOrigin,
				PreferentialCountryGroup = preferentialCountryGroup,
				StatQty = statQty,
				StatUQ = statUQ,
				SuppQty = suppQty,
				SuppUQ = suppUQ,
				ValueForDuty = valueForDuty,
				DateForDutyRate = dateForDutyRate,
				PartsOfClassification = partsOfClassification,
				IsZeroRatedDuty = isZeroRatedDuty,
				IsZeroRatedExcise = isZeroRatedExcise,
				IsZeroRatedLevies = isZeroRatedLevies,
			});
		}

		class DutyParameters_ForTesting : IHaveTheParametersRequiredToCalculateDuty
		{
			public DutyParameters_ForTesting(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public ZString ClassificationCode { get; set; }

			public ZString ConcessionCode { get; set; }

			public ZBool QualifiesForPreferentialDuty { get; set; }

			public ZString CountryOfOrigin { get; set; }

			public ZString PreferentialCountryGroup { get; set; }

			public ZDecimal StatQty { get; set; }

			public ZString StatUQ { get; set; }

			public ZDecimal SuppQty { get; set; }

			public ZString SuppUQ { get; set; }

			public ZDecimal ValueForDuty { get; set; }

			public ZDateTime DateForDutyRate { get; set; }

			public ZString PartsOfClassification { get; set; }

			public ZBool IsZeroRatedDuty { get; set; }

			public ZBool IsZeroRatedExcise { get; set; }

			public ZBool IsZeroRatedLevies { get; set; }

			public JobComInvoiceLine InvoiceLine { get; set; }

			public BusinessObjectFactory Factory => factory;
			readonly BusinessObjectFactory factory;
		}
		#endregion
	}
}
