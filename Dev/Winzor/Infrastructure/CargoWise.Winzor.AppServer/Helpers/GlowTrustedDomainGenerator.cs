using System;
using System.Collections.Generic;
using Enterprise.Registry.Business;

namespace CargoWise.Winzor.AppServer.Helpers
{
	public interface ITrustedDomainGenerator
	{
		public List<string> GetTrustedDomains();
	}

	public class GlowTrustedDomainGenerator : ITrustedDomainGenerator
	{
		public List<string> GetTrustedDomains()
		{
			return new List<string>
			{
				GlowPortalsUri
			};
		}

		string GlowPortalsUri
		{
			get
			{
				return GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}
	}
}
