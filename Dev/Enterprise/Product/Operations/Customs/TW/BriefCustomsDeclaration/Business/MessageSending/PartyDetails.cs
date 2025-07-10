using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class PartyDetails : PartyDetailsWrapper
	{
		public PartyDetails(OrgAddress orgAddress) : base(orgAddress)
		{
		}

		protected override ZString IDCore => GetCustomsRegNo(orgHeader?.CustomsCodes, OrgCusCode.CodeTypes.VATCode);

		protected override ZString TypeCodeCore => PartyIdentifierCodeList.Codes._58;
	}
}
