using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeader.Loader))]
	sealed class OrgHeaderLoaderTest : LoaderTestCase
	{
		public void TestLoadDBOrganisations()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.CustomsCodes.AddNew("AAA", "123456");

			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2.CustomsCodes.AddNew("AAA", "123457");

			Factory.Save();

			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			OrgHeader[] result = new OrgHeader.Loader(Factory).LoadDBOrganisations(countryCode, "AAA", "123456");
			AssertEquals("One result", 1, result.Length);
			AssertEquals(organisation, result[0]);

			result = new OrgHeader.Loader(Factory).LoadDBOrganisations(countryCode, "AAA", "123457");
			AssertEquals("One result", 1, result.Length);
			AssertEquals(organisation2, result[0]);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new OrgHeader.Loader(Factory);
		}
	}
}
