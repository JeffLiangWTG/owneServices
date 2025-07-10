using System;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class PreloadForTest : Preload
	{
		public void OnLoadForTest() => OnLoad(EventArgs.Empty);
	}
}
