//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondEquipmentValidation
//
//    This class should be used for overriding validation in AutoCusInBondEquipmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusInBondEquipmentValidation : AutoCusInBondEquipmentValidation
	{
		public CusInBondEquipmentValidation(AutoCusInBondEquipment parent)
			: base(parent) { }
	}
}
