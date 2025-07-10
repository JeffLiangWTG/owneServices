using System.Threading;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CargoIMPPhase2ServiceTask.Code,
	"CargoIMP Phase 2 Messaging",
	"FRT",
	typeof(CargoIMPPhase2ServiceTask),
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "15minutes",
	CanRunInAnyBranch = true)
]

[assembly: HostedServiceBusinessObjectBinding(
	CargoIMPPhase2ServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status            + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit   + "=" + ReceiveTransmitList.Codes.Transmit,
		EDIMessageSchema.Constants.EM_IsActive          + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode   + "=" + ApplicationCodeList.Codes.CargoIMPPhase2
	},
	"Cargo IMP Phase 2")]

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2
{
	public class CargoIMPPhase2ServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			DoOutboundProcesses(token);
		}

		void DoOutboundProcesses(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany())
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					CargoIMPPhase2InterchangeSender.New(ServiceLogger).ExecuteBatch(token);
				}
			}
		}
		
		public const string Code = "CI2";
	}
}
