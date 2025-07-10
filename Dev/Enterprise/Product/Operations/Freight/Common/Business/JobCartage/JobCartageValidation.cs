//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobCartageValidation
//
//    This class should be used for overriding validation in AutoJobCartageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobCartageValidation : AutoJobCartageValidation
	{
		public JobCartageValidation(AutoJobCartage parent)
			: base(parent)
		{
		}
	}
}
