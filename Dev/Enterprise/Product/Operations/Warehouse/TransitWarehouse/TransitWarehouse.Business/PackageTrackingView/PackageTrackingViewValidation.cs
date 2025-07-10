//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPackageTrackingViewValidation
//
//    This class should be used for overriding validation in AutoPackageTrackingViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class PackageTrackingViewValidation : AutoPackageTrackingViewValidation
	{
		public PackageTrackingViewValidation(AutoPackageTrackingView parent) : base(parent)
		{
		}
	}
}

