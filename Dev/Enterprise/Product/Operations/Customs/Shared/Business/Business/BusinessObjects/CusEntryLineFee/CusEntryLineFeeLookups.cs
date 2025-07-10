//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryLineFeeLookups
//
//    This class should be used for overriding collections in AutoCusEntryLineFeeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusEntryLineFeeLookups : AutoCusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(AutoCusEntryLineFee parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList RateOverrideReasonList => new CodeDescriptionPairList();
		public virtual CodeDescriptionPairList MethodOfPaymentList => new CodeDescriptionPairList();
		public virtual CodeDescriptionPairList MethodOfCalculationList => new CodeDescriptionPairList();
	}
}
