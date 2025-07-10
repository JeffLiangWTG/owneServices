using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Packing.Module;
using Enterprise.TransportBookings.GUI;

namespace Enterprise.TransportBookings.Module
{
	public class MasterBookingPackingPlugIn : PackingPlugIn
	{
		public MasterBookingPackingPlugIn(IBusiness parent) : base(parent)
		{
		}

		protected override Control GetNewUserControl() => new MasterBookingPackingUserControl();
	}
}
