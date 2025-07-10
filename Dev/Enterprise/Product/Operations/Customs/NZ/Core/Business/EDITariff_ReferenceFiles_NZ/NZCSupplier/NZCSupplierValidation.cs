//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCSupplierValidation
//
//    This class should be used for overriding validation in AutoNZCSupplierValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCSupplierValidation : AutoNZCSupplierValidation
	{
		public NZCSupplierValidation(AutoNZCSupplier parent)
			: base(parent)
		{
		}
	}
}
