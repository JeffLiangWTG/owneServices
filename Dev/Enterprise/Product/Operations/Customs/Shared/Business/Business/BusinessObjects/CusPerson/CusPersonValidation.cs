//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusPersonValidation
//
//    This class should be used for overriding validation in AutoCusPersonValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusPersonValidation : AutoCusPersonValidation
	{
		public CusPersonValidation(AutoCusPerson parent) : base(parent)
		{
		}
	}
}
