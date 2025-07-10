//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobDecRefsLookups
//
//    This class should be used for overriding collections in AutoJobDecRefsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class JobDecRefsLookups : AutoJobDecRefsLookups
	{
		public JobDecRefsLookups(AutoJobDecRefs parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList ReferenceTypeList
		{
			get { return Factory.GetCachedValue<CodeDescriptionPairList>(); }
		}
	}
}
