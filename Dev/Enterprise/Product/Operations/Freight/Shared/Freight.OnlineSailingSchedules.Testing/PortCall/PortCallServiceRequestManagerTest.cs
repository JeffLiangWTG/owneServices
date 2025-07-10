using Enterprise.Freight.OnlineSailingSchedules.PortCall;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	public class PortCallServiceRequestManagerTest : AdjustableServiceRequestManagerTest
	{
		protected override AdjustableServiceRequestManager CreateManager()
		{
			return new PortCallServiceRequestManager(Notifications);
		}
	}
}
