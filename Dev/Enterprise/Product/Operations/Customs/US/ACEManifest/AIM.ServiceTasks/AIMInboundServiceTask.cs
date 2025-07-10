using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.Customs.US.AIM.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	AIMInboundServiceTask.ServiceTaskCode,
	AIMInboundServiceTask.ServiceTaskDescription,
	AIMInboundServiceTask.ServiceTaskCategory,
	typeof(AIMInboundServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	AIMInboundServiceTask.ServiceTaskCode,
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USAMA,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y"
	},
	AIMInboundServiceTask.ServiceTaskDescription
	)]

namespace Enterprise.Customs.US.AIM.ServiceTasks
{
	public class AIMInboundServiceTask : Customs.ServiceTasks.NudgeCustomsServiceTask
	{
		public const string ServiceTaskCategory = "USC";
		public const string ServiceTaskCode = "AMI";
		public const string ServiceTaskDescription = "US Air Manifest Inbound";

		protected override string CurrentServiceTaskCode => ServiceTaskCode;

		protected override void RunMainTask(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var factory = new BusinessObjectFactory();
			var firstActiveBranch = factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_IsActive, true));
			using (Environment.DisposableEnvironment.ForBranch(firstActiveBranch.PK.ToGuid()))
			{
				var processor = new AIMInboundInterchangeProcessor(Logger);
				processor.ExecuteBatch(token);
			}
		}
	}
}
