using System;
using System.Threading;
using Enterprise.Customs.NZ.Business.BatchProcessor;
using Enterprise.Customs.NZ.ServiceTasks.CUSMOD;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	MessageSenderService.ServiceTaskCode,
	MessageSenderService.ServiceTaskDescription,
	Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.NewZealandCustoms,
	typeof(MessageSenderService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.NewZealand,
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	MessageSenderService.ServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.NZCustoms
	},
	"NZ Customs Messages outbound")]

namespace Enterprise.Customs.NZ.ServiceTasks.CUSMOD
{
	class MessageSenderService : MessagingService
	{
		public const string ServiceTaskCode = "NCS";
		public const string ServiceTaskDescription = "NZ Customs Message Sender";

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in ValidBranches)
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var log = GetNewLogger();
						var processor = new NZCOutgoingMessageProcessor(log);
						processor.ProcessMessage(token);

						using (var sender = new InterchangeSender(log))
						{
							sender.ExecuteBatch(token);
						}
					});
				}
			}
			SetObsoleteMessagesToFail(token);
		}

		void SetObsoleteMessagesToFail(CancellationToken token)
		{
			foreach (var branch in BranchesWithoutBrokerageID)
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch))
				{
					var log = GetNewLogger();
					var processor = new NZCOutgoingMessageProcessor(log);
					var proxy = new ObsoleteMessageProcessorProxy(processor);
					proxy.Process(token, EDIMessage.Status.Failed, defaultTimeSpanForObsoleteMessages);
				}
			}
		}

		readonly TimeSpan defaultTimeSpanForObsoleteMessages = TimeSpan.FromDays(180);
	}
}
