using System;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ForwarderDetailsUserControlForTest : ForwarderDetailsUserControl
	{
		public void OnLoadForTest()
		{
			base.OnLoad(EventArgs.Empty);
		}
	}
}
