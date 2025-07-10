using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCusCode.Loader))]
	sealed class OrgCusCodeLoaderTest : LoaderTestCase
	{
		public void TestGetQuery()
		{
			var organisation = Factory.New<OrgHeader>();
			var code1 = organisation.CustomsCodes.AddNew("AAA", "123456");
			var code2 = organisation.CustomsCodes.AddNew("BBB", "123456");

			var companyCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var query = OrgCusCode.Loader.GetQuery(companyCode, "AAA", "123456");
			AssertEquals(code1, Factory.LoadTop1<OrgCusCode>(query));

			query = OrgCusCode.Loader.GetQuery(companyCode, "BBB", "123456");
			AssertEquals(code2, Factory.LoadTop1<OrgCusCode>(query));
		}

		public void TestLoad()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgCusCode code1 = organisation.CustomsCodes.AddNew("AAA", "123456");
			OrgCusCode code2 = organisation.CustomsCodes.AddNew("BBB", "123456");

			ZString companyCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			OrgCusCode[] result = new OrgCusCode.Loader(Factory).Load(companyCode, "AAA", "123456");
			AssertEquals(1, result.Length);
			AssertEquals(code1, result[0]);

			result = new OrgCusCode.Loader(Factory).Load(companyCode, "BBB", "123456");
			AssertEquals(1, result.Length);
			AssertEquals(code2, result[0]);
		}

		public void TestLoadRegardlesPremisesAddress()
		{
			OrgHeader organisation1 = Factory.New<OrgHeader>();
			OrgHeader organisation2 = Factory.New<OrgHeader>();
			OrgCusCode code1 = organisation1.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "11111", "NZ");
			OrgCusCode code2 = organisation2.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "22222", "NZ");
			var premisesHeader = Factory.New<OrgHeader>();
			var premisesAddress = Factory.New<OrgAddress>();
			premisesAddress.OA_OH = premisesHeader.PK;
			code1.OK_OA_PremisesAddress = premisesAddress.PK;
			var result = new OrgCusCode.Loader(Factory).LoadRegardlesPremisesAddress(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "NZ", organisation1.PK);
			AssertEquals("11111", result.OK_CustomsRegNo);
			result = new OrgCusCode.Loader(Factory).LoadRegardlesPremisesAddress(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "NZ", organisation2.PK);
			AssertEquals("22222", result.OK_CustomsRegNo);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new OrgCusCode.Loader(Factory);
		}
	}
}
