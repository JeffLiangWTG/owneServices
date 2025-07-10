//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffBRCharacteristicValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffBRCharacteristicValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTariffBRCharacteristicValidation : AutoRefCusTariffBRCharacteristicValidation
	{
		public RefCusTariffBRCharacteristicValidation(AutoRefCusTariffBRCharacteristic parent) : base(parent)
		{
		}
	}
}


