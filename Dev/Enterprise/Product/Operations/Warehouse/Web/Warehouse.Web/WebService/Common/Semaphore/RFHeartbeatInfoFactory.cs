using Enterprise.Semaphores.Common;
using Enterprise.Warehouse.Web.WebService;

namespace Enterprise.Warehouse.Web
{
	public class RFHeartbeatInfoFactory : IHeartbeatInfoFactory
	{
		public RFHeartbeatInfoFactory(SecuritySOAPHeader securityHeader)
		{
			SecurityHeader = securityHeader;
		}

		readonly SecuritySOAPHeader SecurityHeader;

		IHeartbeatInfo IHeartbeatInfoFactory.New()
		{
			return new RFHeartbeatInfo(SecurityHeader);
		}
	}
}