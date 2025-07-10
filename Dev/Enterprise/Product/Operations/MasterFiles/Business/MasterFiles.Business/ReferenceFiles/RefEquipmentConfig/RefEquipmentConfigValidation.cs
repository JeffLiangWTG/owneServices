//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefEquipmentConfigValidation
//
//    This class should be used for overriding validation in AutoRefEquipmentConfigValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefEquipmentConfigValidation : AutoRefEquipmentConfigValidation
	{
		public RefEquipmentConfigValidation(AutoRefEquipmentConfig parent) : base(parent)
		{
		}
	}
}
