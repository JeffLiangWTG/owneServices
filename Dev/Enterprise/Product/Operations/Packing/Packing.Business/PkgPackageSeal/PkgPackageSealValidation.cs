//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageSealValidation
//
//    This class should be used for overriding validation in AutoPkgPackageSealValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Packing.Business
{
	public class PkgPackageSealValidation : AutoPkgPackageSealValidation
	{
		public PkgPackageSealValidation(AutoPkgPackageSeal parent) : base(parent)
		{
		}
	}
}
