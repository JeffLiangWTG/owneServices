using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class GlbCompanyCredentialLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPasswordTypesList()
		{
			var externalPassword = Factory.New<GlbCompanyCredential>();
			externalPassword.GP_PasswordType = PasswordTypesList.Codes.TVF;
			var list1 = externalPassword.Lookups.PasswordTypeList;
			CombineAssertions(() =>
			{
				AssertEquals(1, list1.Count);
				AssertEquals(PasswordTypesList.Descriptions.TVF, list1.GetDescriptionFromCode("TVF"));
				AssertNull(list1.GetDescriptionFromCode("NXM"));
			});

			externalPassword.GP_PasswordType = PasswordTypesList.Codes.NXM;
			var list2 = externalPassword.Lookups.PasswordTypeList;
			CombineAssertions(() =>
			{
				AssertEquals(1, list2.Count);
				AssertNull(list2.GetDescriptionFromCode("TVF"));
				AssertEquals(PasswordTypesList.Descriptions.NXM, list2.GetDescriptionFromCode("NXM"));
			});
		}
	}
}
