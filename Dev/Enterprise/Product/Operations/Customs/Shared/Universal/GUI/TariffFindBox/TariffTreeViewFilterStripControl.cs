using System.Linq;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public class TariffTreeViewFilterStripControl : ZFilterStripBaseControl
	{
		public TariffTreeViewFilterStripControl(RefCusTariffFilterStripBusinessObject filterBusinessObject) : base(filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;
		}
		public readonly RefCusTariffFilterStripBusinessObject filterBusinessObject;

		protected override bool GetDefaultShouldRunSearchOnStripsInitialized()
			=> base.GetDefaultShouldRunSearchOnStripsInitialized() && filterBusinessObject.ActiveModuleFilters.OfType<ModuleTextFilter>().Any(filter =>
			(filter.Description == Constants.RefCusTariffFilters.TariffCode || filter.Description == Constants.RefCusTariffFilters.DefaultLanguageDescription) && !filter.Property.IsEmpty);

		public override void Find(bool isManualSearch = true)
		{
			FirePerformSearch(Visible, isManualSearch);
		}

		public void SetFindStatus(bool enabled)
		{
			ToolStrip.Enabled = enabled;
		}
	}
}
