using System.Threading;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.ZA.ServiceTasks.MessageRetrieverService.Code,
	Enterprise.Customs.ZA.ServiceTasks.MessageRetrieverService.FriendlyName,
	Enterprise.Customs.ZA.ServiceTasks.MessagingService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.ZA.ServiceTasks.MessageRetrieverService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.ZA.ServiceTasks.MessageRetrieverService.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.SouthAfricanCustoms },
	"ZA Customs interchanges inbound"
	)]

namespace Enterprise.Customs.ZA.ServiceTasks
{
	class MessageRetrieverService : MessagingService
	{
		public const string Code = "ZCR";
		public const string FriendlyName = "ZA Customs Message Retriever";

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.SouthAfrica))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var logger = GetNewLogger();
						//create messages from interchange
						using (var processor = new Business.BatchProcessor.InboundInterchangeProcessor(logger))
						{
							processor.ExecuteBatch(token);
						}
					});
				}
			}
		}
	}
}
