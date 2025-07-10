//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefMessagingBussAttributeInfoValidation
//
//    This class should be used for overriding validation in AutoRefMessagingBussAttributeInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefMessagingBussAttributeInfoValidation : AutoRefMessagingBussAttributeInfoValidation
	{
		public RefMessagingBussAttributeInfoValidation(AutoRefMessagingBussAttributeInfo parent) : base(parent)
		{
		}
	}
}
