//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGSubstancePivotValidation
//
//    This class should be used for overriding validation in AutoUNDGSubstancePivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstancePivotValidation : AutoUNDGSubstancePivotValidation
	{
		public UNDGSubstancePivotValidation(AutoUNDGSubstancePivot parent) : base(parent)
		{
		}
	}
}
