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

namespace Enterprise.Customs.US.Business
{
	public class CusStatementHeaderValidation : Customs.Business.CusStatementHeaderValidation
	{
		public CusStatementHeaderValidation(CusStatementHeader parent)
			: base(parent)
		{
		}
	}
}
