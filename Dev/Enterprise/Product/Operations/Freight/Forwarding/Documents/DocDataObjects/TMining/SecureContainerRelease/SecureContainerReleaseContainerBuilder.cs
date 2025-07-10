using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class SecureContainerReleaseContainerBuilder
	{
		public SecureContainerReleaseContainer Build(CommonContainer containerBO, string formMode)
		{
			if (containerBO == null)
			{
				return null;
			}

			var container = new SecureContainerReleaseContainer(containerBO.PK, formMode);
			var eventHelper = new SecureContainerReleaseContainerEventHelper(containerBO);

			container.Number = containerBO.JC_ContainerNum;
			container.ReleaseIdentification = containerBO.JC_ContainerImportDORelease;
			container.IsNonOperativeReefer = containerBO.JC_IsNonOperativeReefer;
			container.IsTranferToForwarder = true;
			container.CurrentStatus = eventHelper.GetCurrentStatusFromEvents().currentStatus;

			return container;
		}
	}
}
