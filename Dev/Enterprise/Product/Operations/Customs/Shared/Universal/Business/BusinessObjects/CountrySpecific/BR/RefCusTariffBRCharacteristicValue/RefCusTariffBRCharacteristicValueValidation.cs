//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffBRCharacteristicValueValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffBRCharacteristicValueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTariffBRCharacteristicValueValidation : AutoRefCusTariffBRCharacteristicValueValidation
	{
		public RefCusTariffBRCharacteristicValueValidation(AutoRefCusTariffBRCharacteristicValue parent) : base(parent)
		{
		}
	}
}


