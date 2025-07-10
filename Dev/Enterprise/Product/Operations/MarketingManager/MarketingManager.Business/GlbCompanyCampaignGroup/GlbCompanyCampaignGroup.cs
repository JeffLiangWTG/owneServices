using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignGroup : AutoGlbCompanyCampaignGroup
	{
		public GlbCompanyCampaignGroup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		Collection<GlbCompanyCampaign> touches;
		public Collection<GlbCompanyCampaign> Touches
		{
			get
			{
				if (touches == null)
				{
					touches = new Collection<GlbCompanyCampaign>();
					var query = new ZQuery(GlbCompanyCampaignSchema.G0_GCG_Group, PK);
					var campaignsInGroup = Factory.Load<GlbCompanyCampaign>(query);

					foreach (var touch in campaignsInGroup.Distinct())
					{
						touches.Add(touch);
					}
				}

				return touches;
			}
		}

		public override void Delete()
		{
			base.Delete();
			Factory.Load<GlbCompanyCampaignDripMarketing>(new ZQuery(GlbCompanyCampaignDripMarketingSchema.GCD_GCG_Group, PK)).DeleteAll();
			Factory.Load<GlbCompanyCampaignSendSettings>(new ZQuery(GlbCompanyCampaignSendSettingsSchema.GSC_GCG_Group, PK)).DeleteAll();
		}
	}
}
