//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobPaymentBasisValidation
//
//    This class should be used for overriding validation in AutoJobPaymentBasisValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Rating.Business
{
	using CargoWise.EntityFramework;

	public class JobPaymentBasisValidation : AutoJobPaymentBasisValidation
	{
		public JobPaymentBasisValidation(AutoJobPaymentBasis parent) : base(parent)
		{
		}

		protected override void CheckPBS_RX_NKRateCurrency()
		{
			ListValidation.ErrorIfInvalidCode(Parent.PBS_RX_NKRateCurrencyInfo);
		}
	}
}

