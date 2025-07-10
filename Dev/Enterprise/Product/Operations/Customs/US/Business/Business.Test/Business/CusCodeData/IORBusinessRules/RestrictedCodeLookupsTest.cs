using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class RestrictedCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var restrictedCode = Factory.New<RestrictedCode>();
			AssertEquals(typeof(RestrictedCodeTypeList), restrictedCode.Lookups.CY_CodeList.GetType());
		}

		public void TestRestrictedCodeList()
		{
			var restrictedCode = Factory.New<RestrictedCode>();
			restrictedCode.CY_Code = RestrictedCodeTypeList.Codes.RestrictedEntryType;
			AssertEquals(typeof(EntryTypeList), restrictedCode.Lookups.RestrictedCodeList.GetType());
			restrictedCode.CY_Code = RestrictedCodeTypeList.Codes.RestrictedSPI;
			AssertEquals(typeof(SPICompleteList), restrictedCode.Lookups.RestrictedCodeList.GetType());
			restrictedCode.CY_Code = RestrictedCodeTypeList.Codes.RestrictedTariff;
			AssertEquals(typeof(CodeDescriptionPairList), restrictedCode.Lookups.RestrictedCodeList.GetType());
		}
	}
}
