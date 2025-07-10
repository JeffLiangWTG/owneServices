//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInvPackLookups
//
//    This class should be used for overriding collections in AutoCusInvPackLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusInvPackLookups : AutoCusInvPackLookups
	{
		public CusInvPackLookups(AutoCusInvPack parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList UnitTypeList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList TypeOfDifferenceList
		{
			get { return Factory.GetCachedValue<CusUnloadedStateList>(); }
		}
	}
}
