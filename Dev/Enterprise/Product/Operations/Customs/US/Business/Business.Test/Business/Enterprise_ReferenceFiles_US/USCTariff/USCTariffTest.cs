using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariff))]
	sealed class USCTariffTest : EnterpriseBusinessObjectTestCase
	{
		public void TestA99AndI99MutuallyExclusive()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9903181000";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);

			var tariffRule1 = Factory.New<USCTariffRule>();
			tariffRule1.U1_RuleCode = "A99";
			tariffRule1.U1_Tariff = "990318";
			tariffRule1.U1_DateFrom = ZDateTime.Today;

			var tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_RuleCode = "I99";
			tariffRule2.U1_Tariff = "9903";
			tariffRule2.U1_DateFrom = ZDateTime.Today;

			Assert(tariff.Applies("A99", ZDateTime.Today));
			Assert(!tariff.Applies("I99", ZDateTime.Today));
		}

		public void TestGlobalTariffA99Applied()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "55556666";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);

			var tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_RuleCode = "B99";
			tariffRule2.U1_Tariff = "5555";
			tariffRule2.U1_DateFrom = ZDateTime.Today;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = new ZDate(2016, 01, 01);
			var endDate = new ZDate(2079, 01, 01);

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			var tariff9903 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "55556666", new ZDateTime(2018, 09, 24), new ZDateTime(2079, 06, 06));
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate = helper.CreateRate(tariff9903, rateCode.PK, startDate, endDate);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, "HK", startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			var applicability = helper.CreateCusApplicability(rate, tradeGroup, startDate, endDate);
			var relationship = helper.CreateTariffRelationship(tariff9903.PK, hsnTariffType.PK, "73");
			var tariffAttribute = helper.CreateTariffAttribute("RULE", "C99", tariff9903);
			Factory.Save();

			Assert("C99 Rule from Global Tariff", tariff.Applies("C99", ZDateTime.Today));
			Assert("B99 Rule from US Tariff", tariff.Applies("B99", ZDateTime.Today));
		}

		public void TestIUSCTariffIsCorrectSetup()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "26020006";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_ShortDescription = "Test Description";
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "U";
			tariff.UE_Unit3 = "ZZ";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var tariff2 = newFactory.Load<Integration.Customs.US.IUSCTariff>(tariff.PK);
			AssertEquals("26020006", tariff2.UE_Tariff);
		}

		public void TestUniversalITariffMembers()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "26020006";
			tariff.UE_ShortDescription = "Test Description";
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "U";
			tariff.UE_Unit3 = "ZZ";

			Universal.ITariff iTariff = tariff;

			AssertEquals("iTariff.TariffCode", "26020006", iTariff.Code);
			AssertEquals("iTariff.Description", "Test Description", iTariff.Description);
			AssertEquals("iTariff.UQ1", "KG", iTariff.UQ1);
			AssertEquals("iTariff.UQ2", "U", iTariff.UQ2);
			AssertEquals("iTariff.UQ3", "ZZ", iTariff.UQ3);
			AssertEquals("iTariff.UQ4", ZString.Empty, iTariff.UQ4);
			AssertEquals("iTariff.UQ5", ZString.Empty, iTariff.UQ5);
		}

		public void TestCPSCTariff()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_OGACodes = "";
			tariff.UE_PGACodes = "CP2";
			AssertEquals(true, tariff.DoesRequireCPSC);
			AssertEquals(false, tariff.MayRequireCPSC);
			AssertEquals(true, tariff.HasCPSCRequirement);

			tariff.UE_PGACodes = "CP1";
			AssertEquals(false, tariff.DoesRequireCPSC);
			AssertEquals(true, tariff.MayRequireCPSC);
			AssertEquals(true, tariff.HasCPSCRequirement);
		}

		public void TestAMSTariff()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_OGACodes = "";
			tariff.UE_PGACodes = "AM2";
			AssertEquals(true, tariff.DoesRequireAMSEG);
			AssertEquals(false, tariff.MayRequireAMSEG);

			tariff.UE_PGACodes = "AM1";
			AssertEquals(false, tariff.DoesRequireAMSEG);
			AssertEquals(true, tariff.MayRequireAMSEG);

			tariff.UE_PGACodes = "AM4";
			AssertEquals(true, tariff.DoesRequireAMSMO);

			tariff.UE_PGACodes = "AM3AM6";
			AssertEquals(true, tariff.MayRequireAMSMO);
			AssertEquals(true, tariff.DoesRequireAMSPeanuts);

			tariff.UE_PGACodes = "AM7";
			AssertEquals(true, tariff.MayRequireAMSOrganics);
			AssertEquals(false, tariff.DoesRequireAMSOrganics);

			tariff.UE_PGACodes = "AM8";
			AssertEquals(false, tariff.MayRequireAMSOrganics);
			AssertEquals(true, tariff.DoesRequireAMSOrganics);
		}

		public void TestTTBTariff()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_OGACodes = "";
			tariff.UE_PGACodes = "TB2";
			AssertEquals(true, tariff.DoesRequireTTB);
			AssertEquals(false, tariff.MayRequireTTB);

			tariff.UE_PGACodes = "TB1";
			AssertEquals(false, tariff.DoesRequireTTB);
			AssertEquals(true, tariff.MayRequireTTB);

			tariff.UE_PGACodes = "TB3";
			AssertEquals(false, tariff.DoesRequireTTB);
			AssertEquals(true, tariff.MayRequireTTB);

			CreateTariffRule("0407", TariffRuleList.Codes.PermitNumberTTBProgramRequirement, ZDateTime.BrettsBirthday.AddMonths(-2), ZDateTime.Today);
			CreateTariffRule("0407000020", TariffRuleList.Codes.ForeignCertificateTTBProgramRequirement, ZDateTime.BrettsBirthday.AddMonths(-6), ZDateTime.Today);
			CreateTariffRule("04070000", TariffRuleList.Codes.COLANumberTTBProgramRequirement, ZDateTime.BrettsBirthday.AddMonths(-4), ZDateTime.Today);

			var date = ZDateTime.BrettsBirthday.AddMonths(-5);
			AssertEquals(true, tariff.IsForeignCertificateRequiredForTTB(date));
			AssertEquals(false, tariff.IsCOLANumberRequiredForTTB(date));
			AssertEquals(false, tariff.IsPermitNumberRequiredForTTB(date));

			date = ZDateTime.BrettsBirthday.AddMonths(-3);
			AssertEquals(true, tariff.IsForeignCertificateRequiredForTTB(date));
			AssertEquals(true, tariff.IsCOLANumberRequiredForTTB(date));
			AssertEquals(false, tariff.IsPermitNumberRequiredForTTB(date));

			date = ZDateTime.BrettsBirthday.AddMonths(-1);
			AssertEquals(true, tariff.IsForeignCertificateRequiredForTTB(date));
			AssertEquals(true, tariff.IsCOLANumberRequiredForTTB(date));
			AssertEquals(true, tariff.IsPermitNumberRequiredForTTB(date));

			date = ZDateTime.BrettsBirthday.AddMonths(-7);
			AssertEquals(false, tariff.IsForeignCertificateRequiredForTTB(date));
			AssertEquals(false, tariff.IsCOLANumberRequiredForTTB(date));
			AssertEquals(false, tariff.IsPermitNumberRequiredForTTB(date));

			date = ZDateTime.Today.AddMonths(+1);
			AssertEquals(false, tariff.IsForeignCertificateRequiredForTTB(date));
			AssertEquals(false, tariff.IsCOLANumberRequiredForTTB(date));
			AssertEquals(false, tariff.IsPermitNumberRequiredForTTB(date));
		}

		public void TestOMCTariff()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_OGACodes = "";
			tariff.UE_PGACodes = "OM2";
			AssertEquals(true, tariff.DoesRequireOMC);
			AssertEquals(false, tariff.MayRequireOMC);

			tariff.UE_PGACodes = "OM1";
			AssertEquals(false, tariff.DoesRequireOMC);
			AssertEquals(true, tariff.MayRequireOMC);
		}

		public static USCTariff CreateTariff(BusinessObjectFactory factory, ZString tariffNumber)
		{
			USCTariff tariff = factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNumber;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			return tariff;
		}

		public static USCTariff CreateNewTariffIfNotExist(BusinessObjectFactory factory, ZString tariffNumber, ZString computationCode, ZDecimal rateAdValorem, ZString customsUnit)
		{
			var tariff = factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffNumber)).LastOrDefault();
			if (tariff == null)
			{
				tariff = factory.New<USCTariff>();
				tariff.UE_Tariff = tariffNumber;
				tariff.UE_DutyComputationCode = computationCode;
				tariff.UE_Column1RateAdValorem = rateAdValorem;
				tariff.UE_Unit1 = customsUnit;
			}

			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			return tariff;
		}

		void CreateTariffRule(ZString tariff, ZString ruleCode, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var ttbRule = Factory.LoadFromNaturalKey<USCRule>(USCRuleSchema.U0_Code, ruleCode);
			if (ttbRule == null)
			{
				ttbRule = Factory.New<USCRule>();
				ttbRule.U0_Code = ruleCode;
			}

			var tariffRuleQuery = new ZQuery(USCTariffRuleSchema.U1_RuleCode, ruleCode);
			tariffRuleQuery.AddToFilter(USCTariffRuleSchema.U1_Tariff, tariff);
			tariffRuleQuery.AddToFilter(USCTariffRuleSchema.U1_DateFrom, dateFrom);
			tariffRuleQuery.AddToFilter(USCTariffRuleSchema.U1_DateTo, dateTo);
			var tariffRule = Factory.LoadTop1<USCTariffRule>(tariffRuleQuery);
			if (tariffRule == null)
			{
				tariffRule = Factory.New<USCTariffRule>();
				tariffRule.U1_RuleCode = ruleCode;
				tariffRule.U1_Tariff = tariff;
				tariffRule.U1_DateFrom = dateFrom;
				tariffRule.U1_DateTo = dateTo;
			}
		}

		public void TestTSCATariff()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "7007110010";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_OGACodes = "";
			tariff.UE_PGACodes = "EP8";
			AssertEquals("MayRequireTSCA", false, tariff.MayRequireTSCA);

			tariff.UE_PGACodes = "EP7";
			AssertEquals("MayRequireTSCA", true, tariff.MayRequireTSCA);
		}

		public void TestCheckTaxApply()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";

			AssertEquals("PreCondition", true, tariff.IsTaxRequired);

			var bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Code = ZString.Empty;
			tariff.CheckTaxApply(bizObj.Z0_CodeInfo, Core.Constants.USCustoms.FeeCodes.Wines);
			AssertHasMessageError(bizObj.Z0_CodeInfo, USCTariff.TaxIsRequired);

			bizObj.Z0_Code = TaxApplyList.Codes.No;
			tariff.CheckTaxApply(bizObj.Z0_CodeInfo, Core.Constants.USCustoms.FeeCodes.Wines);
			AssertHasMessageError(bizObj.Z0_CodeInfo, USCTariff.TaxIsRequired);

			bizObj.Z0_Code = TaxApplyList.Codes.Yes;
			tariff.CheckTaxApply(bizObj.Z0_CodeInfo, Core.Constants.USCustoms.FeeCodes.Wines);
			AssertNoMessageError(bizObj.Z0_CodeInfo, USCTariff.TaxIsRequired);

			tariff.CheckTaxApply(bizObj.Z0_CodeInfo, Core.Constants.USCustoms.FeeCodes.DistilledSpirits);
			AssertHasMessageError(bizObj.Z0_CodeInfo, USCTariff.TaxRateMustBeOverridenForDiffTaxCode);

			bizObj.Z0_Code = TaxApplyList.Codes.Override;
			tariff.CheckTaxApply(bizObj.Z0_CodeInfo, Core.Constants.USCustoms.FeeCodes.DistilledSpirits);
			AssertNoMessageError(bizObj.Z0_CodeInfo, USCTariff.TaxRateMustBeOverridenForDiffTaxCode);

			if (ErrorReporter.LastKeyReported == "Validation:Z0_Code")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestGetTaxCodeList()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "00000000";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;

			USCTariffDutyRate dutyRate1 = tariff1.DutyRates.AddNew();
			dutyRate1.UD_TaxFeeClassCode = "016";
			USCTariffDutyRate dutyRate2 = tariff1.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = "018";

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "000000002";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;

			USCTariffDutyRate dutyRate3 = tariff2.DutyRates.AddNew();
			dutyRate3.UD_TaxFeeClassCode = "018";

			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "00000001";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.MaxSmallDateTime;

			USCTariffDutyRate dutyRate4 = tariff3.DutyRates.AddNew();
			dutyRate4.UD_TaxFeeClassCode = "017";
			USCTariffDutyRate dutyRate5 = tariff3.DutyRates.AddNew();
			dutyRate5.UD_TaxFeeClassCode = "022";

			AssertEquals(3, USCTariff.GetTaxCodeList(tariff1, Factory).Count);

			//016 and 017 are interchangeable
			Assert(USCTariff.GetTaxCodeList(tariff1, Factory).ContainsCode("016"));
			Assert(USCTariff.GetTaxCodeList(tariff1, Factory).ContainsCode("017"));
			Assert(USCTariff.GetTaxCodeList(tariff1, Factory).ContainsCode("018"));

			AssertEquals(1, USCTariff.GetTaxCodeList(tariff2, Factory).Count);
			Assert(USCTariff.GetTaxCodeList(tariff2, Factory).ContainsCode("018"));

			AssertEquals(3, USCTariff.GetTaxCodeList(tariff1, Factory).Count);

			//016 and 017 are interchangeable
			Assert(USCTariff.GetTaxCodeList(tariff3, Factory).ContainsCode("016"));
			Assert(USCTariff.GetTaxCodeList(tariff3, Factory).ContainsCode("017"));
			Assert(USCTariff.GetTaxCodeList(tariff3, Factory).ContainsCode("022"));
		}

		public void TestTaxRateList()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = "016";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.11111m;
			dutyRate.UD_TaxFeeFlag = "1";

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000002";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariffDutyRate dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = "022";
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate2.UD_TaxFeeAdvalorem = 0.25m;
			dutyRate2.UD_TaxFeeFlag = "1";

			AssertEquals(3, USCTariff.GetTaxRateList(tariff, "016", ZString.Empty, Factory).Count);

			Assert(USCTariff.GetTaxRateList(tariff, "016", ZString.Empty, Factory).ContainsCode(tariff.GetTaxFeeRateDescription("016", "")));
			Assert(USCTariff.GetTaxRateList(tariff, "016", ZString.Empty, Factory).ContainsCode(AppendixBTaxRateList.Codes.DistilledSpirits));
			Assert(USCTariff.GetTaxRateList(tariff, "016", ZString.Empty, Factory).ContainsCode(AppendixBTaxRateList.Codes.Specify));

			AssertEquals(6, USCTariff.GetTaxRateList(tariff2, "022", ZString.Empty, Factory).Count);
		}

		public void TestTaxFeeCode()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			USCTariffDutyRate dutyRate11 = tariff1.DutyRates.AddNew();
			dutyRate11.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate11.UD_TaxFeeComputationCode = "X";
			dutyRate11.UD_TaxFeeFlag = "2";
			USCTariffDutyRate dutyRate12 = tariff1.DutyRates.AddNew();
			dutyRate12.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			dutyRate12.UD_TaxFeeFlag = "X";
			dutyRate12.UD_TaxFeeFlag = "2";
			AssertEquals("017,016", tariff1.TaxFeeCode);

			USCTariff tariff2 = Factory.New<USCTariff>();
			USCTariffDutyRate dutyRate21 = tariff2.DutyRates.AddNew();
			dutyRate21.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Cotton;
			dutyRate21.UD_TaxFeeComputationCode = "2";
			dutyRate21.UD_TaxFeeFlag = "1";
			AssertEquals("056", tariff2.TaxFeeCode);

			USCTariff tariff3 = Factory.New<USCTariff>();
			USCTariffDutyRate dutyRate31 = tariff3.DutyRates.AddNew();
			dutyRate31.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate31.UD_TaxFeeComputationCode = "C";
			dutyRate31.UD_TaxFeeFlag = "1";
			AssertEquals("017", tariff3.TaxFeeCode);
		}

		public void TestTaxFeeComputationCode()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			USCTariffDutyRate dutyRate11 = tariff1.DutyRates.AddNew();
			dutyRate11.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			AssertEquals(ZString.Empty, tariff1.TaxFeeComputationCode);
			dutyRate11.UD_TaxFeeComputationCode = "X";
			dutyRate11.UD_TaxFeeFlag = "2";
			AssertEquals("X", tariff1.TaxFeeComputationCode);
		}

		public void TestTaxFeeRate()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			USCTariffDutyRate dutyRate11 = tariff1.DutyRates.AddNew();
			dutyRate11.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate11.UD_TaxFeeComputationCode = "X";
			dutyRate11.UD_TaxFeeFlag = "2";
			USCTariffDutyRate dutyRate12 = tariff1.DutyRates.AddNew();
			dutyRate12.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			dutyRate12.UD_TaxFeeFlag = "X";
			dutyRate12.UD_TaxFeeFlag = "2";
			AssertEquals(ZString.Empty, tariff1.TaxFeeRate);

			USCTariff tariff2 = Factory.New<USCTariff>();
			USCTariffDutyRate dutyRate21 = tariff2.DutyRates.AddNew();
			dutyRate21.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Cotton;
			dutyRate21.UD_TaxFeeComputationCode = "2";
			dutyRate21.UD_TaxFeeFlag = "1";
			dutyRate21.UD_TaxFeeSpecificRate = 0.09874m;
			AssertEquals("0.09874", tariff2.TaxFeeRate);

			USCTariff tariff3 = Factory.New<USCTariff>();
			USCTariffDutyRate dutyRate31 = tariff3.DutyRates.AddNew();
			dutyRate31.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate31.UD_TaxFeeComputationCode = "C";
			dutyRate31.UD_TaxFeeFlag = "1";
			dutyRate31.UD_TaxFeeSpecificRate = 0.898178m;
			dutyRate31.UD_TaxFeeAdvalorem = 0.871761m;
			AssertEquals("0.898178 or 0.871761", tariff3.TaxFeeRate);

			USCTariff tariff4 = Factory.New<USCTariff>();
			USCTariffDutyRate dutyRate41 = tariff4.DutyRates.AddNew();
			dutyRate41.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate41.UD_TaxFeeComputationCode = "C";
			dutyRate41.UD_TaxFeeFlag = "1";
			dutyRate41.UD_TaxFeeSpecificRate = 0.0m;
			dutyRate41.UD_TaxFeeAdvalorem = 0.871761m;
			AssertEquals("0.871761", tariff4.TaxFeeRate);
		}

		public void TestIsTaxApplicableRequiredOrConditional()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;

			Assert(tariff.IsTaxRequired);
			Assert(tariff.IsTaxApplicable);
			Assert(!tariff.IsTaxConditional);

			dutyRate.UD_TaxFeeFlag = "2";
			Assert(!tariff.IsTaxRequired);
			Assert(tariff.IsTaxApplicable);
			Assert(tariff.IsTaxConditional);
			Assert(tariff.IsTaxComputationUnknown);

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000002";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			USCTariffDutyRate dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Beef;
			dutyRate2.UD_TaxFeeFlag = "1";
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;

			Assert(!tariff2.IsTaxRequired);
			Assert(!tariff2.IsTaxApplicable);
			Assert(!tariff2.IsTaxConditional);
			Assert(!tariff2.IsTaxComputationUnknown);

			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Coffee;
			Assert(tariff2.PRCoffeeFeeMightBeRequired);
		}

		public void TestIsRepairOrAssembly()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_RuleCode = TariffRuleList.Codes.AssembledAbroadOfUSProducts;
			tariffRule.U1_Tariff = "0000000000";

			USCTariffRule tariffRule1 = Factory.New<USCTariffRule>();
			tariffRule1.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule1.U1_RuleCode = TariffRuleList.Codes.RepairTariffs;
			tariffRule1.U1_Tariff = "0000000011";

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			Assert(tariff.IsRepairOrAssembly(ZDateTime.Now));

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000011";
			Assert(tariff.IsRepairOrAssembly(ZDateTime.Now));

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000010";
			Assert(!tariff.IsRepairOrAssembly(ZDateTime.Now));
		}

		public void TestIsDutyFreeFor()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Column1RateAdValorem = 0m;
			tariff.UE_Column1RateSpecific = 0m;
			tariff.UE_Column1RateOther = 0m;

			tariff.UE_Column2RateAdValorem = 10m;
			tariff.UE_Column2RateSpecific = 20m;
			tariff.UE_Column2RateOther = 30m;

			AssertEquals("IsDutyFreeFor", true, tariff.IsDutyFreeFor("KR", ZDateTime.Today));
			AssertEquals("Is not duty free for Rate 2", false, tariff.IsDutyFreeFor("CU", ZDateTime.Today));
		}

		public void TestGetRelatedFeeCodes()
		{
			USCTariff tariff = Factory.New<USCTariff>();

			USCTariffDutyRate rate1 = tariff.DutyRates.AddNew();
			rate1.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Honey;
			rate1.UD_TaxFeeFlag = "1";

			USCTariffDutyRate rate2 = tariff.DutyRates.AddNew();
			rate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Sugar;
			rate2.UD_TaxFeeFlag = "2";

			USCTariffDutyRate rate3 = tariff.DutyRates.AddNew();
			rate3.UD_TaxFeeClassCode = ZString.Empty;

			List<ZString> requiredCodes = new List<ZString>(tariff.GetRelatedFeeCodes());
			AssertEquals(2, requiredCodes.Count);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Honey, requiredCodes[0]);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Sugar, requiredCodes[1]);
		}

		public void TestRequiredFeeCodes()
		{
			USCTariff tariff = Factory.New<USCTariff>();

			USCTariffDutyRate rate1 = tariff.DutyRates.AddNew();
			rate1.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Honey;
			rate1.UD_TaxFeeFlag = "1";

			USCTariffDutyRate rate2 = tariff.DutyRates.AddNew();
			rate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Sugar;
			rate2.UD_TaxFeeFlag = "2";

			USCTariffDutyRate rate3 = tariff.DutyRates.AddNew();
			rate3.UD_TaxFeeClassCode = ZString.Empty;

			List<ZString> requiredCodes = new List<ZString>(tariff.GetRequiredFeeCodes());
			AssertEquals(1, requiredCodes.Count);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Honey, requiredCodes[0]);
		}

		public void TestIsSPICountryValid()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_SPICode = "CACLILAUB ";

			AssertEquals(true, tariff.IsSPICountryValid(SpecialProgramList.Codes.BSharp));
			AssertEquals(false, tariff.IsSPICountryValid(SpecialProgramList.Codes.LSharp));

			AssertEquals(true, tariff.IsSPICountryValid(SpecialProgramList.Codes.AU));
			AssertEquals(false, tariff.IsSPICountryValid(SpecialProgramList.Codes.BH));

			tariff.UE_SPICode = "D E J P AUBHCACLILJOMAMXSG";
			AssertEquals(true, tariff.IsSPICountryValid(SpecialProgramList.Codes.AU));
			AssertEquals(true, tariff.IsSPICountryValid(SpecialProgramList.Codes.MA));
			AssertEquals(false, tariff.IsSPICountryValid("OM"));

			tariff.UE_SPICode = "D E J P ";
			AssertEquals(false, tariff.IsSPICountryValid(SpecialProgramList.Codes.AU));
			AssertEquals(false, tariff.IsSPICountryValid(SpecialProgramList.Codes.MA));
		}

		public void TestIsValidForGSP()
		{
			USCCountry rate3Country = Factory.New<USCCountry>();
			rate3Country.UC_RateColumnIndicator = "3";
			rate3Country.UC_GSPIndicator = true;

			USCCountry gspCountry = Factory.New<USCCountry>();
			gspCountry.UC_GSPIndicator = true;

			USCCountry gspExcludedCountry = Factory.New<USCCountry>();
			gspExcludedCountry.UC_Code = "AA";
			gspExcludedCountry.UC_GSPIndicator = true;

			USCCountry nonGSPCountry = Factory.New<USCCountry>();
			nonGSPCountry.UC_GSPIndicator = false;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_SPICode = "A CLILAUB ";

			AssertEquals("GSP valid for an applicable country", true, tariff.IsValidForGSP(rate3Country, ZDateTime.Today));

			AssertEquals("GSP valid for an applicable country", true, tariff.IsValidForGSP(gspCountry, ZDateTime.Today));

			AssertEquals("GSP valid for an applicable country", true, tariff.IsValidForGSP(gspExcludedCountry, ZDateTime.Today));

			AssertEquals("GSP not applicable", false, tariff.IsValidForGSP(nonGSPCountry, ZDateTime.Today));

			tariff.UE_SPICode = "A+CLILAUB ";//only applicable for rate 3 country
			AssertEquals("GSP valid for an applicable country", true, tariff.IsValidForGSP(rate3Country, ZDateTime.Today));

			AssertEquals("GSP invalid for non-rate3 countries", false, tariff.IsValidForGSP(gspCountry, ZDateTime.Today));

			AssertEquals("GSP invalid for non-rate3 countries", false, tariff.IsValidForGSP(gspExcludedCountry, ZDateTime.Today));

			tariff.UE_SPICode = "A*CLILAUB ";//only applicable for non-excluded
			tariff.UE_GSPExcludedCountries = "AABB";
			AssertEquals("GSP valid for an applicable country", true, tariff.IsValidForGSP(rate3Country, ZDateTime.Today));

			AssertEquals("GSP valid for an applicable country", true, tariff.IsValidForGSP(gspCountry, ZDateTime.Today));

			AssertEquals("GSP invalid as it is excluded", false, tariff.IsValidForGSP(gspExcludedCountry, ZDateTime.Today));
		}

		public void TestHasSpecialProgramsIndicatorForE_J()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_SPICode = "E*CLILAUJ ";
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("E"));
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("E*"));
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("J"));
			AssertEquals(false, tariff.HasSpecialProgramsIndicator("J*"));

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_SPICode = "E CLILAUJ*";
			AssertEquals(true, tariff2.HasSpecialProgramsIndicator("E"));
			AssertEquals(false, tariff2.HasSpecialProgramsIndicator("E*"));
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("J"));
			AssertEquals(false, tariff.HasSpecialProgramsIndicator("J*"));
		}

		public void TestHasSpecialProgramsIndicatorForBSharp()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_SPICode = "B CLILAUJ ";
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("B"));
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("B#"));
			AssertEquals(false, tariff.HasSpecialProgramsIndicator("C#"));
		}

		public void TestBecomesDutyFreeDueToSPIs()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Column1RateAdValorem = 0.0543m;
			tariff.UE_SPICode = "AUMXA ";

			USCTariffDutyRate dutyRateForMX = tariff.DutyRates.AddNew();
			dutyRateForMX.UD_ISOCountryCode = "MX";
			dutyRateForMX.UD_TaxFeeComputationCode = ComputationCodeList.Codes.AdValorem;
			dutyRateForMX.UD_AdValoremSpecialRate = 0.0243m;

			AssertEquals("BecomesDutyFree", true, tariff.BecomesDutyFreeDueTo("AU"));
			AssertEquals("BecomesDutyFree", false, tariff.BecomesDutyFreeDueTo("MX"));
			AssertEquals("BecomesDutyFree", true, tariff.BecomesDutyFreeDueTo("A "));
		}

		public void TestIsDutyFree()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.Free;
			AssertEquals(true, tariff.IsDutyFree);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.FunctionalAdValorem;
			AssertEquals(false, tariff.IsDutyFree);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.MultipleSpecific;
			AssertEquals(false, tariff.IsDutyFree);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.Derived;
			AssertEquals(false, tariff.IsDutyFree);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			AssertEquals(false, tariff.IsDutyFree);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			AssertEquals(false, tariff.IsDutyFree);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			AssertEquals(false, tariff.IsDutyFree);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificFunctionalAdValorem;
			AssertEquals(false, tariff.IsDutyFree);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificCompound;
			AssertEquals(false, tariff.IsDutyFree);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificCompound;
			AssertEquals(false, tariff.IsDutyFree);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			AssertEquals(false, tariff.IsDutyFree);
		}

		[TestDate(2008, 1, 1)]
		public void TestConformToDateRestriction()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = USCTariff.CAFTABenefitsApplicable;
			AssertEquals("ConformToDateRestriction", true, tariff.ConformToDateRestriction(new ZDateTime(2008, 1, 1)));

			tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("1", 331, 831);
			tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("2", 1101, 229);

			AssertEquals("ConformToDateRestriction", true, tariff.ConformToDateRestriction(new ZDateTime(2009, 1, 1)));
			AssertEquals("ConformToDateRestriction", false, tariff.ConformToDateRestriction(new ZDateTime(2008, 3, 1)));
		}

		public void TestIsValueToBeDeclaredInAlternateTariffForDerived()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			AssertEquals(false, tariff.IsValueToBeDeclaredInAlternateTariff(ZDateTime.Today));

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.Derived;
			AssertEquals(true, tariff.IsValueToBeDeclaredInAlternateTariff(ZDateTime.Today));
		}

		public void TestIsEligibleForCAFTATPLClaims()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = USCTariff.CAFTABenefitsApplicable;
			ZDateTime today = ZDateTime.Today;

			AssertEquals(true, tariff.IsEligibleForCAFTAClaims(today));
			AssertEquals(true, tariff.IsValueToBeDeclaredInAlternateTariff(today));

			tariff.UE_Tariff = USCTariff.CottonFeeApplicable;
			AssertEquals(false, tariff.IsEligibleForCAFTAClaims(today));
			AssertEquals(false, tariff.IsValueToBeDeclaredInAlternateTariff(today));

			tariff.UE_Tariff = "9915620000";
			AssertEquals(true, tariff.IsEligibleForCAFTAClaims(today));
			AssertEquals(true, tariff.IsValueToBeDeclaredInAlternateTariff(today));
		}

		public void TestIsFeeApplicable()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			USCTariffDutyRate rate = tariff.DutyRates.AddNew();

			rate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Potato;
			AssertEquals(false, tariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Potato));

			rate.UD_TaxFeeFlag = "1";
			AssertEquals(true, tariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Potato));
		}

		public void TestIsSpecificSpecificDutyRate()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9802008042";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSugarJ;
			AssertEquals("IsSpecificSpecificDutyRate", false, tariff.IsSpecificSpecificDutyRate);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			AssertEquals("IsSpecificSpecificDutyRate", true, tariff.IsSpecificSpecificDutyRate);
		}

		public void TestIsEligibleForAGOATextileBenefits()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9802008042";
			ZDateTime today = ZDateTime.Today;

			AssertEquals(true, tariff.IsEligibleForAGOATextileBenefits(today));
			AssertEquals(false, tariff.IsEligibleForCBTPATextileBenefits(today));
			AssertEquals(false, tariff.IsValueToBeDeclaredInAlternateTariff(today));

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "98191103";
			AssertEquals(true, tariff.IsEligibleForAGOATextileBenefits(today));
			AssertEquals(true, tariff.IsValueToBeDeclaredInAlternateTariff(today));

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9802008044";
			AssertEquals(false, tariff.IsEligibleForAGOATextileBenefits(today));
			AssertEquals(true, tariff.IsEligibleForCBTPATextileBenefits(today));
			AssertEquals(false, tariff.IsValueToBeDeclaredInAlternateTariff(today));

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "98201103";
			AssertEquals(true, tariff.IsEligibleForCBTPATextileBenefits(today));
			AssertEquals(true, tariff.IsValueToBeDeclaredInAlternateTariff(today));
		}

		public void TestIsPastaTariff()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1902112010";
			Assert(tariff.IsPastaTariff);

			tariff.UE_Tariff = "1902112020";
			Assert(tariff.IsPastaTariff);

			tariff.UE_Tariff = "1902112030";
			Assert(tariff.IsPastaTariff);

			tariff.UE_Tariff = "1902192010";
			Assert(tariff.IsPastaTariff);

			tariff.UE_Tariff = "1902192020";
			Assert(tariff.IsPastaTariff);

			tariff.UE_Tariff = "1902192030";
			Assert(tariff.IsPastaTariff);

			tariff.UE_Tariff = "2122000000";
			AssertEquals(false, tariff.IsPastaTariff);
		}

		public void TestIsTextileTariff()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1902112010";
			AssertEquals(false, tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5001000000";
			Assert(tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5204110000";
			Assert(tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6305900000";
			Assert(tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6309000020";
			Assert(tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6310902000";
			Assert(tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6401100000";
			AssertEquals(false, tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6501003000";
			Assert(tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6504009045";
			Assert(tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6506920000";
			AssertEquals(false, tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6505909089";
			Assert(tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6505100000";
			AssertEquals(false, tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9502910000";
			Assert(tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9502990000";
			AssertEquals(false, tariff.Applies(TariffRuleList.Codes.TextileEntryMID, ZDateTime.Today));
		}

		public void TestDeleteWithDeleteAllChildren()
		{
			var tariff = Factory.New<USCTariff>();
			var value1 = Factory.New<USCTariffValue>();
			value1.UA_UE = tariff.PK;
			var value2 = Factory.New<USCTariffValue>();
			value2.UA_UE = tariff.PK;
			var quantity1 = Factory.New<USCTariffQuantity>();
			quantity1.UQ_UE = tariff.PK;
			var quantity2 = Factory.New<USCTariffQuantity>();
			quantity2.UQ_UE = tariff.PK;
			tariff.Delete();
			AssertEquals("value1.IsDeleted", true, value1.IsDeleted);
			AssertEquals("value2.IsDeleted", true, value2.IsDeleted);
			AssertEquals("quantity1.IsDeleted", true, quantity1.IsDeleted);
			AssertEquals("quantity2.IsDeleted", true, quantity2.IsDeleted);
		}

		public void TestHasValidCountryOfOrigin()
		{
			var tariff = Factory.New<USCTariff>();
			AssertEquals("", tariff.ValidCountryOfOrigin);
			AssertEquals("HasValidCountryOfOrigin", true, tariff.HasValidCountryOfOrigin("KR"));

			tariff.UE_ISOCountryofOriginEditCode = "01";
			AssertEquals("", tariff.ValidCountryOfOrigin);
			AssertEquals("HasValidCountryOfOrigin", true, tariff.HasValidCountryOfOrigin("KR"));

			tariff.UE_ISOCountryofOriginEditCode = "CA";
			AssertEquals("CA", tariff.ValidCountryOfOrigin);

			AssertEquals("HasValidCountryOfOrigin", false, tariff.HasValidCountryOfOrigin("KR"));
			AssertEquals("HasValidCountryOfOrigin", true, tariff.HasValidCountryOfOrigin(CanadaProvinceTerritoryCodes.Codes.XA));
			AssertEquals("HasValidCountryOfOrigin", true, tariff.HasValidCountryOfOrigin(CanadaProvinceTerritoryCodes.Codes.XE));
			AssertEquals("HasValidCountryOfOrigin", true, tariff.HasValidCountryOfOrigin(CanadaProvinceTerritoryCodes.Codes.XD));
		}

		public void TestIsTheQuantityRequired()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "KG";
			tariff.UE_Unit3 = "KG";

			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "111";
			AssertEquals(true, tariff.RequiresFirstQuantity());
			AssertEquals(false, tariff.RequiresSecondQuantity());
			AssertEquals(false, tariff.RequiresThirdQuantity());

			tariffValue.UA_ValueEditCode = "121";
			AssertEquals(false, tariff.RequiresFirstQuantity());
			AssertEquals(true, tariff.RequiresSecondQuantity());
			AssertEquals(false, tariff.RequiresThirdQuantity());

			tariffValue.UA_ValueEditCode = "131";
			AssertEquals(false, tariff.RequiresFirstQuantity());
			AssertEquals(false, tariff.RequiresSecondQuantity());
			AssertEquals(true, tariff.RequiresThirdQuantity());

			tariffValue.UA_ValueEditCode = "231";
			AssertEquals(false, tariff.RequiresFirstQuantity());
			AssertEquals(false, tariff.RequiresSecondQuantity());
			AssertEquals(false, tariff.RequiresThirdQuantity());

			var tariffQuantity = tariff.GetOrCreateNewTariffQuantity();
			tariffQuantity.UQ_QuantityEditCode = "122";
			AssertEquals(true, tariff.RequiresFirstQuantity());
			AssertEquals(true, tariff.RequiresSecondQuantity());
			AssertEquals(false, tariff.RequiresThirdQuantity());

			tariffQuantity.UQ_QuantityEditCode = "312";
			AssertEquals(true, tariff.RequiresFirstQuantity());
			AssertEquals(false, tariff.RequiresSecondQuantity());
			AssertEquals(true, tariff.RequiresThirdQuantity());

			tariff.UE_Unit1 = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			tariff.UE_Unit2 = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			tariff.UE_Unit3 = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			tariffQuantity.UQ_QuantityEditCode = "312";
			AssertEquals(false, tariff.RequiresFirstQuantity());
			AssertEquals(false, tariff.RequiresSecondQuantity());
			AssertEquals(false, tariff.RequiresThirdQuantity());

			tariffQuantity.UQ_QuantityEditCode = "122";
			AssertEquals(false, tariff.RequiresSecondQuantity());
		}

		public void TestIsTIB110PercentTariff()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2122000000";
			AssertEquals(false, tariff.IsTIB110PercentTariff);

			tariff.UE_Tariff = "98130020";
			AssertEquals(true, tariff.IsTIB110PercentTariff);

			tariff.UE_Tariff = "98130025";
			AssertEquals(true, tariff.IsTIB110PercentTariff);

			tariff.UE_Tariff = "98130050";
			AssertEquals(true, tariff.IsTIB110PercentTariff);

			tariff.UE_Tariff = "1902192010";
			AssertEquals(false, tariff.IsTIB110PercentTariff);
		}

		[TestDate(2007, 03, 30)]
		public void TestConstants()
		{
			Assert("FDAPriorNoticeRequiredTariff", new USCTariff.Loader(Factory).LoadBestMatch(USCTariff.FDAPriorNoticeRequiredTariff, ZDateTime.Today).FDAPriorNoticeAndAdmissibilityReviewRequired);
			Assert("FDAAdmissibilityReviewRequiredTariff", new USCTariff.Loader(Factory).LoadBestMatch(USCTariff.FDAAdmissibilityReviewRequiredTariff, ZDateTime.Today).FDAAdmissibilityReviewRequired);
			var dOTtariff = new USCTariff.Loader(Factory).LoadBestMatch(USCTariff.DOTIsApplicable, ZDateTime.Today);
			dOTtariff.UE_PGACodes = "DT2";
			Assert("DOTIsApplicable", new USCTariff.Loader(Factory).LoadBestMatch(USCTariff.DOTIsApplicable, ZDateTime.Today).DoesRequireDOT);

			Assert("FDAPriorNoticeMayBeRequiredTariff", new USCTariff.Loader(Factory).LoadBestMatch(USCTariff.FDAPriorNoticeMayBeRequiredTariff, ZDateTime.Today).FDAPriorNoticeAndAdmissibilityReviewMayBeRequired);
			Assert("FDAAdmissibilityReviewMayBeRequiredTariff", new USCTariff.Loader(Factory).LoadBestMatch(USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff, ZDateTime.Today).FDAAdmissibilityReviewMayBeRequired);
			Assert("DOTMayBeApplicable", new USCTariff.Loader(Factory).LoadBestMatch(USCTariff.DOTMayBeApplicable, ZDateTime.Today).MayRequireDOT);
		}

		public void TestHasSingleCharacterSpecialProgramsIndicator()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_SPICode = "L";
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("L"));
		}

		public void TestHasSpecialProgramsIndicatorA()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_SPICode = "A";
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("A"));
			tariff.UE_SPICode = "A*";
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("A"));
			tariff.UE_SPICode = "A+";
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("A"));
			tariff.UE_SPICode = "";
			AssertEquals(false, tariff.HasSpecialProgramsIndicator("A"));
		}

		public void TestHasSpecialProgramsIndicator()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.HasSpecialProgramsIndicator(ZString.Empty));
			tariff.UE_SPICode = "ABCD";
			AssertEquals(false, tariff.HasSpecialProgramsIndicator("BC"));
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("AB"));
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("CD"));
			AssertEquals(false, tariff.HasSpecialProgramsIndicator("EF"));
			AssertEquals(true, tariff.HasSpecialProgramsIndicator("Z"));    // All tariffs support Z
		}

		public void TestLoadUniqueStartsWith()
		{
			ZDateTime today = ZDateTime.Today;

			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "01999991";
			tariff3.UE_DateFrom = today.AddDays(-10);
			tariff3.UE_DateTo = tariff3.UE_DateFrom;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "01999991";
			tariff2.UE_DateFrom = today;
			tariff2.UE_DateTo = today;

			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "01999900";
			tariff1.UE_DateFrom = today;
			tariff1.UE_DateTo = today;

			USCTariff[] tariffs = new USCTariff.Loader(Factory).LoadCurrentlyValidTariffs("01999991", today);
			AssertEquals(1, tariffs.Length);
			AssertEquals(tariff2, tariffs[0]);
		}

		public void TestCodeDescription()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_ShortDescription = "Short description";

			AssertEquals("Code", "0000000000", CodePropertyAttribute.CodeFromBusinessObject(tariff));
			AssertEquals("Desc", "Short description", DescriptionPropertyAttribute.DescriptionFromBusinessObject(tariff));
		}

		public void TestFormattedTariff()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals("UE_FormattedTariff", "", tariff.UE_FormattedTariff);
			tariff.UE_Tariff = "1234567890";
			AssertEquals("UE_FormattedTariff", "1234.56.7890", tariff.UE_FormattedTariff);
		}

		public void TestOGARequirements()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_OGACodes = "AAABBBDDD";
			AssertEquals(3, tariff.OGARequirements.Count);
			AssertEquals("AAA", tariff.OGARequirements[0]);
			AssertEquals("BBB", tariff.OGARequirements[1]);
			AssertEquals("DDD", tariff.OGARequirements[2]);
		}

		public void TestRequiresLumberPermit()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.RequiresCanadianLumberPermit);
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber;
			AssertEquals(true, tariff.RequiresCanadianLumberPermit);
		}

		[TestDate(2007, 9, 11)]
		public void TestMayRequireADD()
		{
			USCTariff tariff = new USCTariff.Loader(Factory).LoadBestMatch("9303904000", ZDateTime.Today);
			AssertEquals(false, tariff.MayRequireADD);
			tariff = new USCTariff.Loader(Factory).LoadBestMatch("7304296015", ZDateTime.Today);
			AssertEquals(true, tariff.MayRequireADD);
		}

		[TestDate(2007, 9, 11)]
		public void TestMayRequireCVD()
		{
			USCTariff tariff = new USCTariff.Loader(Factory).LoadBestMatch("9303904000", ZDateTime.Today);
			AssertEquals(false, tariff.MayRequireCVD);
			tariff = new USCTariff.Loader(Factory).LoadBestMatch("6403591530", ZDateTime.Today);
			AssertEquals(true, tariff.MayRequireCVD);
		}

		public void TestAPHISRequirements()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.DoesRequireAPHIS);
			AssertEquals(false, tariff.MayRequireAPHIS);
			AssertEquals(false, tariff.HasAPHISRequirement);
			AssertEquals(false, tariff.MayRequireAPHISNoDisclaimRequired);
			tariff.UE_PGACodes = "   AQX";
			AssertEquals(false, tariff.DoesRequireAPHIS);
			AssertEquals(false, tariff.MayRequireAPHIS);
			AssertEquals(true, tariff.HasAPHISRequirement);
			AssertEquals(true, tariff.MayRequireAPHISNoDisclaimRequired);
			tariff.UE_PGACodes = "   AQ2";
			AssertEquals(true, tariff.DoesRequireAPHIS);
			AssertEquals(false, tariff.MayRequireAPHIS);
			AssertEquals(true, tariff.HasAPHISRequirement);
			AssertEquals(false, tariff.MayRequireAPHISNoDisclaimRequired);
			tariff.UE_PGACodes = "   AQ1";
			AssertEquals(false, tariff.DoesRequireAPHIS);
			AssertEquals(true, tariff.MayRequireAPHIS);
			AssertEquals(true, tariff.HasAPHISRequirement);
			AssertEquals(false, tariff.MayRequireAPHISNoDisclaimRequired);
		}

		public void TestFWSRequirements()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.DoesRequireFWS);
			AssertEquals(false, tariff.MayRequireFWS);
			AssertEquals(false, tariff.HasFWSRequirement);
			tariff.UE_PGACodes = "   FW2";
			AssertEquals(true, tariff.DoesRequireFWS);
			AssertEquals(false, tariff.MayRequireFWS);
			AssertEquals(true, tariff.HasFWSRequirement);
			tariff.UE_PGACodes = "   FW1";
			AssertEquals(false, tariff.DoesRequireFWS);
			AssertEquals(true, tariff.MayRequireFWS);
			AssertEquals(true, tariff.HasFWSRequirement);
			tariff.UE_PGACodes = "   FW3";
			AssertEquals(false, tariff.DoesRequireFWS);
			AssertEquals(true, tariff.MayRequireFWS);
			AssertEquals(true, tariff.HasFWSRequirement);
		}

		public void TestFDAAdmissibilityReviewDONOTSUBMIT()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.FDAAdmissibilityReviewDONOTSUBMIT);
			tariff.UE_OGACodes = "   FD0";
			AssertEquals(true, tariff.FDAAdmissibilityReviewDONOTSUBMIT);
		}

		public void TestFDAAdmissibilityReviewMayBeRequired()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.FDAAdmissibilityReviewMayBeRequired);
			tariff.UE_OGACodes = "   FD1";
			AssertEquals(true, tariff.FDAAdmissibilityReviewMayBeRequired);
		}

		public void TestFDAPriorNoticeAndAdmissibilityReviewMayBeRequired()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.FDAPriorNoticeAndAdmissibilityReviewMayBeRequired);
			tariff.UE_OGACodes = "   FD3";
			AssertEquals(true, tariff.FDAPriorNoticeAndAdmissibilityReviewMayBeRequired);
		}

		public void TestFDAAdmissibilityReviewRequired()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.FDAAdmissibilityReviewRequired);
			tariff.UE_OGACodes = "   FD2";
			AssertEquals(true, tariff.FDAAdmissibilityReviewRequired);
		}

		public void TestFDAPriorNoticeAndAdmissibilityReviewRequired()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.FDAPriorNoticeAndAdmissibilityReviewRequired);
			tariff.UE_OGACodes = "   FD4";
			AssertEquals(true, tariff.FDAPriorNoticeAndAdmissibilityReviewRequired);
		}

		public void TestFDARequired()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.HasFDARequirement);

			tariff.UE_OGACodes = "   FD1";
			AssertEquals(true, tariff.HasFDARequirement);

			tariff.UE_OGACodes = "   FD2";
			AssertEquals(true, tariff.HasFDARequirement);

			tariff.UE_OGACodes = "   FD3";
			AssertEquals(true, tariff.HasFDARequirement);

			tariff.UE_OGACodes = "   FD4";
			AssertEquals(true, tariff.HasFDARequirement);

			tariff.UE_OGACodes = "   FD0";
			AssertEquals(false, tariff.HasFDARequirement);
		}

		public void TestMayRequireDOT()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.MayRequireDOT);
			tariff.UE_OGACodes = "   DT1";
			AssertEquals(true, tariff.MayRequireDOT);
		}

		public void TestDoesRequireDOT()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.DoesRequireDOT);
			tariff.UE_OGACodes = "   DT2";
			AssertEquals(true, tariff.DoesRequireDOT);
		}

		public void TestIsGSPExcluded()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.IsGSPExcluded("AU"));
			tariff.UE_GSPExcludedCountries = "NAUS";
			AssertEquals(false, tariff.IsGSPExcluded("AU"));
			tariff.UE_GSPExcludedCountries = "NAUSAU";
			AssertEquals(true, tariff.IsGSPExcluded("AU"));
		}

		public void TestHasSPIForN()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "99090410";//JO FTA
			tariff.UE_SPICode = "JO";
			tariff.UE_ISOCountryofOriginEditCode = "JO";
			AssertEquals("N is invalid for 99090410", false, tariff.HasSpecialProgramsIndicator(PrimarySpecProgramIndicatorList.Codes.N));
		}

		internal static USCTariff CreateUSCTariffWithPermitLicenseIndicator(BusinessObjectFactory factory, ZString tariffNumber, ZString permitLicenseIndicator, ZDateTime effectiveDate)
		{
			USCTariff uscTariff = new USCTariff.Loader(factory).LoadBestMatch(tariffNumber, effectiveDate);
			if (uscTariff == null)
			{
				uscTariff = factory.New<USCTariff>();
				uscTariff.UE_Tariff = tariffNumber;
				uscTariff.UE_PermitLicenseIndicator = permitLicenseIndicator;
				uscTariff.UE_DateFrom = effectiveDate.AddYears(-1);
				uscTariff.UE_DateTo = effectiveDate.AddYears(1);
			}
			return uscTariff;
		}

		public void TestMiscLicenseTypeLabel()
		{
			var today = ZDateTime.Today;
			var uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "2922292700", ZString.Empty, today);
			AssertEquals("Misc. License No.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "7306191010", MiscellaneousPermitLicenseList.Codes.SteelImportLicense, today);
			AssertEquals("Steel License No.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "99106145", MiscellaneousPermitLicenseList.Codes.SingaporeTPLCertificate, today);
			AssertEquals("SG TPL License No.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "99990056", MiscellaneousPermitLicenseList.Codes.CANAFTATPLCertificate, today);
			AssertEquals("CA NAFTA Cert. No.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "99990060", MiscellaneousPermitLicenseList.Codes.MXNAFTATPLCertificate, today);
			AssertEquals("MX NAFTA Cert. No.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "0201101090", MiscellaneousPermitLicenseList.Codes.BeefExportCertificate, today);
			AssertEquals("Beef Certificate No.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "7102213000", MiscellaneousPermitLicenseList.Codes.DiamondCertificate, today);
			AssertEquals("Diamond Cert. No.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "4407100119", MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber, today);
			AssertEquals("Lumber Permit No.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "98211119", MiscellaneousPermitLicenseList.Codes.ATPDEACertificateHTS98211119, today);
			AssertEquals("ATPDEA Cert No.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "99130465", MiscellaneousPermitLicenseList.Codes.AustraliaFreeTradeExportCertificate, today);
			AssertEquals("AU FT Export Cert.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "2523290000", MiscellaneousPermitLicenseList.Codes.MexicanCementImportLicense, today);
			AssertEquals("MX Cement Imp. Lic.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "99156101", MiscellaneousPermitLicenseList.Codes.CAFTATPLCertificate, today);
			AssertEquals("NI CAFTA TPL Cert.:", uscTariff.MiscLicenseTypeLabel);

			uscTariff = CreateUSCTariffWithPermitLicenseIndicator(Factory, "99025211", MiscellaneousPermitLicenseList.Codes.CottonShirtingFabricLicenseNumber, today);
			AssertEquals("Cotton Shirting Lic.:", uscTariff.MiscLicenseTypeLabel);
		}

		public void TestNoCategoryNumberToBeEntered()
		{
			var cBTPATextileBenefits = Factory.New<USCTariff>();
			cBTPATextileBenefits.UE_Tariff = "9802008046";
			cBTPATextileBenefits.UE_DateFrom = ZDateTime.Today;
			cBTPATextileBenefits.UE_DateTo = ZDateTime.Today.AddYears(1);
			AssertEquals(true, cBTPATextileBenefits.NoCategoryNumberToBeEntered(ZDate.Today));

			var tariff9820 = Factory.New<USCTariff>();
			tariff9820.UE_Tariff = "98201109";
			tariff9820.UE_DateFrom = ZDateTime.Today;
			tariff9820.UE_DateTo = ZDateTime.Today.AddYears(1);
			AssertEquals(true, tariff9820.NoCategoryNumberToBeEntered(ZDate.Today));

			var tariff98201130 = Factory.New<USCTariff>();
			tariff98201130.UE_Tariff = "98201130";
			tariff98201130.UE_DateFrom = ZDateTime.Today;
			tariff98201130.UE_DateTo = ZDateTime.Today.AddYears(1);
			AssertEquals(true, tariff98201130.NoCategoryNumberToBeEntered(ZDate.Today));

			var tariff98206125 = Factory.New<USCTariff>();
			tariff98206125.UE_Tariff = "98206125";
			tariff98206125.UE_DateFrom = ZDateTime.Today;
			tariff98206125.UE_DateTo = ZDateTime.Today.AddYears(1);
			AssertEquals("Tariffs under 98206 require category no.", false, tariff98206125.NoCategoryNumberToBeEntered(ZDate.Today));

			var tariff98206230 = Factory.New<USCTariff>();
			tariff98206230.UE_Tariff = "98206230";
			tariff98206230.UE_DateFrom = ZDateTime.Today;
			tariff98206230.UE_DateTo = ZDateTime.Today.AddYears(1);
			AssertEquals("Tariffs under 98206 require category no.", false, tariff98206230.NoCategoryNumberToBeEntered(ZDate.Today));
		}

		public void TestDutyMightBeOverridable()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000110";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			AssertEquals(true, tariff.DutyMightBeOverridable(ZDateTime.Today));

			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_RuleCode = TariffRuleList.Codes.RepairTariffs;
			tariffRule.U1_Tariff = "0000000110";
			tariff.TariffRules.Add(tariffRule);
			AssertEquals(false, tariff.DutyMightBeOverridable(ZDateTime.Today));
		}

		public void TestHasHTSValueRestriction()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5407820090";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			AssertEquals(false, tariff.HasHTSValueRestriction);

			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "115";
			AssertEquals(true, tariff.HasHTSValueRestriction);
		}

		public void TestDelete()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5407820090";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			USCTariffDutyRate rate1 = tariff.DutyRates.AddNew();
			rate1.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Honey;
			rate1.UD_TaxFeeFlag = "1";

			USCTariffDutyRate rate2 = tariff.DutyRates.AddNew();
			rate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Sugar;
			rate2.UD_TaxFeeFlag = "2";

			USCTariffDateRestriction restriction = tariff.TariffDateRestrictions.AddNew();

			USCTariffValue value = Factory.New<USCTariffValue>();
			value.UA_UE = tariff.PK;

			USCTariffQuantity qty = Factory.New<USCTariffQuantity>();
			qty.UQ_UE = tariff.PK;

			ZGuid tariffPK = tariff.PK;
			tariff.Delete();

			AssertNull(Factory.LoadTop1<USCTariffValue>(new ZQuery(USCTariffValueSchema.UA_UE, tariffPK)));
			AssertNull(Factory.LoadTop1<USCTariffQuantity>(new ZQuery(USCTariffQuantitySchema.UQ_UE, tariffPK)));

			AssertEquals(0, Factory.Load<USCTariffDutyRate>(new ZQuery(USCTariffDutyRateSchema.UD_UE, tariffPK)).Length);
			AssertEquals(0, Factory.Load<USCTariffDateRestriction>(new ZQuery(USCTariffDateRestrictionSchema.UF_UE, tariffPK)).Length);
		}

		public void TestPGARequirements()
		{
			var tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.DoesRequireFSIS);
			AssertEquals(false, tariff.DoesRequireODS);
			AssertEquals(false, tariff.DoesRequireVNE);
			AssertEquals(false, tariff.DoesRequireDOT);
			AssertEquals(false, tariff.DoesRequirePST);
			AssertEquals(false, tariff.DoesRequireNMFSAMR);
			AssertEquals(false, tariff.DoesRequireNMFSHMS);
			AssertEquals(false, tariff.DoesRequireNMFSHMS);
			AssertEquals(false, tariff.DoesRequireNMFS370);
			AssertEquals(false, tariff.MayRequireNMFSAMR);
			AssertEquals(false, tariff.DoesRequireNMFSSIM);
			AssertEquals(false, tariff.MayRequireNMFS370);
			AssertEquals(false, tariff.FDAAdmissibilityReviewRequired);
			AssertEquals(false, tariff.FDAPriorNoticeAndAdmissibilityReviewRequired);
			AssertEquals(false, tariff.FDAAdmissibilityReviewMayBeRequired);
			AssertEquals(false, tariff.HasTTBRequirement);
			AssertEquals(false, tariff.HasTTBRequirement);
			AssertEquals(false, tariff.DoesRequireHFC);
			AssertEquals(false, tariff.MayRequireHFC);
			AssertEquals(false, tariff.HasHFCRequirement);

			tariff.UE_PGACodes = "   EP2EP4FS4EP6FD2FD4";
			AssertEquals(true, tariff.DoesRequireFSIS);
			AssertEquals(true, tariff.DoesRequireODS);
			AssertEquals(true, tariff.DoesRequireVNE);
			AssertEquals(false, tariff.DoesRequireDOT);
			AssertEquals(true, tariff.DoesRequirePST);
			AssertEquals(false, tariff.DoesRequireNMFSAMR);
			AssertEquals(false, tariff.DoesRequireNMFSHMS);
			AssertEquals(false, tariff.DoesRequireNMFSSIM);
			AssertEquals(false, tariff.DoesRequireNMFS370);
			AssertEquals(false, tariff.MayRequireNMFSAMR);
			AssertEquals(false, tariff.MayRequireNMFSHMS);
			AssertEquals(false, tariff.MayRequireNMFS370);
			AssertEquals(true, tariff.ACEFDAAdmissibilityReviewRequired);
			AssertEquals(true, tariff.ACEFDAPriorNoticeAndAdmissibilityReviewRequired);
			AssertEquals(false, tariff.HasTTBRequirement);

			tariff.UE_PGACodes = "   NM1";
			AssertEquals(false, tariff.DoesRequireNMFSAMR);
			AssertEquals(false, tariff.DoesRequireNMFSHMS);
			AssertEquals(false, tariff.DoesRequireNMFSSIM);
			AssertEquals(false, tariff.DoesRequireNMFS370);
			AssertEquals(false, tariff.MayRequireNMFSAMR);
			AssertEquals(false, tariff.MayRequireNMFSHMS);
			AssertEquals(true, tariff.MayRequireNMFS370);

			tariff.UE_PGACodes = "   NM2";
			AssertEquals(false, tariff.DoesRequireNMFSAMR);
			AssertEquals(false, tariff.DoesRequireNMFSHMS);
			AssertEquals(false, tariff.DoesRequireNMFSSIM);
			AssertEquals(true, tariff.DoesRequireNMFS370);
			AssertEquals(false, tariff.MayRequireNMFSAMR);
			AssertEquals(false, tariff.MayRequireNMFSHMS);
			AssertEquals(false, tariff.MayRequireNMFS370);

			tariff.UE_PGACodes = "   NM3";
			AssertEquals(false, tariff.DoesRequireNMFSAMR);
			AssertEquals(false, tariff.DoesRequireNMFSHMS);
			AssertEquals(false, tariff.DoesRequireNMFSSIM);
			AssertEquals(false, tariff.DoesRequireNMFS370);
			AssertEquals(true, tariff.MayRequireNMFSAMR);
			AssertEquals(false, tariff.MayRequireNMFSHMS);
			AssertEquals(false, tariff.MayRequireNMFS370);

			tariff.UE_PGACodes = "   NM4";
			AssertEquals(true, tariff.DoesRequireNMFSAMR);
			AssertEquals(false, tariff.DoesRequireNMFSHMS);
			AssertEquals(false, tariff.DoesRequireNMFSSIM);
			AssertEquals(false, tariff.DoesRequireNMFSSIM);
			AssertEquals(false, tariff.DoesRequireNMFS370);
			AssertEquals(false, tariff.MayRequireNMFSAMR);
			AssertEquals(false, tariff.MayRequireNMFSHMS);
			AssertEquals(false, tariff.MayRequireNMFS370);

			tariff.UE_PGACodes = "   NM5";
			AssertEquals(false, tariff.DoesRequireNMFSAMR);
			AssertEquals(false, tariff.DoesRequireNMFSHMS);
			AssertEquals(false, tariff.DoesRequireNMFSSIM);
			AssertEquals(false, tariff.DoesRequireNMFS370);
			AssertEquals(false, tariff.MayRequireNMFSAMR);
			AssertEquals(true, tariff.MayRequireNMFSHMS);
			AssertEquals(false, tariff.MayRequireNMFS370);

			tariff.UE_PGACodes = "   NM6";
			AssertEquals(false, tariff.DoesRequireNMFSAMR);
			AssertEquals(true, tariff.DoesRequireNMFSHMS);
			AssertEquals(false, tariff.DoesRequireNMFSSIM);
			AssertEquals(false, tariff.DoesRequireNMFS370);
			AssertEquals(false, tariff.MayRequireNMFSAMR);
			AssertEquals(false, tariff.MayRequireNMFSHMS);
			AssertEquals(false, tariff.MayRequireNMFS370);
			tariff.UE_PGACodes = "   EP2EP4FS4EP6FD2FD1";
			AssertEquals(true, tariff.ACEFDAAdmissibilityReviewMayBeRequired);
			AssertEquals(false, tariff.HasTTBRequirement);

			tariff.UE_PGACodes = "   NM8";
			AssertEquals(false, tariff.DoesRequireNMFSAMR);
			AssertEquals(false, tariff.DoesRequireNMFSHMS);
			AssertEquals(true, tariff.DoesRequireNMFSSIM);
			AssertEquals(false, tariff.DoesRequireNMFS370);
			AssertEquals(false, tariff.MayRequireNMFSAMR);
			AssertEquals(false, tariff.MayRequireNMFSHMS);
			AssertEquals(false, tariff.MayRequireNMFS370);
			tariff.UE_PGACodes = "   TB2";
			AssertEquals(true, tariff.HasTTBRequirement);

			tariff.UE_PGACodes = "   TB1";
			AssertEquals(true, tariff.HasTTBRequirement);

			tariff.UE_PGACodes = "   EH2";
			AssertEquals(true, tariff.DoesRequireHFC);
			AssertEquals(false, tariff.MayRequireHFC);
			AssertEquals(true, tariff.HasHFCRequirement);

			tariff.UE_PGACodes = "   EH1";
			AssertEquals(false, tariff.DoesRequireHFC);
			AssertEquals(true, tariff.MayRequireHFC);
			AssertEquals(true, tariff.HasHFCRequirement);
		}

		public void TestLaceyActRequirements()
		{
			var tariff = Factory.New<USCTariff>();
			AssertEquals(false, tariff.DoesRequireLaceyAct);
			AssertEquals(false, tariff.MayRequireLaceyAct);
			AssertEquals(false, tariff.HasLaceyActRequirement);

			tariff.UE_PGACodes = "   AL1";
			AssertEquals(false, tariff.DoesRequireLaceyAct);
			AssertEquals(true, tariff.MayRequireLaceyAct);
			AssertEquals(true, tariff.HasLaceyActRequirement);

			tariff.UE_PGACodes = "   AL2";
			AssertEquals(false, tariff.DoesRequireLaceyAct);
			AssertEquals(true, tariff.MayRequireLaceyAct);
			AssertEquals(true, tariff.HasLaceyActRequirement);
		}

		public void TestDEATariff()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_OGACodes = "";
			AssertEquals(false, tariff.MayRequireDEA);
			AssertEquals(false, tariff.HasDEARequirement);

			tariff.UE_PGACodes = "DE1";
			AssertEquals(true, tariff.MayRequireDEA);
			AssertEquals(true, tariff.HasDEARequirement);
		}

		public void TestIsProvDutyAlwaysRequired()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Tariff = "99038001";
			AssertEquals(true, tariff.IsProvDutyAlwaysRequired);
			tariff.UE_Tariff = "99038501";
			AssertEquals(true, tariff.IsProvDutyAlwaysRequired);
			tariff.UE_Tariff = "99038801";
			AssertEquals(true, tariff.IsProvDutyAlwaysRequired);
			tariff.UE_Tariff = "98020010";
			AssertEquals(false, tariff.IsProvDutyAlwaysRequired);
			tariff.UE_Tariff = "99038801";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			AssertEquals(false, tariff.IsProvDutyAlwaysRequired);
		}

		public void TestRegulatedPeriodForTariff()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0805100040";
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff(ZString.Empty, ZDateTime.Empty));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO1", ZDateTime.Empty));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff(ZString.Empty, new ZDateTime(2018, 07, 01)));
			AssertEquals("1/Sep through 30/Jun", tariff.RegulatedPeriodForTariff("MO1", new ZDateTime(2018, 07, 01)));
			AssertEquals("1/Sep through 30/Jun", tariff.RegulatedPeriodForTariff("MO6", new ZDateTime(2018, 08, 31)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO1", new ZDateTime(2018, 06, 30)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO6", new ZDateTime(2018, 09, 01)));
			AssertEquals("1/Sep through 30/Jun", tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 06, 30)));
			AssertEquals("1/Sep through 30/Jun", tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 09, 01)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 07, 01)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 08, 31)));

			tariff.UE_Tariff = "0806104000";
			AssertEquals("10/Apr through 10/Jul", tariff.RegulatedPeriodForTariff("MO1", new ZDateTime(2018, 04, 9)));
			AssertEquals("10/Apr through 10/Jul", tariff.RegulatedPeriodForTariff("MO6", new ZDateTime(2018, 07, 11)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO1", new ZDateTime(2018, 04, 10)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO6", new ZDateTime(2018, 07, 10)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 04, 9)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 07, 11)));
			AssertEquals("10/Apr through 10/Jul", tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 04, 10)));
			AssertEquals("10/Apr through 10/Jul", tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 07, 10)));

			tariff.UE_Tariff = "0702002099";
			AssertEquals("10/Oct through 15/Jun", tariff.RegulatedPeriodForTariff("MO1", new ZDateTime(2018, 06, 16)));
			AssertEquals("10/Oct through 15/Jun", tariff.RegulatedPeriodForTariff("MO6", new ZDateTime(2018, 10, 09)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO1", new ZDateTime(2018, 06, 15)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO6", new ZDateTime(2018, 10, 10)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 06, 16)));
			AssertEquals(ZString.Empty, tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 10, 09)));
			AssertEquals("10/Oct through 15/Jun", tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 06, 15)));
			AssertEquals("10/Oct through 15/Jun", tariff.RegulatedPeriodForTariff("MO7", new ZDateTime(2018, 10, 10)));
		}

		[TestDate(2020, 03, 03)]
		public void TestIsEmbroideryTariff()
		{
			var tariff = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "5810929080")).LastOrDefault();
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "5810929080";
			}
			tariff.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff.UE_DateTo = new ZDateTime(2021, 01, 01);
			AssertEquals(false, tariff.IsEmbroideryTariff(ZDateTime.Today));

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "5810929080", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffAttribute("RULE", "EMB", zzTariff);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedTariff = newFactory.Load<USCTariff>(tariff.PK);
			AssertEquals(true, loadedTariff.IsEmbroideryTariff(ZDateTime.Today));
		}

		[TestDate(2020, 05, 11)]
		public void TestLoadCachedBestMatch()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1100010001";
			tariff1.UE_ShortDescription = "TEST TARIFF 1";
			tariff1.UE_DateFrom = new ZDateTime(2019, 07, 01);
			tariff1.UE_DateTo = new ZDateTime(2020, 06, 30);
			Factory.Save();

			var loader = new USCTariff.Loader(Factory);
			var result = loader.LoadCachedBestMatch(tariff1.UE_Tariff, ZDateTime.Today);
			AssertNotNull(result);
			AssertEquals(tariff1.UE_ShortDescription, result.UE_ShortDescription);

			tariff1.UE_ShortDescription = "TEST TARIFF 2";
			Factory.Save();
			result = loader.LoadCachedBestMatch(tariff1.UE_Tariff, ZDateTime.Today);
			AssertNotNull(result);
			AssertEquals(tariff1.UE_ShortDescription, result.UE_ShortDescription);
		}

		[TestDate(2019, 05, 11)]
		public void TestLoadBestMatchOrMostRecent()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1100010001";
			tariff1.UE_ShortDescription = "TEST TARIFF 1";
			tariff1.UE_DateFrom = new ZDateTime(2019, 07, 01);
			tariff1.UE_DateTo = new ZDateTime(2020, 06, 30);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1100010001";
			tariff2.UE_ShortDescription = "TEST TARIFF 2";
			tariff2.UE_DateFrom = new ZDateTime(2018, 07, 01);
			tariff2.UE_DateTo = new ZDateTime(2019, 06, 30);
			Factory.Save();

			var loader = new USCTariff.Loader(Factory);
			var result = loader.LoadBestMatchOrMostRecent(tariff1.UE_Tariff, ZDateTime.Today);
			AssertNotNull(result);
			AssertEquals(tariff2.UE_ShortDescription, result.UE_ShortDescription);

			result = loader.LoadBestMatchOrMostRecent(tariff1.UE_Tariff, new ZDateTime(2020, 07, 01));
			AssertNotNull(result);
			AssertEquals(tariff1.UE_ShortDescription, result.UE_ShortDescription);

			tariff1.UE_ShortDescription = "TEST TARIFF 3";
			Factory.Save();

			result = loader.LoadBestMatchOrMostRecent(tariff1.UE_Tariff, new ZDateTime(2020, 07, 01));
			AssertNotNull(result);
			AssertEquals(tariff1.UE_ShortDescription, result.UE_ShortDescription);
		}

		[TestDate(2007, 07, 01)]
		public void TestDoesRequireNMFSCOA()
		{
			var country = Core.Constants.CountryCodes.UnitedStates;

			var tariff = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0111101000")).LastOrDefault();
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "0111101000";
			}
			tariff.UE_DateFrom = new ZDateTime(2007, 1, 1);
			tariff.UE_DateTo = ZDateTime.Today;

			AssertEquals(false, tariff.DoesRequireNMFSCOA(country, ZDateTime.Today));

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(country, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(country, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.PGA);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(country, TariffConditionValueTypes.Codes.PGA);
			Factory.Save();
			var zzTariff = helper.CreateTariff(country, tariffType.PK, "0111101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(country, conditionType.PK, zzTariff.PK, "test", true, false, new ZDateTime(2007, 1, 1), ZDateTime.Today);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, GovernmentAgencyProgramCodeList.Codes.COA);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedTariff = newFactory.Load<USCTariff>(tariff.PK);
			AssertEquals(true, loadedTariff.DoesRequireNMFSCOA(country, ZDateTime.Today));
		}
	}
}
