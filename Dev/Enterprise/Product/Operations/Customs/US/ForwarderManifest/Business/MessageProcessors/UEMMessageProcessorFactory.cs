using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class UEMMessageProcessorFactory : Enterprise.Messaging.Business.BaseMessageProcessor
	{
		public UEMMessageProcessorFactory(LoggingInformation logger) : base(logger)
		{
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new UEMMessageProcessor(Logger));
			return result;
		}

		protected override ZQuery ValidBranchesForMessageFilter => null;
	}
}
