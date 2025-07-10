//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCashAdvanceRequestHeaderLookups
//
//    This class should be used for overriding collections in AutoAccCashAdvanceRequestHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceRequestHeaderLookups : AutoAccCashAdvanceRequestHeaderLookups
	{
		public AccCashAdvanceRequestHeaderLookups(AutoAccCashAdvanceRequestHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList StatusCodeList => CashAdvanceStatusCodes.RequestHeader.CodesList;
	}
}
