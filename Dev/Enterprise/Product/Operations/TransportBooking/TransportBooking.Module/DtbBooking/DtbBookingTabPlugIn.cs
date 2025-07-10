using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingTabPlugIn : ZPlugIn
	{
		public DtbBookingTabPlugIn(IBusiness transportBookingParent)
			: base(transportBookingParent)
		{
		}

		public override string Name
		{
			get { return (NoResString)"Transport Booking Tab Plug-in"; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.TransportBookings; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new DtbBookingUserControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			var result = base.GetBusinessEntityForPlugIn();

			if (HostBusinessEntity is BookingDirectionSelection selection && selection.Parent != null)
			{
				var direction = GetValidBookingDirection(selection.Parent, selection.Direction);
				result = new DtbBookingParentWrapper(selection.Parent, direction);
			}
			else if (HostBusinessEntity is IDtbBookingParent parent)
			{
				var directions = parent.GetSupportedDirections();
				var direction = directions.Length == 1 ? directions[0] : DtbBookingDirection.None;
				result = new DtbBookingParentWrapper(parent, direction);
			}

			return result;
		}

		static DtbBookingDirection GetValidBookingDirection(IDtbBookingParent parent, DtbBookingDirection direction)
		{
			return parent != null && parent.GetSupportedDirections().Contains(direction) ? direction : DtbBookingDirection.None;
		}

		protected override ZTabPagePlugIn GetTabPage()
		{
			return new ZAutoSizedTabPagePlugIn(this);
		}
	}
}
