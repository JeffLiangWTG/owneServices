using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.Universal.ConditionChecker;
using CoreCountry = Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.Customs.Universal.Testing
{
	public class ConditionCheckerTests : TestCaseWithFactory
	{
		public void TestGetApplicableConditions()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(CoreCountry.France, "Fance", eun);
			helper.CreateNewOrGetExistingDataGrouping(CoreCountry.Italy, "Italy", eun);
			Factory.Save();
			var tariffType = helper.CreateNewOrGetExistingTariffType("EUN", "TTT");
			var rateType1 = helper.CreateOrGetExistingRefCusConditionType("EUN", RefCusConditionTypes.ConditionClass.Rate, "TSTR1", "Test Rate Condition Type 1");
			var ctrlType1 = helper.CreateOrGetExistingRefCusConditionType("EUN", RefCusConditionTypes.ConditionClass.Control, "TSTC1", "Test Ctrl Condition Type 1");
			var ctrlType2 = helper.CreateOrGetExistingRefCusConditionType("EUN", RefCusConditionTypes.ConditionClass.Control, "TSTC2", "Test Ctrl Condition Type 2");
			var preference = helper.CreatePreferenceForCountry("P", "TestPreference", "EUN");
			var tradeGroupPAC = helper.CreateTradeGroup("EUN", "PAC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroupPAC, CoreCountry.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(tradeGroupPAC, CoreCountry.NewZealand, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tradeGroupFJ = helper.CreateTradeGroup("EUN", "Fiji", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroupFJ, CoreCountry.Fiji, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
			var tariff = helper.CreateTariff(CoreCountry.France, tariffType.PK, "Tariff1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition1 = helper.CreateOrGetExistingRefCusCondition("EUN", rateType1.PK, tariff.PK, "C1", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			var condition2 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.France, rateType1.PK, tariff.PK, "C2", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			var condition3 = helper.CreateOrGetExistingRefCusCondition("EUN", ctrlType1.PK, tariff.PK, "C3", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicabilityInternal(condition3, tradeGroupPAC, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "A", orderNumber: "B");
			helper.CreateCusApplicabilityInternal(condition3, tradeGroupPAC, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "C");
			var condition4 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.France, ctrlType1.PK, tariff.PK, "C4", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateCusApplicabilityInternal(condition4, tradeGroupPAC, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "A", orderNumber: "B");
			helper.CreateCusApplicabilityInternal(condition4, tradeGroupPAC, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "C");
			var condition5 = helper.CreateOrGetExistingRefCusCondition("EUN", ctrlType2.PK, tariff.PK, "C5", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicabilityInternal(condition5, tradeGroupFJ, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition6 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.France, ctrlType2.PK, tariff.PK, "C6", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition7 = helper.CreateOrGetExistingRefCusCondition("EUN", ctrlType2.PK, tariff.PK, "C7", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			//a)	The blank ZZT_AdditionalCode(in refcusapplicability) matches to all additional codes
			//b)	The blank ZX1_Preference(in refcuscondition) matches to all preferences
			//c)	The blank ZZT_OrderNumber(in refcusapplicability) matches QuotaNumber when preference has a value other than ‘’. When preference has a value = null, then the blank ZZT_OrderNumber matches to all quota numbers.
			//d)	Similarly, when @conditionClass, nor a @conditionType are blank, then return all records without filtering on these values – (already works)
			//e)	Conditions should be selected for both the parentDataGrouping AND DataGrouping
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("FR - Preference:, Import/Export, AdditionalCode:, OrderNumber:, TradeGroup:", new[] { "C6", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "", "", null, "", "FR", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("IT - Preference:, Import/Export, AdditionalCode:, OrderNumber:, TradeGroup:", new[] { "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "", "", null, "", "IT", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("FR - Preference:P, Import, AdditionalCode:, OrderNumber:, TradeGroup:", new[] { "C1", "C6", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "", "P", null, "", "FR", ConditionDirection.Import, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("IT - Preference:P, Import, AdditionalCode:, OrderNumber:, TradeGroup:", new[] { "C1", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "", "P", null, "", "IT", ConditionDirection.Import, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("FR - Preference:P, Export, AdditionalCode:, OrderNumber:, TradeGroup:", new[] { "C2", "C6", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "", "P", null, "", "FR", ConditionDirection.Export, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("IT - Preference:P, Export, AdditionalCode:, OrderNumber:, TradeGroup:", new[] { "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "", "P", null, "", "IT", ConditionDirection.Export, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("FR - Preference:P, Import/Export, AdditionalCode:, OrderNumber:, TradeGroup:AU", new[] { "C1", "C2", "C6", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "AU", "P", null, "", "FR", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("IT - Preference:P, Import/Export, AdditionalCode:, OrderNumber:, TradeGroup:AU", new[] { "C1", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "AU", "P", null, "", "IT", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("FR - Preference:P, Import/Export, AdditionalCode:, OrderNumber:, TradeGroup:FJ", new[] { "C1", "C2", "C5", "C6", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "FJ", "P", null, "", "FR", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("IT - Preference:P, Import/Export, AdditionalCode:, OrderNumber:, TradeGroup:FJ", new[] { "C1", "C5", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "FJ", "P", null, "", "IT", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("FR - Preference:P, Import/Export, AdditionalCode:A, OrderNumber:, TradeGroup:FJ", new[] { "C1", "C2", "C5", "C6", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "FJ", "P", new HashSet<ZString> { "A" }, "", "FR", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("IT - Preference:P, Import/Export, AdditionalCode:A, OrderNumber:, TradeGroup:FJ", new[] { "C1", "C5", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "FJ", "P", new HashSet<ZString> { "A" }, "", "IT", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("FR - Preference:P, Import/Export, AdditionalCode:A, OrderNumber:B, TradeGroup:AU", new[] { "C1", "C2", "C3", "C4", "C6", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "AU", "P", new HashSet<ZString> { "A" }, "B", "FR", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("IT - Preference:P, Import/Export, AdditionalCode:A, OrderNumber:B, TradeGroup:AU", new[] { "C1", "C3", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "AU", "P", new HashSet<ZString> { "A" }, "B", "IT", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("FR - Preference:P, Import/Export, AdditionalCode:C, OrderNumber:B, TradeGroup:AU", new[] { "C1", "C2", "C3", "C6", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "AU", "P", new HashSet<ZString> { "C" }, "B", "FR", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("IT - Preference:P, Import/Export, AdditionalCode:C, OrderNumber:B, TradeGroup:AU", new[] { "C1", "C3", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "AU", "P", new HashSet<ZString> { "C" }, "B", "IT", ConditionDirection.Either, "", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("FR - Preference:P, Import/Export, AdditionalCode:A, OrderNumber:B, TradeGroup:AU, ConditionClass:CTRL", new[] { "C3", "C4", "C6", "C7" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "AU", "P", new HashSet<ZString> { "A" }, "B", "FR", ConditionDirection.Either, "CTRL", "")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("FR - Preference:P, Import/Export, AdditionalCode:A, OrderNumber:B, TradeGroup:AU, ConditionType:TSTC1", new[] { "C3", "C4" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new ZZConditionSelectionCriteria(ZDateTime.Now, "AU", "P", new HashSet<ZString> { "A" }, "B", "FR", ConditionDirection.Either, "", "TSTC1")).Select(x => x.ZX1_Comment));
				AssertContainsExactElementsInAnyOrder("FR - Preference:P, Import, AdditionalCode:, OrderNumber:, TradeGroup AND same with Export:",
					new[] { "C1", "C6", "C7", "C2" }, ConditionChecker.GetApplicableConditions(Factory, tariff, new[] {
					new ZZConditionSelectionCriteria(ZDateTime.Now, "", "P", null, "", "FR", ConditionDirection.Import, "", ""),
					new ZZConditionSelectionCriteria(ZDateTime.Now, "", "P", null, "", "FR", ConditionDirection.Export, "", "") }).Select(x => x.ZX1_Comment));
			});
		}

		public void TestLogicalANDWithinGroup()
		{
			var dataGrouping = RefDataGrouping.Codes.EuropeanUnionEUN;
			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, Constants.TariffTypes.HarmonizedSystem);
			var type122 = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, RefCusConditionTypes.ConditionClass.Control, "122", "Condition Type 122");
			var type125 = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, RefCusConditionTypes.ConditionClass.Control, "125", "Condition Type 125");
			anotherFactory.Save();
			var tariff = helper.CreateTariff(dataGrouping, tariffType.PK, "100630929", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var supConditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.ConditionValueType.SupportingDocument);
			var condition1 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, type122.PK, tariff.PK, "Condition1", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_LogicalANDWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(supConditionValueType.PK, condition1.PK, "U003");
			var condition2 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, type122.PK, tariff.PK, "Condition2", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_LogicalANDWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(supConditionValueType.PK, condition2.PK, "Y100");
			var condition3 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, type122.PK, tariff.PK, "Condition3", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_LogicalANDWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(supConditionValueType.PK, condition3.PK, "L001");
			var condition4 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, type125.PK, tariff.PK, "Condition4", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_LogicalANDWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(supConditionValueType.PK, condition4.PK, "ABC");
			var condition5 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, type125.PK, tariff.PK, "Condition5", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_LogicalANDWithinGroup = 1);
			helper.CreateOrGetExistingRefCusConditionValue(supConditionValueType.PK, condition5.PK, "DEF");
			Factory.Save();
			anotherFactory.Save();
			tariff = Factory.Load<TariffView>(tariff.PK);
			CombineAssertions(() =>
			{
				AssertCheckConditionsAreMet("Direction Import", @"The Customs Control condition is not satisfied
    Condition Type 122:
        Condition1: U003 AND
        Condition2: Y100 AND
        Condition3: L001
    Condition Type 125:
        Condition4: ABC OR
        Condition5: DEF
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, dataGrouping);
				AssertCheckConditionsAreMet("Direction Import", @"The Customs Control condition is not satisfied
    Condition Type 122:
        Condition2: Y100 AND
        Condition3: L001
    Condition Type 125:
        Condition4: ABC OR
        Condition5: DEF
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, dataGrouping, evaluationDelegate: (_, __, input) => new[] { "U003" }.ToList().Contains(input));
				AssertCheckConditionsAreMet("Direction Import", @"The Customs Control condition is not satisfied
    Condition Type 125:
        Condition4: ABC OR
        Condition5: DEF
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, dataGrouping, evaluationDelegate: (_, __, input) => new[] { "U003", "Y100", "L001" }.ToList().Contains(input));
				AssertCheckConditionsAreMet("Direction Import", "", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, dataGrouping, evaluationDelegate: (_, __, input) => new[] { "U003", "Y100", "L001", "ABC" }.ToList().Contains(input));
				AssertCheckConditionsAreMet("Direction Import", "", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, dataGrouping, evaluationDelegate: (_, __, input) => new[] { "U003", "Y100", "L001", "DEF" }.ToList().Contains(input));
			}

			);
		}

		public void TestShouldStopForCondition()
		{
			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CoreCountry.SouthAfrica, "TTT");
			var rateType1 = helper.CreateOrGetExistingRefCusConditionType(CoreCountry.SouthAfrica, RefCusConditionTypes.ConditionClass.Rate, "TSTR1", "Test Rate Condition Type 1");
			var ctrlType1 = helper.CreateOrGetExistingRefCusConditionType(CoreCountry.SouthAfrica, RefCusConditionTypes.ConditionClass.Control, "TSTC1", "Test Ctrl Condition Type 1");
			var ctrlType2 = helper.CreateOrGetExistingRefCusConditionType(CoreCountry.SouthAfrica, RefCusConditionTypes.ConditionClass.Control, "TSTC2", "Test Ctrl Condition Type 2");
			var preference = helper.CreatePreferenceForCountry("Pref1", "TestPref1", CoreCountry.SouthAfrica);
			anotherFactory.Save();
			var tradeGroupPAC = helper.CreateTradeGroup(CoreCountry.SouthAfrica, "PAC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroupPAC, CoreCountry.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(tradeGroupPAC, CoreCountry.NewZealand, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(tradeGroupPAC, CoreCountry.Fiji, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tradeGroupFJ = helper.CreateTradeGroup(CoreCountry.SouthAfrica, "Fiji", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroupFJ, CoreCountry.Fiji, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			anotherFactory.Save();
			var tariff = helper.CreateTariff(CoreCountry.SouthAfrica, tariffType.PK, "Tariff1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var testValueType = helper.CreateOrGetExistingRefCusConditionValueType(CoreCountry.SouthAfrica, "TSTVT");
			var formulaValueType = helper.CreateOrGetExistingRefCusConditionValueType(CoreCountry.SouthAfrica, "FRMVT", afterCreate: t => t.ZX4_IsFormula = true);
			var testCondCtrl1_1 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.SouthAfrica, ctrlType1.PK, tariff.PK, "Direction:Import", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_Source = "www.google.com");
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XA", v => v.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XB", v => v.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XC", v => v.ZX3_LogicalORWithinGroup = 1);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XD", v => v.ZX3_LogicalORWithinGroup = 2);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XE", v => v.ZX3_LogicalORWithinGroup = 2);
			var testCondCtrl1_2 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.SouthAfrica, ctrlType1.PK, tariff.PK, "Direction:Export", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_2.PK, "YA");
			var testCondCtrl1_3 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.SouthAfrica, ctrlType1.PK, tariff.PK, "Direction:Either", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_3.PK, "YB");
			var testCondCtrl1_4 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.SouthAfrica, ctrlType1.PK, tariff.PK, "Date: ExpiredYesterday", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1));
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_4.PK, "YC");
			var testCondCtrl1_5 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.SouthAfrica, ctrlType1.PK, tariff.PK, "Preference: Pref1", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_5.PK, "YD");
			var testCondCtrl1_6 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.SouthAfrica, ctrlType1.PK, tariff.PK, "TradeGroup: AUNZFJ-FJ", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_6.PK, "YE");
			var tradeGroupApplicability1_6 = helper.CreateCusApplicabilityInternal(testCondCtrl1_6, tradeGroupPAC, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var excludedTradeGroup1_6 = testCondCtrl1_6.Factory.New<RefCusExcludedTradeGroup>();
			excludedTradeGroup1_6.ZZC_ZZT_Applicability = tradeGroupApplicability1_6.PK;
			excludedTradeGroup1_6.ZZC_ZZA_TradeGroup = tradeGroupFJ.PK;
			testCondCtrl1_6.Factory.Save();
			var testCondCtrl1_7 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.SouthAfrica, ctrlType1.PK, tariff.PK, "AdditionalCode: AC", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1_7.PK, "[KGM] > 2.00");
			helper.CreateCusApplicabilityInternal(testCondCtrl1_7, tradeGroupPAC, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "AC");
			var testCondCtrl1_8 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.SouthAfrica, ctrlType1.PK, tariff.PK, "OrderNumber: ON", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1_8.PK, "[NO] > 2.00", v => v.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1_8.PK, "[KGM] < 2000.00", v => v.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1_8.PK, "[NO] > 20.00", v => v.ZX3_LogicalORWithinGroup = 1);
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1_8.PK, "[TNE] < 20.00", v => v.ZX3_LogicalORWithinGroup = 1);
			helper.CreateCusApplicabilityInternal(testCondCtrl1_8, tradeGroupPAC, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "ON");
			var testCondCtrl2_1 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.SouthAfrica, ctrlType2.PK, tariff.PK, "cond2_1", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl2_1.PK, "ZA");
			var testCondRate1_1 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.SouthAfrica, rateType1.PK, tariff.PK, "rate1_1", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondRate1_1.PK, "R1");
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondRate1_1.PK, "R2");
			var testCondCtrl2_2 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.SouthAfrica, ctrlType2.PK, tariff.PK, "TrueStop", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ConditionValueTrueMeansStop = true);
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl2_2.PK, "ZB");
			helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl2_2.PK, "ZC");
			helper.CreateCusApplicabilityInternal(testCondCtrl2_2, tradeGroupPAC, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "<TS>");
			Factory.Save();
			anotherFactory.Save();
			tariff = Factory.Load<TariffView>(tariff.PK);
			CombineAssertions(() =>
			{
				AssertCheckConditionsAreMet("Direction Import", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either: YB AND
        Direction:Import: (XA or XB) and XC and (XD or XE)
            (Please refer to: www.google.com)
    Test Ctrl Condition Type 2:
        cond2_1: ZA
The Rate condition is not satisfied
    Test Rate Condition Type 1:
        rate1_1: R1 or R2
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, CoreCountry.SouthAfrica);
				AssertCheckConditionsAreMet("Direction Export", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either: YB AND
        Direction:Export: YA
    Test Ctrl Condition Type 2:
        cond2_1: ZA
The Rate condition is not satisfied
    Test Rate Condition Type 1:
        rate1_1: R1 or R2
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Export, CoreCountry.SouthAfrica, CoreCountry.SouthAfrica);
				AssertCheckConditionsAreMet("Date: The Day Before Yesterday", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Date: ExpiredYesterday: YC AND
        Direction:Either: YB AND
        Direction:Import: (XA or XB) and XC and (XD or XE)
            (Please refer to: www.google.com)
    Test Ctrl Condition Type 2:
        cond2_1: ZA
The Rate condition is not satisfied
    Test Rate Condition Type 1:
        rate1_1: R1 or R2
", tariff, ZDateTime.Today.AddDays(-2), ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, CoreCountry.SouthAfrica);
				AssertCheckConditionsAreMet("Preference", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either: YB AND
        Direction:Import: (XA or XB) and XC and (XD or XE)
            (Please refer to: www.google.com) AND
        Preference: Pref1: YD
    Test Ctrl Condition Type 2:
        cond2_1: ZA
The Rate condition is not satisfied
    Test Rate Condition Type 1:
        rate1_1: R1 or R2
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, CoreCountry.SouthAfrica, preference: "Pref1");
				AssertCheckConditionsAreMet("Class: Ctrl", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either: YB AND
        Direction:Import: (XA or XB) and XC and (XD or XE)
            (Please refer to: www.google.com)
    Test Ctrl Condition Type 2:
        cond2_1: ZA
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, CoreCountry.SouthAfrica, conditionClass: "CTRL");
				AssertCheckConditionsAreMet("Class: Rate", @"The Rate condition is not satisfied
    Test Rate Condition Type 1:
        rate1_1: R1 or R2
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, CoreCountry.SouthAfrica, conditionClass: "RATE");
				AssertCheckConditionsAreMet("Type: TSTC2", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 2:
        cond2_1: ZA
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, CoreCountry.SouthAfrica, conditionType: "TSTC2");
				AssertCheckConditionsAreMet("TradeGroup", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either: YB AND
        Direction:Import: (XA or XB) and XC and (XD or XE)
            (Please refer to: www.google.com) AND
        TradeGroup: AUNZFJ-FJ: YE
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Australia, CoreCountry.SouthAfrica, conditionType: "TSTC1");
				AssertCheckConditionsAreMet("TradeGroup: Exclude", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either: YB AND
        Direction:Import: (XA or XB) and XC and (XD or XE)
            (Please refer to: www.google.com)
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Fiji, CoreCountry.SouthAfrica, conditionType: "TSTC1");
				AssertCheckConditionsAreMet("TradeGroup: Exclude", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either AND
        Direction:Import
            (Please refer to: www.google.com)
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Fiji, CoreCountry.SouthAfrica, conditionType: "TSTC1", getFriendlyConditionValue: ((_, __, input) => ""));
				AssertCheckConditionsAreMet("AdditionalCode:AC", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        AdditionalCode: AC: ([KGM] > 2.00) AND
        Direction:Either: YB AND
        Direction:Import: (XA or XB) and XC and (XD or XE)
            (Please refer to: www.google.com) AND
        TradeGroup: AUNZFJ-FJ: YE
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Australia, CoreCountry.SouthAfrica, conditionType: "TSTC1", additionalCodes: new[] { "AC" });
				AssertCheckConditionsAreMet("OrderNumber, ON", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either: YBDesc AND
        Direction:Import: (XADesc or XBDesc) and XCDesc and (XDDesc or XEDesc)
            (Please refer to: www.google.com) AND
        OrderNumber: ON: (([KGM] < 2000.00) or ([NO] > 2.00)) and (([NO] > 20.00) or ([TNE] < 20.00)) AND
        TradeGroup: AUNZFJ-FJ: YEDesc
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Australia, CoreCountry.SouthAfrica, conditionType: "TSTC1", orderNumber: "ON", getFriendlyConditionValue: ((_, __, input) => input + "Desc"));
				AssertCheckConditionsAreMet("Half Value, condition Value Meet A", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either: YB AND
        Direction:Import: (XA or XB) and XC and (XD or XE)
            (Please refer to: www.google.com)
The Rate condition is not satisfied
    Test Rate Condition Type 1:
        rate1_1: R1 or R2
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, CoreCountry.SouthAfrica, evaluationDelegate: ((_, __, input) => (new string[] { "XA", "ZA" }).ToList().Contains(input)));
				AssertCheckConditionsAreMet("All Meet, condition Value Meet B", @"", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.SouthAfrica, CoreCountry.SouthAfrica, evaluationDelegate: ((_, __, input) => (new string[] { "XB", "XC", "XD", "YA", "YB", "ZA", "R1" }).ToList().Contains(input)));
				AssertCheckConditionsAreMet("Meeting Condition True -> STOP, AdditionalCode:<TS>", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        TradeGroup: AUNZFJ-FJ: YE
    Test Ctrl Condition Type 2:
        TrueStop: not (ZB or ZC)
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Australia, CoreCountry.SouthAfrica, evaluationDelegate: ((_, __, input) => (new string[] { "XB", "XC", "XD", "YA", "YB", "ZA", "R1", "ZB" }).ToList().Contains(input)), additionalCodes: new[] { "<TS>" });
				AssertCheckConditionsAreMet("Meeting Condition True -> STOP, AdditionalCode:<TS>,TT", @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        AdditionalCode: AC: ([KGM] > 2.00) AND
        TradeGroup: AUNZFJ-FJ: YE
    Test Ctrl Condition Type 2:
        TrueStop: not (ZB or ZC)
", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Australia, CoreCountry.SouthAfrica, evaluationDelegate: ((_, __, input) => (new string[] { "XB", "XC", "XD", "YA", "YB", "ZA", "R1", "ZB", "ZD" }).ToList().Contains(input)), additionalCodes: new[] { "<TS>", "AC" });
				AssertCheckConditionsAreMet("Not Meeting Condition True -> STOP", "", tariff, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Australia, CoreCountry.SouthAfrica, evaluationDelegate: ((_, __, input) => (new string[] { "XB", "XC", "XD", "YA", "YB", "YE", "ZA", "R1" }).ToList().Contains(input)), additionalCodes: new[] { "<TS>" });
			});
		}

		public void TestCheckConditionsAreMet_HasInformationCondition()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CoreCountry.China, "TTT");
			Factory.Save();
			var tariff1 = helper.CreateTariff(CoreCountry.China, tariffType.PK, "Tariff1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff2 = helper.CreateTariff(CoreCountry.China, tariffType.PK, "Tariff2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff3 = helper.CreateTariff(CoreCountry.China, tariffType.PK, "Tariff3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff4 = helper.CreateTariff(CoreCountry.China, tariffType.PK, "Tariff4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var conditionTypeNormal = helper.CreateOrGetExistingRefCusConditionType(CoreCountry.China, RefCusConditionTypes.ConditionClass.Control, "TSTCT");
			var conditionValueType1 = helper.CreateOrGetExistingRefCusConditionValueType(CoreCountry.China, "CVT");
			var conditionValueType2 = helper.CreateOrGetExistingRefCusConditionValueType(CoreCountry.China, "INF");
			conditionTypeNormal.Factory.Save();
			var conditionNormalWithCondValue1 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.China, conditionTypeNormal.PK, tariff1.PK, "Normal_withCondValue1", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType1.PK, conditionNormalWithCondValue1.PK, "Y1");
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType2.PK, conditionNormalWithCondValue1.PK, "Information");
			var conditionNormalWithCondValue2 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.China, conditionTypeNormal.PK, tariff2.PK, "Normal_withCondValue2", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType1.PK, conditionNormalWithCondValue2.PK, "Y21");
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType1.PK, conditionNormalWithCondValue2.PK, "Y22");
			var conditionNormalWithCondValue3 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.China, conditionTypeNormal.PK, tariff2.PK, "Normal_withCondValue3", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType1.PK, conditionNormalWithCondValue3.PK, "Y31");
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType1.PK, conditionNormalWithCondValue3.PK, "Y32");
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType2.PK, conditionNormalWithCondValue3.PK, "Information");
			var conditionNormalWithCondValue4 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.China, conditionTypeNormal.PK, tariff3.PK, "Normal_withCondValue4", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType1.PK, conditionNormalWithCondValue4.PK, "Y4");
			var conditionNormalWithCondValue5 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.China, conditionTypeNormal.PK, tariff4.PK, "Normal_withCondValue5", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType1.PK, conditionNormalWithCondValue5.PK, "Y5");
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType2.PK, conditionNormalWithCondValue5.PK, "Information");
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertCheckConditionsAreMet("A condition with multi ConditionValues: return INF condition message When having one ConditionValue is INF type and conditions not met", "",
					tariff1, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Australia, CoreCountry.China, expectedNotMetMessageForInformation: @"The Customs Control condition is not satisfied
    Default Condition Description:
        Normal_withCondValue1: Y1 or Information
");

				AssertCheckConditionsAreMet("Multi conditions not met: the condition NOT including INF type ConditionValue return condition message and the condition including INF type ConditionValue return INF condition message",
					@"The Customs Control condition is not satisfied
    Default Condition Description:
        Normal_withCondValue2: Y21 or Y22
", tariff2, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Australia, CoreCountry.China, expectedNotMetMessageForInformation: @"The Customs Control condition is not satisfied
    Default Condition Description:
        Normal_withCondValue3: Y31 or Y32 or Information
");

				AssertCheckConditionsAreMet("No INF type ConditionValue: non-INF condition message returned", @"The Customs Control condition is not satisfied
    Default Condition Description:
        Normal_withCondValue4: Y4
", tariff3, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Australia, CoreCountry.China);

				EvaluateConditionValue validation = (_, __, input) => input == "Y5";
				AssertCheckConditionsAreMet("Has INF type ConditionValue and other condition values are met: no message returned", "", tariff4, ZDateTime.Today, ConditionChecker.ConditionDirection.Import, CoreCountry.Australia, CoreCountry.China, evaluationDelegate: validation);
			});
		}

		public void TestCheckConditionSeverity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CoreCountry.UnitedStates, "HSN");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(CoreCountry.UnitedStates, RefCusConditionTypes.ConditionClass.Control, "MELT");
			Factory.Save();

			TestCase("00000001", "MSG", NotificationType.MessageError);
			TestCase("00000002", "WAR", NotificationType.Warning);
			TestCase("00000003", null, NotificationType.Warning);

			void TestCase(ZString tariffCode, ZString severity, CargoWise.ComponentModel.INotificationType expectResult)
			{
				var tariff = helper.CreateTariff(CoreCountry.UnitedStates, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				var condition = helper.CreateOrGetExistingRefCusCondition(CoreCountry.UnitedStates, conditionType.PK, tariff.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, severity: severity);

				AssertEquals(expectResult, CheckConditionSeverity(condition));
			}
		}

		public void TestCheckUS_RN_NKMeltCtry_TariffCode99038185()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(CoreCountry.UnitedStates, "HSN");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(CoreCountry.UnitedStates, RefCusConditionTypes.ConditionClass.Control, "MELT");

			var usTradeGroup = helper.CreateTradeGroup(CoreCountry.UnitedStates, CoreCountry.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var usTradeGroupCountry = helper.AddCountry(usTradeGroup, CoreCountry.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tradeGroup = helper.CreateTradeGroup(CoreCountry.UnitedStates, "All Countries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroup, CoreCountry.China, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(tradeGroup, CoreCountry.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariff = helper.CreateTariff(CoreCountry.UnitedStates, tariffType.PK, "00000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(CoreCountry.UnitedStates, conditionType.PK, tariff.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var applicability = helper.CreateCusApplicability(condition, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateExcludedTradeGroup(usTradeGroup, applicability);
			Factory.Save();

			AssertEquals(true, CheckConditionApplicabilitiesExcludeCountry(condition, "US", ZDateTime.Today));
			AssertEquals(false, CheckConditionApplicabilitiesExcludeCountry(condition, "CN", ZDateTime.Today));
		}

		readonly Dictionary<string, int> expectedDbHits = new Dictionary<string, int>
		{
			{ RefCusConditionSchema.Constants.TableName, 1 },
			{ RefCusConditionTypeSchema.Constants.TableName, 1 },
			{ RefCusConditionTypeLanguageSchema.Constants.TableName, 1 },
			{ RefCusConditionValueSchema.Constants.TableName, 1 },
			{ RefCusConditionValueTypeSchema.Constants.TableName, 1 },
			{ RefCusConditionLanguageSchema.Constants.TableName, 1 }
		};

		public void TestCheckConditionsAreMetForConditionClass_HasCondition()
		{
			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CoreCountry.Germany, "TTT");
			var classType1 = helper.CreateOrGetExistingRefCusConditionType(CoreCountry.Germany, RefCusConditionTypes.ConditionClass.Class, "TSTC1", "Test Class Condition Type 1");

			anotherFactory.Save();
			var tariff = helper.CreateTariff(CoreCountry.Germany, tariffType.PK, "Tariff1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var formulaValueType = helper.CreateOrGetExistingRefCusConditionValueType(CoreCountry.Germany, "FRMVT", afterCreate: t => t.ZX4_IsFormula = true);

			var testCondCtrl1 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.Germany, classType1.PK, tariff.PK, "Comment", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1.PK, "[KGM]/[NAR] < 165.001 & [KGM]/[NAR] >= 60.000");

			anotherFactory.Save();
			tariff = Factory.Load<TariffView>(tariff.PK);

			EvaluateConditionValue forSelectOnly = (_, __, x) => false;
			var (notMetMessageForNonInformation, notMetMessageForInformation) = tariff.CheckConditionsAreMetForConditionClass(RefCusConditionTypes.ConditionClass.Class, forSelectOnly, null);
			AssertMultilineASCIIEquals("notMetMessageForNonInformation should not be empty", @"The Classification condition is not satisfied
    Test Class Condition Type 1:
        Comment: ([KGM]/[NAR] < 165.001 & [KGM]/[NAR] >= 60.000)
", notMetMessageForNonInformation);
		}

		void AssertCheckConditionsAreMet(string errorMessage, string expectedNotMetMessageForNonInformation, TariffView tariff, ZDateTime effectiveDate, ConditionDirection direction, ZString tradeGroupCountry, ZString dataGrouping,
			string conditionClass = "", string conditionType = "", string preference = "", string[] additionalCodes = null, string orderNumber = "", EvaluateConditionValue evaluationDelegate = null,
			GetFriendlyConditionValue getFriendlyConditionValue = null, string expectedNotMetMessageForInformation = "")
		{
			EvaluateConditionValue forSelectOnly = (_, __, x) => false;
			var factory = tariff.Factory;
			factory.ResetDatabaseLoadCount();
			using (AssertDbHitsForAllFactories(errorMessage, expectedDbHits, hitTolerance: 1, includeFactoryPredicate: (f) => f == factory))
			{
				var (notMetMessageForNonInformation, notMetMessageForInformation) = tariff.CheckConditionsAreMet(
					new[] { new ZZConditionSelectionCriteria(effectiveDate, tradeGroupCountry, preference,
						additionalCodes?.Select(x => new ZString(x)).ToHashSet(), orderNumber, dataGrouping, direction,
						conditionClass, conditionType) }, evaluationDelegate ?? forSelectOnly, getFriendlyConditionValue);
				AssertMultilineASCIIEquals(errorMessage + " - notMetMessageForNonInformation", expectedNotMetMessageForNonInformation, notMetMessageForNonInformation);
				AssertMultilineASCIIEquals(errorMessage + " - notMetMessageForInformation", expectedNotMetMessageForInformation, notMetMessageForInformation);
			}
		}
	}
}
