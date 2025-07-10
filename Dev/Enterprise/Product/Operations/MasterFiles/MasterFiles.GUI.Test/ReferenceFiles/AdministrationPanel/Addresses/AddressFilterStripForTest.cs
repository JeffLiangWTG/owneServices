using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressFilterStripForTest : AddressFilterStrip
	{
		public Control[] GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter)
		{
			return GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
