//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCACCaseBondCashValidation
//
//    This class should be used for overriding validation in AutoUSCACCaseBondCashValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseBondCashValidation : AutoUSCACCaseBondCashValidation
	{
		public USCACCaseBondCashValidation(AutoUSCACCaseBondCash parent) : base(parent)
		{
		}
	}
}
