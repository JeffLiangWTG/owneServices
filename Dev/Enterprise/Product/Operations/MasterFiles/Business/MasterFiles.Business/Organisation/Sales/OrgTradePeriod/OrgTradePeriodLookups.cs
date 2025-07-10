//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgTradePeriodLookups
//
//    This class should be used for overriding collections in AutoOrgTradePeriodLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradePeriodLookups : AutoOrgTradePeriodLookups
	{
		public OrgTradePeriodLookups(AutoOrgTradePeriod parent) : base(parent)
		{
		}

		#region Unit Of Weight / Volume List

		public CodeDescriptionPairList UnitOfWeightList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList UnitOfVolumeList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion
	}
}
