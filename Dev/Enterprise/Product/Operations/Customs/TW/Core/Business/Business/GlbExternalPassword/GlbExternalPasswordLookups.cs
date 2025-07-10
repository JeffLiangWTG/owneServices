using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class GlbExternalPasswordLookups : MasterFiles.Business.GlbExternalPasswordLookups
	{
		public GlbExternalPasswordLookups(GlbExternalPassword parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList PasswordTypeList
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.TW.Business.GlbExternalPasswordLookups.PasswordTypeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(PasswordTypesList.Codes.TVA, PasswordTypesList.Descriptions.TVA);
					result.AddPair(PasswordTypesList.Codes.UVC, PasswordTypesList.Descriptions.UVC);
					return result;
				});
			}
		}
	}
}
