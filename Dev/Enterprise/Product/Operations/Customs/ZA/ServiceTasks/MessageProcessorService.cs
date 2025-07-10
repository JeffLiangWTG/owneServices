using System.Threading;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.ZA.ServiceTasks.MessageProcessorService.Code,
	Enterprise.Customs.ZA.ServiceTasks.MessageProcessorService.FriendlyName,
	Enterprise.Customs.ZA.ServiceTasks.MessagingService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.ZA.ServiceTasks.MessageProcessorService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.ZA.ServiceTasks.MessageProcessorService.Code,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
			EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
			EDIMessageSchema.Constants.EM_IsActive + "=Y",
			EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.SouthAfricanCustoms },
	"ZA Customs Response Messages")]

namespace Enterprise.Customs.ZA.ServiceTasks
{
	class MessageProcessorService : MessagingService
	{
		public const string Code = "ZCP";
		public const string FriendlyName = "ZA Customs Message Processor";

		protected override void RunTaskCore(CancellationToken token)
		{
			if (!ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now))
			{
				foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.SouthAfrica))
				{
					token.ThrowIfCancellationRequested();
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						RunTaskHandleEmailSendFailure(() =>
						{
							var logger = GetNewLogger();
							using (var processor = new ZACIncomingMessageProcessor(logger))
							{
								processor.ExecuteBatch(token);
							}
						});
					}
				}
			}
		}
	}
}
