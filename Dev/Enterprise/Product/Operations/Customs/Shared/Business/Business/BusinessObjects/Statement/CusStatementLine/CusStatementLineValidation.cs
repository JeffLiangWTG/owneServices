//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusStatementLineValidation
//
//    This class should be used for overriding validation in AutoCusStatementLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusStatementLineValidation : AutoCusStatementLineValidation
	{
		public CusStatementLineValidation(AutoCusStatementLine parent) : base(parent)
		{
		}
	}
}
