//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefMessagingBussPackageInfoAttributeValidation
//
//    This class should be used for overriding validation in AutoRefMessagingBussPackageInfoAttributeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefMessagingBussPackageInfoAttributeValidation : AutoRefMessagingBussPackageInfoAttributeValidation
	{
		public RefMessagingBussPackageInfoAttributeValidation(AutoRefMessagingBussPackageInfoAttribute parent) : base(parent)
		{
		}
	}
}
