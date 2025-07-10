using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class GlbExternalPasswordLookups_SGv4 : GlbExternalPasswordLookups
	{
		public GlbExternalPasswordLookups_SGv4(GlbExternalPassword_SGv4 parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList PasswordStatusList => Factory.GetCachedValue<CodeDescriptionPairList>("GlbExternalPasswordLookups_SGv4_PasswordStatusList", () =>
		{
			var result = new SGDeactivationCodes();
			result.AddPair(MasterFiles.Business.PasswordStatusList.Codes.Invalid, MasterFiles.Business.PasswordStatusList.Descriptions.Invalid);
			result.AddPair(MasterFiles.Business.PasswordStatusList.Codes.PasswordOK, MasterFiles.Business.PasswordStatusList.Descriptions.PasswordOK);
			result.Sort();
			return result;
		});
	}
}
