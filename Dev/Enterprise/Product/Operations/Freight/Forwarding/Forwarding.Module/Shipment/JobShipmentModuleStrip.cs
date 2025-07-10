using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Module;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Module;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class JobShipmentModuleStrip : WorkflowFilterStripWithRoutingSupport
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			var voyageVesselModuleFilter = currentModuleFilter as VoyageVesselModuleFilter;
			if (voyageVesselModuleFilter != null)
			{
				var control = new VoyageVesselModuleFilterControl(this, FilterControlBindingSource);
				control.SetMaxLength(voyageVesselModuleFilter);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				result = new Control[] { control };
			}
			else if (currentModuleFilter is ModuleWarehouseLocationFilter)
			{
				result = ModuleWarehouseLocationFilterControlHelper.GetNewWarehouseFilterControls(this, FilterControlBindingSource);
			}
			else if (CusEntryNumberFilterStripControlHelper.IsModuleFilterSupported(currentModuleFilter))
			{
				result = CusEntryNumberFilterStripControlHelper.GetFilterControls(this, currentModuleFilter, FilterControlBindingSource);
			}
			else if (currentModuleFilter is OrgRelatedPartiesModuleFilter)
			{
				OrgRelatedPartiesFilterControl control = new OrgRelatedPartiesFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				result = new Control[] { control };
			}
			else if (currentModuleFilter is ReferenceNumberFilter)
			{
				result = ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(this, FilterControlBindingSource);
			}
			else if (currentModuleFilter is IJobManagementAmountFilter)
			{
				result = new Control[] { (Control)((IJobManagementAmountFilter)currentModuleFilter).GetFilterControl() };
			}
			else if (currentModuleFilter is OrgClientAssignedStaffModuleFilter)
			{
				OrgClientAssignedStaffFilterStrip control = new OrgClientAssignedStaffFilterStrip(true);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				result = new Control[] { control };
			}
			else if (currentModuleFilter is DateLocationFilter)
			{
				DateLocationFilterControl control = new DateLocationFilterControl(this);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				FilterControlBindingSource.SetBindingMember(control, ".");

				result = new Control[] { control };
			}
			else if (currentModuleFilter is DateOrganizationFilter)
			{
				var control = new DateOrganizationFilterControl(this);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				FilterControlBindingSource.SetBindingMember(control, ".");

				result = new Control[] { control };
			}
			else if (currentModuleFilter is DGClassDGSubstanceFilter)
			{
				var control = new DGClassDGSubstanceFilterControl(this);
				control.RestrictDGSubstanceFindBoxResize();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				result = new Control[] { control };
			}
			else if (currentModuleFilter is ServiceTypeDateFilter)
			{
				var control = ServiceTypeDateFilter.GetServiceTypeDateFilterControl(this, FilterControlBindingSource);
				PreferredHeight = control.Height;
				result = new[] { control };
			}
			else if (currentModuleFilter is CO2eStatusAndCO2eKgRangeNumberFilter && ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				var control = new CO2eStatusAndCO2eKgRangeNumberFilterControl(this);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;
				FilterControlBindingSource.SetBindingMember(control, ".");
				result = new Control[] { control };
			}
			else if (currentModuleFilter is EntryStatusFilter entryStatusFilter)
			{
				result = EntryStatusFilterGUIProvider.GetEntryStatusFilterControls(this, FilterControlBindingSource, entryStatusFilter.ShowComparisonOperator, entryStatusFilter.ShowFilterType);
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
