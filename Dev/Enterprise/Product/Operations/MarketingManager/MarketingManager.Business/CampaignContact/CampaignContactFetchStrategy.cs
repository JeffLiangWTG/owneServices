using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class CampaignContactFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CampaignContactFetchStrategy(CampaignContact campaignContact)
			: base(campaignContact)
		{
		}

		CampaignContact OrgCampaignContacts
		{
			get { return (CampaignContact)BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			bool requireOrganisation = false;

			foreach (var column in columns)
			{
				if (column.ColumnName.StartsWith("Header+"))
				{
					requireOrganisation = true;
				}
			}

			if (requireOrganisation)
			{
				Factory.AddFetchHint(OrgHeaderSchema.PK, OrgCampaignContacts.VCC_OH);
			}
		}
	}
}
