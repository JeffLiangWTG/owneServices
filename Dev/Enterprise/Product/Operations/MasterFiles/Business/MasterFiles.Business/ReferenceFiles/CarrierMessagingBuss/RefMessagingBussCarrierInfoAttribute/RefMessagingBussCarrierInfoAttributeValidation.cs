//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefMessagingBussCarrierInfoAttributeValidation
//
//    This class should be used for overriding validation in AutoRefMessagingBussCarrierInfoAttributeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefMessagingBussCarrierInfoAttributeValidation : AutoRefMessagingBussCarrierInfoAttributeValidation
	{
		public RefMessagingBussCarrierInfoAttributeValidation(AutoRefMessagingBussCarrierInfoAttribute parent) : base(parent)
		{
		}
	}
}
