//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgHandlingUnitValidation
//
//    This class should be used for overriding validation in AutoPkgHandlingUnitValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Packing.Business
{
	public class PkgHandlingUnitValidation : AutoPkgHandlingUnitValidation
	{
		public PkgHandlingUnitValidation(AutoPkgHandlingUnit parent)
			: base(parent)
		{
		}
	}
}

