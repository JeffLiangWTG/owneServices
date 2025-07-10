using System.Threading;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.BatchProcessor;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.TW.ServiceTasks.MessageRetrieverService.Code,
	Enterprise.Customs.TW.ServiceTasks.MessageRetrieverService.FriendlyName,
	Enterprise.Customs.TW.ServiceTasks.MessagingService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.TW.ServiceTasks.MessageRetrieverService),
	MinimumPeriod = "30Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Taiwan,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.TW.ServiceTasks.MessageRetrieverService.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.TaiwanCustoms },
	"TW Customs interchanges inbound"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.TW.ServiceTasks.MessageRetrieverService.Code,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
				 EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
				 EDIMessageSchema.Constants.EM_IsActive + "=Y",
				 EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.TaiwanCustoms },
	"TW Customs messages inbound"
	)]

namespace Enterprise.Customs.TW.ServiceTasks
{
	public class MessageRetrieverService : MessagingService
	{
		public const string Code = "TWC";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Name")]
		public const string FriendlyName = "TW Customs Message Retriever";

		protected override void RunTaskCore(CancellationToken token)
		{
			var isUCMPFunctionalityValid = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.Taiwan, ZDateTime.Now);
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Taiwan))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var logger = GetNewLogger();
						using (var interchangeProcessor = new TWCInboundInterchangeProcessor(logger))
						{
							interchangeProcessor.ExecuteBatch(token);
						}

						if (!isUCMPFunctionalityValid)
						{
							using (var messageProcessor = new TWCIncomingMessageProcessor(logger))
							{
								messageProcessor.ExecuteBatch(token);
							}
						}
					});
				}
			}
		}

		[HostedServiceRequirement]
		public static string CheckTWCompanyHasCredentialsExist() => ServiceTaskEnvironmentChecker.CheckTWCompanyHasCredentials();
	}
}
