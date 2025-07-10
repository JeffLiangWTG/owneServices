using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.N5203
{
	class NotifyParty : PartyDetails
	{
		public NotifyParty(JobDeclaration declaration, OrgHeader orgHeader) : base(declaration, orgHeader)
		{
		}

		protected override ZString LPCOAuthorizedPartyIDCore => ZString.Empty;

		protected override ZString IDCore => SharedHelper.GetIDStartWithNO(base.IDCore, TypeCodeCore);
	}
}
