using CargoWise.Types;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GlbStaffPopupModuleHelperForTest : GlbStaffPopupModuleHelper
	{
		public void SetReplacementStaffCode(string code)
		{
			manuallySetCode = code;
		}

		ZString manuallySetCode;

		protected override ZString ReplacementStaffCode => manuallySetCode;
	}
}
