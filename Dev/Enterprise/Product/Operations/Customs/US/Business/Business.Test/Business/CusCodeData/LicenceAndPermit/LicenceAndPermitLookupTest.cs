using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class LicenceAndPermitLookupTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "99", "TEST code", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();

			var permit = Factory.New<LicenceAndPermit>();
			AssertEquals("CY_CodeList.Count", 1, permit.Lookups.CY_CodeList.Count);
			AssertEquals("Lookups.CY_CodeList[0]", "99", permit.Lookups.CY_CodeList[0].Code);
		}
	}
}
