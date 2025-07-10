using System.Threading;
using Enterprise.Customs.NZ.Business.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.NZ.ServiceTasks.CUSMOD.MessageProcessorService.Code,
	Enterprise.Customs.NZ.ServiceTasks.CUSMOD.MessageProcessorService.FriendlyName,
	Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.NewZealandCustoms,
	typeof(Enterprise.Customs.NZ.ServiceTasks.CUSMOD.MessageProcessorService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.NewZealand,
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.NZ.ServiceTasks.CUSMOD.MessageProcessorService.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.NewZealandCustoms
	},
	"NZ Customs Response Messages")]

namespace Enterprise.Customs.NZ.ServiceTasks.CUSMOD
{
	class MessageProcessorService : MessagingService
	{
		public const string Code = "NCP";
		public const string FriendlyName = "NZ Customs Message Processor";

		protected sealed override void RunTaskCore(CancellationToken token)
		{
			foreach (var branchPK in ValidBranches)
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branchPK))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var logger = GetNewLogger();
						using (var processor = new Processor(logger))
						{
							processor.ExecuteBatch(token);
						}
					});
				}
			}
		}
	}
}
