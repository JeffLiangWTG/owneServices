//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSCAPivotValidation
//
//    This class should be used for overriding validation in AutoCusSCAPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusSCAPivotValidation : AutoCusSCAPivotValidation
	{
		public CusSCAPivotValidation(AutoCusSCAPivot parent) : base(parent)
		{
		}
	}
}
