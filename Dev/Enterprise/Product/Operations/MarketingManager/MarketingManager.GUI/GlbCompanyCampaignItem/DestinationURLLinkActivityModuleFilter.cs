using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class DestinationURLLinkActivityModuleFilter : LinkActivityModuleFilter
	{
		public DestinationURLLinkActivityModuleFilter(ZString description, FilterStripBusinessObject filterStripBusinessObject, GlbCompanyCampaign campaign)
			: base(description, filterStripBusinessObject, campaign)
		{
		}

		protected override string XmlElementName
		{
			get
			{
				return "LinkActivityDestinationURL";
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
				if (trackingURLList == null)
				{
					var list = new TrackingLinkList(new[] { base.Campaign });
					trackingURLList = list.ListByURL();
				}
				return trackingURLList;
			}
		}
		TrackingLinkList trackingURLList;
	}
}
