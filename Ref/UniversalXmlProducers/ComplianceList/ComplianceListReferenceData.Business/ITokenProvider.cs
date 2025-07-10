using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.ComplianceListReferenceData.Business
{
	public interface ITokenProvider
	{
		Task<string> GetAuthorizationTokenAsync(string tenantId, string clientId, string serviceId, string privateKeyFileName, string certificateFileName);
	}
}
