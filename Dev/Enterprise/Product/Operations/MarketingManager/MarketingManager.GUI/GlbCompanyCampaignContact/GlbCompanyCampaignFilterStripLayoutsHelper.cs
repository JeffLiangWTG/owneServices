using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignFilterStripLayoutsHelper : FilterStripLayoutsHelper
	{
		internal ZGuid BizObjPK;
		internal ZGuid AdditionalObjPK;

		protected override ZString GetCurrentUserTablePrefix()
		{
			return GlbCompanyCampaignSchema.Constants.Prefix;
		}

		protected override ZGuid GetCurrentUserPk()
		{
			return BizObjPK;
		}

		protected override ZGuid GetAdditionalEntityID()
		{
			return AdditionalObjPK;
		}
	}
}
