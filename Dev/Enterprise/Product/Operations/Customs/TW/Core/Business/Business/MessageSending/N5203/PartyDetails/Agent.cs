using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.N5203
{
	class Agent : PartyDetails
	{
		public Agent(JobDeclaration declaration, OrgAddress orgAddress) : base(declaration, orgAddress?.Header, orgAddress)
		{
		}

		protected override ZString IDCore => declaration.CusEntryInstruction?.CEI_BoxNumber ?? ZString.Empty;

		protected override ZString TypeCodeCore => ZString.Empty;

		protected override ZString RoleCodeCore => MessageConstants.RoleCodeCB;

		protected override ZString SubBoxIDCore => SharedHelper.ExtractSubBoxID(declaration.JE_CustomsProfile);
	}
}
