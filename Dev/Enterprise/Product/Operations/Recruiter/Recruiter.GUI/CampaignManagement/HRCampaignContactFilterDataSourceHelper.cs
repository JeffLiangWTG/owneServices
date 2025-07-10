using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.GUI
{
	public static class HRCampaignContactFilterDataSourceHelper
	{
		internal static ZString GetCampaignContactDataSource(BusinessObjectFactory factory)
		{
			var lastUsedCampaignDataSource = MarketingManager.GUI.CampaignContactFilterDataSourceHelper.GetLastUsedCampaignDatasourceStmDataForCompany(factory);
			if (lastUsedCampaignDataSource != null)
			{
				var contactDataSource = lastUsedCampaignDataSource.SD_Name;
				var suffixes = contactDataSource.Split(new char[] { '_' });

				if (suffixes.Length > 1 && suffixes[1].Equals(HRContactDataSourceList.FilterModuleIDSuffixes.JobApplicant))
				{
					return HRContactDataSourceList.Codes.JobApplicant;
				}
			}
			return HRContactDataSourceList.Codes.Staff;
		}

		public static void UpdateFilterBusinessObjectLayoutContext(HRGlbCompanyCampaignContactFilterBusinessObject filterBusinessObject, HRGlbCompanyCampaign campaign, ZString defaultContext)
		{
			if (campaign != null)
			{
				campaign.ContactDataSource = defaultContext.IsEmpty ? GetCampaignContactDataSource(filterBusinessObject.LastUsedCampaignContactLayoutFactory) : defaultContext;
				((IFilterStripBusinessObjectInternals)filterBusinessObject).LayoutContext = filterBusinessObject.CurrentLayoutContext;
			}
		}
	}
}
