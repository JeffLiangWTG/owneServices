using System;
using Enterprise.Core.Environment.Semaphores;
using Enterprise.Semaphores.Common;
using Enterprise.Warehouse.Web.WebService;

namespace Enterprise.Warehouse.Web
{
	public class RFHeartbeatInfo : IHeartbeatInfo
	{
		public RFHeartbeatInfo(SecuritySOAPHeader securityHeader)
		{
			SecurityHeader = securityHeader;
		}

		readonly SecuritySOAPHeader SecurityHeader;

		readonly IHeartbeatInfo EnterpriseHeartbeatInfo = ((IHeartbeatInfoFactory)new EnterpriseHeartbeatInfoFactory()).New();

		string IHeartbeatInfo.FullUserName
		{
			get { return EnterpriseHeartbeatInfo.FullUserName; }
		}

		Guid IHeartbeatInfo.HeartbeatId
		{
			get { return EnterpriseHeartbeatInfo.HeartbeatId; }
		}

		string IHeartbeatInfo.HostName
		{
			get { return SecurityHeader.DeviceID; }
		}

		int IHeartbeatInfo.ProcessId
		{
			get { return SecurityHeader.ProcessID; }
		}

		string IHeartbeatInfo.SessionReference
		{
			get { return EnterpriseHeartbeatInfo.SessionReference; }
		}

		Guid IHeartbeatInfo.UserPk
		{
			get { return EnterpriseHeartbeatInfo.UserPk; }
		}

		string IHeartbeatInfo.EmailAddress
		{
			get { return EnterpriseHeartbeatInfo.EmailAddress; }
		}

		string IHeartbeatInfo.LogonIdentificationCode
		{
			get { return EnterpriseHeartbeatInfo.LogonIdentificationCode; }
		}

		LogonType IHeartbeatInfo.LogonType
		{
			get { return EnterpriseHeartbeatInfo.LogonType; }
		}

		string IHeartbeatInfo.HeartbeatType
		{
			get { return HeartbeatTypes.WarehouseRF; }
		}

		public string ClientIdentifier => null;
	}
}
