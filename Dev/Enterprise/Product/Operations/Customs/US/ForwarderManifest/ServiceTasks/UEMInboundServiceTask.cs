using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.Customs.US.ForwarderManifest.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	UEMInboundServiceTask.ServiceTaskCode,
	UEMInboundServiceTask.ServiceTaskDescription,
	UEMInboundServiceTask.ServiceTaskCategory,
	typeof(UEMInboundServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	UEMInboundServiceTask.ServiceTaskCode,
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USExportManifest,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y"
	},
	UEMInboundServiceTask.ServiceTaskDescription
	)]

namespace Enterprise.Customs.US.ForwarderManifest.ServiceTasks
{
	public class UEMInboundServiceTask : NudgeCustomsServiceTask
	{
		public const string ServiceTaskCategory = "USC";
		public const string ServiceTaskCode = "UEI";
		public const string ServiceTaskDescription = "US Export Manifest Inbound";

		protected override string CurrentServiceTaskCode => ServiceTaskCode;

		protected override void RunMainTask(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var factory = new BusinessObjectFactory();
			var firstActiveBranch = factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_IsActive, true));
			using (Environment.DisposableEnvironment.ForBranch(firstActiveBranch.PK.ToGuid()))
			{
				var processor = new UEMInboundInterchangeProcessor(Logger);
				processor.ExecuteBatch(token);
			}
		}
	}
}
