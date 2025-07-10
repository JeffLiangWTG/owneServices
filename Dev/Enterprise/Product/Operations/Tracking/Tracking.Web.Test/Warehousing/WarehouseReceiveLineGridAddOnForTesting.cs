using System;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WarehouseReceiveLineGridAddOnForTesting : WarehouseReceiveLineGridAddOn
	{
		public void OnPreRenderForTesting()
		{
			OnPreRender(new EventArgs());
		}
	}
}
