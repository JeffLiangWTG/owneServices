//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobConsolValidation
//
//    This class should be used for overriding validation in AutoJobConsolValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobConsolValidation : AutoJobConsolValidation
	{
		public JobConsolValidation(AutoJobConsol parent)
			: base(parent)
		{
		}
	}
}
