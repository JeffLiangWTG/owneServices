using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Module;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public class QuotedBookingFilterStrip : WorkflowFilterStripWithRoutingSupport
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is OrgRelatedPartiesModuleFilter)
			{
				OrgRelatedPartiesFilterControl control = new OrgRelatedPartiesFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				return new Control[] { control };
			}
			if (currentModuleFilter is ReferenceNumberFilter)
			{
				return ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(this, FilterControlBindingSource);
			}

			if (currentModuleFilter is ServiceTypeDateFilter)
			{
				var control = ServiceTypeDateFilter.GetServiceTypeDateFilterControl(this, FilterControlBindingSource);
				PreferredHeight = control.Height;
				return new[] { control };
			}

			if (currentModuleFilter is IJobManagementAmountFilter amountManagenemtFilter)
			{
				return new Control[] { (Control)amountManagenemtFilter.GetFilterControl() };
			}

			var voyageVesselModuleFilter = currentModuleFilter as VoyageVesselModuleFilter;
			if (voyageVesselModuleFilter != null)
			{
				var control = new VoyageVesselModuleFilterControl(this, FilterControlBindingSource);
				control.SetMaxLength(voyageVesselModuleFilter);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				return new Control[] { control };
			}

			if (currentModuleFilter is DGClassDGSubstanceFilter)
			{
				var control = new DGClassDGSubstanceFilterControl(this);
				control.RestrictDGSubstanceFindBoxResize();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				return new Control[] { control };
			}

			if (currentModuleFilter is CO2eStatusAndCO2eKgRangeNumberFilter && ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				var control = new CO2eStatusAndCO2eKgRangeNumberFilterControl(this);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;
				FilterControlBindingSource.SetBindingMember(control, ".");
				return new Control[] { control };
			}

			return base.GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
