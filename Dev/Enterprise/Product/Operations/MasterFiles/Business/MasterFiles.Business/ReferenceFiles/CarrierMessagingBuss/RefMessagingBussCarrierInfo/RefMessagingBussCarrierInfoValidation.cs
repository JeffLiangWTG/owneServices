//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefMessagingBussCarrierInfoValidation
//
//    This class should be used for overriding validation in AutoRefMessagingBussCarrierInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefMessagingBussCarrierInfoValidation : AutoRefMessagingBussCarrierInfoValidation
	{
		public RefMessagingBussCarrierInfoValidation(AutoRefMessagingBussCarrierInfo parent) : base(parent)
		{
		}
	}
}
