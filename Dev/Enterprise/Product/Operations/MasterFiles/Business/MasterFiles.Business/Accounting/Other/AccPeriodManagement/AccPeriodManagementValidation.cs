//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPeriodManagementValidation
//
//    This class should be used for overriding validation in AutoAccPeriodManagementValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccPeriodManagementValidation : AutoAccPeriodManagementValidation
	{
		public AccPeriodManagementValidation(AutoAccPeriodManagement parent) : base(parent)
		{
		}
	}
}
