using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class AIMMessageProcessorFactory : Enterprise.Messaging.Business.BaseMessageProcessor
	{
		public AIMMessageProcessorFactory(LoggingInformation logger) : base(logger)
		{
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new AIMMessageProcessor(Logger));
			return result;
		}

		protected override ZQuery ValidBranchesForMessageFilter => null;
	}
}
