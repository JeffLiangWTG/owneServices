//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusStatementLineGroupFinancialDetailValidation
//
//    This class should be used for overriding validation in AutoCusStatementLineGroupFinancialDetailValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusStatementLineGroupFinancialDetailValidation : AutoCusStatementLineGroupFinancialDetailValidation
	{
		public CusStatementLineGroupFinancialDetailValidation(AutoCusStatementLineGroupFinancialDetail parent) : base(parent)
		{
		}
	}
}
