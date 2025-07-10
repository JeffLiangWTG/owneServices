using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	public partial class USCTariffRuleFilterUserControl : ZFilterStripControl
	{
		public USCTariffRuleFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip() => new USCTariffRuleFilterStrip();
	}

	public class USCTariffRuleFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is TariffProvTariffModuleFilter)
			{
				var control = TariffProvTariffFilterControl.New(true);
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
