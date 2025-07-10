using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Environment.Business
{
	class OrgAddressExtensionsTest : TestCaseWithFactory
	{
		#region TestGetCountryCode

		public void TestGetCountryCode()
		{
			AssertEquals("", ((OrgAddress)null).GetCountryCode());

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = "";
			AssertEquals("", orgAddress.GetCountryCode());

			orgAddress.OA_RN_NKCountryCode = "US";
			AssertEquals("US", orgAddress.GetCountryCode());
		}

		#endregion
	}
}
