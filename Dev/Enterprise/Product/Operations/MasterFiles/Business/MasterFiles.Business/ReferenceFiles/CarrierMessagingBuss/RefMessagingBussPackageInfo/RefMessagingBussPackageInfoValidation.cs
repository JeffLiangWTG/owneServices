//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefMessagingBussPackageInfoValidation
//
//    This class should be used for overriding validation in AutoRefMessagingBussPackageInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefMessagingBussPackageInfoValidation : AutoRefMessagingBussPackageInfoValidation
	{
		public RefMessagingBussPackageInfoValidation(AutoRefMessagingBussPackageInfo parent) : base(parent)
		{
		}
	}
}
