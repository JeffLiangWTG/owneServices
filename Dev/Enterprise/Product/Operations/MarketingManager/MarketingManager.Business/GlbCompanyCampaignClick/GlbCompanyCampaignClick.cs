using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignClick : AutoGlbCompanyCampaignClick,
		IGlbCompanyCampaignClick
	{
		public GlbCompanyCampaignClick(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && Link != null && Link.Campaign != null && CampaignItem != null)
			{
				Link.Campaign.TransitionAndSchedule(new[] { CampaignItem.PK });
			}
		}

		[RelatedBusinessObject("Link")]
		public override ZGuid GCC_GCL
		{
			get { return base.GCC_GCL; }
			set { base.GCC_GCL = value; }
		}

		public virtual GlbCompanyCampaignLink Link
		{
			get { return Factory.Load<GlbCompanyCampaignLink>(GCC_GCL); }
		}

		public virtual GlbCompanyCampaignItem CampaignItem
		{
			get { return Factory.Load<GlbCompanyCampaignItem>(GCC_G8_Recipient); }
		}

		public ZString CampaignName
		{
			get
			{
				return Link != null ? Link.Campaign.G0_CampaignNameMultilingual : ZString.Empty;
			}
		}

		public ZString ContactName
		{
			get { return CampaignItem != null ? CampaignItem.ContactName : ZString.Empty; }
		}

		public ZString ContactEmail
		{
			get { return CampaignItem != null ? CampaignItem.EmailAddress : ZString.Empty; }
		}

		public ZString OrgCode
		{
			get { return CampaignItem != null && CampaignItem.ClientOrg != null ? CampaignItem.ClientOrg.OH_Code : ZString.Empty; }
		}

		public ZString OrgName
		{
			get { return CampaignItem != null ? CampaignItem.OrganisationFullName : ZString.Empty; }
		}

		public ZBool IsTracking
		{
			get { return Link != null ? Link.GCL_IsTracked : ZBool.False; }
		}

		public ZString TrackingContext
		{
			get { return Link != null ? Link.GCL_Context : ZString.Empty; }
		}

		public ZBool IsImage
		{
			get { return Link != null ? Link.GCL_IsImage : ZBool.False; }
		}

		public ZString DestinationURL
		{
			get { return Link != null ? Link.GCL_URL : ZString.Empty; }
		}

		public ZDateTime LocalActivityTime
		{
			get { return GCC_ClickTimeUtc.ToLocalBranchTime(Factory); }
		}
	}
}
