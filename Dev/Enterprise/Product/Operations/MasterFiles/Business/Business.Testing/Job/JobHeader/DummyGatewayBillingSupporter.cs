using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummyGatewayBillingSupporter : IGatewayBillingSupporter
	{
		public bool IsGatewayBillingEnabled(ICompany company)
		{
			return true;
		}

		public (IOrgHeader, IOrgHeader) GatewayAgent(ICompany company)
		{
			return (GlbCompany.CurrentCompany.OrgProxy, null);
		}
	}
}
