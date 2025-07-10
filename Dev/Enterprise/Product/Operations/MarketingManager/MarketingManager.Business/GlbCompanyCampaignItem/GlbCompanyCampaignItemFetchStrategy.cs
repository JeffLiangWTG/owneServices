using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignItemFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public GlbCompanyCampaignItemFetchStrategy(GlbCompanyCampaignItem campaignItem)
			: base(campaignItem)
		{
		}

		GlbCompanyCampaignItem CampaignItem
		{
			get { return (GlbCompanyCampaignItem)BusinessObject; }
		}

		#region FetchForLoad

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(CampaignContact), ViewCampaignContactSchema.PK, CampaignItem.G8_RecipientID);
		}

		#endregion

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			SalesRelationActivityFetchStrategyHelper.AddFetchHintsForView(Factory, (ISalesRelationActivity)BusinessObject, columns);

			bool requireHRJobApplicant = false;
			bool requireOrgContact = false;

			foreach (var column in columns)
			{
				if (CampaignItem.G8_RecipientTableCode == HRJobApplicantSchema.Constants.Prefix)
				{
					if (IsRecipientColumn(column))
					{
						requireHRJobApplicant = true;
					}
				}
				else
				{
					if (IsRecipientColumn(column))
					{
						requireOrgContact = true;
					}
				}
			}

			if (requireHRJobApplicant)
			{
				Factory.AddFetchHint(HRJobApplicantSchema.PK, CampaignItem.G8_RecipientID);
			}

			if (requireOrgContact)
			{
				Factory.AddFetchHint(typeof(OrgContact), OrgContactSchema.PK, CampaignItem.G8_RecipientID);
				Factory.AddFetchHint(typeof(SalesEnquiry), OrgColdCallRegisterSchema.PK, CampaignItem.G8_RecipientID);
				Factory.AddFetchHint(typeof(CampaignContact), ViewCampaignContactSchema.PK, CampaignItem.G8_RecipientID);
			}
		}

		static bool IsRecipientColumn(TableColumn column)
		{
			return
				column.ColumnName == GlbCompanyCampaignItem.Schema.ContactName ||
				column.ColumnName == GlbCompanyCampaignItem.Schema.WorkPhone ||
				column.ColumnName == GlbCompanyCampaignItem.Schema.EmailAddress ||
				column.ColumnName.StartsWith("Recipient+") ||
				column.ColumnName == nameof(GlbCompanyCampaignItem.ScheduleTimeRecipientTime);
		}

		#endregion
	}
}
