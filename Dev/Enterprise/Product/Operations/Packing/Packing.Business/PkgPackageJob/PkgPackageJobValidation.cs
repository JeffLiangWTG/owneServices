//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageJobValidation
//
//    This class should be used for overriding validation in AutoPkgPackageJobValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
namespace Enterprise.Packing.Business
{
	public class PkgPackageJobValidation : AutoPkgPackageJobValidation
	{
		public PkgPackageJobValidation(AutoPkgPackageJob parent) : base(parent)
		{
		}
	}
}
