//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusPackingListLookups
//
//    This class should be used for overriding collections in AutoCusPackingListLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusPackingListLookups : AutoCusPackingListLookups
	{
		public CusPackingListLookups(AutoCusPackingList parent) : base(parent)
		{
		}

		public CodeDescriptionPairList WeightUQs => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
	}
}
