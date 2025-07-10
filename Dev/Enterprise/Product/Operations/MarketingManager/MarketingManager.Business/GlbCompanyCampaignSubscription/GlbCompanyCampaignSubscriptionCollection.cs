using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.Business
{
	public abstract class GlbCompanyCampaignSubscriptionCollection<TParent, TElement> : ActiveBusinessObjectCollection<TElement>, IGlbCompanyCampaignSubscriptionCollection
		where TParent : BusinessObject
		where TElement : GlbCompanyCampaignSubscription
	{
		protected GlbCompanyCampaignSubscriptionCollection(TParent parent, BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}
			if (businessObjectFactory == null)
			{
				throw new ArgumentNullException(nameof(businessObjectFactory));
			}

			Parent = parent;
			CountChanged += (sender, args) => Parent.MarkAsNeedingValidation();
		}

		protected GlbCompanyCampaignSubscriptionCollection(TParent parent)
			: base(parent.Factory, parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			Parent = parent;
			CountChanged += (sender, args) => Parent.MarkAsNeedingValidation();
		}

		protected TParent Parent { get; }

		IEnumerator<IGlbCompanyCampaignSubscription> IEnumerable<IGlbCompanyCampaignSubscription>.GetEnumerator() => GetEnumerator();

		IGlbCompanyCampaignSubscription IGlbCompanyCampaignSubscriptionCollection.this[int index] => base[index];

		IGlbCompanyCampaignSubscription IGlbCompanyCampaignSubscriptionCollection.AddNew() => AddNew();

		public void ValidateAllMembers()
		{
			((IEnumerable<TElement>)this).ForEach(subscription => subscription.ClearRowNotifications());
			if (Count > 1)
			{
				var errorRows = ((IEnumerable<TElement>)this)
					.Where(subscription => subscription.ShouldValidateOnSave)
					.GroupBy(subscription => new { subscription.GCS_OH, subscription.GCS_Email, subscription.GCS_MediaCategory, subscription.GCS_MediaType })
					.Select(grouping => new { grouping.Key, Count = grouping.Count(), grouping })
					.Where(arg => arg.Count > 1)
					.SelectMany(arg => arg.grouping);

				var errorMessage = Res.GetString("003c6108-5972-404b-90c7-6d43637131c9", "Subscription must have unique set of (Media Category and Media Type).");
				errorRows.ForEach(subscription => subscription.AddRowError(errorMessage));
			}
		}
	}
}
