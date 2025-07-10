//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryHeaderChargesLookups
//
//    This class should be used for overriding collections in AutoCusEntryHeaderChargesLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusEntryHeaderChargesLookups : AutoCusEntryHeaderChargesLookups
	{
		public CusEntryHeaderChargesLookups(AutoCusEntryHeaderCharges parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList PaymentMethodsList => new CodeDescriptionPairList();
	}
}
