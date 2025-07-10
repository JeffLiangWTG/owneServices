using System;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.Tracking.Business
{
	public class GlowTrackingUrlGenerator : IGlowTrackingUrlGenerator
	{
		public GlowTrackingUrlGenerator()
		{
		}

		public Uri GenerateURL(ZGuid contactPK)
		{
			var baseURL = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			return !string.IsNullOrEmpty(baseURL) ? UrlBuilder.GenerateURLForContact(contactPK.ToGuid(), new Uri(baseURL), "TRK") : null;
		}
	}
}
