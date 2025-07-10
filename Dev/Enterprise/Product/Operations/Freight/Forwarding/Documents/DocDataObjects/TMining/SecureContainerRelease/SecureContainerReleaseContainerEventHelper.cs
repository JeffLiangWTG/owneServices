using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class SecureContainerReleaseContainerEventHelper
	{
		public SecureContainerReleaseContainerEventHelper(CommonContainer containerBO)
		{
			Argument.NotNull(containerBO, nameof(containerBO));
			this.containerBO = containerBO;
		}

		readonly CommonContainer containerBO;

		public (string currentStatus, string eventTypeCode) GetCurrentStatusFromEvents()
			=> Business.SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(CertifiedPickupLogs);

		#region Implementation

		public IEnumerable<StmALog> CertifiedPickupLogs => certifiedPickupLogs ?? (certifiedPickupLogs = Business.SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(containerBO));
		IEnumerable<StmALog> certifiedPickupLogs;

		#endregion
	}
}
