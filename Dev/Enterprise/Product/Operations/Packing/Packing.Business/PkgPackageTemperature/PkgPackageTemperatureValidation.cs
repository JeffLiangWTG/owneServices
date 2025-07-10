//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageTemperatureValidation
//
//    This class should be used for overriding validation in AutoPkgPackageTemperatureValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Packing.Business
{
	public class PkgPackageTemperatureValidation : AutoPkgPackageTemperatureValidation
	{
		public PkgPackageTemperatureValidation(AutoPkgPackageTemperature parent) : base(parent)
		{
		}
	}
}
