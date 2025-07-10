//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageBookedDetailValidation
//
//    This class should be used for overriding validation in AutoPkgPackageBookedDetailValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Packing.Business
{
	public class PkgPackageBookedDetailValidation : AutoPkgPackageBookedDetailValidation
	{
		public PkgPackageBookedDetailValidation(AutoPkgPackageBookedDetail parent) : base(parent)
		{
		}
	}
}
