using System;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class GlobalForLoginCompleteTest : Global
	{
		public void OnCustomSessionStart()
		{
			OnCustomSessionStart(this, EventArgs.Empty);
		}
	}
}
