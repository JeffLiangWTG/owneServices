using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ZAWhsInventoryDutyAndTaxCalculatorTestHelper : IZAWhsInventoryDutyAndTaxCalculatorTestHelper
	{
		public ZDateTime StartDate
		{
			get
			{
				if (!startDate.HasValue)
				{
					startDate = ZDateTime.Today.AddYears(-1);
				}
				return startDate.Value;
			}
		}
		ZDateTime? startDate;

		public ZDateTime EndDate
		{
			get
			{
				if (!endDate.HasValue)
				{
					endDate = StartDate.AddMonths(2);
				}
				return endDate.Value;
			}
		}
		ZDateTime? endDate;

		public ZString TariffCode => "1111111";

		public void SetupTestData(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var tariffType12B = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			var tariffType15A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "15A");
			var tariffType15B = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "15B");
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_DTY.PK);
			var rateType_ZA_LVY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Levy);
			var rateCode_ZA_LVY_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_LVY.PK);
			var rateType_ZA_EXC = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Excise);
			var rateCode_ZA_EXC_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_EXC.PK);
			var rateType_ZA_EX1 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.AdValoremExcise);
			rateType_ZA_EX1.ZZR_IsPayable = true;
			rateType_ZA_EX1.ZZR_CustomsValueFormula = "CV";
			var rateCode_ZA_EX1_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_EX1.PK);

			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddYears(1), "VAT Normal");
			factory.Save();

			var tariff1P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, TariffCode, StartDate, EndDate, taxOrFeeCode: "VAT");
			helper.CreateTariffUOM(tariff1P1, "CU1", "LI");
			helper.CreateTariffUOM(tariff1P1, "CU2", "KG");
			helper.CreateTariffUOM(tariff1P1, "RU1", "NO");
			var tariff12A = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "1111112", StartDate, EndDate, taxOrFeeCode: "VAT");
			helper.CreateTariffUOM(tariff12A, "CU1", "LI");
			helper.CreateTariffUOM(tariff12A, "CU2", "KG");
			helper.CreateTariffUOM(tariff12A, "RU1", "NO");
			var tariff12B = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12B.PK, "1111113", StartDate, EndDate, taxOrFeeCode: "VAT");
			helper.CreateTariffUOM(tariff12B, "CU1", "LI");
			helper.CreateTariffUOM(tariff12B, "CU2", "KG");
			helper.CreateTariffUOM(tariff12B, "RU1", "NO");
			var tariff15A = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType15A.PK, "1111151", StartDate, EndDate, taxOrFeeCode: "VAT");
			helper.CreateTariffUOM(tariff15A, "CU1", "LI");
			helper.CreateTariffUOM(tariff15A, "CU2", "KG");
			helper.CreateTariffUOM(tariff15A, "RU1", "NO");
			var tariff15B = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType15B.PK, "1111152", StartDate, EndDate, taxOrFeeCode: "VAT");
			helper.CreateTariffUOM(tariff15B, "CU1", "LI");
			helper.CreateTariffUOM(tariff15B, "CU2", "KG");
			helper.CreateTariffUOM(tariff15B, "RU1", "NO");

			helper.CreateTariffRelationship(tariff12A.PK, tariff1P1.ZZ1_ZZI_TariffType, tariff1P1.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff12B.PK, tariff1P1.ZZ1_ZZI_TariffType, tariff1P1.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff15A.PK, tariff1P1.ZZ1_ZZI_TariffType, tariff1P1.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff15B.PK, tariff1P1.ZZ1_ZZI_TariffType, tariff1P1.ZZ1_TariffCode);
			helper.CreateTaxOrFee(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, 0.14, Core.Constants.CountryCodes.SouthAfrica, StartDate, EndDate, "VAT Normal");

			var tariff1P1Rate = helper.CreateRate(tariff1P1, rateCode_ZA_DTY_D.PK, StartDate, EndDate, "0.0135049 * [LI] + 0.015 * [KG]", preference.PK);
			var tariff12ARate = helper.CreateRate(tariff12A, rateCode_ZA_EXC_D.PK, StartDate, EndDate, "0.04 * [LI] + 0.02 * [NO]", preference.PK);
			var tariff12BRate = helper.CreateRate(tariff12B, rateCode_ZA_EX1_D.PK, StartDate, EndDate, "0.0201249 * [LI] + 0.015 * [NO]", preference.PK);
			var tariff15ARate = helper.CreateRate(tariff15A, rateCode_ZA_LVY_D.PK, StartDate, EndDate, "2.55 * [LI] + 1.5 * [KG]", preference.PK);
			var tariff15BRate = helper.CreateRate(tariff15B, rateCode_ZA_LVY_D.PK, StartDate, EndDate, "1.54 * [LI] + 2 * [NO]", preference.PK);
			factory.Save();

			var tradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", StartDate, EndDate);
			helper.AddCountry(tradeGroup1, Enterprise.Core.Constants.CountryCodes.China, StartDate.Date, EndDate.Date);
			helper.AddCountry(tradeGroup1, Enterprise.Core.Constants.CountryCodes.NewZealand, StartDate.Date, EndDate.Date);
			var testApplicability1 = helper.CreateCusApplicability(tariff1P1Rate, tradeGroup1, StartDate, EndDate);
			var testApplicability2 = helper.CreateCusApplicability(tariff12ARate, null, StartDate, EndDate);
			var testApplicability3 = helper.CreateCusApplicability(tariff15ARate, null, StartDate, EndDate);
			var testApplicability4 = helper.CreateCusApplicability(tariff15BRate, null, StartDate, EndDate);
			var testApplicability5 = helper.CreateCusApplicability(tariff12BRate, null, StartDate, EndDate);
			factory.Save();
		}
	}

	sealed class WhsInventoryDutyAndTaxCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateDutyAndTaxWithNoDeclarationCreated()
		{
			var declarationCount = GetDeclarationCount();

			var helper = new ZAWhsInventoryDutyAndTaxCalculatorTestHelper();
			helper.SetupTestData(Factory);
			var startDate = helper.StartDate;
			var endDate = helper.EndDate;
			var tariffCode = helper.TariffCode;
			Calculate(1m, startDate.AddMonths(1), endDate.AddMonths(3), tariffCode);
			Factory.Save();

			var declarationCountAfterCalculation = GetDeclarationCount();
			AssertEquals("No new Declaration created", declarationCount, declarationCountAfterCalculation);
		}

		int GetDeclarationCount()
		{
			var sql = "SELECT count(*) FROM dbo.JobDeclaration";
			return (int)TestConnection.ExecuteScalar(sql);
		}

		public void TestCalculation()
		{
			var helper = new ZAWhsInventoryDutyAndTaxCalculatorTestHelper();
			helper.SetupTestData(Factory);
			var startDate = helper.StartDate;
			var endDate = helper.EndDate;
			var tariffCode = helper.TariffCode;
			CombineAssertions(() =>
			{
				// Using arrival date
				var data = Calculate(1m, startDate.AddMonths(1), endDate.AddMonths(3), tariffCode);
				AssertEquals("1", 4, data.Count);
				AssertEquals("1 " + UniversalReferenceConstants.CusEntryPayTypes.Duty, new ZDecimal(13233.50m), data[UniversalReferenceConstants.CusEntryPayTypes.Duty]);
				AssertEquals("1 " + UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax, new ZDecimal(2015.86m), data[UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax]);
				AssertEquals("1 " + "12B", new ZDecimal(65.12m), data["12B"]);
				AssertEquals("1 " + UniversalReferenceConstants.CusEntryPayTypes.AllDuties, new ZDecimal(13298.62m), data[UniversalReferenceConstants.CusEntryPayTypes.AllDuties]);

				// ratio at 0.6m
				data = Calculate(0.6m, startDate.AddMonths(1), endDate.AddMonths(3), tariffCode);
				AssertEquals("2", 4, data.Count);
				AssertEquals("2 " + UniversalReferenceConstants.CusEntryPayTypes.Duty, new ZDecimal(7940.1m), data[UniversalReferenceConstants.CusEntryPayTypes.Duty]);
				AssertEquals("2 " + UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax, new ZDecimal(1209.52m), data[UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax]);
				AssertEquals("2 " + "12B", new ZDecimal(39.07m), data["12B"]);
				AssertEquals("2 " + UniversalReferenceConstants.CusEntryPayTypes.AllDuties, new ZDecimal(7979.17m), data[UniversalReferenceConstants.CusEntryPayTypes.AllDuties]);

				// Using valuation date
				data = Calculate(1m, startDate.AddMonths(-1), endDate.AddMonths(-1), tariffCode);
				AssertEquals("3", 4, data.Count);
				AssertEquals("3 " + UniversalReferenceConstants.CusEntryPayTypes.Duty, new ZDecimal(13233.50m), data[UniversalReferenceConstants.CusEntryPayTypes.Duty]);
				AssertEquals("3 " + UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax, new ZDecimal(2015.86m), data[UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax]);
				AssertEquals("3 " + "12B", new ZDecimal(65.12m), data["12B"]);
				AssertEquals("3 " + UniversalReferenceConstants.CusEntryPayTypes.AllDuties, new ZDecimal(13298.62m), data[UniversalReferenceConstants.CusEntryPayTypes.AllDuties]);

				// No matching tariff
				data = Calculate(1m, startDate.AddMonths(1), endDate.AddMonths(3), "21212121");
				AssertEquals("4", 4, data.Count);
				AssertEquals("4 " + UniversalReferenceConstants.CusEntryPayTypes.Duty, new ZString("NO RATE AVAILABLE"), data[UniversalReferenceConstants.CusEntryPayTypes.Duty]);
				AssertEquals("4 " + UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax, new ZString("NO RATE AVAILABLE"), data[UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax]);
				AssertEquals("4 " + "12B", new ZString("NO RATE AVAILABLE"), data["12B"]);
				AssertEquals("4 " + UniversalReferenceConstants.CusEntryPayTypes.AllDuties, new ZString("NO RATE AVAILABLE"), data[UniversalReferenceConstants.CusEntryPayTypes.AllDuties]);

				// No tariff
				data = Calculate(1m, startDate.AddMonths(1), endDate.AddMonths(3), "");
				AssertEquals("5", 4, data.Count);
				AssertEquals("5 " + UniversalReferenceConstants.CusEntryPayTypes.Duty, new ZString("NO RATE AVAILABLE"), data[UniversalReferenceConstants.CusEntryPayTypes.Duty]);
				AssertEquals("5 " + UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax, new ZString("NO RATE AVAILABLE"), data[UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax]);
				AssertEquals("5 " + "12B", new ZString("NO RATE AVAILABLE"), data["12B"]);
				AssertEquals("5 " + UniversalReferenceConstants.CusEntryPayTypes.AllDuties, new ZString("NO RATE AVAILABLE"), data[UniversalReferenceConstants.CusEntryPayTypes.AllDuties]);
			});
		}

		Dictionary<ZString, IZType> Calculate(ZDecimal ratio, ZDateTime arrivalDate, ZDateTime valuationDate, ZString tariff)
		{
			var data = new Dictionary<ZString, IZType>();
			new WhsInventoryDutyAndTaxCalculator(Factory).Calculate(data, ratio, arrivalDate, valuationDate, tariff, Core.Constants.CountryCodes.China, 1000.0049m, 1000.00499m, "LI", 1999.99901m, "KG", 3000m, "NO");
			return data;
		}
	}
}
