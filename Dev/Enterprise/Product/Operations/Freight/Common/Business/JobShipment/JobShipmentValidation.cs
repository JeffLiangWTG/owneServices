//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobShipmentValidation
//
//    This class should be used for overriding validation in AutoJobShipmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Common.Business
{
	public class JobShipmentValidation : AutoJobShipmentValidation
	{
		public JobShipmentValidation(AutoJobShipment parent)
			: base(parent)
		{
		}

		protected override void CheckJS_RX_NKFrtRateCurrency()
		{
			base.CheckJS_RX_NKFrtRateCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JS_RX_NKFrtRateCurrencyInfo);
		}
	}
}
