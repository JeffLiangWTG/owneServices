using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class ContextLinkActivityModuleFilter : LinkActivityModuleFilter
	{
		public ContextLinkActivityModuleFilter(ZString description, FilterStripBusinessObject filterStripBusinessObject, GlbCompanyCampaign campaign)
			: base(description, filterStripBusinessObject, campaign)
		{
		}

		protected override string XmlElementName
		{
			get
			{
				return "LinkActivityContext";
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
				if (trackingContextList == null)
				{
					var list = new TrackingLinkList(new[] { base.Campaign });
					trackingContextList = list.ListByContext();
				}
				return trackingContextList;
			}
		}
		TrackingLinkList trackingContextList;
	}
}
