//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCashAdvanceRequestLineLookups
//
//    This class should be used for overriding collections in AutoAccCashAdvanceRequestLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceRequestLineLookups : AutoAccCashAdvanceRequestLineLookups
	{
		public AccCashAdvanceRequestLineLookups(AutoAccCashAdvanceRequestLine parent) : base(parent)
		{
		}

		public CodeDescriptionPairList StatusCodeList => CashAdvanceStatusCodes.RequestLine.CodesList;
	}
}
