using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class BulkCommunicationCollection : BusinessObjectCollection<OrgSalesCall>
	{
		public BulkCommunicationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void CreateFromCampaignItems(IEnumerable<GlbCompanyCampaignItem> campaignItems)
		{
			RemoveAndDeleteAll();
			if (campaignItems != null && campaignItems.Any())
			{
				foreach (var item in campaignItems)
				{
					var orgSalesCall = AddNew();
					orgSalesCall.ShouldSendInvitation = false;
					BulkCommunication.CreateRelationshipForNewEntity(orgSalesCall, item, (message) =>
					{
						orgSalesCall.AddRowError(message);
					});
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			return base.AddNewCore(bizOType) as OrgSalesCall;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			OrgSalesCall orgSalesCall = (OrgSalesCall)child;
			base.SetDefaultsForNewChild(orgSalesCall);
			orgSalesCall.OQ_GS_NKSalesRep = GlbStaff.CurrentUser.GS_Code;
			orgSalesCall.OQ_Duration = TimeSpan.FromMinutes(30);
		}
	}
}
