//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbCompanyCampaignDripMarketingLookups
//
//    This class should be used for overriding collections in AutoGlbCompanyCampaignDripMarketingLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignDripMarketingLookups : AutoGlbCompanyCampaignDripMarketingLookups
	{
		public GlbCompanyCampaignDripMarketingLookups(AutoGlbCompanyCampaignDripMarketing parent) : base(parent)
		{
		}
		new GlbCompanyCampaignDripMarketing Parent
		{
			get { return (GlbCompanyCampaignDripMarketing)base.Parent; }
		}

		public CodeDescriptionPairList EligibleParentHorizontalIds
		{
			get
			{
				var result = new CodeDescriptionPairList();

				if (Parent.MasterCampaign == null || Parent.NextTouches == null || Parent.NextTouches.Count == 0)
				{
					return result;
				}

				foreach (var horizontal in Parent.MasterCampaign.Horizontals.Where(h => h.Id < Parent.NextTouches[0].G0_HorizontalId))
				{
					result.AddPairIfNotExist(horizontal.Id.ToString(), horizontal.Name);
				}

				return result;
			}
		}

		public IList<GlbCompanyCampaign> EligibleTouchParents
		{
			get
			{
				var result = new List<GlbCompanyCampaign>();
				if (Parent.MasterCampaign != null)
				{
					if (Parent.GCD_ParentHorizontalId == 0)
					{
						result.Add(Parent.MasterCampaign);
					}
					else
					{
						result.AddRange(Parent.MasterCampaign.AllTouches.Where(t => t.G0_HorizontalId == Parent.GCD_ParentHorizontalId));
					}
				}

				return result;
			}
		}
	}
}
