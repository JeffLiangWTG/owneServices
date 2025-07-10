//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobScheduleChangeValidation
//
//    This class should be used for overriding validation in AutoJobScheduleChangeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Business
{
	public class JobScheduleChangeValidation : AutoJobScheduleChangeValidation
	{
		public JobScheduleChangeValidation(AutoJobScheduleChange parent) : base(parent)
		{
		}
	}
}
