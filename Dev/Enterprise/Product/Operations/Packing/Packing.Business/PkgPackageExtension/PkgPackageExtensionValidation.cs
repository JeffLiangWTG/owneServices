//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageExtensionValidation
//
//    This class should be used for overriding validation in AutoPkgPackageExtensionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Packing.Business
{
	public class PkgPackageExtensionValidation : AutoPkgPackageExtensionValidation
	{
		public PkgPackageExtensionValidation(AutoPkgPackageExtension parent) : base(parent)
		{
		}
	}
}
