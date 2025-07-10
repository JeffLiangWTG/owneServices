using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.DeniedPartyScreening.Integration
{
	public interface IDpsManager
	{
		Task<DpsResponse> Screen(
			DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching,
			IDpsServiceV4 service);

		Task<DpsResponse> GetScreenResult(
			DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching,
			List<IDpsServiceV4> services);

		IDpsServiceV4 GetDpsService(string url);

		List<IDpsServiceV4> GetDpsServices(IDpsServiceV4 service);

		void DeactivateBillingEntities(Guid[] entityPKs);
	}
}
