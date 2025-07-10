//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDmgDeviceHeaderValidation
//
//    This class should be used for overriding validation in AutoDmgDeviceHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.RemoteDeviceManagement
{
	public class DmgDeviceHeaderValidation : AutoDmgDeviceHeaderValidation
	{
		public DmgDeviceHeaderValidation(AutoDmgDeviceHeader parent) : base(parent)
		{
		}
	}
}
