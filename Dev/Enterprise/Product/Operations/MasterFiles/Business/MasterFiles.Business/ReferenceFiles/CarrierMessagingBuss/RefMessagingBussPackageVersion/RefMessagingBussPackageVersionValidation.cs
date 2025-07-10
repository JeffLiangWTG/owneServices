//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefMessagingBussPackageVersionValidation
//
//    This class should be used for overriding validation in AutoRefMessagingBussPackageVersionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefMessagingBussPackageVersionValidation : AutoRefMessagingBussPackageVersionValidation
	{
		public RefMessagingBussPackageVersionValidation(AutoRefMessagingBussPackageVersion parent) : base(parent)
		{
		}
	}
}
