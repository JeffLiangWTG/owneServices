using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class TWConsignorOrConsigneeAddressLookups : JobDocAddressLookups
	{
		public TWConsignorOrConsigneeAddressLookups(TWConsignorOrConsigneeAddress parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList GovRegNumTypes => Factory.GetCachedValue(
			"Enterprise.Customs.TW.Business.TWConsignorOrConsigneeAddressLookups.GovRegNumTypes",
			() => Factory.GetCodeTypeList(OrgCusCode.CodeTypes.VATCode, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.CodeTypes.PassportID, Constants.CCPPrefix)
		);
	}
}
