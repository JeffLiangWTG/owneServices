using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Integration.Customs.US;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class DispositioinCodeListLoaderTest : TestCaseWithFactory
	{
		public void TestIDispositionCodeListLoader()
		{
			var codeTypeString = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(codeTypeString, codeTypeString, dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z1", "Z1", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z2", "Z2", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z3", "Z3", startDate, endDate);
			Factory.Save();

			var loader = ObjectFactory.Get<IDispositionCodeListLoader>();
			var newFactory = new BusinessObjectFactory();
			var collection = (ZZRefCusCodeListCombinedCollection)loader.GetAMSDispositionCollection(newFactory);
			collection.Load();
			AssertEquals(3, collection.Count);
			Assert(collection.Contains(code1.PK));
			Assert(collection.Contains(code2.PK));
			Assert(collection.Contains(code3.PK));

			var filters = collection.FilterBusinessObjectDefaults;
			AssertEquals("List Type:Property", codeTypeString, filters["List Type:Property"].Value);
			AssertEquals("EffectiveDate:Property1", ZDateTime.Today, filters["Effective Date:Property1"].Value);
		}

		public void TestDispositionCodeListLoader()
		{
			var codeTypeString = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(codeTypeString, "AMSAD", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition, "IMakeBondCloseDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition6263, "IMakeBondCloseDisposition6263", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INeutralInBondDisposition, "INeutralInBondDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName4 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, "IsExamDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName5 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, "IsHoldDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName6 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, "IsHoldExamRemovedDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName7 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, "HoldRemovedExamCompletedMapCode", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z1", "Z1", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z2", "Z2", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z3", "Z3", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z4", "Z4", startDate, endDate);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z5", "Z5", startDate, endDate);
			var code6 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z6", "Z6", startDate, endDate);
			var code7 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z7", "Z7", startDate, endDate);
			var code9 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "A7", "A7", startDate, endDate);
			var attribute11 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName1.ZXE_Name, "Y");
			var attribute12 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName2.ZXE_Name, "Y");
			var attribute21 = helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName2.ZXE_Name, "Y");
			var attribute22 = helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName3.ZXE_Name, "Y");
			var attribute31 = helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, attributeName3.ZXE_Name, "Y");
			var attribute32 = helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, attributeName4.ZXE_Name, "Y");
			var attribute41 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName4.ZXE_Name, "Y");
			var attribute42 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName5.ZXE_Name, "Y");
			var attribute51 = helper.CreateNewOrGetExistingCusCodeListAttribute(code5.PK, attributeName5.ZXE_Name, "Y");
			var attribute52 = helper.CreateNewOrGetExistingCusCodeListAttribute(code5.PK, attributeName6.ZXE_Name, "Y");
			var attribute61 = helper.CreateNewOrGetExistingCusCodeListAttribute(code6.PK, attributeName6.ZXE_Name, "Y");
			var attribute62 = helper.CreateNewOrGetExistingCusCodeListAttribute(code6.PK, attributeName7.ZXE_Name, code7.ZZD_Code);
			var attribute71 = helper.CreateNewOrGetExistingCusCodeListAttribute(code7.PK, attributeName7.ZXE_Name, code1.ZZD_Code);
			var attribute72 = helper.CreateNewOrGetExistingCusCodeListAttribute(code7.PK, attributeName1.ZXE_Name, "Y");
			Factory.Save();
			var dispositionCodes = DispositionCodeListLoader.GetDispositionCodes(Factory, codeTypeString);
			AssertEquals(nameof(CodeDescriptionPairList), dispositionCodes.GetType().Name);
			var count = dispositionCodes.Count;
			var newFactory = new BusinessObjectFactory();
			var newHelper = new Universal.Testing.UniversalReferenceTestDataHelper(newFactory);
			var code8 = newHelper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z8", "Z8", startDate, endDate);
			newFactory.Save();
			dispositionCodes = DispositionCodeListLoader.GetDispositionCodes(Factory, codeTypeString);
			AssertEquals(count, dispositionCodes.Count);
			dispositionCodes = DispositionCodeListLoader.GetDispositionCodes(newFactory, codeTypeString);
			AssertEquals(count + 1, dispositionCodes.Count);
			AssertEquals("First code.", "A7", dispositionCodes[0].Code);
			AssertEquals("Last code.", "Z8", dispositionCodes[dispositionCodes.Count - 1].Code);
			CombineAssertions(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition, () =>
			{
				AssertEquals(true, DispositionCodeListLoader.IsInBondClosed(Factory, code1.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsInBondClosed(Factory, code2.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondClosed(Factory, code3.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondClosed(Factory, code4.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondClosed(Factory, code5.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondClosed(Factory, code6.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsInBondClosed(Factory, code7.ZZD_Code, codeTypeString));
			});
			CombineAssertions(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition6263, () =>
			{
				AssertEquals(true, DispositionCodeListLoader.IsInBondClosedForType62And63(Factory, code1.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsInBondClosedForType62And63(Factory, code2.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondClosedForType62And63(Factory, code3.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondClosedForType62And63(Factory, code4.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondClosedForType62And63(Factory, code5.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondClosedForType62And63(Factory, code6.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondClosedForType62And63(Factory, code7.ZZD_Code, codeTypeString));
			});
			CombineAssertions(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INeutralInBondDisposition, () =>
			{
				AssertEquals(false, DispositionCodeListLoader.IsInBondDispositionCode(Factory, code1.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsInBondDispositionCode(Factory, code2.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsInBondDispositionCode(Factory, code3.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondDispositionCode(Factory, code4.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondDispositionCode(Factory, code5.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondDispositionCode(Factory, code6.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsInBondDispositionCode(Factory, code7.ZZD_Code, codeTypeString));
			});
			CombineAssertions(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, () =>
			{
				AssertEquals(false, DispositionCodeListLoader.IsExam(Factory, code1.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsExam(Factory, code2.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsExam(Factory, code3.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsExam(Factory, code4.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsExam(Factory, code5.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsExam(Factory, code6.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsExam(Factory, code7.ZZD_Code, codeTypeString));
			});
			CombineAssertions(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, () =>
			{
				AssertEquals(false, DispositionCodeListLoader.IsHold(Factory, code1.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHold(Factory, code2.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHold(Factory, code3.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsHold(Factory, code4.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsHold(Factory, code5.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHold(Factory, code6.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHold(Factory, code7.ZZD_Code, codeTypeString));
			});
			CombineAssertions(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, () =>
			{
				AssertEquals(false, DispositionCodeListLoader.IsHoldExamRemoved(Factory, code1.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldExamRemoved(Factory, code2.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldExamRemoved(Factory, code3.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldExamRemoved(Factory, code4.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsHoldExamRemoved(Factory, code5.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsHoldExamRemoved(Factory, code6.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldExamRemoved(Factory, code7.ZZD_Code, codeTypeString));
			});
			CombineAssertions(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, () =>
			{
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code1.ZZD_Code, code7.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code2.ZZD_Code, code7.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code3.ZZD_Code, code7.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code4.ZZD_Code, code7.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code5.ZZD_Code, code7.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code6.ZZD_Code, code7.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code7.ZZD_Code, code6.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code1.ZZD_Code, code6.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code2.ZZD_Code, code1.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code3.ZZD_Code, code1.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code4.ZZD_Code, code1.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code5.ZZD_Code, code1.ZZD_Code, codeTypeString));
				AssertEquals(false, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code6.ZZD_Code, code1.ZZD_Code, codeTypeString));
				AssertEquals(true, DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(Factory, code7.ZZD_Code, code1.ZZD_Code, codeTypeString));
			});
		}
	}
}
