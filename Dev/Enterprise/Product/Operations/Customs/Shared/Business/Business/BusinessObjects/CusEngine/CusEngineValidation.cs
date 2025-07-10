//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEngineValidation
//
//    This class should be used for overriding validation in AutoCusEngineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusEngineValidation : AutoCusEngineValidation
	{
		public CusEngineValidation(AutoCusEngine parent) : base(parent)
		{
		}
	}
}
