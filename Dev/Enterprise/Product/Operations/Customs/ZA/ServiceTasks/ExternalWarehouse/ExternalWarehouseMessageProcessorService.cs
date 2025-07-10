using System.Threading;
using Enterprise.Customs.ZA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.ZA.ServiceTasks.ExternalWarehouseMessageProcessorService.Code,
	Enterprise.Customs.ZA.ServiceTasks.ExternalWarehouseMessageProcessorService.FriendlyName,
	Enterprise.Customs.ZA.ServiceTasks.ExternalWarehouseMessageProcessorService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.ZA.ServiceTasks.ExternalWarehouseMessageProcessorService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Seconds",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.ZA.ServiceTasks.ExternalWarehouseMessageProcessorService.Code,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
				 EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
				 EDIMessageSchema.Constants.EM_IsActive + "=Y",
				 EDIMessageSchema.Constants.EM_MessageType + "=" + ZAEDIMessageTypeList.Codes.ExternalWarehouse,
				 EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.SouthAfricanTransactionOrders },
	"ZA External Warehouse Message Processor"
	)]

namespace Enterprise.Customs.ZA.ServiceTasks
{
	class ExternalWarehouseMessageProcessorService : MessagingService
	{
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
						using (var processor = new ExternalWarehouseIncomingMessageProcessor(logger))
						{
							processor.ExecuteBatch(token);
						}
					});
				}
			}
		}

		public const string Code = "ZWP";
		public const string FriendlyName = "ZA External Warehouse Message Processor";
	}
}
