using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCACCase))]
	class USCACCaseTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCaseStatusDesc()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			AssertEquals(ACCaseStatusList.Descriptions.AC, acCase.CaseStatusDesc);
		}

		public void TestIsReportable()
		{
			var today = new ZDateTime(2019, 8, 15);

			var lastWeek = today.AddDays(-7);
			var nextWeek = today.AddDays(7);
			var yesterday = today.AddDays(-1);

			//0
			var acCase0 = Factory.New<USCACCase>();
			acCase0.U5_CaseNumber = "A0";
			acCase0.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			AssertEquals(false, ((IACCase)acCase0).IsReportable(today));

			//1
			var acCase1 = Factory.New<USCACCase>();
			acCase1.U5_CaseNumber = "A1";
			acCase1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			var suspension1 = acCase1.LiqSuspensions.AddNew();
			suspension1.UN_Action = "START";
			suspension1.UN_EffectiveDate = lastWeek;
			suspension1.UN_AddedDate = lastWeek;
			suspension1.UN_InactivatedDate = ZDateTime.Empty;
			AssertEquals(true, ((IACCase)acCase1).IsReportable(today));

			//2
			var acCase2 = Factory.New<USCACCase>();
			acCase2.U5_CaseNumber = "A2";
			acCase2.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			var suspension2 = acCase2.LiqSuspensions.AddNew();
			suspension2.UN_Action = "START";
			suspension2.UN_EffectiveDate = lastWeek;
			suspension2.UN_AddedDate = lastWeek;
			suspension2.UN_InactivatedDate = yesterday;
			AssertEquals(false, ((IACCase)acCase2).IsReportable(today));

			//3
			var acCase3 = Factory.New<USCACCase>();
			acCase3.U5_CaseNumber = "A3";
			acCase3.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			var suspension3 = acCase3.LiqSuspensions.AddNew();
			suspension3.UN_Action = "START";
			suspension3.UN_EffectiveDate = lastWeek;
			suspension3.UN_AddedDate = lastWeek;
			suspension3.UN_InactivatedDate = nextWeek;
			AssertEquals(false, ((IACCase)acCase3).IsReportable(today));

			//4
			var acCase4 = Factory.New<USCACCase>();
			acCase4.U5_CaseNumber = "A4";
			acCase4.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			var suspension4 = acCase4.LiqSuspensions.AddNew();
			suspension4.UN_Action = "STOP";
			suspension4.UN_EffectiveDate = lastWeek;
			suspension4.UN_AddedDate = lastWeek;
			suspension4.UN_InactivatedDate = ZDateTime.Empty;
			AssertEquals(false, ((IACCase)acCase4).IsReportable(today));

			//5
			var acCase5 = Factory.New<USCACCase>();
			acCase5.U5_CaseNumber = "A5";
			acCase5.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			var suspension5 = acCase5.LiqSuspensions.AddNew();
			suspension5.UN_Action = "STOP";
			suspension5.UN_EffectiveDate = lastWeek;
			suspension5.UN_AddedDate = lastWeek;
			suspension5.UN_InactivatedDate = yesterday;
			AssertEquals(false, ((IACCase)acCase5).IsReportable(today));

			//6
			var acCase6 = Factory.New<USCACCase>();
			acCase6.U5_CaseNumber = "A6";
			acCase6.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			var suspension6 = acCase6.LiqSuspensions.AddNew();
			suspension6.UN_Action = "STOP";
			suspension6.UN_EffectiveDate = lastWeek;
			suspension6.UN_AddedDate = lastWeek;
			suspension6.UN_InactivatedDate = nextWeek;
			AssertEquals(false, ((IACCase)acCase6).IsReportable(today));

			//7
			var acCase7 = Factory.New<USCACCase>();
			acCase7.U5_CaseNumber = "A7";
			acCase7.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			var suspension7 = acCase7.LiqSuspensions.AddNew();
			suspension7.UN_Action = "START";
			suspension7.UN_EffectiveDate = lastWeek;
			suspension7.UN_AddedDate = lastWeek;
			suspension7.UN_InactivatedDate = today;
			var suspension8 = acCase7.LiqSuspensions.AddNew();
			suspension8.UN_Action = "START";
			suspension8.UN_EffectiveDate = lastWeek;
			suspension8.UN_AddedDate = lastWeek;
			suspension8.UN_InactivatedDate = ZDateTime.Empty;
			AssertEquals(true, ((IACCase)acCase7).IsReportable(today));

			//8
			var acCase8 = Factory.New<USCACCase>();
			acCase8.U5_CaseNumber = "A8";
			acCase8.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			var suspension9 = acCase8.LiqSuspensions.AddNew();
			suspension9.UN_Action = "START";
			suspension9.UN_EffectiveDate = lastWeek;
			suspension9.UN_AddedDate = lastWeek;
			suspension9.UN_InactivatedDate = today;
			var suspension10 = acCase8.LiqSuspensions.AddNew();
			suspension10.UN_Action = "START";
			suspension10.UN_EffectiveDate = lastWeek;
			suspension10.UN_AddedDate = lastWeek;
			suspension10.UN_InactivatedDate = ZDateTime.Empty;
			var suspension11 = acCase8.LiqSuspensions.AddNew();
			suspension11.UN_Action = "STOP";
			suspension11.UN_EffectiveDate = lastWeek;
			suspension11.UN_AddedDate = lastWeek;
			suspension11.UN_InactivatedDate = nextWeek;
			AssertEquals(true, ((IACCase)acCase8).IsReportable(today));

			//9
			var acCase9 = Factory.New<USCACCase>();
			acCase9.U5_CaseNumber = "A9";
			acCase9.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			var suspension12 = acCase9.LiqSuspensions.AddNew();
			suspension12.UN_Action = "START";
			suspension12.UN_EffectiveDate = lastWeek;
			suspension12.UN_AddedDate = lastWeek;
			suspension12.UN_InactivatedDate = today;
			var suspension13 = acCase9.LiqSuspensions.AddNew();
			suspension13.UN_Action = "START";
			suspension13.UN_EffectiveDate = lastWeek;
			suspension13.UN_AddedDate = lastWeek;
			suspension13.UN_InactivatedDate = ZDateTime.Empty;
			var suspension14 = acCase9.LiqSuspensions.AddNew();
			suspension14.UN_Action = "STOP";
			suspension14.UN_EffectiveDate = lastWeek;
			suspension14.UN_AddedDate = lastWeek;
			suspension14.UN_InactivatedDate = nextWeek;
			var suspension15 = acCase9.LiqSuspensions.AddNew();
			suspension15.UN_Action = "STOP";
			suspension15.UN_EffectiveDate = lastWeek;
			suspension15.UN_AddedDate = lastWeek;
			suspension15.UN_InactivatedDate = ZDateTime.Empty;
			AssertEquals(true, ((IACCase)acCase9).IsReportable(today));

			//CS00798629
			var acCase10 = Factory.New<USCACCase>();
			acCase10.U5_CaseNumber = "A10";
			acCase10.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			AssertEquals(false, ((IACCase)acCase10).IsReportable(today));

			var suspension16 = acCase10.LiqSuspensions.AddNew();
			suspension16.UN_Action = "START";
			suspension16.UN_EffectiveDate = lastWeek.AddYears(-3);
			suspension16.UN_AddedDate = lastWeek;
			AssertEquals(true, ((IACCase)acCase10).IsReportable(today));

			var suspension17 = acCase10.LiqSuspensions.AddNew();
			suspension17.UN_Action = "START";
			suspension17.UN_EffectiveDate = lastWeek;
			suspension17.UN_AddedDate = yesterday.AddDays(-3);
			suspension17.UN_InactivatedDate = yesterday;
			AssertEquals(true, ((IACCase)acCase10).IsReportable(today));

			var suspension18 = acCase10.LiqSuspensions.AddNew();
			suspension18.UN_Action = "START";
			suspension18.UN_EffectiveDate = yesterday;
			suspension18.UN_AddedDate = yesterday;
			AssertEquals(true, ((IACCase)acCase10).IsReportable(today));

			var suspension19 = acCase10.LiqSuspensions.AddNew();
			suspension19.UN_Action = "STOP";
			suspension19.UN_EffectiveDate = yesterday;
			suspension19.UN_AddedDate = yesterday;
			AssertEquals(true, ((IACCase)acCase10).IsReportable(today));

			var suspension20 = acCase10.LiqSuspensions.AddNew();
			suspension20.UN_Action = "START";
			suspension20.UN_EffectiveDate = lastWeek.AddYears(-3);
			suspension20.UN_AddedDate = yesterday;
			AssertEquals(true, ((IACCase)acCase10).IsReportable(today));
		}

		public void TestFormattedPhone()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_Phone1 = "5555555555";
			AssertEquals("(555)555-5555", acCase.FormattedPhone1);

			acCase.U5_Phone1 = "5555555555|123";
			AssertEquals("(555)555-5555(Ex.123)", acCase.FormattedPhone1);

			acCase.U5_Phone2 = "5555555556";
			AssertEquals("(555)555-5556", acCase.FormattedPhone2);

			acCase.U5_Phone2 = "5555555556|123";
			AssertEquals("(555)555-5556(Ex.123)", acCase.FormattedPhone2);
		}

		public void TestGetDepositRate()
		{
			var acCase = Factory.New<USCACCase>();
			var caseRate = acCase.CaseRates.AddNew();
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_AdValoremRate = 0.52m;
			caseRate.U6_SpecificRate = 0.62m;

			AssertEquals(0.62m, acCase.GetDepositRate(ZDate.Today).U6_SpecificRate);
			AssertEquals(0.52m, acCase.GetDepositRate(ZDate.Today).U6_AdValoremRate);
		}

		public void TestLatestRates()
		{
			var dumpingCase = Factory.New<USCACCase>();
			dumpingCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var rate = dumpingCase.CaseRates.AddNew();
			rate.U6_AdValoremRate = 1.23m;
			rate.U6_SpecificRate = 0.06m;
			rate.U6_Unit = "KG";
			rate.U6_EffectiveDate = ZDate.Today.AddDays(-22);

			var rate1 = dumpingCase.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 1.24m;
			rate1.U6_SpecificRate = 0.07m;
			rate1.U6_Unit = "T";
			rate1.U6_EffectiveDate = ZDate.Today.AddDays(-21);

			Factory.Save();

			var dumpingCaseLoaded = new BusinessObjectFactory().Load<USCACCase>(dumpingCase.PK);
			AssertEquals("LatestAdValoremRate", 1.2400m, dumpingCaseLoaded.LatestAdValoremRate);
			AssertEquals("1.2400", dumpingCaseLoaded.LatestAdValoremRate.ToString());
			AssertEquals("LatestSpecificRate", "7c/T", dumpingCaseLoaded.LatestSpecificRate);

			var rate2 = dumpingCase.CaseRates.AddNew();
			rate2.U6_AdValoremRate = 0.078m;
			rate2.U6_SpecificRate = 0.07m;
			rate2.U6_Unit = "KG";
			rate2.U6_EffectiveDate = ZDate.Today.AddDays(-20);
			rate2.U6_AddedDate = ZDate.Today.AddDays(-3);

			var rate3 = dumpingCase.CaseRates.AddNew();
			rate3.U6_AdValoremRate = 0.0766m;
			rate3.U6_SpecificRate = 0.08m;
			rate3.U6_Unit = "T";
			rate3.U6_EffectiveDate = ZDate.Today.AddDays(-20);
			rate3.U6_AddedDate = ZDate.Today.AddDays(-1);

			Factory.Save();

			dumpingCaseLoaded = new BusinessObjectFactory().Load<USCACCase>(dumpingCase.PK);
			AssertEquals("LatestAdValoremRate", 0.0766m, dumpingCaseLoaded.LatestAdValoremRate);
			AssertEquals("0.0766", dumpingCaseLoaded.LatestAdValoremRate.ToString());
			AssertEquals("LatestSpecificRate", "8c/T", dumpingCaseLoaded.LatestSpecificRate);
		}

		public void TestTariff()
		{
			var dumpingCase = Factory.New<USCACCase>();
			dumpingCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var tariff = dumpingCase.CaseTariffs.AddNew();
			tariff.U9_TariffNumber = "1";
			tariff = dumpingCase.CaseTariffs.AddNew();
			tariff.U9_TariffNumber = "2";
			tariff = dumpingCase.CaseTariffs.AddNew();
			tariff.U9_TariffNumber = "3";
			tariff = dumpingCase.CaseTariffs.AddNew();
			tariff.U9_TariffNumber = "4";
			tariff = dumpingCase.CaseTariffs.AddNew();
			tariff.U9_TariffNumber = "5";
			AssertEquals("1, 2, 3, 4, 5", dumpingCase.Tariffs);

			dumpingCase = Factory.New<USCACCase>();
			tariff = dumpingCase.CaseTariffs.AddNew();
			tariff.U9_TariffNumber = "1";
			tariff = dumpingCase.CaseTariffs.AddNew();
			tariff.U9_TariffNumber = "2";
			tariff = dumpingCase.CaseTariffs.AddNew();
			tariff.U9_TariffNumber = "3";
			tariff = dumpingCase.CaseTariffs.AddNew();
			tariff.U9_TariffNumber = "4";
			tariff = dumpingCase.CaseTariffs.AddNew();
			tariff.U9_TariffNumber = "5";
			tariff = dumpingCase.CaseTariffs.AddNew();
			tariff.U9_TariffNumber = "6";
			AssertEquals("> 5 Tariffs (double click line to view)", dumpingCase.Tariffs);
		}
	}
}
