//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobEquipmentItemValidation
//
//    This class should be used for overriding validation in AutoJobEquipmentItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class JobEquipmentItemValidation : AutoJobEquipmentItemValidation
	{
		public JobEquipmentItemValidation(AutoJobEquipmentItem parent) : base(parent)
		{
		}
	}
}
