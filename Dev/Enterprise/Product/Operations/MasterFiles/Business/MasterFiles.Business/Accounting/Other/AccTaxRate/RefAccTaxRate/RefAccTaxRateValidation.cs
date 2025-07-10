//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefAccTaxRateValidation
//
//    This class should be used for overriding validation in AutoRefAccTaxRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefAccTaxRateValidation : AutoRefAccTaxRateValidation
	{
		public RefAccTaxRateValidation(AutoRefAccTaxRate parent) : base(parent)
		{
		}
	}
}
