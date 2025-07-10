//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusStatementHeaderValidation
//
//    This class should be used for overriding validation in AutoCusStatementHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusStatementHeaderValidation : AutoCusStatementHeaderValidation
	{
		public CusStatementHeaderValidation(AutoCusStatementHeader parent) : base(parent)
		{
		}
	}
}
