//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusRulingCombinedLookups
//
//    This class should be used for overriding collections in AutoZZRefCusRulingCombinedLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusRulingCombinedLookups : AutoZZRefCusRulingCombinedLookups
	{
		public ZZRefCusRulingCombinedLookups(AutoZZRefCusRulingCombined parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList RulingTypeList => Factory.GetCachedValue<RefCusRulingTypeList>();

		public virtual IBusinessObjectCollection AppliesToOrganizationList => new OrgHeaderCollection(Factory);
	}
}
