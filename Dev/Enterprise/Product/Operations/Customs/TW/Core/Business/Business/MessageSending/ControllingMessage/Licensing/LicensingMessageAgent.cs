using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageAgent : IDeclarationAgent
	{
		readonly CusTWControllingMessageHeader header;

		public LicensingMessageAgent(CusTWControllingMessageHeader header)
		{
			this.header = header;
		}

		public ZString ID => header.EntryInstruction?.CEI_BoxNumber ?? ZString.Empty;

		public ZString RoleCode => MessageConstants.RoleCodeCB;

		public ZString SubBoxID => SharedHelper.ExtractSubBoxID(header.Declaration?.JE_CustomsProfile ?? ZString.Empty);
	}
}
