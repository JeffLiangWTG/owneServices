using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TransistionRulesFromTouchCollection : GlbCompanyCampaignDripMarketingCollection
	{
		public TransistionRulesFromTouchCollection(GlbCompanyCampaign parentTouch)
			: base(parentTouch.Factory)
		{
			this.parentTouch = parentTouch;
			parentTouch.G0_HorizontalIdInfo.ValueChanged += (sender, args) => RefreshFilter();

			AdditionalFilter = GetAdditionalFilter(parentTouch);
		}

		static ZQuery GetAdditionalFilter(GlbCompanyCampaign parentTouch)
		{
			var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignDripMarketing));
			query.AddToFilter(GlbCompanyCampaignDripMarketingSchema.GCD_G0_ParentTouch, parentTouch.PK);

			var horizontalPart1 = new ZDBOnlyQuery(typeof(GlbCompanyCampaignDripMarketing));
			horizontalPart1.AddToFilter(GlbCompanyCampaignDripMarketingSchema.GCD_ParentHorizontalId, parentTouch.G0_HorizontalId);
			horizontalPart1.AddToFilter(GlbCompanyCampaignDripMarketingSchema.GCD_G0_ParentTouch, DBNull.Value);

			var touchQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaign), GlbCompanyCampaignDripMarketingSchema.GCD_G0_NextTouch);
			touchQuery.AddToFilter(GlbCompanyCampaignSchema.G0_G0_Master, parentTouch.G0_G0_Master);

			horizontalPart1.AddSubQuery(GlbCompanyCampaignDripMarketingSchema.GCD_G0_NextTouch, GlbCompanyCampaignSchema.PK, touchQuery, JoinCondition.And);

			var horizontalPart2 = new ZDBOnlyQuery(typeof(GlbCompanyCampaignDripMarketing));
			horizontalPart2.AddToFilter(GlbCompanyCampaignDripMarketingSchema.GCD_ParentHorizontalId, parentTouch.G0_HorizontalId);
			horizontalPart2.AddToFilter(GlbCompanyCampaignDripMarketingSchema.GCD_G0_ParentTouch, DBNull.Value);

			var groupQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaign), GlbCompanyCampaignDripMarketingSchema.GCD_GCG_Group);
			groupQuery.AddToFilter(GlbCompanyCampaignSchema.G0_G0_Master, parentTouch.G0_G0_Master);

			horizontalPart2.AddSubQuery(GlbCompanyCampaignDripMarketingSchema.GCD_GCG_Group, GlbCompanyCampaignSchema.G0_GCG_Group, groupQuery, JoinCondition.And);

			query.AddToFilter(horizontalPart1, JoinCondition.Or);
			query.AddToFilter(horizontalPart2, JoinCondition.Or);

			return query;
		}

		readonly GlbCompanyCampaign parentTouch;

		void RefreshFilter()
		{
			AdditionalFilter = GetAdditionalFilter(parentTouch);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		protected override void SetDefaultsForNewElementCore(GlbCompanyCampaignDripMarketing newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.GCD_ParentHorizontalId = parentTouch.G0_HorizontalId;
			newElement.GCD_G0_ParentTouch = parentTouch.PK;

			var x = newElement.FilterRule;  //Initialise filter rule
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}

	public class TransistionRulesToTouchCollection : GlbCompanyCampaignDripMarketingCollection
	{
		public TransistionRulesToTouchCollection(GlbCompanyCampaign nextTouch)
			: base(nextTouch.Factory,
				nextTouch.G0_GCG_Group.IsEmpty
					? new ZQuery(GlbCompanyCampaignDripMarketingSchema.GCD_G0_NextTouch, nextTouch.PK)
					: new ZQuery(GlbCompanyCampaignDripMarketingSchema.GCD_GCG_Group, nextTouch.CurrentGroup.PK))
		{
			this.nextTouch = nextTouch;
		}

		readonly GlbCompanyCampaign nextTouch;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		protected override void SetDefaultsForNewElementCore(GlbCompanyCampaignDripMarketing newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (nextTouch.G0_HorizontalId != 0)
			{
				newElement.GCD_ParentHorizontalId = (ZByte)(nextTouch.G0_HorizontalId - 1);

				if (newElement.GCD_ParentHorizontalId == 0)
				{
					newElement.GCD_G0_ParentTouch = nextTouch.G0_G0_Master;
				}
				else
				{
					if (!nextTouch.TouchSourceCampaignPKsForValidationInfo.HasErrors())
					{
						var parentTouch = GetParentTouchHorizontal(nextTouch, newElement.GCD_ParentHorizontalId);
						if (parentTouch != null)
						{
							newElement.GCD_G0_ParentTouch = parentTouch.PK;
						}
					}
				}
			}

			if (nextTouch.G0_GCG_Group.IsEmpty)
			{
				newElement.GCD_G0_NextTouch = nextTouch.PK;
			}
			else
			{
				newElement.GCD_GCG_Group = nextTouch.G0_GCG_Group;
			}

			var x = newElement.FilterRule;  //Initialise filter rule
		}

		GlbCompanyCampaign GetParentTouchHorizontal(GlbCompanyCampaign touch, ZByte parentHorizontalId)
		{
			return touch.MasterCampaign.Horizontals.FirstOrDefault(h => h.Id == parentHorizontalId)?.Campaigns.FirstOrDefault();
		}

		protected override bool AllowNew
		{
			get { return nextTouch.G0_HorizontalId > 0; }
		}
	}
}
