//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusPermitLineTransactionValidation
//
//    This class should be used for overriding validation in AutoCusPermitLineTransactionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusPermitLineTransactionValidation : AutoCusPermitLineTransactionValidation
	{
		public CusPermitLineTransactionValidation(AutoCusPermitLineTransaction parent) : base(parent)
		{
		}
	}
}
