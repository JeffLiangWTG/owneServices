//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoItemPackagingAddInfoLookups
//
//    This class should be used for overriding collections in AutoItemPackagingAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class ItemPackagingAddInfoLookups : AutoItemPackagingAddInfoLookups
	{
		public ItemPackagingAddInfoLookups(AutoItemPackagingAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PackageUQList => UniversalReferenceHelper.GetUNEPackageTypeList(Factory);
	}
}
