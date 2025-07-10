using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	class UnsubscribeContactsItem : NonPersistentBusinessObject
	{
		public UnsubscribeContactsItem(GlbCompanyCampaignItem glbCompanyCampaignItem)
		{
			GlbCompanyCampaignPk = glbCompanyCampaignItem.PK;
			ClientName = glbCompanyCampaignItem.OrganisationFullName;
			PrimaryContact = glbCompanyCampaignItem.ContactName;
			Email = glbCompanyCampaignItem.EmailAddress;
		}

		public ZGuid GlbCompanyCampaignPk { get; }
		public ZPropertyInfo GlbCompanyCampaignPkInfo => GetZPropertyInfo(nameof(GlbCompanyCampaignPk));

		public ZString ClientName { get; }
		public ZPropertyInfo ClientNameInfo => GetZPropertyInfo(nameof(ClientName));

		public ZString PrimaryContact { get; }
		public ZPropertyInfo PrimaryContactInfo => GetZPropertyInfo(nameof(PrimaryContact));

		public ZString Email { get; }
		public ZPropertyInfo EmailInfo => GetZPropertyInfo(nameof(Email));

		public ZString UnsubscribeResult
		{
			get { return unsubscribeResult; }
			set { SetNonPersistentPropertyValue(UnsubscribeResultInfo, ref unsubscribeResult, value); }
		}

		public ZPropertyInfo UnsubscribeResultInfo => GetZPropertyInfo(nameof(UnsubscribeResult));
		protected bool UnsubscribeResult_ReadOnly => true;

		ZString unsubscribeResult;
	}
}
