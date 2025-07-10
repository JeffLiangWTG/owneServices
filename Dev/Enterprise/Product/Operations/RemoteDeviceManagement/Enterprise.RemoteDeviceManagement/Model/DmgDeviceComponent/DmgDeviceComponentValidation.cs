//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDmgDeviceComponentValidation
//
//    This class should be used for overriding validation in AutoDmgDeviceComponentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.RemoteDeviceManagement
{
	public class DmgDeviceComponentValidation : AutoDmgDeviceComponentValidation
	{
		public DmgDeviceComponentValidation(AutoDmgDeviceComponent parent) : base(parent)
		{
		}
	}
}
