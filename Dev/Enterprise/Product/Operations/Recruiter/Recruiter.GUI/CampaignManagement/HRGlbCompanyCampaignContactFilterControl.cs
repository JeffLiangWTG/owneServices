using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.GUI.CampaignManagement;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.GUI
{
	public class HRGlbCompanyCampaignContactFilterControl : GlbCompanyCampaignContactFilterControl
	{
		public HRGlbCompanyCampaignContactFilterControl(IBusinessObjectCollection collection, GlbCompanyCampaignContactFilterBusinessObject filterBusinessObject, HRGlbCompanyCampaign campaign)
			: base(collection, filterBusinessObject, campaign)
		{
			grid.RemoveFromAvailableColumns(new string[] {
				ViewCampaignContactSchema.VCC_OrgCode.Name,
				ViewCampaignContactSchema.VCC_OrgFullName.Name,
				ViewCampaignContactSchema.VCC_JobCategory.Name,
				"Header+OverallSalesRepStaff"
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new HRGlbCompanyCampaignContactFilterBusinessObject FilterBusinessObject
		{
			get { return (HRGlbCompanyCampaignContactFilterBusinessObject)base.FilterBusinessObject; }
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new HRCampaignFilterStrip();
		}

		protected override void SetDefaultDataSource()
		{
			var defaultDataSource = (Campaign != null && Campaign.IsInDatabase) ? ZString.Empty : HRContactDataSourceList.Codes.Staff;
			HRCampaignContactFilterDataSourceHelper.UpdateFilterBusinessObjectLayoutContext(FilterBusinessObject, (HRGlbCompanyCampaign)Campaign, defaultDataSource);
		}
		protected override ZBool ShouldPerformSearch()
		{
			ZBool result = base.ShouldPerformSearch();
			if (Campaign != null && result)
			{
				if (Campaign.ContactDataSource == HRContactDataSourceList.Codes.Staff)
				{
					result = Env.Security.HRCampaignManagementEditFindByStaff.IsAllowed;
					if (!result)
					{
						Env.Security.HRCampaignManagementEditFindByStaff.ShowError();
					}
				}
				else if (Campaign.ContactDataSource == HRContactDataSourceList.Codes.JobApplicant)
				{
					result = Env.Security.HRCampaignManagementEditFindByApplicants.IsAllowed;
					if (!result)
					{
						Env.Security.HRCampaignManagementEditFindByApplicants.ShowError();
					}
				}
			}
			return result;
		}
	}
}
