using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.Customs.US.ForwarderManifest.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	UEMMessageServiceTask.ServiceTaskCode,
	UEMMessageServiceTask.ServiceTaskDescription,
	UEMMessageServiceTask.ServiceTaskCategory,
	typeof(UEMMessageServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	UEMMessageServiceTask.ServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USExportManifest,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	UEMMessageServiceTask.ServiceTaskDescription
	)]

namespace Enterprise.Customs.US.ForwarderManifest.ServiceTasks
{
	public class UEMMessageServiceTask : NudgeCustomsServiceTask
	{
		public const string ServiceTaskCategory = "USC";
		public const string ServiceTaskCode = "UEM";
		public const string ServiceTaskDescription = "US Export Manifest Message";

		protected override string CurrentServiceTaskCode => ServiceTaskCode;

		protected override void RunMainTask(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var factory = new BusinessObjectFactory();
			var firstActiveBranch = factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_IsActive, true));
			using (Environment.DisposableEnvironment.ForBranch(firstActiveBranch.PK.ToGuid()))
			{
				var processor = new UEMMessageProcessorFactory(Logger);
				processor.ExecuteBatch(token);
			}
		}
	}
}
