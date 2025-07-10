using System;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WarehouseOrderLineGridAddOnForTesting : WarehouseOrderLineGridAddOn
	{
		public void OnPreRenderForTesting()
		{
			OnPreRender(new EventArgs());
		}
	}
}
