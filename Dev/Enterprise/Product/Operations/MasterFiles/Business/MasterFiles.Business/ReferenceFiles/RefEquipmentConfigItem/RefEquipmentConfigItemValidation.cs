//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefEquipmentConfigItemValidation
//
//    This class should be used for overriding validation in AutoRefEquipmentConfigItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefEquipmentConfigItemValidation : AutoRefEquipmentConfigItemValidation
	{
		public RefEquipmentConfigItemValidation(AutoRefEquipmentConfigItem parent) : base(parent)
		{
		}
	}
}
