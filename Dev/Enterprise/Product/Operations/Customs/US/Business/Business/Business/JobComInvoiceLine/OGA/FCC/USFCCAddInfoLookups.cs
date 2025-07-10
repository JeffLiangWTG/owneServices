//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFCCAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSFCCAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USFCCAddInfoLookups : AutoUSFCCAddInfoLookups
	{
		public USFCCAddInfoLookups(AutoUSFCCAddInfo parent)
			: base(parent)
		{
		}

		public FCCImportConditionNumberList FCCImportConditionNumbers
		{
			get { return new FCCImportConditionNumberList(); }
		}
	}
}
