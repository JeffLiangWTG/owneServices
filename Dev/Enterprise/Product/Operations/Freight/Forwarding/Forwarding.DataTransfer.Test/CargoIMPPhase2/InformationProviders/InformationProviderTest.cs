using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class InformationProviderTest : TestCaseWithFactory
	{
		public void TestGetPartyIDValidCharacters()
		{
			InformationProviderTestClass provider = new InformationProviderTestClass(Factory);

			OrgHeader org = Factory.New<OrgHeader>();

			org.OH_Code = "ABCD0123 -./";
			AssertEquals((ZString)"ABCD0123 -./", provider.ExposeGetPartyId(org));

			org.OH_Code = "Ab!@01?,\\-/.";
			AssertEquals((ZString)"AB01-/.", provider.ExposeGetPartyId(org));
		}
	}
}
