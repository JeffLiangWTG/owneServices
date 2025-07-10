using System;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class BookingPackLineGridAddOnForTesting : BookingPackLineGridAddOn
	{
		public void OnPreRenderForTesting() => OnPreRender(new EventArgs());
	}
}
