using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.Business.Testing
{
	class RefContainerMapProviderTest : TestCaseWithFactory
	{
		public void TestIsUsageNeeded()
		{
			AssertEquals(UsageRequirement.MayRequire, usRefContainerCodeMap.ContainerMapProvider?.IsUsageNeeded);
		}

		public void TestUsageList()
		{
			var usageList = usRefContainerCodeMap.Lookups.UsageList;

			Assert(usageList.Count == 1);
			AssertEquals(true, usageList.ContainsCode("AMS"));
			AssertEquals("US AMS", usageList.GetDescriptionFromCode("AMS"));
		}

		public void TestCodeList()
		{
			var codeList = usRefContainerCodeMap.Lookups.CodeList;

			Assert(codeList.Count == 176);
			AssertEquals(true, codeList.ContainsCode("20T0"));
			AssertEquals("Minimum pressure 0,45 bar.", codeList.GetDescriptionFromCode("20T0"));
			AssertEquals(true, codeList.ContainsCode("20T1"));
			AssertEquals("Minimum pressure 1,5 bar.", codeList.GetDescriptionFromCode("20T1"));
			AssertEquals(true, codeList.ContainsCode("20T2"));
			AssertEquals("Minimum pressure 2,65 bar.", codeList.GetDescriptionFromCode("20T2"));
			AssertEquals(true, codeList.ContainsCode("20T3"));
			AssertEquals("Minimum pressure 1,5 bar.", codeList.GetDescriptionFromCode("20T3"));
			AssertEquals(true, codeList.ContainsCode("20T4"));
			AssertEquals("Minimum pressure 2,65 bar.", codeList.GetDescriptionFromCode("20T4"));
			AssertEquals(true, codeList.ContainsCode("20T5"));
			AssertEquals("Minimum pressure 4,0 bar.", codeList.GetDescriptionFromCode("20T5"));
			AssertEquals(true, codeList.ContainsCode("20T6"));
			AssertEquals("Minimum pressure 6,0 bar.", codeList.GetDescriptionFromCode("20T6"));
			AssertEquals(true, codeList.ContainsCode("20T7"));
			AssertEquals("Minimum pressure 9,1 bar.", codeList.GetDescriptionFromCode("20T7"));
			AssertEquals(true, codeList.ContainsCode("20T8"));
			AssertEquals("Minimum pressure 22 bar.", codeList.GetDescriptionFromCode("20T8"));
			AssertEquals(true, codeList.ContainsCode("20T9"));
			AssertEquals("Minimum pressure (to be developed)", codeList.GetDescriptionFromCode("20T9"));

			usRefContainerCodeMap.RCM_Usage = USContainerUsageList.Codes.AMS;
			codeList = usRefContainerCodeMap.Lookups.CodeList;
			Assert(codeList.Count == 182);
			AssertEquals(true, codeList.ContainsCode(USContainerCodeList.Codes._25T2));
			AssertEquals(true, codeList.ContainsCode(USContainerCodeList.Codes._22T0));
			AssertEquals(true, codeList.ContainsCode(USContainerCodeList.Codes._22T5));
			AssertEquals(true, codeList.ContainsCode(USContainerCodeList.Codes._22T7));
			AssertEquals(true, codeList.ContainsCode(USContainerCodeList.Codes._22T8));
			AssertEquals(true, codeList.ContainsCode(USContainerCodeList.Codes._42T0));

			AssertEquals(true, Factory.TryGetValueFromCacheOnly("Enterprise.Customs.US.Business.RefContainerMapProvider.AMSCodeList", out UntranslatableCodeDescriptionPairList newList));
			AssertEquals(codeList, newList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			usRefContainerCodeMap = Factory.NewWithValidTestData<RefContainerCodeMap>();
			usRefContainerCodeMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
		}
		RefContainerCodeMap usRefContainerCodeMap;
	}
}
