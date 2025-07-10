using System;
using Enterprise.Semaphores.Common;
using Enterprise.Warehouse.Web.WebService;

namespace Enterprise.Warehouse.Web
{
	public class RFSemaphoreProvider : SemaphoreProvider, IHeartBeatRemoteLogoff
	{
		public RFSemaphoreProvider(SecuritySOAPHeader securityHeader)
		{
			HeartbeatInfoFactory = new RFHeartbeatInfoFactory(securityHeader);
		}

		readonly IHeartbeatInfoFactory HeartbeatInfoFactory;

		protected override TimeSpan HeartbeatDuration
		{
			get { return TimeSpan.FromMinutes(3); }
		}

		protected override IHeartbeatInfoFactory HeartbeatSessionInfoFactory
		{
			get { return HeartbeatInfoFactory; }
		}

		protected override IHeartBeatRemoteLogoff LogoffHandler
		{
			get { return this; }
		}

		public Guid HeartbeatID
		{
			get { return internalHeartbeat != null ? internalHeartbeat.HeartbeatId : Guid.Empty; }
		}

		public void OnRemoteLogoff()
		{
			DisposeInternalHeartbeat();
		}

		public void OnRemoteUpgradeLogoff(DateTime upgradeDateTimeUtc, Func<bool> updateExists)
		{
		}
	}
}
