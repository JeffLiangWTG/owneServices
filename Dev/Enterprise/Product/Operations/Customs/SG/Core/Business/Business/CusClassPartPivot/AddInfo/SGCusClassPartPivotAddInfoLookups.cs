//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoSGCusClassPartPivotAddInfoLookups
//
//    This class should be used for overriding collections in AutoSGCusClassPartPivotAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGCusClassPartPivotAddInfoLookups : AutoSGCusClassPartPivotAddInfoLookups
	{
		public SGCusClassPartPivotAddInfoLookups(AutoSGCusClassPartPivotAddInfo parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList CustomsUQs
		{
			get { return new UnitOfQuantityCodeList(); }
		}
	}
}
