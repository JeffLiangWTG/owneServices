//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyageExRateValidation
//
//    This class should be used for overriding validation in AutoJobVoyageExRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Freight.Common.Business
{
	public class JobVoyageExRateValidation : AutoJobVoyageExRateValidation
	{
		public JobVoyageExRateValidation(AutoJobVoyageExRate parent)
			: base(parent)
		{
		}

		protected override void CheckE8_RX_NKExCurrency()
		{
			base.CheckE8_RX_NKExCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.E8_RX_NKExCurrencyInfo);
		}

		protected override void CheckE8_RL_NKPort()
		{
			base.CheckE8_RL_NKPort();
			ListValidation.ErrorIfInvalidCode(Parent.E8_RL_NKPortInfo);
		}
	}
}
