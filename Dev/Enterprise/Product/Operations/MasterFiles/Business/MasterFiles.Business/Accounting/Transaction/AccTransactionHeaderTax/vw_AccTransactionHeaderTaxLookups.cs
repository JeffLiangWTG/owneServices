//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM Autovw_AccTransactionHeaderTaxLookups
//
//    This class should be used for overriding collections in Autovw_AccTransactionHeaderTaxLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class vw_AccTransactionHeaderTaxLookups : Autovw_AccTransactionHeaderTaxLookups
	{
		public vw_AccTransactionHeaderTaxLookups(Autovw_AccTransactionHeaderTax parent) : base(parent)
		{
		}
	}
}
