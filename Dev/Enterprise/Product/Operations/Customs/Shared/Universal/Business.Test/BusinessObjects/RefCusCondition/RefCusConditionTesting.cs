using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.Universal.ConditionChecker;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCondition))]
	public class RefCusConditionTesting : EnterpriseBusinessObjectTestCase
	{
		public void TestSupportsNotes()
		{
			var condition = (RefCusCondition)GetNewBusinessObject();
			AssertEquals("SupportsNotes", false, condition.SupportsNotes);
		}

		public void TestShouldStop()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CoreConstants.CountryCodes.China, "TTT");
			Factory.Save();
			var tariff = helper.CreateTariff(CoreConstants.CountryCodes.China, tariffType.PK, "Tariff1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var conditionTypeNormal = helper.CreateOrGetExistingRefCusConditionType(CoreConstants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTCT");
			var conditionTypeProhibition = helper.CreateOrGetExistingRefCusConditionType(CoreConstants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTPH");
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(CoreConstants.CountryCodes.China, "CVT");
			conditionTypeNormal.Factory.Save();
			var conditionNormalWithNoCondValue = helper.CreateOrGetExistingRefCusCondition(CoreConstants.CountryCodes.China, conditionTypeNormal.PK, tariff.PK, "Normal_withNoCondValue", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var conditionNormalWithCondValue = helper.CreateOrGetExistingRefCusCondition(CoreConstants.CountryCodes.China, conditionTypeNormal.PK, tariff.PK, "Normal_withCondValue", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, conditionNormalWithCondValue.PK, "Y");
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, conditionNormalWithCondValue.PK, "T");
			var conditionProhibitionWithNoCondValue = helper.CreateOrGetExistingRefCusCondition(CoreConstants.CountryCodes.China, conditionTypeProhibition.PK, tariff.PK, "Prohibitation_withNoCondValue", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			conditionProhibitionWithNoCondValue.ZX1_ConditionValueTrueMeansStop = true;
			var conditionProhibitionWithCondValue = helper.CreateOrGetExistingRefCusCondition(CoreConstants.CountryCodes.China, conditionTypeProhibition.PK, tariff.PK, "Prohibitation_withCondValue", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			conditionProhibitionWithCondValue.ZX1_ConditionValueTrueMeansStop = true;
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, conditionProhibitionWithCondValue.PK, "Y");
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, conditionProhibitionWithCondValue.PK, "T");
			Factory.Save();

			EvaluateConditionValue validation = (_, __, input) => input == "Y";
			CombineAssertions(() =>
			{
				AssertEquals("Normal condition without condition value", false, conditionNormalWithNoCondValue.ShouldStop(validation));
				AssertEquals("Normal condition with condition value", false, conditionNormalWithCondValue.ShouldStop(validation));
				AssertEquals("Prohibition condition without condition value", true, conditionProhibitionWithNoCondValue.ShouldStop(validation));
				AssertEquals("Prohibition condition with condition value", true, conditionProhibitionWithCondValue.ShouldStop(validation));
			});
		}

		public void TestShouldStop_IsInformationCondition()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CoreConstants.CountryCodes.China, "TTT");
			Factory.Save();
			var tariff = helper.CreateTariff(CoreConstants.CountryCodes.China, tariffType.PK, "Tariff1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var conditionTypeNormal = helper.CreateOrGetExistingRefCusConditionType(CoreConstants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTCT");
			var conditionValueType1 = helper.CreateOrGetExistingRefCusConditionValueType(CoreConstants.CountryCodes.China, "INF");
			var conditionValueType2 = helper.CreateOrGetExistingRefCusConditionValueType(CoreConstants.CountryCodes.China, "CVT");
			conditionTypeNormal.Factory.Save();
			var conditionNormalWithNoINFCondValue = helper.CreateOrGetExistingRefCusCondition(CoreConstants.CountryCodes.China, conditionTypeNormal.PK, tariff.PK, "Normal_withNoINFCondValue", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType2.PK, conditionNormalWithNoINFCondValue.PK, "Y");
			conditionNormalWithNoINFCondValue.ZX1_ConditionValueTrueMeansStop = true;
			var conditionNormalWithINFCondValue1 = helper.CreateOrGetExistingRefCusCondition(CoreConstants.CountryCodes.China, conditionTypeNormal.PK, tariff.PK, "Normal_withINFCondValue1", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType1.PK, conditionNormalWithINFCondValue1.PK, "INFORMATION");
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType2.PK, conditionNormalWithINFCondValue1.PK, "Y");
			conditionNormalWithINFCondValue1.ZX1_ConditionValueTrueMeansStop = true;
			var conditionNormalWithINFCondValue2 = helper.CreateOrGetExistingRefCusCondition(CoreConstants.CountryCodes.China, conditionTypeNormal.PK, tariff.PK, "Normal_withINFCondValue2", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType1.PK, conditionNormalWithINFCondValue2.PK, "INFORMATION");
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType2.PK, conditionNormalWithINFCondValue2.PK, "N");
			conditionNormalWithINFCondValue2.ZX1_ConditionValueTrueMeansStop = true;
			Factory.Save();

			EvaluateConditionValue validation = (_, __, input) => input == "Y";
			CombineAssertions(() =>
			{
				AssertEquals("For non-informationCondition, ShouldStop based on condition value is met or not", true, conditionNormalWithNoINFCondValue.ShouldStop(validation));
				AssertEquals("For informationCondition, ShouldStop based on condition values other than INF are met or not: true", true, conditionNormalWithINFCondValue1.ShouldStop(validation));
				AssertEquals("For informationCondition, ShouldStop based on condition values other than INF are met or not: false", false, conditionNormalWithINFCondValue2.ShouldStop(validation));
			});
		}

		public void TestShouldStop_LogicalORWithinGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CoreConstants.CountryCodes.China, "TTT");
			Factory.Save();
			var tariff = helper.CreateTariff(CoreConstants.CountryCodes.China, tariffType.PK, "Tariff1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var conditionTypeNormal = helper.CreateOrGetExistingRefCusConditionType(CoreConstants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTCT");
			var conditionTypeProhibition = helper.CreateOrGetExistingRefCusConditionType(CoreConstants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTPH");
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(CoreConstants.CountryCodes.China, "CVT");
			var conditionFormulaType = helper.CreateOrGetExistingRefCusConditionValueType(CoreConstants.CountryCodes.China, "FRM", afterCreate: t => t.ZX4_IsFormula = true);
			var conditionNormalWithCondValue = helper.CreateOrGetExistingRefCusCondition(CoreConstants.CountryCodes.China, conditionTypeNormal.PK, tariff.PK, "Normal_withCondValue", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, conditionNormalWithCondValue.PK, "Y", v => v.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, conditionNormalWithCondValue.PK, "T", v => v.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(conditionFormulaType.PK, conditionNormalWithCondValue.PK, "[KGM] > 2.0", v => v.ZX3_LogicalORWithinGroup = 1);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, conditionNormalWithCondValue.PK, "A", v => v.ZX3_LogicalORWithinGroup = 1);
			helper.CreateOrGetExistingRefCusConditionValue(conditionFormulaType.PK, conditionNormalWithCondValue.PK, "[NO] > 10", v => v.ZX3_LogicalORWithinGroup = 2);
			var conditionProhibitionWithCondValue = helper.CreateOrGetExistingRefCusCondition(CoreConstants.CountryCodes.China, conditionTypeProhibition.PK, tariff.PK, "Prohibitation_withCondValue", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			conditionProhibitionWithCondValue.ZX1_ConditionValueTrueMeansStop = true;
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, conditionProhibitionWithCondValue.PK, "Y", v => v.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, conditionProhibitionWithCondValue.PK, "T", v => v.ZX3_LogicalORWithinGroup = 0);
			helper.CreateOrGetExistingRefCusConditionValue(conditionFormulaType.PK, conditionProhibitionWithCondValue.PK, "[KGM] > 2.0", v => v.ZX3_LogicalORWithinGroup = 1);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, conditionProhibitionWithCondValue.PK, "A", v => v.ZX3_LogicalORWithinGroup = 1);
			helper.CreateOrGetExistingRefCusConditionValue(conditionFormulaType.PK, conditionProhibitionWithCondValue.PK, "[NO] > 10", v => v.ZX3_LogicalORWithinGroup = 2);
			Factory.Save();
			var calcData = new UniversalRateDataForTest();
			calcData.UnitOfMeasureValueList.Add("KGM", 2m);
			calcData.UnitOfMeasureValueList.Add("NO", 10m);
			AssertEquals(true, conditionNormalWithCondValue.ShouldStop((_, __, input) => "Y".Contains(input)));
			AssertEquals(true, conditionNormalWithCondValue.ShouldStop((_, __, input) => "YA".Contains(input), calcData));
			AssertEquals(true, conditionNormalWithCondValue.ShouldStop((_, __, input) => "C".Contains(input), calcData));
			AssertEquals(false, conditionProhibitionWithCondValue.ShouldStop((_, __, input) => "Y".Contains(input)));
			AssertEquals(false, conditionProhibitionWithCondValue.ShouldStop((_, __, input) => "YA".Contains(input), calcData));
			AssertEquals(false, conditionProhibitionWithCondValue.ShouldStop((_, __, input) => "C".Contains(input), calcData));
			calcData.UnitOfMeasureValueList["KGM"] = 3m;
			calcData.UnitOfMeasureValueList["NO"] = 11m;
			AssertEquals(false, conditionNormalWithCondValue.ShouldStop((_, __, input) => "Y".Contains(input), calcData));
			AssertEquals(false, conditionNormalWithCondValue.ShouldStop((_, __, input) => "T".Contains(input), calcData));
			AssertEquals(true, conditionProhibitionWithCondValue.ShouldStop((_, __, input) => "Y".Contains(input), calcData));
			AssertEquals(true, conditionProhibitionWithCondValue.ShouldStop((_, __, input) => "T".Contains(input), calcData));
		}

		public void TestPropertyNamesFromFriends()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var preference = helper.CreatePreferenceForCountry("TT1", "TT1 Description", CoreConstants.CountryCodes.China);
			Factory.Save();
			var refCusConditionCreater = new RefCusConditionCreatorForTest(Factory);
			var cusCondition = refCusConditionCreater.CreateRefCusCondition();
			cusCondition.ZX1_ZZS_Preference = preference.PK;
			Factory.Save();
			AssertEquals("CIQCI", cusCondition.ConditionType);
			AssertEquals("CIQCI DESC", cusCondition.ConditionTypeDescription);
			AssertEquals("TT1", cusCondition.PreferenceCode);
			AssertEquals("TT1 Description", cusCondition.PreferenceDescription);
		}

		public void TestPropertyValueTranslated()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("FR", "French");
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
			Factory.Save();
			var refCusCondition = GetNewBusinessObject() as RefCusCondition;
			var refCusConditionLanguage = Factory.New<RefCusConditionLanguage>();
			refCusConditionLanguage.ZXJ_ZX1_Condition = refCusCondition.PK;
			refCusConditionLanguage.ZXJ_ZX6_NKLanguage = "FR";
			refCusConditionLanguage.ZXJ_Comment = "Quelques mots en français";
			refCusConditionLanguage.ZXJ_Source = "Quelques mots en français 2";
			Factory.Save();
			AssertEquals("ZX1_Comment should be translated.", "Quelques mots en français", new BusinessObjectFactory().Load<RefCusCondition>(refCusCondition.PK).ZX1_Comment);
			AssertEquals("ZX1_Source should be translated.", "Quelques mots en français 2", new BusinessObjectFactory().Load<RefCusCondition>(refCusCondition.PK).ZX1_Source);
		}

		#region Overrides of BusinessObjectBaseTestCase
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RefCusConditionCreatorForTest(Factory).CreateRefCusCondition();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
		#endregion
	}
}
