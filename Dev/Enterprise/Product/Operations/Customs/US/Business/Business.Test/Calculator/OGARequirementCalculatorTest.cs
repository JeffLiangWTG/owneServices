using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class OGARequirementCalculatorTest : TestCaseWithFactory
	{
		public void TestOMCRequirementCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();

			tariff.UE_PGACodes = "OM1";
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.OM1 + " (OM1)", invoiceLine1.US_OMCReqDesc);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			tariff2.UE_PGACodes = "OM2";
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.OM2 + " (OM2)", invoiceLine2.US_OMCReqDesc);
		}

		public void TestHFCRequirementDesc()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();

			tariff.UE_PGACodes = "EH1";
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EH1 + " (EH1)", invoiceLine1.US_HFCReqDesc);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			tariff2.UE_PGACodes = "EH2";
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EH2 + " (EH2)", invoiceLine2.US_HFCReqDesc);
		}

		public void TestCPSCRequirementDesc()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();

			tariff.UE_PGACodes = "CP1";
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.CP1 + " (CP1)", invoiceLine1.US_CPSCReqDesc);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			tariff2.UE_PGACodes = "CP2";
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.CP2 + " (CP2)", invoiceLine2.US_CPSCReqDesc);
		}

		public void TestAMSRequirementDesc()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();

			tariff.UE_PGACodes = "AM1";
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AM1 + " (AM1)", invoiceLine1.US_AMSReqDesc);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			tariff2.UE_PGACodes = "AM2";
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AM2 + " (AM2)", invoiceLine2.US_AMSReqDesc);

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "0000000002";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Today;
			tariff3.UE_PGACodes = "AM4";
			invoiceLine3.JI_Tariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AM4 + " (AM4)", invoiceLine3.US_AMSReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine1.JI_Tariff = ZString.Empty;
			invoiceLine1.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AM1 + " (AM1)", invoiceLine1.US_AMSReqDesc);

			tariff3.UE_PGACodes = "AM3";
			invoiceLine1.US_FTZCurrentTariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AM3 + " (AM3)", invoiceLine1.US_AMSReqDesc);
		}

		public void TestNOPRequirementDesc()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();

			tariff.UE_PGACodes = "AM7";
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AM7 + " (AM7)", invoiceLine1.US_NOPReqDesc);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			tariff2.UE_PGACodes = "AM8";
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AM8 + " (AM8)", invoiceLine2.US_NOPReqDesc);

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "0000000002";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Today;
			tariff3.UE_PGACodes = "AM7";
			invoiceLine3.JI_Tariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AM7 + " (AM7)", invoiceLine3.US_NOPReqDesc);
		}

		public void TestTSCARequirementDesc()
		{
			CusClassPartPivot partPivot = Factory.New<CusClassPartPivot>();
			tariff.UE_PGACodes = "EP8";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			tariff2.UE_PGACodes = "EP7";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EP7 + " (EP7)", invoiceLine.US_TSCAReqDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "EP8";
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EP8 + " (EP8)", invoiceLine.US_TSCAReqDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EP7 + " (EP7)", invoiceLine.US_TSCAReqDesc);
		}

		public void TestGetTariffRuleCodeFromRequirementCode()
		{
			var query1 = new ZQuery(USCTariffRuleSchema.U1_RuleCode, TariffRuleList.Codes.LaceyAct);
			query1.AddToFilter(USCTariffRuleSchema.U1_Tariff, tariff.UE_Tariff);
			var tariffRule = Factory.LoadTop1<USCTariffRule>(query1);
			if (tariffRule == null)
			{
				tariffRule = Factory.New<USCTariffRule>();
				tariffRule.U1_RuleCode = TariffRuleList.Codes.LaceyAct;
				tariffRule.U1_Tariff = tariff.UE_Tariff;
			}
			tariffRule.U1_DateFrom = ZDateTime.Today;

			var declaration = invoiceLine.Declaration;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			tariff.UE_PGACodes = "AL1";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AL1 + " (AL1)", invoiceLine.US_LaceyRequirementDesc);

			tariff2.UE_PGACodes = "TB1";

			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.TB1 + " (TB1)", invoiceLine.US_TTBReqDesc);
		}

		public void TestNHTSARequirementDesc()
		{
			tariff.UE_PGACodes = "DT2";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.DT2 + " (DT2)", invoiceLine.US_NHTSAReqDesc);

			tariff2.UE_PGACodes = "DT1";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.DT1 + " (DT1)", invoiceLine.US_NHTSAReqDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "DT2";
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.DT2 + " (DT2)", invoiceLine.US_NHTSAReqDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.DT2 + " (DT2)", invoiceLine.US_NHTSAReqDesc);
		}

		public void TestTTBRequirementDesc()
		{
			tariff.UE_PGACodes = "TB1";
			tariff2.UE_PGACodes = "TB2";

			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "0000000002";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Today;
			tariff3.UE_PGACodes = "TB3";

			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.TB1 + " (TB1)", invoiceLine.US_TTBReqDesc);

			invoiceLine.JI_Tariff = tariff2.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.TB2 + " (TB2)", invoiceLine.US_TTBReqDesc);

			invoiceLine.JI_Tariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.TB3 + " (TB3)", invoiceLine.US_TTBReqDesc);

			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.TB2 + " (TB2)", invoiceLine.US_TTBReqDesc);

			invoiceLine.US_SupTariff = tariff3.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.TB2 + " (TB2)", invoiceLine.US_TTBReqDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.TB1 + " (TB1)", invoiceLine.US_TTBReqDesc);

			invoiceLine.US_FTZCurrentTariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.TB3 + " (TB3)", invoiceLine.US_TTBReqDesc);
		}

		public void TestDOTRequirementDesc()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			tariff.UE_OGACodes = "DT1";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.DT1 + " (DT1)", invoiceLine.US_DOTRequirementDesc);
			AssertEquals(OGARequirementList.Descriptions.DT1 + " (DT1)", partPivot.US_DOTRequirement);

			tariff2.UE_OGACodes = "FD0DT2";
			tariff2.UE_PGACodes = "DT2";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			partPivot.CI_TariffNum = tariff2.UE_Tariff;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.DT2 + " (DT2)", invoiceLine.US_DOTRequirementDesc);
			AssertEquals(OGARequirementList.Descriptions.DT2 + " (DT2)", partPivot.US_DOTRequirement);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.DT1 + " (DT1)", invoiceLine.US_DOTRequirementDesc);
		}

		public void TestPSTRequirementDesc()
		{
			tariff.UE_PGACodes = "EP6";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EP6 + " (EP6)", invoiceLine.US_PSTReqDesc);

			tariff2.UE_PGACodes = "EP5";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EP5 + " (EP5)", invoiceLine.US_PSTReqDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "EP6";
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EP6 + " (EP6)", invoiceLine.US_PSTReqDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EP6 + " (EP6)", invoiceLine.US_PSTReqDesc);
		}

		public void TestACELaceyRequirementDesc()
		{
			var declaration = invoiceLine.Declaration;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			tariff.UE_PGACodes = "AL2";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AL2 + " (AL2)", invoiceLine.US_LaceyRequirementDesc);

			tariff2.UE_PGACodes = "AL1";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AL1 + " (AL1)", invoiceLine.US_LaceyRequirementDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "AL2";
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AL2 + " (AL2)", invoiceLine.US_LaceyRequirementDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AL2 + " (AL2)", invoiceLine.US_LaceyRequirementDesc);
		}

		public void TestLaceyRequirementDesc()
		{
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var rule = Factory.New<USCRule>();
			rule.U0_Code = TariffRuleList.Codes.LaceyAct;

			var tariff_rule = Factory.New<USCTariffRule>();
			tariff_rule.U1_RuleCode = TariffRuleList.Codes.LaceyAct;
			tariff_rule.U1_Tariff = "0000000000";
			tariff_rule.U1_DateFrom = new ZDate(2000, 1, 1);
			tariff_rule.U1_DateTo = ZDateTime.Today.AddYears(1);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			tariff.TariffRules.Load(tariff.UE_Tariff);

			AssertEquals(OGARequirementCalculator.LaceyMayBeRequired, invoiceLine.US_LaceyRequirementDesc);
		}

		public void TestOGARequirementWithSupTariff()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			tariff.UE_OGACodes = "FC3";
			tariff2.UE_OGACodes = "FD0FC4";

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_SupplementalTariff = tariff.UE_Tariff;

			invoiceLine.US_SupTariff = tariff2.UE_Tariff;
			partPivot.CI_SupplementalTariff = tariff2.UE_Tariff;
			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.US_LaceyRequirementDesc);
			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.US_PSTReqDesc);

			AssertEquals(OGARequirementCalculator.NoRequirement, partPivot.US_LaceyRequirementDesc);
			AssertEquals(OGARequirementCalculator.NoRequirement, partPivot.US_PSTRequirementDesc);

			var rule = Factory.New<USCRule>();
			rule.U0_Code = TariffRuleList.Codes.LaceyAct;

			var tariff_rule = Factory.New<USCTariffRule>();
			tariff_rule.U1_RuleCode = TariffRuleList.Codes.LaceyAct;
			tariff_rule.U1_Tariff = "0000000000";
			tariff_rule.U1_DateFrom = new ZDate(2000, 1, 1);
			tariff_rule.U1_DateTo = ZDateTime.Today.AddYears(1);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			tariff.TariffRules.Load(tariff.UE_Tariff);
			AssertEquals(OGARequirementCalculator.LaceyMayBeRequired, invoiceLine.US_LaceyRequirementDesc);

			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = "ACS";
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;
			AssertEquals(OGARequirementCalculator.LaceyMayBeRequired, invoiceLine.US_LaceyRequirementDesc);

			tariff.UE_PGACodes = "AL2";
			AssertEquals("Lacey Act specific data is required (AL2)", partPivot.US_LaceyRequirementDesc);
		}

		public void TestFSISRequirementDesc()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			tariff.UE_OGACodes = "EPC";
			tariff.UE_PGACodes = "FS3";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.FS3 + " (FS3)", invoiceLine.US_FSISReqDesc);
			AssertEquals(OGARequirementList.Descriptions.FS3 + " (FS3)", partPivot.US_FSISRequirementDesc);

			tariff2.UE_OGACodes = "FD0";
			tariff2.UE_PGACodes = "DT2FS4";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			partPivot.CI_TariffNum = tariff2.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.FS4 + " (FS4)", invoiceLine.US_FSISReqDesc);
			AssertEquals(OGARequirementList.Descriptions.FS4 + " (FS4)", partPivot.US_FSISRequirementDesc);

			tariff.UE_OGACodes = ZString.Empty;
			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.US_FSISReqDesc);
			AssertEquals(OGARequirementCalculator.NoRequirement, partPivot.US_FSISRequirementDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_OGACodes = "FS4";
			tariff3.UE_PGACodes = "EP4FS4";
			tariff.UE_OGACodes = "EPC";
			tariff.UE_PGACodes = "FS3";

			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			partPivot.CI_SupplementalTariff = tariff3.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.FS4 + " (FS4)", invoiceLine.US_FSISReqDesc);

			AssertEquals(OGARequirementList.Descriptions.FS4 + " (FS4)", partPivot.US_FSISRequirementDesc);

			var tariff4 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0202200200"));
			if (tariff4 == null)
			{
				tariff4 = Factory.New<USCTariff>();
				tariff4.UE_Tariff = "0202200200";
			}
			tariff4.UE_DateFrom = ZDateTime.Today;
			tariff4.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff4.UE_OGACodes = "AP2FD3FS2";
			tariff4.UE_PGACodes = "FS4";
			invoiceLine.US_SupTariff = tariff4.UE_Tariff;
			partPivot.CI_SupplementalTariff = tariff4.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.FS4 + " (FS4)", invoiceLine.US_FSISReqDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.FS3 + " (FS3)", invoiceLine.US_FSISReqDesc);
		}

		public void TestODSRequirementDesc()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			tariff.UE_PGACodes = "EP1";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.EP1 + " (EP1)", invoiceLine.US_ODSReqDesc);
			AssertEquals(OGARequirementList.Descriptions.EP1 + " (EP1)", partPivot.US_ODSRequirementDesc);

			tariff2.UE_PGACodes = "EP2";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			partPivot.CI_TariffNum = tariff2.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.EP2 + " (EP2)", invoiceLine.US_ODSReqDesc);
			AssertEquals(OGARequirementList.Descriptions.EP2 + " (EP2)", partPivot.US_ODSRequirementDesc);

			tariff2.UE_PGACodes = ZString.Empty;
			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.US_ODSReqDesc);
			AssertEquals(OGARequirementCalculator.NoRequirement, partPivot.US_ODSRequirementDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "EP1FD1";

			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			partPivot.CI_SupplementalTariff = tariff3.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.EP1 + " (EP1)", invoiceLine.US_ODSReqDesc);

			tariff2.UE_PGACodes = "EP2";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EP2 + " (EP2)", invoiceLine.US_ODSReqDesc);

			tariff.UE_PGACodes = "EP1";
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EP1 + " (EP1)", invoiceLine.US_ODSReqDesc);
		}

		public void TestAPHISRequirementDesc()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			tariff2.UE_PGACodes = "AQX";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			partPivot.CI_TariffNum = tariff2.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.AQX + " (AQX)", invoiceLine.US_APHISReqDesc);
			AssertEquals(OGARequirementList.Descriptions.AQX + " (AQX)", partPivot.US_APHISRequirementDesc);

			tariff.UE_PGACodes = "AQ2FD2";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.AQ2 + " (AQ2)", invoiceLine.US_APHISReqDesc);
			AssertEquals(OGARequirementList.Descriptions.AQ2 + " (AQ2)", partPivot.US_APHISRequirementDesc);

			tariff2.UE_OGACodes = "AQ2";
			tariff2.UE_PGACodes = "";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			partPivot.CI_TariffNum = tariff2.UE_Tariff;

			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.US_APHISReqDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "AQ1";

			invoiceLine.US_SupTariff = tariff3.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.AQ1 + " (AQ1)", invoiceLine.US_APHISReqDesc);

			tariff2.UE_PGACodes = "AQ2";
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.AQ2 + " (AQ2)", invoiceLine.US_APHISReqDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(ZString.Empty, invoiceLine.US_APHISReqDesc);
		}

		public void TestFWSRequirementDesc()
		{
			tariff.UE_PGACodes = "FW2FD2";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.FW2 + " (FW2)", invoiceLine.US_FWSReqDesc);

			tariff2.UE_OGACodes = "FW2";
			tariff2.UE_PGACodes = "";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;

			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.US_FWSReqDesc);

			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "0000000002";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Today;
			tariff3.UE_PGACodes = "FW3";
			invoiceLine.JI_Tariff = tariff3.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.FW3 + " (FW3)", invoiceLine.US_FWSReqDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.FW2 + " (FW2)", invoiceLine.US_FWSReqDesc);

			invoiceLine.US_FTZCurrentTariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.FW3 + " (FW3)", invoiceLine.US_FWSReqDesc);
		}

		public void TestNMFS370RequirementDesc()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			tariff.UE_PGACodes = "FD2NM2";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.NM2 + " (NM2)", invoiceLine.US_NMFS370ReqDesc);
			AssertEquals(OGARequirementList.Descriptions.NM2 + " (NM2)", partPivot.US_NMFS370RequirementDesc);

			tariff2.UE_PGACodes = "FD0DT2FS4NM1";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			partPivot.CI_TariffNum = tariff2.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.NM1 + " (NM1)", invoiceLine.US_NMFS370ReqDesc);
			AssertEquals(OGARequirementList.Descriptions.NM1 + " (NM1)", partPivot.US_NMFS370RequirementDesc);

			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.US_NMFS370ReqDesc);
			AssertEquals(OGARequirementCalculator.NoRequirement, partPivot.US_NMFS370RequirementDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "FD3NM2FS4";

			tariff.UE_PGACodes = "NM2";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			partPivot.CI_SupplementalTariff = tariff3.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.NM2 + " (NM2)", invoiceLine.US_NMFS370ReqDesc);
			AssertEquals(OGARequirementList.Descriptions.NM2 + " (NM2)", partPivot.US_NMFS370RequirementDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.NM2 + " (NM2)", invoiceLine.US_NMFS370ReqDesc);
		}

		public void TestNMFSCOARequirementDesc()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.PGA);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, TariffConditionValueTypes.Codes.PGA);
			Factory.Save();
			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0000000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, zzTariff.PK, "test", true, false, ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, GovernmentAgencyProgramCodeList.Codes.COA);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();

			var tariff1 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0000000000")).LastOrDefault();
			if (tariff1 == null)
			{
				tariff1 = Factory.New<USCTariff>();
				tariff1.UE_Tariff = "0000000000";
			}
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;
			invoiceLine1.JI_Tariff = tariff1.UE_Tariff;

			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine1.US_NMFSCOAReqDesc);

			var tariff2 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0000000001")).LastOrDefault();
			if (tariff2 == null)
			{
				tariff2 = Factory.New<USCTariff>();
				tariff2.UE_Tariff = "0000000001";
			}
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.COA, invoiceLine2.US_NMFSCOAReqDesc);
		}

		public void TestNMFSAMRRequirementDesc()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			tariff.UE_PGACodes = "FD2NM4";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.NM4 + " (NM4)", invoiceLine.US_NMFSAMRReqDesc);
			AssertEquals(OGARequirementList.Descriptions.NM4 + " (NM4)", partPivot.US_NMFSAMRRequirementDesc);

			tariff2.UE_PGACodes = "FD0DT2FS4NM3";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			partPivot.CI_TariffNum = tariff2.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.NM3 + " (NM3)", invoiceLine.US_NMFSAMRReqDesc);
			AssertEquals(OGARequirementList.Descriptions.NM3 + " (NM3)", partPivot.US_NMFSAMRRequirementDesc);

			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.US_NMFSAMRReqDesc);
			AssertEquals(OGARequirementCalculator.NoRequirement, partPivot.US_NMFSAMRRequirementDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "FD3NM4FS4";

			tariff.UE_PGACodes = "NM4";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			partPivot.CI_SupplementalTariff = tariff3.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.NM4 + " (NM4)", invoiceLine.US_NMFSAMRReqDesc);
			AssertEquals(OGARequirementList.Descriptions.NM4 + " (NM4)", partPivot.US_NMFSAMRRequirementDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.NM4 + " (NM4)", invoiceLine.US_NMFSAMRReqDesc);
		}

		public void TestNMFSHMSRequirementDesc()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			tariff.UE_PGACodes = "FD2NM6";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.NM6 + " (NM6)", invoiceLine.US_NMFSHMSReqDesc);
			AssertEquals(OGARequirementList.Descriptions.NM6 + " (NM6)", partPivot.US_NMFSHMSRequirementDesc);

			tariff2.UE_PGACodes = "FD0DT2FS4NM5";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			partPivot.CI_TariffNum = tariff2.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.NM5 + " (NM5)", invoiceLine.US_NMFSHMSReqDesc);
			AssertEquals(OGARequirementList.Descriptions.NM5 + " (NM5)", partPivot.US_NMFSHMSRequirementDesc);

			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.US_NMFSHMSReqDesc);
			AssertEquals(OGARequirementCalculator.NoRequirement, partPivot.US_NMFSHMSRequirementDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "FD3NM6FS4";

			tariff.UE_PGACodes = "NM6";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			partPivot.CI_SupplementalTariff = tariff3.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.NM6 + " (NM6)", invoiceLine.US_NMFSHMSReqDesc);
			AssertEquals(OGARequirementList.Descriptions.NM6 + " (NM6)", partPivot.US_NMFSHMSRequirementDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.NM6 + " (NM6)", invoiceLine.US_NMFSHMSReqDesc);
		}

		public void TestNMFSSIMRequirementDesc()
		{
			tariff.UE_PGACodes = "FD2NM8";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.NM8 + " (NM8)", invoiceLine.US_NMFSSIMReqDesc);

			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.US_NMFSSIMReqDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "FD3NM8FS4";

			tariff.UE_PGACodes = "NM8";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.NM8 + " (NM8)", invoiceLine.US_NMFSSIMReqDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.NM8 + " (NM8)", invoiceLine.US_NMFSSIMReqDesc);
		}

		public void TestVNERequirementDesc()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			tariff.UE_PGACodes = "FD2EP4";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.EP4 + " (EP4)", invoiceLine.US_VNEReqDesc);
			AssertEquals(OGARequirementList.Descriptions.EP4 + " (EP4)", partPivot.US_VNERequirementDesc);

			tariff2.UE_PGACodes = "FD0DT2FS4EP3";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			partPivot.CI_TariffNum = tariff2.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.EP3 + " (EP3)", invoiceLine.US_VNEReqDesc);
			AssertEquals(OGARequirementList.Descriptions.EP3 + " (EP3)", partPivot.US_VNERequirementDesc);

			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.US_VNEReqDesc);
			AssertEquals(OGARequirementCalculator.NoRequirement, partPivot.US_VNERequirementDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "FD3EP4FS4";

			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			partPivot.CI_SupplementalTariff = tariff3.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.EP4 + " (EP4)", invoiceLine.US_VNEReqDesc);

			tariff.UE_PGACodes = "EP3";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EP4 + " (EP4)", invoiceLine.US_VNEReqDesc);
			AssertEquals(OGARequirementList.Descriptions.EP4 + " (EP4)", partPivot.US_VNERequirementDesc);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.EP3 + " (EP3)", invoiceLine.US_VNEReqDesc);
		}

		public void TestACE_FDARequirementDescFD2()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_EnableENS = true;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			tariff.UE_OGACodes = "FD4EP4";
			tariff.UE_PGACodes = "FD2EP4";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.FD2 + " (FD2)", invoiceLine.JI_FDARequirementDesc);
			AssertEquals(OGARequirementList.Descriptions.FD2 + " (FD2)", partPivot.US_ACEFDARequirementDesc);
		}

		public void TestACE_FDARequirementDescFD0()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			invoiceLine.Declaration.US_EnableENS = true;
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			tariff2.UE_PGACodes = "FD0DT2FS4EP3";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			partPivot.CI_TariffNum = tariff2.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.FD0 + " (FD0)", invoiceLine.JI_FDARequirementDesc);
			AssertEquals(OGARequirementList.Descriptions.FD0 + " (FD0)", partPivot.US_ACEFDARequirementDesc);
		}

		public void TestACE_FDARequirementDescNone()
		{
			CusClassPartPivot partPivot = Factory.New<CusClassPartPivot>();

			invoiceLine.Declaration.US_EnableENS = true;
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;

			AssertEquals(OGARequirementCalculator.NoRequirement, invoiceLine.JI_FDARequirementDesc);
			AssertEquals(OGARequirementCalculator.NoRequirement, partPivot.US_ACEFDARequirementDesc);//
		}

		public void TestACE_FDARequirementDescFD3()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			invoiceLine.Declaration.US_EnableENS = true;
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "FD3EP4FS4";

			tariff.UE_PGACodes = "FD3EP4";
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			partPivot.CI_SupplementalTariff = tariff3.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.FD3 + " (FD3)", invoiceLine.JI_FDARequirementDesc);
			AssertEquals(OGARequirementList.Descriptions.FD3 + " (FD3)", partPivot.US_ACEFDARequirementDesc);
		}

		public void TestACE_FDARequirementDescFD4()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			tariff.UE_OGACodes = "FD4EP4";
			tariff.UE_PGACodes = "FD2EP4";

			invoiceLine.Declaration.US_EnableENS = true;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.FD2 + " (FD2)", invoiceLine.JI_FDARequirementDesc);

			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff3 == null)
			{
				tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "9801008000";
			}
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "FD3EP4FS4";

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			tariff3.UE_OGACodes = "FD3EP4FS4";
			tariff3.UE_PGACodes = "FD3EP4FS4";

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			partPivot.CI_TariffNum = tariff.UE_Tariff;
			partPivot.CI_SupplementalTariff = tariff.UE_Tariff;

			AssertEquals(OGARequirementList.Descriptions.FD4 + " (FD4)", invoiceLine.JI_FDARequirementDesc);
			AssertEquals(OGARequirementList.Descriptions.FD4 + " (FD4)", partPivot.US_FDARequirement);
		}

		public void TestDEARequirementDesc()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			tariff.UE_PGACodes = "DE1";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGARequirementList.Descriptions.DE1 + " (DE1)", invoiceLine.US_DEAReqDesc);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;

			tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
		}

		JobComInvoiceLine invoiceLine;
		USCTariff tariff;
		USCTariff tariff2;
	}
}
