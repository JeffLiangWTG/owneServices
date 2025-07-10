using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.GPS.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.GPS.Module
{
	public class GPSSupporterPlugIn : ZAlwaysLoadPlugIn
	{
		public GPSSupporterPlugIn(IBusiness hostEntity)
			: base(hostEntity)
		{
			equipment = hostEntity as RefEquipment;
			gPSSupporter = new GPSSupporter(equipment);
		}

		readonly RefEquipment equipment;

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return gPSSupporter;
		}

		readonly GPSSupporter gPSSupporter;

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.LocalTransportVehicleMonitoringAndManagement; }
		}

		public override string Name
		{
			get { return "GPS"; }
		}

		protected override Control GetNewUserControl()
		{
			Control result = new GPSSupporterDetailsControl();
			result.Dock = DockStyle.Fill;
			return result;
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override ZTabPagePlugIn GetTabPage()
		{
			ZAutoSizedTabPagePlugIn tabPage = new ZAutoSizedTabPagePlugIn(this);
			tabPage.MinimumAutoSizedHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(600);
			tabPage.MinimumAutoSizedWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			return tabPage;
		}
	}
}
