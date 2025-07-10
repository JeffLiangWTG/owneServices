//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusRulingConfigCombinedLookups
//
//    This class should be used for overriding collections in AutoZZRefCusRulingConfigCombinedLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class CusRulingConfigCombinedLookups : AutoCusRulingConfigCombinedLookups
	{
		public CusRulingConfigCombinedLookups(AutoCusRulingConfigCombined parent) : base(parent)
		{
		}

		protected new CusRulingConfigCombined Parent
		{
			get { return (CusRulingConfigCombined)base.Parent; }
		}

		public virtual CodeDescriptionPairList RulingConfigCategoryList => Factory.GetCachedValue<RefCusRulingConfigCategories>();

		public virtual CodeDescriptionPairList RulingConfigTypeList => Factory.GetCachedValue<RefCusRulingConfigTypes>();

		public virtual ICollection RulingConfigValueList => Factory.GetCachedValue<RefCusRulingConfigValuesForAcceptType>();
	}
}
