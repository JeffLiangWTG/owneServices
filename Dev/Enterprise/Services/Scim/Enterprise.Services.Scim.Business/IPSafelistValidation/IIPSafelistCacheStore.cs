using System.Collections.Generic;
using System.Net;

namespace Enterprise.Services.Scim.Business
{
	public interface IIPSafelistCacheStore
	{
		List<IPNetwork2> GetSafelistedIpsFromCache();
	}
}
