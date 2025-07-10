using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSubscriptionForOrganisationContactCollection : GlbCompanyCampaignSubscriptionCollection<OrgContact, GlbCompanyCampaignSubscriptionForContact>, IGlbCompanyCampaignSubscriptionForOrganisationContactCollection
	{
		public GlbCompanyCampaignSubscriptionForOrganisationContactCollection(OrgContact parent)
			: base(parent, parent.Factory)
		{
		}

		protected sealed override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			if (Parent == null || Parent.OC_Email.IsEmpty)
			{
				result.AddToFilter(ZQuery.NoResultQuery);
			}
			else
			{
				var additionalFilter = new ZQuery(GlbCompanyCampaignSubscriptionSchema.GCS_Email, Parent.OC_Email);
				if (Parent.ParentOrg != null)
				{
					additionalFilter.AddToFilter(JoinCondition.Or, GlbCompanyCampaignSubscriptionSchema.GCS_OH, Parent.ParentOrg.PK);
				}
				result.AddToFilter(additionalFilter);
			}
			return result;
		}

		protected override void SetDefaultsForNewElementCore(GlbCompanyCampaignSubscriptionForContact newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.GCS_Email = Parent.OC_Email;
		}

		public override void Delete(GlbCompanyCampaignSubscriptionForContact businessObject)
		{
			if (businessObject.GCS_Email == Parent.OC_Email)
			{
				base.Delete(businessObject);
			}
		}

		IGlbCompanyCampaignSubscription IGlbCompanyCampaignSubscriptionForOrganisationContactCollection.this[int index] => base[index];

		public void RejectAllChanges()
		{
			(this as IEnumerable<GlbCompanyCampaignSubscription>).ForEach(subscription => subscription.Reload());
		}
	}
}
