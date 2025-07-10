using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Freight.Module;
using Enterprise.Integration.Freight;
using Enterprise.TransportCommon.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingWorkflowFilterStrip : DtbTransportWorkflowFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is CO2eStatusAndCO2eKgRangeNumberFilter && ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				var control = new CO2eStatusAndCO2eKgRangeNumberFilterControl(this);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;
				FilterControlBindingSource.SetBindingMember(control, ".");
				result = new Control[] { control };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}
	}
}
