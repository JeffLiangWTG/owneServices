using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateBookingDetailCollection : ActiveBusinessObjectCollection<GateBookingDetail>
	{
		public GateBookingDetailCollection(GateBooking parent)
			: base(parent.Factory, parent, null, GateBookingDetailSchema.GTD_GTB_GateBooking)
		{
		}
	}
}
