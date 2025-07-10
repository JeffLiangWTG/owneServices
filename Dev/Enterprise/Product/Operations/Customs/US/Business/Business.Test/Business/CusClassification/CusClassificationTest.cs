using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusClassification))]
	sealed class CusClassificationTest : Customs.Business.Testing.BaseCusClassificationTest
	{
		public void TestCC_DescriptionIsDefaultedFromTariffDescription()
		{
			ZString tariffCode1 = "1020304050";
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = tariffCode1;
			tariff1.UE_Unit1 = "KG";
			tariff1.UE_ShortDescription = "What a lovely day!!";
			tariff1.UE_DateFrom = ZDateTime.Today;
			tariff1.UE_DateTo = ZDateTime.Today.AddYears(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var scheduleB1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, tariffCode1, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), description: "Test Tariff scheduleB1", isSystem: false, ensureDataGroupingExists: true);
			scheduleB1.ZZ1_Description = "How are you today??";
			helper.CreateTariffUOM(scheduleB1, "CU1", "KG");
			ZString tariffCode2 = "2030405060";
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = tariffCode2;
			tariff2.UE_Unit2 = "KG";
			tariff2.UE_ShortDescription = "What a lovely night!!";
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(2);
			var scheduleB2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, tariffCode2, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), description: "Test Tariff scheduleB2", isSystem: false, ensureDataGroupingExists: true);
			scheduleB2.ZZ1_Description = "How are you tonight??";
			helper.CreateTariffUOM(scheduleB2, "CU2", "KG");
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = tariffCode1;
			AssertEquals(tariff1.UE_ShortDescription, classification.CC_Description);
			classification.CC_TariffNum = tariffCode2;
			AssertEquals(tariff2.UE_ShortDescription, classification.CC_Description);
			classification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			classification.CC_TariffNum = tariffCode1;
			AssertEquals(tariff2.UE_ShortDescription, classification.CC_Description);
			classification.CC_Description = ZString.Empty;
			classification.CC_TariffNum = tariffCode2;
			AssertEquals(scheduleB2.ZZ1_Description, classification.CC_Description);
			classification.CC_TariffNum = tariffCode1;
			AssertEquals(scheduleB1.ZZ1_Description, classification.CC_Description);
		}

		public override void TestCC_FormattedTariffNum()
		{
			CusClassification classification = Factory.New<CusClassification>();
			ZString tariff = "1234567890";
			classification.CC_FormattedTariffNum = tariff;
			AssertEquals("CC_FormattedTariffNum", "1234.56.7890", classification.CC_FormattedTariffNum);
			tariff = "9876.54.3210";
			classification.CC_FormattedTariffNum = "9876.54.3210";
			AssertEquals("CC_FormattedTariffNum", tariff, classification.CC_FormattedTariffNum);
		}

		[TestDate(2006, 12, 12)]
		public void TestGetAdditionalDataForBorderWise()
		{
			Classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			IHaveAdditionalDataForBorderWise classificationForBorderWise = Classification as IHaveAdditionalDataForBorderWise;
			AdditionalDataForBorderWise additionalData = classificationForBorderWise.GetAdditionalDataForBorderWise("");
			AssertEquals("AdditionalData.ParameterForBorderWise", "I", additionalData.ParameterForBorderWise);
			AssertEquals("AdditionalData.DateForDutyRate", ZDateTime.Now, additionalData.DateForDutyRate);
			Classification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			additionalData = classificationForBorderWise.GetAdditionalDataForBorderWise("");
			AssertEquals("AdditionalData.ParameterForBorderWise", "E", additionalData.ParameterForBorderWise);
			AssertEquals("AdditionalData.DateForDutyRate", ZDateTime.Now, additionalData.DateForDutyRate);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseCusClassification to include a decider for this class", Factory.New(typeof(Customs.Business.BaseCusClassification)).GetType() == GetExpectedBusinessObjectType());
		}

		public override void TestITariffFormatProvider()
		{
			var classification = Factory.New<CusClassification>();
			AssertType<TariffFormatter>("TariffFormatter", ((ITariffFormatProvider)classification).TariffFormatter);
		}
	}
}
