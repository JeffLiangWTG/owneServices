using System.Windows.Forms;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFilters.Module.Testing
{
	sealed class EDICodeMappingFilterStripForTest : EDICodeMappingFilterStrip
	{
		public Control[] GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter) => GetCurrentFilterControls(currentModuleFilter);
	}
}
