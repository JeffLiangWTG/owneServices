using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USStateListTest : TestCaseWithFactory
	{
		public void TestUSStateListIsApplicableToNonUSCompanyLoggedIn()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			Assert(Factory.GetCachedUSStateList().ContainsCode(USStatesList.Codes.Pennsylvania));
		}

		public void TestGetCachedUSStateList()
		{
			var list = Factory.GetCachedUSStateList();
			AssertEquals(list, Factory.GetCachedUSStateList());
			AssertEquals("State description", "Texas", list.GetDescriptionFromCode("TX"));
		}

		public void TestGetCachedUSStateAndDistrictList()
		{
			var list = Factory.GetCachedUSStateAndDistrictList();
			Assert(!list.ContainsCode("UM"));
		}

		public void TestGetCachedUSStateListForVehicles()
		{
			var list = Factory.GetCachedUSStateListForVehicles();
			AssertEquals(list, Factory.GetCachedUSStateListForVehicles());
			var stateList = Factory.GetCachedUSStateList();
			AssertEquals(stateList.Count + 1, list.Count);
			foreach (ICodeDescription pair in stateList)
			{
				AssertEquals(pair.Description, list.GetDescriptionFromCode(pair.Code));
			}
			AssertEquals("State description", "Use for diplomatic vehicle", list.GetDescriptionFromCode("US"));
		}
	}
}
