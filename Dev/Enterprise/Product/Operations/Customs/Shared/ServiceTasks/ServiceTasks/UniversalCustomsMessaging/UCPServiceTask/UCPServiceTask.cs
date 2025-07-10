using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	UniversalCustomsMessagingConstants.ServiceTaskCodes.Packing,
	"UniversalCustomsMessaging Process:Packing outgoing EDIMessage to EDIInterchange",
	"CUS",
	typeof(Enterprise.Customs.ServiceTasks.UCPServiceTask),
	IsMandatory = true,
	MinimumPeriod = "1minute",
	AllowsMultipleInstances = true,
	DefaultScheduleRunEvery = "1minute",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.ServiceTasks
{
	public class UCPServiceTask : UniversalCustomsMessagingInterchangeServiceTask<IUniversalCustomsEDIMessagePacker>
	{
		[HostedServiceRequirement]
		public static string HasUniversalCustomsMessagingSubscribers() =>
			UCPSubscribers.HasUniversalCustomsMessagePackers()
			? string.Empty
			: (NoResString)"There is no Customs message subscribed to Universal Customs Message Packer";

		protected override Dictionary<string, IUniversalCustomsEDIMessagePacker> GetUCMPSubscribers()
		{
			var packers = UCPSubscribers.GetApplicationCodes()
					.ToDictionary(applicationCode => applicationCode, UCPSubscribers.GetMessagePacker);
			return packers;
		}

		protected override IUniversalCustomsMessagingInterchangeProcessor GetProcessor(LoggingInformation logger, string applicationCode, IUniversalCustomsEDIMessagePacker messageHandler, CancellationToken token)
			=> new UCPProcessor(logger, applicationCode, messageHandler, token);

		protected override string ServiceTaskCode => UniversalCustomsMessagingConstants.ServiceTaskCodes.Packing;
	}
}
