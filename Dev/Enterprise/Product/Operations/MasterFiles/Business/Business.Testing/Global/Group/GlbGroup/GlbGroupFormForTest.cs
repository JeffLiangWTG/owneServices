using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbGroupFormForTest
	{
		public GlbGroupFormForTest(GlbGroup group)
		{
			group.ChangeActualSecurityPermissions += new EventHandler(OnEventFired);
		}

		public void OnEventFired(object sender, EventArgs e)
		{
			EventFired = true;
		}

		public bool EventFired;
	}
}
