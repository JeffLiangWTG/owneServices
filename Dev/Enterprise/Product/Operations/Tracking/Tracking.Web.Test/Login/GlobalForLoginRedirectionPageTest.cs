using System;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class GlobalForLoginRedirectionPageTest : Global
	{
		public void OnCustomSessionStart()
		{
			OnCustomSessionStart(this, EventArgs.Empty);
		}
	}
}
