using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USRefTariffDataLoaderTest : TestCaseWithFactory
	{
		public void TestGetSupTariffsWithNotApplicable()
		{
			PrepareData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
			invoiceLine.JI_Tariff = "1111111111";
			var supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(2, supTariffs.Length);
			AssertEquals("Sup tariff list includes N/A.", true, supTariffs.FirstOrDefault(x => x.Tariff == TariffViewAsCodeDescription.NotApplicableCode) != null);

			invoiceLine.JI_Tariff = "2222222222";
			supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(3, supTariffs.Length);
			AssertEquals("Sup tariff list includes N/A.", true, supTariffs.FirstOrDefault(x => x.Tariff == TariffViewAsCodeDescription.NotApplicableCode) != null);

			invoiceLine.JI_Tariff = "3333333333";
			supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(1, supTariffs.Length);
			AssertEquals("Sup tariff list does not include N/A.", false, supTariffs.FirstOrDefault(x => x.Tariff == TariffViewAsCodeDescription.NotApplicableCode) != null);
		}

		public void TestGetSupTariffsWithisSteelOriginFromEU()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var tariffNum2 = "7811111111";
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNum2;
			tariff.UE_DateFrom = startDate;
			tariff.UE_DateTo = endDate;
			tariff.UE_QuotaIndicator = true;

			var tariffNum = "7255333377";
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = tariffNum;
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tradeGroupDE = helper.LoadOrCreateTradeGroup(dataGrouping.ZZZ_DataGrouping, Core.Constants.CountryCodes.Germany);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, tariffNum, startDate, endDate);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, tariffNum2, startDate, endDate);
			var attribute1 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff1);
			var attribute2 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff2);

			var child01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "4444444444", startDate, endDate);
			var attribute11 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, child01);
			var child02 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "5555555555", startDate, endDate);
			var attribute21 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, child02);
			var relation12 = helper.CreateTariffRelationship(child01.PK, tariffType.PK, tariff1.ZZ1_TariffCode);
			var relation22 = helper.CreateTariffRelationship(child02.PK, tariffType.PK, tariff1.ZZ1_TariffCode);

			var childDE1 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "66444444444", startDate, endDate);
			var attributeDE1 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, childDE1);
			var attributeDE12 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, childDE1);
			var childDE2 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "77555555555", startDate, endDate);
			var attributeDE2 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, childDE2);
			var attributeDE22 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, childDE2);
			var relationDE1 = helper.CreateTariffRelationship(childDE1.PK, tariffType.PK, tariff2.ZZ1_TariffCode);
			var relationDE2 = helper.CreateTariffRelationship(childDE2.PK, tariffType.PK, tariff2.ZZ1_TariffCode);

			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.Duty, dutyRateType.PK);
			var rate3 = helper.CreateRate(child01, rateCode.PK, startDate, endDate, "0");
			var rate4 = helper.CreateRate(child02, rateCode.PK, startDate, endDate, "0");

			var rateDE3 = helper.CreateRate(childDE1, rateCode.PK, startDate, endDate, "0");
			var rateDE4 = helper.CreateRate(childDE2, rateCode.PK, startDate, endDate, "0");

			helper.AddCountry(tradeGroupDE, Core.Constants.CountryCodes.Germany, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(rateDE3, tradeGroupDE, startDate, endDate);
			helper.CreateCusApplicability(rateDE4, tradeGroupDE, startDate, endDate);

			var tradeGroupHK = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			helper.AddCountry(tradeGroupHK, Core.Constants.CountryCodes.HongKong, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(rate3, tradeGroupHK, startDate, endDate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_Tariff = "7255333377";
			var supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(0, supTariffs.Length);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(1, supTariffs.Length);

			invoiceLine.JI_Tariff = "7811111111";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(2, supTariffs.Length);
		}

		public void TestGetSupTariffsForCA()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var tariffNum2 = "99038181";
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNum2;
			tariff.UE_DateFrom = startDate;
			tariff.UE_DateTo = endDate;
			tariff.UE_QuotaIndicator = true;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tradeGroupCA = helper.LoadOrCreateTradeGroup(dataGrouping.ZZZ_DataGrouping, Core.Constants.CountryCodes.Canada);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, tariffNum2, startDate, endDate);
			var attribute2 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff2);

			var child01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "4444444444", startDate, endDate);
			var attribute11 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, child01);
			var child02 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "5555555555", startDate, endDate);
			var attribute21 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, child02);

			var childCA = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "66444444444", startDate, endDate);
			var attributeCA1 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, childCA);
			var attributeCA12 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, childCA);
			var childCA2 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "77555555555", startDate, endDate);
			var attributeCA2 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, childCA2);
			var attributeCA22 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, childCA2);
			var relationCA1 = helper.CreateTariffRelationship(childCA.PK, tariffType.PK, tariff2.ZZ1_TariffCode);
			var relationCA2 = helper.CreateTariffRelationship(childCA2.PK, tariffType.PK, tariff2.ZZ1_TariffCode);

			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.Duty, dutyRateType.PK);
			var rate3 = helper.CreateRate(child01, rateCode.PK, startDate, endDate, "0");
			var rate4 = helper.CreateRate(child02, rateCode.PK, startDate, endDate, "0");

			var rateCA3 = helper.CreateRate(childCA, rateCode.PK, startDate, endDate, "0");
			var rateCA4 = helper.CreateRate(childCA2, rateCode.PK, startDate, endDate, "0");

			helper.AddCountry(tradeGroupCA, Core.Constants.CountryCodes.Canada, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(rateCA3, tradeGroupCA, startDate, endDate);
			helper.CreateCusApplicability(rateCA4, tradeGroupCA, startDate, endDate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "99038181";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Canada;
			var supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(2, supTariffs.Length);

			foreach (var code in new CanadaProvinceTerritoryCodes().GetAllCodes())
			{
				invoiceLine.US_UC_NKCountryOfOrigin = code;
				supTariffs = invoiceLine.ApplicableSupTariffs;
				AssertEquals($"GetSupTariffs For CA Territory: {code}", 2, supTariffs.Length);
			}
		}

		void PrepareData()
		{
			var startDate = ZDateTime.Now.AddMonths(-1);
			var endDate = ZDateTime.Now.AddMonths(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping.ZZZ_DataGrouping, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tradeGroup = helper.LoadOrCreateTradeGroup(dataGrouping.ZZZ_DataGrouping, Core.Constants.CountryCodes.Russia);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia, startDate.Date, endDate.Date);
			var rateType = helper.CreateNewOrGetExistingRateType(dataGrouping.ZZZ_DataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RateCodes.Codes.Duty, rateType.PK);

			var parent01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "1111111111", startDate, endDate);
			var parent02 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "2222222222", startDate, endDate);
			var parent03 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "3333333333", startDate, endDate);

			var child01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "4444444444", startDate, endDate);
			var child02 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "5555555555", startDate, endDate);
			var child03 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "6666666666", startDate, endDate);

			var attribute11 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.RussianTariffs, child01);
			var attribute12 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula, child01);
			var attribute21 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.RussianTariffs, child02);
			var attribute32 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula, child03);

			var rate01 = helper.CreateRefCusRate(child01.PK, rateCode.PK, startDate, endDate);
			var rate02 = helper.CreateRefCusRate(child02.PK, rateCode.PK, startDate, endDate);
			var rate03 = helper.CreateRefCusRate(child03.PK, rateCode.PK, startDate, endDate);

			var applicability01 = helper.CreateCusApplicability(rate01.PK, tradeGroup, startDate, endDate);
			var applicability02 = helper.CreateCusApplicability(rate02.PK, tradeGroup, startDate, endDate);
			var applicability03 = helper.CreateCusApplicability(rate03.PK, tradeGroup, startDate, endDate);

			var relation11 = helper.CreateTariffRelationship(child01.PK, tariffType.PK, parent01.ZZ1_TariffCode);
			var relation12 = helper.CreateTariffRelationship(child01.PK, tariffType.PK, parent02.ZZ1_TariffCode);
			var relation22 = helper.CreateTariffRelationship(child02.PK, tariffType.PK, parent02.ZZ1_TariffCode);
			var relation23 = helper.CreateTariffRelationship(child02.PK, tariffType.PK, parent03.ZZ1_TariffCode);
			var relation33 = helper.CreateTariffRelationship(child03.PK, tariffType.PK, parent03.ZZ1_TariffCode);
			var relation31 = helper.CreateTariffRelationship(child03.PK, tariffType.PK, parent01.ZZ1_TariffCode);

			Factory.Save();

			var uscTariff01 = Factory.New<USCTariff>();
			uscTariff01.UE_Tariff = parent01.ZZ1_TariffCode;
			uscTariff01.UE_DateFrom = parent01.ZZ1_StartDate;
			uscTariff01.UE_DateTo = parent01.ZZ1_EndDate;

			var uscTariff02 = Factory.New<USCTariff>();
			uscTariff02.UE_Tariff = parent02.ZZ1_TariffCode;
			uscTariff02.UE_DateFrom = parent02.ZZ1_StartDate;
			uscTariff02.UE_DateTo = parent02.ZZ1_EndDate;

			var uscTariff03 = Factory.New<USCTariff>();
			uscTariff03.UE_Tariff = parent03.ZZ1_TariffCode;
			uscTariff03.UE_DateFrom = parent03.ZZ1_StartDate;
			uscTariff03.UE_DateTo = parent03.ZZ1_EndDate;

			Factory.Save();
		}
	}
}
