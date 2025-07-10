using System.Windows.Forms;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Module
{
	[SuppressFormDesignerAnalysis]
	public class OrgSupplierPartFilterStrip : ImporterSupplierFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is TariffProvTariffModuleFilter)
			{
				var control = TariffProvTariffFilterControl.New(false);
				PreferredHeight = control.Height;
				result = new Control[] { control };
			}
			else if (currentModuleFilter is TariffInvalidModuleFilter)
			{
				var control = TariffInvalidFilterControl.New();
				PreferredHeight = control.Height;
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
