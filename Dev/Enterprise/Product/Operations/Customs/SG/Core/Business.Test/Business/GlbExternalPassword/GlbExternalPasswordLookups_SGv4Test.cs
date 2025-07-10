using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	class GlbExternalPasswordLookups_SGv4Test : BusinessObjectLookupsTestCase
	{
		public void TestPasswordStatusList()
		{
			var externalPassword = Factory.New<GlbExternalPassword_SGv4>();
			var list1 = externalPassword.Lookups.PasswordStatusList;
			var list2 = externalPassword.Lookups.PasswordStatusList;
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			AssertEquals(9, list1.Count);
			AssertEquals(PasswordStatusList.Descriptions.Invalid, list1.GetDescriptionFromCode("INV"));
			AssertEquals(PasswordStatusList.Descriptions.PasswordOK, list1.GetDescriptionFromCode("OK"));
			AssertEquals(SGDeactivationCodes.Descriptions.ANE, list1.GetDescriptionFromCode("ANE"));
			AssertEquals(SGDeactivationCodes.Descriptions.FRZ, list1.GetDescriptionFromCode("FRZ"));
			AssertEquals(SGDeactivationCodes.Descriptions.IID, list1.GetDescriptionFromCode("IID"));
			AssertEquals(SGDeactivationCodes.Descriptions.PCH, list1.GetDescriptionFromCode("PCH"));
			AssertEquals(SGDeactivationCodes.Descriptions.PCS, list1.GetDescriptionFromCode("PCS"));
			AssertEquals(SGDeactivationCodes.Descriptions.PEX, list1.GetDescriptionFromCode("PEX"));
			AssertEquals(SGDeactivationCodes.Descriptions.PIC, list1.GetDescriptionFromCode("PIC"));
		}
	}
}
