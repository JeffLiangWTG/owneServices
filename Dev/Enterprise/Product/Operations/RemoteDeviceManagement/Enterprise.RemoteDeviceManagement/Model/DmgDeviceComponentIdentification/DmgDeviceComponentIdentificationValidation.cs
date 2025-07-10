//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDmgDeviceComponentIdentificationValidation
//
//    This class should be used for overriding validation in AutoDmgDeviceComponentIdentificationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.RemoteDeviceManagement
{
	public class DmgDeviceComponentIdentificationValidation : AutoDmgDeviceComponentIdentificationValidation
	{
		public DmgDeviceComponentIdentificationValidation(AutoDmgDeviceComponentIdentification parent) : base(parent)
		{
		}
	}
}
