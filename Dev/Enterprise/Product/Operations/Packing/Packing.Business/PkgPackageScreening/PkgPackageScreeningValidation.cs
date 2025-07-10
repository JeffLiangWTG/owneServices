//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageScreeningValidation
//
//    This class should be used for overriding validation in AutoPkgPackageScreeningValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Packing.Business
{
	public class PkgPackageScreeningValidation : AutoPkgPackageScreeningValidation
	{
		public PkgPackageScreeningValidation(AutoPkgPackageScreening parent) : base(parent)
		{
		}
	}
}
