using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class OrganizationTest : TestCaseWithFactory
	{
		public void TestOrganization()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Monsters Inc";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			orgHeader.MainAddress.OA_Address1 = "line 1";

			var code = orgHeader.CustomsCodes.AddNew();
			code.OK_CodeType = "XXX";
			code.OK_CustomsRegNo = "12345";

			var organization = new Organization(orgHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Code", "MONSTESYD", organization.Code);
				AssertEquals("Name", "Monsters Inc", organization.Name);
				AssertEquals("PK", orgHeader.PK, organization.PK);
				AssertEquals("Unloco.Code", "AUSYD", organization.Unloco.Code);
				AssertEquals("MainAddress.AddressLine1", "line 1", organization.MainAddress.AddressLine1);
				AssertEquals("organization.RegistrationNumbers", 1, organization.RegistrationNumbers.Count);
			});
		}

		public void TestNullOrganization()
		{
			var organization = new Organization(null);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Code", organization.Code);
				AssertNullOrEmpty("Name", organization.Name);
				AssertEquals("PK", ZGuid.Empty, organization.PK);
				AssertNotNull("Unloco", organization.Unloco);
				AssertNotNull("MainAddress", organization.MainAddress);
				AssertNotNull("organization.RegistrationNumbers", organization.RegistrationNumbers);
			});
		}
	}
}
