using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignDripMarketing : AutoGlbCompanyCampaignDripMarketing, IRelatedModuleFilterSupportable
	{
		public GlbCompanyCampaignDripMarketing(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		GlbCompanyCampaign masterCampaign;
		public GlbCompanyCampaign MasterCampaign
		{
			get
			{
				if (masterCampaign == null)
				{
					var touch = (NextTouches.Count > 0 ? NextTouches[0] : null)
						?? ParentTouch;

					if (touch == null)
					{
						return null;
					}

					masterCampaign = touch.IsMasterCampaign ? touch : touch.MasterCampaign;
				}

				return masterCampaign;
			}
		}

		[List("Lookups.EligibleTouchParents")]
		[RelatedBusinessObject("ParentTouch")]
		public override ZGuid GCD_G0_ParentTouch
		{
			get { return base.GCD_G0_ParentTouch; }
			set { base.GCD_G0_ParentTouch = value; }
		}

		public override ZGuid GCD_G0_NextTouch
		{
			get { return base.GCD_G0_NextTouch; }
			set
			{
				if (GCD_G0_NextTouch != value)
				{
					base.GCD_G0_NextTouch = value;
					nextTouches = null;

					if (!value.IsEmpty && !GCD_GCG_Group.IsEmpty)
					{
						GCD_GCG_Group = ZGuid.Empty;
					}
				}
			}
		}

		public override ZGuid GCD_GCG_Group
		{
			get { return base.GCD_GCG_Group; }
			set
			{
				if (GCD_GCG_Group != value)
				{
					base.GCD_GCG_Group = value;
					nextTouches = null;

					if (!value.IsEmpty && !GCD_G0_NextTouch.IsEmpty)
					{
						GCD_G0_NextTouch = ZGuid.Empty;
					}
				}
			}
		}

		[List("Lookups.EligibleParentHorizontalIds")]
		[BusinessObjectTestExclude]
		public ZString ParentHorizontalIdForBinding
		{
			get { return GCD_ParentHorizontalId.IsEmpty || GCD_ParentHorizontalId == 0 ? string.Empty : GCD_ParentHorizontalId.ToString(); }
			set
			{
				ZString id = GCD_ParentHorizontalId.ToString();
				if (SetNonPersistentPropertyValue(ParentHorizontalIdForBindingInfo, ref id, value))
				{
					if (id.IsEmpty)
					{
						GCD_ParentHorizontalId = 0;
						return;
					}

					ZByte result;
					if (ZByte.TryParse(id, out result))
					{
						GCD_ParentHorizontalId = result;
					}
				}
			}
		}

		public ZPropertyInfo ParentHorizontalIdForBindingInfo
		{
			get { return GetZPropertyInfo(nameof(ParentHorizontalIdForBinding)); }
		}

		public ZString ParentTouchSummary
		{
			get
			{
				var sb = new StringBuilder();

				if (ParentTouch != null)
				{
					sb.Append(ParentTouch.G0_EstimatedStartedDate.IsEmpty ? ZString.Empty : (ZString)ParentTouch.G0_EstimatedStartedDate.ToString("yyMMdd"));
					sb.Append("_");
					sb.Append(ParentTouch.G0_Category);
					sb.Append("_");
					sb.Append(ParentTouch.G0_Type);
					sb.Append("_");
					sb.Append(ParentTouch.G0_CampaignNameMultilingual);

					if (ParentTouch.IsTouchCampaign)
					{
						sb.Append("_");
						sb.Append(ParentTouch.TouchId);
					}
				}

				return sb.ToString();
			}
		}

		internal ModuleIdentifier DripMarketingFilterRuleModule => DripMarketingFilterRuleModuleCore;

		protected virtual ModuleIdentifier DripMarketingFilterRuleModuleCore
		{
			get
			{
				return (masterCampaign != null && masterCampaign.IsHRCampaign) ? ModuleIDs.DripMarketingFilterRuleHR : ModuleIDs.DripMarketingFilterRule;
			}
		}

		public StmModuleFilter FilterRule => FilterRuleProvider.GetOrCreateAndCacheFilter();

		FilterRuleProvider FilterRuleProvider
		{
			get
			{
				if (filterRuleProvider == null)
				{
					filterRuleProvider = new FilterRuleProvider(DripMarketingFilterRuleModuleCore, this)
					{
						ReloadExistingRowsOnFilterLoad = true
					};
				}

				return filterRuleProvider;
			}
		}

		FilterRuleProvider filterRuleProvider;

		public GlbCompanyCampaign ParentTouch
		{
			get
			{
				return Factory.Load<GlbCompanyCampaign>(GCD_G0_ParentTouch);
			}
		}

		Collection<GlbCompanyCampaign> nextTouches;

		public Collection<GlbCompanyCampaign> NextTouches
		{
			get
			{
				if (nextTouches == null)
				{
					nextTouches = new Collection<GlbCompanyCampaign>();

					if (!GCD_G0_NextTouch.IsEmpty)
					{
						var campaign = Factory.Load<GlbCompanyCampaign>(GCD_G0_NextTouch);
						if (campaign != null)
						{
							nextTouches.Add(campaign);
						}
					}
					else if (TouchGroup != null)
					{
						foreach (var touch in TouchGroup.Touches.Distinct())
						{
							nextTouches.Add(touch);
						}
					}

					foreach (var touch in nextTouches)
					{
						if (touch != null)
						{
							touch.TouchSourceCampaignPKs = ParentHorizontalsTouches.Select(t => t.PK).ToArray();
						}
					}
				}

				return nextTouches;
			}
		}

		public GlbCompanyCampaignGroup TouchGroup
		{
			get
			{
				return !GCD_GCG_Group.IsEmpty
					? Factory.Load<GlbCompanyCampaignGroup>(GCD_GCG_Group)
					: null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public GlbCompanyCampaign[] ParentHorizontalsTouches
		{
			get
			{
				if (!GCD_G0_ParentTouch.IsEmpty)
				{
					return new[] { ParentTouch };
				}

				if (MasterCampaign == null)
				{
					return System.Array.Empty<GlbCompanyCampaign>();
				}

				if (GCD_ParentHorizontalId == 0)
				{
					return new[] { MasterCampaign };
				}

				return MasterCampaign.AllTouches.Where(t => t.G0_HorizontalId == GCD_ParentHorizontalId).ToArray();
			}
		}

		#endregion

		#region ReadOnly's

		public bool GCD_G0_ParentTouch_ReadOnly
		{
			get { return GCD_ParentHorizontalId.IsEmpty || GCD_ParentHorizontalId == 0; }
		}

		public bool ParentHorizontalIdForBinding_ReadOnly
		{
			get { return GCD_G0_ParentTouch == MasterCampaign.PK; }
		}

		#endregion

		#region BusinessObject Overrides

		public override void Delete()
		{
			FilterRuleProvider.DeleteFilter();
			base.Delete();
		}

		#region Overrides of BusinessObject

		protected override ZString HumanReadableNameCore => MasterCampaign?.HumanReadableName ?? base.HumanReadableNameCore;

		#endregion

		#endregion
	}
}
