using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class OrganizationLoaderTest : TestCaseWithFactory
	{
		public void TestLoadOrganisationByCusCode()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "061234-12345");

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "123-12-1234");

			AssertEquals(org, Loader.LoadAllOrganisationByCusCode("91-013199000").ElementAt(0));
			AssertEquals(org1, Loader.LoadAllOrganisationByCusCode("061234-12345").ElementAt(0));
			AssertEquals(org2, Loader.LoadAllOrganisationByCusCode("123-12-1234").ElementAt(0));

			AssertEquals(org, Loader.LoadTop1OrganisationByCusCode("91-013199000"));
			AssertEquals(org1, Loader.LoadTop1OrganisationByCusCode("061234-12345"));
			AssertEquals(org2, Loader.LoadTop1OrganisationByCusCode("123-12-1234"));
		}

		OrganizationLoader Loader
		{
			get { return loader ?? (loader = new OrganizationLoader(Factory)); }
		}
		OrganizationLoader loader;
	}
}
