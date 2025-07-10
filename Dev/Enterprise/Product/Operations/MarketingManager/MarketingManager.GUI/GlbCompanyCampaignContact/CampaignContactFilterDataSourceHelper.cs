using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public static class CampaignContactFilterDataSourceHelper
	{
		public const string LastUsedLayoutType = "CUR";

		public static StmData GetLastUsedCampaignDatasourceStmDataForCompany(BusinessObjectFactory factory)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StmDataSchema.SD_DepartmentGuid, EnvProxy.Instance.CurrentCompany.PK);
			query.AddToFilter(StmDataSchema.SD_Type, LastUsedLayoutType);

			return factory.LoadTop1<StmData>(query);
		}

#if DEBUG
		public
#endif
		static ZString GetCampaignContactDataSource(BusinessObjectFactory factory)
		{
			StmData lastUsedCampaignDataSource = GetLastUsedCampaignDatasourceStmDataForCompany(factory);
			if (lastUsedCampaignDataSource != null)
			{
				var contactDataSource = lastUsedCampaignDataSource.SD_Name;
				ZString[] suffices = contactDataSource.Split(new char[] { '_' });
				if (suffices.Length > 1 && suffices[1].Equals(ContactDataSourceList.FilterModuleIDSuffixes.CampaignTracking))
				{
					return ContactDataSourceList.Codes.CampaignTracking;
				}
				else if (suffices.Length > 1 && suffices[1].Equals(ContactDataSourceList.FilterModuleIDSuffixes.Inquiries))
				{
					return ContactDataSourceList.Codes.Inquiries;
				}
			}
			return ContactDataSourceList.Codes.ClientIntelligence;
		}

		public static ZQuery CampaignLastUsedLayoutNameQuery()
		{
			ZQuery innerQueryGroup = new ZQuery();
			innerQueryGroup.AddToFilter(StmDataSchema.SD_Type, ZString.Empty);
			innerQueryGroup.AddToFilter(JoinCondition.Or, StmDataSchema.SD_Type, CampaignContactFilterDataSourceHelper.LastUsedLayoutType);
			return innerQueryGroup;
		}

		public static void UpdateFilterBusinessObjectLayoutContext(GlbCompanyCampaignContactFilterBusinessObject filterBusinessObject, GlbCompanyCampaign campaign, ZString defaultContext)
		{
			if (campaign != null)
			{
				campaign.ContactDataSource = defaultContext.IsEmpty ? GetCampaignContactDataSource(filterBusinessObject.LastUsedCampaignContactLayoutFactory) : defaultContext;
				((IFilterStripBusinessObjectInternals)filterBusinessObject).LayoutContext = filterBusinessObject.CurrentLayoutContext;
			}
		}

		public static void SetCampaignLastUsedLayoutValue(StmData lastUsedFilterStmData, BusinessObjectFactory factory)
		{
			lastUsedFilterStmData.SD_Type = LastUsedLayoutType;

			StmData lastUsedCampaignDataSource = GetLastUsedCampaignDatasourceStmDataForCompany(factory);
			if (lastUsedCampaignDataSource != null && !lastUsedCampaignDataSource.PK.Equals(lastUsedFilterStmData.PK))
			{
				lastUsedCampaignDataSource.SD_Type = ZString.Empty;
				factory.Save();
			}
		}
	}
}
