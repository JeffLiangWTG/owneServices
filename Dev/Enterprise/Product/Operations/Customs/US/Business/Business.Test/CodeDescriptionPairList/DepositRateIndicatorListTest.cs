using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DepositRateIndicatorListTest : TestCaseWithFactory
	{
		public void TestGetDepositRate()
		{
			USCACCaseRate rate = Factory.New<USCACCaseRate>();
			rate.U6_AdValoremRate = 0.1234m;
			rate.U6_SpecificRate = 0.2345m;
			AssertEquals(0.1234m, DepositRateIndicatorList.GetDepositRate(rate, ""));
			AssertEquals(0.1234m, DepositRateIndicatorList.GetDepositRate(rate, DepositRateIndicatorList.Codes.AdValorem));
			AssertEquals(0.23m, DepositRateIndicatorList.GetDepositRate(rate, DepositRateIndicatorList.Codes.Specific));
		}

		public void TestGetRateDescriptionFromCaseRecord()
		{
			var caseRecord = Factory.New<USCACCase>();
			caseRecord.U5_CaseNumber = "A342089";

			var caseRate = caseRecord.CaseRates.AddNew();
			caseRate.U6_AdValoremRate = 0.123m;
			caseRate.U6_SpecificRate = 0.234m;
			caseRate.U6_Unit = "KG";
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;

			AssertEquals("12.30%", DepositRateIndicatorList.GetRateDescriptionFromCaseRecord(caseRecord, ZDate.Today, DepositRateIndicatorList.Codes.AdValorem));
			AssertEquals("23c/KG", DepositRateIndicatorList.GetRateDescriptionFromCaseRecord(caseRecord, ZDate.Today, DepositRateIndicatorList.Codes.Specific));
		}

		public void TestGetRateDescriptionFromCaseRecordACE()
		{
			var caseRecord = Factory.New<USCACCase>();
			caseRecord.U5_CaseNumber = "A342089";

			var caseRate = caseRecord.CaseRates.AddNew();
			caseRate.U6_AdValoremRate = 0.1206m;
			caseRate.U6_SpecificRate = 0.02m;
			caseRate.U6_Unit = "KG";
			caseRate.U6_UnitDesc = "Kilogram";
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;

			AssertEquals("12.06%", DepositRateIndicatorList.GetRateDescriptionFromCaseRecord(caseRecord, ZDate.Today, DepositRateIndicatorList.Codes.AdValorem));
			AssertEquals("2c/KG(Kilogram)", DepositRateIndicatorList.GetRateDescriptionFromCaseRecord(caseRecord, ZDate.Today, DepositRateIndicatorList.Codes.Specific));

			AssertEquals("12.06%", DepositRateIndicatorList.GetRateDescription(DepositRateIndicatorList.Codes.AdValorem, caseRate));
			AssertEquals("2c/KG(Kilogram)", DepositRateIndicatorList.GetRateDescription(DepositRateIndicatorList.Codes.Specific, caseRate));
			AssertEquals("2c/KG", DepositRateIndicatorList.GetACERateDescriptionForSpecificOrOverrideSpecific(0.02m, "KG", ""));

			caseRate.U6_SpecificRate = 1.02m;
			AssertEquals("1.02$/KG(Kilogram)", DepositRateIndicatorList.GetRateDescriptionFromCaseRecord(caseRecord, ZDate.Today, DepositRateIndicatorList.Codes.Specific));
			AssertEquals("1.02$/KG", DepositRateIndicatorList.GetACERateDescriptionForSpecificOrOverrideSpecific(1.02m, "KG", ""));
		}

		public void TestConstructor()
		{
			USCACCaseRate rate = Factory.New<USCACCaseRate>();
			rate.U6_CaseNumber = "0000";
			rate.U6_AdValoremRate = 0.1234m;
			rate.U6_SpecificRate = 0.2345m;
			rate.U6_Unit = "KG";
			rate.U6_EffectiveDate = ZDateTime.Today;
			DepositRateIndicatorList list = new DepositRateIndicatorList(null);
			AssertEquals(0, list.Count);
			list = new DepositRateIndicatorList(rate);
			AssertEquals(4, list.Count);
			AssertCodeDescription(list[0], DepositRateIndicatorList.Codes.AdValorem, "12.34%");
			AssertCodeDescription(list[1], DepositRateIndicatorList.Codes.OverrideAdValorem, DepositRateIndicatorList.Descriptions.OverrideAdValorem);
			AssertCodeDescription(list[2], DepositRateIndicatorList.Codes.Specific, "23c/KG");
			AssertCodeDescription(list[3], DepositRateIndicatorList.Codes.OverrideSpecific, DepositRateIndicatorList.Descriptions.OverrideSpecific);
		}

		public void TestAntidumpingDutyDepositRatesForACE()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "A1";

			var caseRate = acCase.CaseRates.AddNew();
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_AdValoremRate = 0.52m;

			var list = new DepositRateIndicatorList(caseRate);
			AssertEquals(2, list.Count);
			AssertCodeDescription(list[0], DepositRateIndicatorList.Codes.AdValorem, "52.00%");
			AssertCodeDescription(list[1], DepositRateIndicatorList.Codes.OverrideAdValorem, DepositRateIndicatorList.Descriptions.OverrideAdValorem);

			caseRate.U6_SpecificRate = 0.62m;
			caseRate.U6_Unit = "KG";
			list = new DepositRateIndicatorList(caseRate);
			AssertEquals(4, list.Count);
			AssertCodeDescription(list[2], DepositRateIndicatorList.Codes.Specific, "62c/KG");
			AssertCodeDescription(list[3], DepositRateIndicatorList.Codes.OverrideSpecific, DepositRateIndicatorList.Descriptions.OverrideSpecific);
		}

		public void TestAntidumpingDutyDepositRatesForACE2()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "A1";

			var caseRate = acCase.CaseRates.AddNew();
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_AdValoremRate = 0m;
			caseRate.U6_SpecificRate = 0.62m;
			caseRate.U6_Unit = "KG";

			DepositRateIndicatorList list = new DepositRateIndicatorList(caseRate);
			AssertEquals(2, list.Count);
			AssertCodeDescription(list[0], DepositRateIndicatorList.Codes.Specific, "62c/KG");
		}

		public void TestAntidumpingDutyDepositRatesForACEToShowAdValoremRateAlways()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "A1";

			var caseRate = acCase.CaseRates.AddNew();
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_AdValoremRate = 0.00m;
			caseRate.U6_SpecificRate = 0.00m;

			DepositRateIndicatorList list = new DepositRateIndicatorList(caseRate);
			AssertEquals(2, list.Count);
			AssertEquals(DepositRateIndicatorList.Codes.AdValorem, list[0].Code);
		}

		void AssertCodeDescription(ICodeDescription codeDescription, string expectedCode, string expectedDescription)
		{
			AssertEquals("Code", expectedCode, codeDescription.Code);
			AssertEquals("Description", expectedDescription, codeDescription.Description);
		}
	}
}
