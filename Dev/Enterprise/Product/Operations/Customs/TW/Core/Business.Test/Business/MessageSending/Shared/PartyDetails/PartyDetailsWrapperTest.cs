using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PartyDetailsWrapperTest : PartyDetailsWrapperAbstractTest<PartyDetailsWrapper>
	{
		protected override PartyDetailsWrapper GetPartyDetailsWrapper(OrgAddress orgAddress) => new PartyDetailsWrapper(idForTesting, roleCodeForTesting, subBoxIDForTesting, orgAddress);
	}
}
