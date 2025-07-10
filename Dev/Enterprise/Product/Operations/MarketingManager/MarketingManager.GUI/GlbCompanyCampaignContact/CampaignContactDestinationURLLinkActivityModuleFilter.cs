using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class CampaignContactDestinationURLLinkActivityModuleFilter : CampaignContactLinkActivityModuleFilter
	{
		public CampaignContactDestinationURLLinkActivityModuleFilter(ZString description, FilterStripBusinessObject filterStripBusinessObject, GlbCompanyCampaign campaign)
			: base(description, filterStripBusinessObject, campaign)
		{
			filterBusinessObject = filterStripBusinessObject as GlbCompanyCampaignContactFilterBusinessObject;
		}

		readonly GlbCompanyCampaignContactFilterBusinessObject filterBusinessObject;

		protected override string XmlElementName
		{
			get
			{
				return "CampaignContactLinkActivityDestinationURL";
			}
		}

		protected override CargoWise.Schema.SchemaStringColumn QueryColumn
		{
			get
			{
				return GlbCompanyCampaignLinkSchema.GCL_URL;
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
					trackingURLList = list.ListByURL();
				}
				else
				{
					trackingURLList = base.LinkTrackingList;
				}
				return trackingURLList;
			}
		}
		TrackingLinkList trackingURLList;
	}
}
