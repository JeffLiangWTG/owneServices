//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM CusClassPartPivotRefLookups
//
//    This class should be used for overriding collections in CusClassPartPivotRefLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusClassPartPivotRefLookups : AutoCusClassPartPivotRefLookups
	{
		public CusClassPartPivotRefLookups(AutoCusClassPartPivotRef parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList ReferenceTypeList => Factory.GetCachedValue<CodeDescriptionPairList>();
	}
}
