using System.Threading;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.NZ.ServiceTasks.CUSMOD.MessageRetrieverService.Code,
	Enterprise.Customs.NZ.ServiceTasks.CUSMOD.MessageRetrieverService.FriendlyName,
	Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.NewZealandCustoms,
	typeof(Enterprise.Customs.NZ.ServiceTasks.CUSMOD.MessageRetrieverService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.NewZealand,
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding("NCR",
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_IsActive          + "=Y",
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit   + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_Status            + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ApplicationCode   + "=" + ApplicationCodeList.Codes.NZCustoms
	},
	"NZ Customs Inbound Interchanges")]

namespace Enterprise.Customs.NZ.ServiceTasks.CUSMOD
{
	class MessageRetrieverService : MessagingService
	{
		public const string Code = "NCR";
		public const string FriendlyName = "NZ Customs Message Retriever";

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
						//create messages from interchange
						using (var processor = new Business.BatchProcessor.InboundInterchangeProcessor(log))
						{
							processor.ExecuteBatch(token);
						}
					});
				}
			}
		}
	}
}
