using System.Threading;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ISF.ServiceTasks.ISFOutgoingServiceTask.ServiceTaskCode,
	"United States ISF Outgoing Customs Messaging",
	"USC",
	typeof(Enterprise.Customs.US.ISF.ServiceTasks.ISFOutgoingServiceTask),
	MinimumPeriod = "30Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ISF.ServiceTasks.ISFOutgoingServiceTask.ServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + CBPEDIInterchange.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"US Customs ISF messages outbound"
	)]

namespace Enterprise.Customs.US.ISF.ServiceTasks
{
	public class ISFOutgoingServiceTask : Customs.ServiceTasks.NudgeCustomsServiceTask
	{
		internal const string ServiceTaskCode = "US3";
		protected override string CurrentServiceTaskCode => ServiceTaskCode;
		protected override void RunMainTask(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany())
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					new ISFOutgoingMessageProcessor(Logger).ProcessMessage(token);
				}
			}
		}
	}
}
