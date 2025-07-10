using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class GlbExternalPasswordLookupsTRTest : BusinessObjectLookupsTestCase
	{
		public void TestCertificateAuthorities()
		{
			var externalPassword = Factory.New<GlbExternalPassword_TR>();
			var list1 = externalPassword.Lookups.CertificateAuthorities;
			AssertEquals(5, list1.Count);
			AssertEquals(CertificateAuthorities.Codes.EIMZATR, list1.GetCodeFromDescription("e-İmza TR"));
		}

		public void TestChipsetList()
		{
			var externalPassword = Factory.New<GlbExternalPassword_TR>();
			var list1 = externalPassword.Lookups.ChipsetList;
			AssertEquals(11, list1.Count);
			AssertEquals(ChipsetList.Descriptions.GEMPLUS, list1.GetDescriptionFromCode("GEMPLUS"));
		}
	}
}
