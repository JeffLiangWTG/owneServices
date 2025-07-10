using System.Collections.Generic;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.NZ.Registry;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.NZ.Business.BatchProcessor
{
	public class BatchProcessorEnvironmentChecker : BaseEnvironmentChecker
	{
		public BatchProcessorEnvironmentChecker()
		{
		}

		protected override void CheckEverythingRequiredToRunIsInPlaceCore(List<string> failureDescriptionsList)
		{
			AddIfNotNullOrEmpty(HostedServiceRequirementAttribute.CheckValueStringLengthIsGreaterThan(NZCustomsDataRegistry.Instance.NZBrokerageID, 4), failureDescriptionsList);
		}
	}
}
