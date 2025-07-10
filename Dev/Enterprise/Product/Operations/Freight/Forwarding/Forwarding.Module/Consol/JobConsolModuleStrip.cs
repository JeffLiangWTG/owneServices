using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Customs.Common.Module;
using Enterprise.Freight.Module;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Module
{
	[SuppressFormDesignerAnalysis]
	public class JobConsolModuleStrip : WorkflowFilterStripWithRoutingSupport
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
			else if (currentModuleFilter is CO2eStatusAndCO2eKgRangeNumberFilter && ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
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
