using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class NX5105NotifyPartyWrapper : NX5105PartyDetailsWrapper
	{
		public NX5105NotifyPartyWrapper(OrgAddress orgAddress, params string[] codesToLookFor)
			: base(orgAddress, orgAddress, codesToLookFor)
		{
		}
		protected override ZString TypeCodeCore => GetTypeCodeBasedOnOrgCusCode();

		protected override ZString IDCore => SharedHelper.GetIDStartWithNO(base.IDCore, TypeCodeCore);
	}
}
