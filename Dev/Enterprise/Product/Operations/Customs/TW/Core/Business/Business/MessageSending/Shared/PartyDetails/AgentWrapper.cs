using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class AgentWrapper : PartyDetailsWrapper
	{
		public AgentWrapper(ZString id, ZString roleCode, ZString subBoxId, OrgAddress orgAddress)
			: base(id, roleCode, subBoxId, orgAddress)
		{
		}

		protected override ZString LPCOAuthorizedPartyIDCore => GetAEONumber();
	}
}
