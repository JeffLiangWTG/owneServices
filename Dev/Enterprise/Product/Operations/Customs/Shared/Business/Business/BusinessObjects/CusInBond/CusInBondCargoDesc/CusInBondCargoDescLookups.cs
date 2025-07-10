//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondCargoDescLookups
//
//    This class should be used for overriding collections in AutoCusInBondCargoDescLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusInBondCargoDescLookups : AutoCusInBondCargoDescLookups
	{
		public CusInBondCargoDescLookups(AutoCusInBondCargoDesc parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList TransportChargesModeOfPaymentList => Factory.GetCachedValue<TransportChargesModeOfPayment>();
	}
}
