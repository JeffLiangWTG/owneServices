using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class CampaignContactContextLinkActivityModuleFilter : CampaignContactLinkActivityModuleFilter
	{
		public CampaignContactContextLinkActivityModuleFilter(ZString description, FilterStripBusinessObject filterStripBusinessObject, GlbCompanyCampaign campaign)
			: base(description, filterStripBusinessObject, campaign)
		{
			filterBusinessObject = filterStripBusinessObject as GlbCompanyCampaignContactFilterBusinessObject;
		}

		readonly GlbCompanyCampaignContactFilterBusinessObject filterBusinessObject;

		protected override string XmlElementName
		{
			get
			{
				return "CampaignContactLinkActivityContext";
			}
		}

		protected override CargoWise.Schema.SchemaStringColumn QueryColumn
		{
			get
			{
				return GlbCompanyCampaignLinkSchema.GCL_Context;
			}
		}

		public override TrackingLinkList LinkTrackingList
		{
			get
			{
				if (filterBusinessObject.SourceCampaignPKs.Length != 0)
				{
					var query = new ZQuery(GlbCompanyCampaignSchema.PK, filterBusinessObject.SourceCampaignPKs);

					var sourceCampaign = filterBusinessObject.Factory.Load<GlbCompanyCampaign>(query);
					var list = new TrackingLinkList(sourceCampaign);
					trackingContextList = list.ListByContext();
				}
				else
				{
					trackingContextList = base.LinkTrackingList;
				}
				return trackingContextList;
			}
		}
		TrackingLinkList trackingContextList;
	}
}
