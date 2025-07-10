//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewVesselRoutingPortsValidation
//
//    This class should be used for overriding validation in AutoViewVesselRoutingPortsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class ViewVesselRoutingPortsValidation : AutoViewVesselRoutingPortsValidation
	{
		public ViewVesselRoutingPortsValidation(AutoViewVesselRoutingPorts parent) : base(parent)
		{
		}
	}
}
