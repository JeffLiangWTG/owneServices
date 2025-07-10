//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobConsolDGRestrictionsValidation
//
//    This class should be used for overriding validation in AutoJobConsolDGRestrictionsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobConsolDGRestrictionsValidation : AutoJobConsolDGRestrictionsValidation
	{
		public JobConsolDGRestrictionsValidation(AutoJobConsolDGRestrictions parent) : base(parent)
		{
		}
	}
}
