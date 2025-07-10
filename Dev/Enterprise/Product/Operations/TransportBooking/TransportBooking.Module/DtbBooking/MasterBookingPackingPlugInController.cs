using CargoWise.EntityFramework;
using Enterprise.Packing.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.TransportBookings.Module
{
	public class MasterBookingPackingPlugInController : PackingPlugInController
	{
		public override ControllerID ID => ControllerIDs.MasterBookingPackingPlugIn;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new MasterBookingPackingPlugIn(businessEntity);
	}
}
