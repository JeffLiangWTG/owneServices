using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class TouchReceivedModuleFilter : ModuleTextFilter
	{
		readonly GlbCompanyCampaign master;

		public TouchReceivedModuleFilter(ZString description, GlbCompanyCampaign master)
			: base(description, (GetTextQuery)EmptyQuery)
		{
			this.master = master;
		}

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get { return new[] { ComparisonConstants.Exact }; }
		}

		public CampaignHorizontal CurrentHorizontal
		{
			get
			{
				if (HorizontalId.IsEmpty)
				{
					return null;
				}

				return master.Horizontals.FirstOrDefault(h => h.Id.ToString().Equals(HorizontalId));
			}
		}

		[List("HorizontalIdList")]
		public ZString HorizontalId
		{
			get
			{
				return horizontalId;
			}
			set
			{
				if (SetNonPersistentPropertyValue(HorizontalIdInfo, ref horizontalId, value))
				{
					VerticalId = ZString.Empty;
					HorizontalIdInfo.RefreshBinding();
				}
			}
		}
		ZString horizontalId;

		public ZPropertyInfo HorizontalIdInfo
		{
			get { return GetZPropertyInfo(nameof(HorizontalId)); }
		}

		[List("VerticalIdList")]
		public ZString VerticalId
		{
			get
			{
				return verticalId;
			}
			set
			{
				if (SetNonPersistentPropertyValue(VerticalIdInfo, ref verticalId, value))
				{
					VerticalIdInfo.RefreshBinding();
				}
			}
		}
		ZString verticalId;

		public ZPropertyInfo VerticalIdInfo
		{
			get { return GetZPropertyInfo(nameof(VerticalId)); }
		}

		public CodeDescriptionPairList HorizontalIdList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				foreach (var horizontal in master.Horizontals)
				{
					list.AddPair(horizontal.Id.ToString(), string.Empty);
				}

				return list;
			}
		}

		public CodeDescriptionPairList VerticalIdList
		{
			get
			{
				var list = new CodeDescriptionPairList();

				if (CurrentHorizontal != null)
				{
					foreach (var campaign in CurrentHorizontal.Campaigns)
					{
						list.AddPair(campaign.G0_VerticalId, campaign.TouchFullName);
					}
				}

				return list;
			}
		}

		#region Query

		static ZQuery EmptyQuery(ZString value)
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			if (HorizontalId.IsEmpty)
			{
				return EmptyQuery(ZString.Empty);
			}

			var horizontal = CurrentHorizontal;
			if (horizontal == null)
			{
				return EmptyQuery(ZString.Empty);
			}

			var query = new ZQuery();

			if (VerticalId.IsEmpty)
			{
				var pks = GetTransitionedToHorizontal(horizontal.Id);

				query.AddToFilter(GlbCompanyCampaignItemSchema.PK, SQLComparisonOperator.Equal, pks.ToArray());
			}
			else
			{
				var vertical = horizontal.Campaigns.FirstOrDefault(c => c.G0_VerticalId.Equals(VerticalId));
				if (vertical == null)
				{
					return EmptyQuery(ZString.Empty);
				}

				var pks = GetTransitionedToTouch(horizontal.Id, vertical.PK);

				query.AddToFilter(GlbCompanyCampaignItemSchema.PK, SQLComparisonOperator.Equal, pks.ToArray());
			}

			return query;
		}

		IEnumerable<ZGuid> GetTransitionedToHorizontal(ZByte targetHorizontalId)
		{
			return master.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
				.Where(r => master.SummaryStats.HasHorizontalsRecipients(r.G8_RecipientID, targetHorizontalId))
				.Select(i => i.PK);
		}

		IEnumerable<ZGuid> GetTransitionedToTouch(ZByte targetHorizontalId, ZGuid touchId)
		{
			return master.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
				.Where(r => master.SummaryStats.HasTouchRecipients(r.G8_RecipientID, targetHorizontalId, touchId))
				.Select(i => i.PK);
		}

		#endregion
	}
}
