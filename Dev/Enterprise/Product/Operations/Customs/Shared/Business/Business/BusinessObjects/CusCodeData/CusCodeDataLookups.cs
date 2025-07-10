//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusCodeDataLookups
//
//    This class should be used for overriding collections in AutoCusCodeDataLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusCodeDataLookups : AutoCusCodeDataLookups
	{
		public CusCodeDataLookups(AutoCusCodeData parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList CY_DataList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList CY_CodeList
		{
			get { return new CodeDescriptionPairList(); }
		}
	}
}
