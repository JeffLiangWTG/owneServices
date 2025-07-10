using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	public static class TariffDataForTestHelper
	{
		public static void NewData(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var cusProcedurePR = helper.CreateOrFindExistingRefCusProcedure("TW", "IM", "PR", "", "", "", "IMP");
			cusProcedurePR.Attributes.AddNew("DTYPaymentMethod", "DEF");
			cusProcedurePR.Attributes.AddNew("VATPaymentMethod", "DEF");
			cusProcedurePR.Attributes.AddNew("TPFPaymentMethod", "DEF");
			cusProcedurePR.Attributes.AddNew("COMPaymentMethod", "DEF");
			cusProcedurePR.Attributes.AddNew("TATPaymentMethod", "DEF");

			var cusProcedureTT = helper.CreateOrFindExistingRefCusProcedure("TW", "IM", "TT", "", "", "", "IMP");
			cusProcedureTT.Attributes.AddNew("DTYPaymentMethod", "CAS");
			cusProcedureTT.Attributes.AddNew("VATPaymentMethod", "CAS");
			cusProcedureTT.Attributes.AddNew("TPFPaymentMethod", "CAS");
			cusProcedureTT.Attributes.AddNew("COMPaymentMethod", "CAS");
			cusProcedureTT.Attributes.AddNew("SSGPaymentMethod", "CAS");

			var cusProcedureKK = helper.CreateOrFindExistingRefCusProcedure("TW", "IM", "KK", "", "", "", "IMP");
			cusProcedureKK.Attributes.AddNew("DTYPaymentMethod", "DEF");
			cusProcedureKK.Attributes.AddNew("VATPaymentMethod", "DEF");
			cusProcedureKK.Attributes.AddNew("TPFPaymentMethod", "DEF");
			cusProcedureKK.Attributes.AddNew("COMPaymentMethod", "DEF");
			cusProcedureKK.Attributes.AddNew("SSGPaymentMethod", "DEF");
			cusProcedureKK.Attributes.AddNew("IsReExportation", "Y");

			var cusProcedureQQ = helper.CreateOrFindExistingRefCusProcedure("TW", "IM", "QQ", "", "", "", "IMP");
			cusProcedureQQ.Attributes.AddNew("DTYPaymentMethod", "DEF");
			cusProcedureQQ.Attributes.AddNew("VATPaymentMethod", "DEF");
			cusProcedureQQ.Attributes.AddNew("COMPaymentMethod", "DEF");
			cusProcedureQQ.Attributes.AddNew("TATPaymentMethod", "DEF");

			var expProcedureNoDuty = helper.CreateOrFindExistingRefCusProcedure("TW", "EX", "AA", "", "", "", "EXP");
			expProcedureNoDuty.Attributes.AddNew("VATPaymentMethod", "CAS");

			var expProcedureDuty = helper.CreateOrFindExistingRefCusProcedure("TW", "EX", "AB", "", "", "", "EXP");
			expProcedureDuty.Attributes.AddNew("VATPaymentMethod", "CAS");

			var impProcedureNoDutyNoVAT = helper.CreateOrFindExistingRefCusProcedure("TW", "IM", "BB", "", "", "", "IMP");
			impProcedureNoDutyNoVAT.ZZ6_CalculateVAT = false;
			factory.Save();

			var rateType = helper.CreateNewOrGetExistingRateType("TW", "DTY");
			rateType.ZZR_CustomsValueFormula = "CV";
			var comRateType = helper.CreateNewOrGetExistingRateType("TW", "COM");
			comRateType.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD";
			var ssgRateType = helper.CreateNewOrGetExistingRateType("TW", "SSG");
			ssgRateType.ZZR_CustomsValueFormula = "CV + DTA + DTS + VAT + CTA + CTS + TAT + HWS + ADD + CVD + ADT + RTD";
			factory.Save();
			var rateCodeDTA = helper.LoadOrCreateNewCusRateCode(factory, "DTA", rateType.PK);
			var rateCodeDTS = helper.LoadOrCreateNewCusRateCode(factory, "DTS", rateType.PK);
			var rateCodeCTA = helper.LoadOrCreateNewCusRateCode(factory, "CTA", comRateType.PK);
			var rateCodeCTS = helper.LoadOrCreateNewCusRateCode(factory, "CTS", comRateType.PK);
			var rateCodeTAT = helper.LoadOrCreateNewCusRateCode(factory, "TAT", comRateType.PK);
			var rateCodeHWS = helper.LoadOrCreateNewCusRateCode(factory, "HWS", comRateType.PK);
			var rateCodeSSG = helper.LoadOrCreateNewCusRateCode(factory, "SSG", ssgRateType.PK);

			var stdPreference = helper.CreatePreferenceForCountry("STD", "STD", "TW");
			var prePreference = helper.CreatePreferenceForCountry("PRE", "PRE", "TW");
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN").PK;
			var tariffTypeSSPK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "SS").PK;
			var tariffTypeCTPK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "CT").PK;
			var tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "TEST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tradeGroup1 = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "TEST1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.AddNewOrExistingCountry(tradeGroup, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddNewOrExistingCountry(tradeGroup1, Core.Constants.CountryCodes.China, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			factory.Save();

			var tariffQUOTA = helper.LoadOrCreateNewTariff("TW", tariffTypePK, "92031000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffQUOTARate = helper.CreateRate(tariffQUOTA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3*VFD", stdPreference.PK, "0.3");
			var tariffQUOTARate1 = helper.CreateRate(tariffQUOTA, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "1000*[KGM]", stdPreference.PK, "1000/KGM");

			helper.CreateCusApplicability(tariffQUOTARate, tradeGroup1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, orderNumber: "QUOTA");
			helper.CreateCusApplicability(tariffQUOTARate1, tradeGroup1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, orderNumber: "QUOTB");

			var tariffQUOTB = helper.LoadOrCreateNewTariff("TW", tariffTypePK, "92031000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffQUOTBRate = helper.CreateRate(tariffQUOTB, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3*VFD", stdPreference.PK, "0.3");
			helper.CreateCusApplicability(tariffQUOTBRate, tradeGroup1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, orderNumber: "QUOTA");

			var childTariffQUOTBRate0 = helper.CreateRate(tariffQUOTB, rateCodeCTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", rateFormulaDeriveFrom: "0.1");
			helper.CreateCusApplicability(childTariffQUOTBRate0, tradeGroup1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, orderNumber: "QUOTA");
			helper.CreateCusApplicability(childTariffQUOTBRate0, tradeGroup1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var childTariffQUOTBRate1 = helper.CreateRate(tariffQUOTB, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "610 * [KLT]", rateFormulaDeriveFrom: "610/KLT");
			helper.CreateCusApplicability(childTariffQUOTBRate1, tradeGroup1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var childTariffQUOTBRate2 = helper.CreateRate(tariffQUOTB, rateCodeTAT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "7 * [LTR] * AlcoholPercentage");
			helper.CreateCusApplicability(childTariffQUOTBRate2, tradeGroup1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, orderNumber: "QUOTA");

			var childTariffQUOTBRate3 = helper.CreateRate(tariffQUOTB, rateCodeHWS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "1000 * [KGM]", rateFormulaDeriveFrom: "1000/KGM");
			helper.CreateCusApplicability(childTariffQUOTBRate3, tradeGroup1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, orderNumber: "QUOTA");
			helper.CreateTariffRelationship(tariffQUOTB.PK, tariffTypePK, "92031000002");

			var tariffA = helper.LoadOrCreateNewTariff("TW", tariffTypePK, "87031000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffARateA = helper.CreateRate(tariffA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3*VFD", stdPreference.PK, "0.3");
			helper.CreateCusApplicability(tariffARateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffB = helper.LoadOrCreateNewTariff("TW", tariffTypePK, "87031000003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "T", tariffB);
			var tariffssBRateA = helper.CreateRate(tariffB, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3*VFD", stdPreference.PK, "0.3");
			helper.CreateCusApplicability(tariffssBRateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffWithTwoRates = helper.LoadOrCreateNewTariff("TW", tariffTypePK, "87031000004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffWithTwoRatesRateA = helper.CreateRate(tariffWithTwoRates, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3*VFD", stdPreference.PK, "0.3");
			helper.CreateCusApplicability(tariffWithTwoRatesRateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffWithTwoRatesRateB = helper.CreateRate(tariffWithTwoRates, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "1000*[KGM]", stdPreference.PK, "1000/KGM");
			helper.CreateCusApplicability(tariffWithTwoRatesRateB, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffWithDTSRate = helper.LoadOrCreateNewTariff("TW", tariffTypePK, "87031000005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffWithDTSRateRateA = helper.CreateRate(tariffWithDTSRate, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "1000*[KGM]", stdPreference.PK, "1000/KGM");
			helper.CreateCusApplicability(tariffWithDTSRateRateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var childTariffTruckbusother = helper.LoadOrCreateNewTariff("TW", tariffTypeCTPK, "TRUCKBUSOTHER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var childTariffCtaRate = helper.CreateRate(childTariffTruckbusother, rateCodeCTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", rateFormulaDeriveFrom: "0.1");
			helper.CreateCusApplicability(childTariffCtaRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var childTariffCtsRate = helper.CreateRate(childTariffTruckbusother, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "610 * [KLT]", rateFormulaDeriveFrom: "610/KLT");
			helper.CreateCusApplicability(childTariffCtsRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var childTariffTatRate = helper.CreateRate(childTariffTruckbusother, rateCodeTAT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "7 * [LTR] * AlcoholPercentage", rateFormulaDeriveFrom: "7/LTR");
			helper.CreateCusApplicability(childTariffTatRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var childTariffHwsRate = helper.CreateRate(childTariffTruckbusother, rateCodeHWS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "1000 * [KGM]", rateFormulaDeriveFrom: "1000/KGM");
			helper.CreateCusApplicability(childTariffHwsRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTariffRelationship(childTariffTruckbusother.PK, tariffTypePK, "87031000003");

			var tariffC = helper.LoadOrCreateNewTariff("TW", tariffTypePK, "03035400900", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffCRateA = helper.CreateRate(tariffC, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.25*VFD", prePreference.PK, rateFormulaDeriveFrom: "0.25");
			helper.CreateCusApplicability(tariffCRateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffCRateB = helper.CreateRate(tariffC, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "3.6*VFD", prePreference.PK, rateFormulaDeriveFrom: "3.6");
			helper.CreateCusApplicability(tariffCRateB, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffSsg = helper.LoadOrCreateNewTariff("TW", tariffTypePK, "03035400901", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "L*", tariffSsg);
			var childTariffForniture = helper.LoadOrCreateNewTariff("TW", tariffTypeSSPK, "FORNITURE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffSsgRate = helper.CreateRate(childTariffForniture, rateCodeSSG.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "IF(UnitCustomsValue >= 500000, 0.1 * VFD, 0)", rateFormulaDeriveFrom: "IF(UnitCustomsValue >= 500000, 0.1 * VFD, 0)");
			helper.CreateCusApplicability(tariffSsgRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTariffRelationship(childTariffForniture.PK, tariffTypePK, "03035400901");

			helper.CreateTaxOrFee("VAT", 0.22, Core.Constants.CountryCodes.Taiwan, ZDateTime.MinSmallDateTimeValue, new ZDateTime(2079, 6, 6, 23, 59, 00));
			var tpfFee = helper.CreateTaxOrFee("TPF", 0.0004, "TW", 2000, 0, "OTH", ZDateTime.MinSmallDateTimeValue, new ZDateTime(2079, 6, 6, 23, 59, 00));
			tpfFee.ZZF_Threshold = 100;

			helper.CreateTaxOrFee("DDF", 200, Core.Constants.CountryCodes.Taiwan, ZDateTime.MinSmallDateTimeValue, new ZDateTime(2079, 6, 6, 23, 59, 00));
			factory.Save();

			var tradeGroupWTO = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "WTO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddNewOrExistingCountry(tradeGroupWTO, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			factory.Save();

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			factory.Save();
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(factory, "DTA", dutyRateType.PK);
			var addRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.RateTypes.AntiDumping, "TPF");
			var rateCode2 = helper.LoadOrCreateNewCusRateCode(factory, "B51", addRateType.PK);
			factory.Save();
			var preferenceSTD = helper.CreatePreferenceForCountry("STD", "Column III Rates(Standard Rates)", Core.Constants.CountryCodes.Taiwan);
			var preferencePRE = helper.CreatePreferenceForCountry("PRE", "Preferred (Least Developed Countries/Subject to reciprocal tariff rate", Core.Constants.CountryCodes.Taiwan);
			var preferencePR1 = helper.CreatePreferenceForCountry("PR1", "Column I Rates(WTO Member/Other Countries with reciprocal relationship)", Core.Constants.CountryCodes.Taiwan);
			factory.Save();

			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "98050000009", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "第０５０７‧９０‧２０‧００號所屬之「其他鹿茸（包括中藥用）」");
			factory.Save();

			var testRate1 = helper.CreateRate(cusTariff, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3*VFD", preferencePk: preferenceSTD.PK, rateFormulaDeriveFrom: "0.3", dataGrouping: "TW");
			helper.CreateCusApplicability(testRate1, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");

			var testRate2 = helper.CreateRate(cusTariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.258*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.258", dataGrouping: "TW");
			helper.CreateCusApplicability(testRate2, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");

			var testRate3 = helper.CreateRate(cusTariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.125*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.125", dataGrouping: "TW");
			helper.CreateCusApplicability(testRate3, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "");
			factory.Save();
		}

		public static void GenerateTariffAndRateIncludingConcessionOrderData(BusinessObjectFactory factory)
		{
			var testHelper = new UniversalReferenceTestDataHelper(factory);

			var tradeGroupWTO = testHelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "WTO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testHelper.AddNewOrExistingCountry(tradeGroupWTO, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			factory.Save();

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			factory.Save();
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(factory, "DTA", dutyRateType.PK);
			var addRateType = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.RateTypes.AntiDumping, "TPF");
			var rateCode2 = testHelper.LoadOrCreateNewCusRateCode(factory, "B51", addRateType.PK);
			factory.Save();
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Column III Rates(Standard Rates)", Core.Constants.CountryCodes.Taiwan);
			var preferencePRE = testHelper.CreatePreferenceForCountry("PRE", "Preferred (Least Developed Countries/Subject to reciprocal tariff rate", Core.Constants.CountryCodes.Taiwan);
			var preferencePR1 = testHelper.CreatePreferenceForCountry("PR1", "Column I Rates(WTO Member/Other Countries with reciprocal relationship)", Core.Constants.CountryCodes.Taiwan);
			factory.Save();

			var cusTariff98050000009 = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "98050000009", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "test tariff 1");
			var cusTariff98050000010 = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "98050000010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "test tariff 2");
			factory.Save();

			var testRate1 = testHelper.CreateRate(cusTariff98050000009, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3*VFD", preferencePk: preferenceSTD.PK, rateFormulaDeriveFrom: "0.3", dataGrouping: "TW");
			testHelper.CreateCusApplicability(testRate1, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");

			var testRate2 = testHelper.CreateRate(cusTariff98050000009, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.258*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.258", dataGrouping: "TW");
			testHelper.CreateCusApplicability(testRate2, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");

			var testRate3 = testHelper.CreateRate(cusTariff98050000009, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.125*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.125", dataGrouping: "TW");
			testHelper.CreateCusApplicability(testRate3, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "");

			var testRate4 = testHelper.CreateRate(cusTariff98050000010, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.215*VFD", preferencePk: preferencePRE.PK, rateFormulaDeriveFrom: "0.215", dataGrouping: "TW");
			testHelper.CreateCusApplicability(testRate4, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");
			factory.Save();
		}

		public static void GenerateAdditionalTaxTariff(BusinessObjectFactory factory)
		{
			var testHelper = new UniversalReferenceTestDataHelper(factory);
			var rateType = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var rateCode = testHelper.LoadOrCreateNewCusRateCode(factory, "TAT", rateType.PK);
			var tradeGroupAllCountry = testHelper.LoadOrCreateTradeGroup("TW", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariffTypeHSN = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var tariffTypeSS = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "SS");
			var tariffTypeCT = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "CT");
			var tariffTypeAT = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			var tariffTypeTT = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "TT");
			factory.Save();

			var tariffT = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "90031", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var childTariffT = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeCT.PK, "OtherBeverage", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testHelper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "T", tariffT);
			testHelper.CreateTariffRelationship(childTariffT.PK, tariffTypeHSN.PK, "90031");
			var tariffTRate = testHelper.CreateRate(childTariffT, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.15 * VFD", rateFormulaDeriveFrom: "0.15");
			testHelper.CreateCusApplicability(tariffTRate, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffB = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "90032", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var childTariffB = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeAT.PK, "Distilled", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testHelper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "B", tariffB);
			testHelper.CreateTariffRelationship(childTariffB.PK, tariffTypeHSN.PK, "90032");
			var tariffBRate = testHelper.CreateRate(childTariffB, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.15 * VFD", rateFormulaDeriveFrom: "0.15");
			testHelper.CreateCusApplicability(tariffBRate, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffC = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "90033", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var childTariffC = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeTT.PK, "Gasoline", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testHelper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "C", tariffC);
			testHelper.CreateTariffRelationship(childTariffC.PK, tariffTypeHSN.PK, "90033");
			var tariffCRate = testHelper.CreateRate(childTariffC, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.15 * VFD", rateFormulaDeriveFrom: "0.15");
			testHelper.CreateCusApplicability(tariffCRate, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffLPartially = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "90034", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var childTariffLPartially = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeSS.PK, "Forniture", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testHelper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "L*", tariffLPartially);
			testHelper.CreateTariffRelationship(childTariffLPartially.PK, tariffTypeHSN.PK, "90034");
			var tariffLRate = testHelper.CreateRate(childTariffLPartially, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.15 * VFD", rateFormulaDeriveFrom: "0.15");
			testHelper.CreateCusApplicability(tariffLRate, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffTPartially = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "90035", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var childTariffTPartially = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeCT.PK, "PassengerCar", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testHelper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "T*", tariffTPartially);
			testHelper.CreateTariffRelationship(childTariffTPartially.PK, tariffTypeHSN.PK, "90035");

			var tariffBPartially = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "90036", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var childTariffBPartially = testHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeAT.PK, "Ethyl", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testHelper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "B*", tariffBPartially);
			testHelper.CreateTariffRelationship(childTariffBPartially.PK, tariffTypeHSN.PK, "90036");
			factory.Save();
		}
	}
}
