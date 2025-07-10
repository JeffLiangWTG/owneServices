//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageHandlingUnitDivotValidation
//
//    This class should be used for overriding validation in AutoPkgPackageHandlingUnitDivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Packing.Business
{
	public class PkgPackageHandlingUnitDivotValidation : AutoPkgPackageHandlingUnitDivotValidation
	{
		public PkgPackageHandlingUnitDivotValidation(AutoPkgPackageHandlingUnitDivot parent)
			: base(parent)
		{
		}
	}
}
