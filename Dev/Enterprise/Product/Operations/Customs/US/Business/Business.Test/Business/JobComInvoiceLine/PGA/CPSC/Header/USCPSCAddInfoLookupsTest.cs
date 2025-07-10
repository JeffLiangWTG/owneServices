using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCPSCAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganizations()
		{
			AssertType<ConsignorCollection>(lookups.Organizations);
		}

		public void TestProcessingCodeList()
		{
			var processingCodeList = lookups.ProcessingCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "FCP, FGC, REF", processingCodeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<CPSCProcessingCodeList>(), processingCodeList);
			});
		}

		public void TestProductIDTypeCodeList()
		{
			var productIDTypeCodeList = lookups.ProductIDTypeCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "AI, SRV", productIDTypeCodeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<ProductIDTypeCodeList>(), productIDTypeCodeList);
			});
		}

		public void TestIntendedUseCodeList()
		{
			var intendedUseCodeList = lookups.IntendedUseCodeList;
			CombineAssertions(() =>
			{
				AssertNotNull(intendedUseCodeList);
				AssertEquals(typeof(CodeDescriptionPairList), intendedUseCodeList.GetType());
				var codeString = intendedUseCodeList.CodesAsString;
				AssertContains("980.000", codeString);
				AssertContains("130.000", codeString);
				AssertNotContains("981.000", codeString);
			});
		}

		public void TestYesNoList()
		{
			AssertSame(YesNoDefaultList.GetCachedYesNoList(Factory), lookups.YesNoList);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, "Test");
			var intendedUseCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, "130.000", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(intendedUseCode.PK, RefCusCodeListAttributeTypes.Codes.PGAIUCAgency, "CPS");

			intendedUseCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, "980.000", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(intendedUseCode.PK, RefCusCodeListAttributeTypes.Codes.PGAIUCAgency, "CPS");

			intendedUseCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, "981.000", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(intendedUseCode.PK, RefCusCodeListAttributeTypes.Codes.PGAIUCAgency, "CPS");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, "981.000", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var headre = Factory.New<CPSCHeader>();
			var addInfo = new CPSCHeaderAddInfo(headre.B7_AddInfoDataInfo);
			lookups = new USCPSCAddInfoLookups(addInfo);
		}
		USCPSCAddInfoLookups lookups;
	}
}
