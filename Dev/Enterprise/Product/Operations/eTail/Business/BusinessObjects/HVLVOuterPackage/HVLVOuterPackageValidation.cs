//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVOuterPackageValidation
//
//    This class should be used for overriding validation in AutoHVLVOuterPackageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.eTail.Business
{
	public class HVLVOuterPackageValidation : AutoHVLVOuterPackageValidation
	{
		public HVLVOuterPackageValidation(AutoHVLVOuterPackage parent)
			: base(parent)
		{
		}
	}
}
