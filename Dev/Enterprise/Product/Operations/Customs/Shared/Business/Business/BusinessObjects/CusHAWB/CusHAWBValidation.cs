//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusHAWBValidation
//
//    This class should be used for overriding validation in AutoCusHAWBValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusHAWBValidation : AutoCusHAWBValidation
	{
		public CusHAWBValidation(AutoCusHAWB parent) : base(parent)
		{
		}
	}
}
