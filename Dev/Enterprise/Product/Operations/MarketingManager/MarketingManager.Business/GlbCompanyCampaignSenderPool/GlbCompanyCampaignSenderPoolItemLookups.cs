//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbCompanyCampaignSenderPoolItemLookups
//
//    This class should be used for overriding collections in AutoGlbCompanyCampaignSenderPoolItemLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSenderPoolItemLookups : AutoGlbCompanyCampaignSenderPoolItemLookups
	{
		public GlbCompanyCampaignSenderPoolItemLookups(AutoGlbCompanyCampaignSenderPoolItem parent) : base(parent)
		{
		}

		public override GlbStaffCollection Senders => new GlbStaffCollection(Factory, new ZQuery(GlbStaffSchema.GS_IsSystemAccount, false));
	}
}
