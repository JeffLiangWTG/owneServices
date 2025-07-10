using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.Customs.US.AIM.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	AIMMessageServiceTask.ServiceTaskCode,
	AIMMessageServiceTask.ServiceTaskDescription,
	AIMMessageServiceTask.ServiceTaskCategory,
	typeof(AIMMessageServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	AIMMessageServiceTask.ServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USAMA,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	AIMMessageServiceTask.ServiceTaskDescription
	)]

namespace Enterprise.Customs.US.AIM.ServiceTasks
{
	public class AIMMessageServiceTask : Customs.ServiceTasks.NudgeCustomsServiceTask
	{
		public const string ServiceTaskCategory = "USC";
		public const string ServiceTaskCode = "AMM";
		public const string ServiceTaskDescription = "US Air Manifest Message";

		protected override string CurrentServiceTaskCode => ServiceTaskCode;

		protected override void RunMainTask(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var factory = new BusinessObjectFactory();
			var firstActiveBranch = factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_IsActive, true));
			using (Environment.DisposableEnvironment.ForBranch(firstActiveBranch.PK.ToGuid()))
			{
				var processor = new AIMMessageProcessorFactory(Logger);
				processor.ExecuteBatch(token);
			}
		}
	}
}
