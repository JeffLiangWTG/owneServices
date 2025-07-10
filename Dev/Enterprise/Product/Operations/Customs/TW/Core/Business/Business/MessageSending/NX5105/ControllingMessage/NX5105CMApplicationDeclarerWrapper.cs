using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class NX5105CMApplicationDeclarerWrapper : NX5105PartyDetailsWrapper
	{
		public NX5105CMApplicationDeclarerWrapper(OrgAddress orgAddress) : base(orgAddress, orgAddress, OrgCusCode.CodeTypes.VATCode)
		{
			Argument.NotNull(orgAddress, nameof(orgAddress));
		}

		protected override ZString TypeCodeCore => PartyIdentifierCodeList.Codes._58;
		protected override ZString Communications1IdCore => orgAddress.OA_Phone;
		protected override ZString Communications2IdCore => orgAddress.OA_Email;
	}
}
