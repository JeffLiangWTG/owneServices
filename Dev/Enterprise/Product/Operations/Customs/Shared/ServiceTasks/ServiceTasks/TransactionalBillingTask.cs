using System.Threading;
using Enterprise.Customs.Business.TransactionalBillingReporting;
using Enterprise.Customs.ServiceTasks.TransactionalBillingReporting;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(TransactionalBillingTask.Code,
	TransactionalBillingTask.Description,
	"SYS",
	typeof(TransactionalBillingTask),
	IsMandatory = true,
	MinimumPeriod = "1day",
	MaximumPeriod = "1month",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.ServiceTasks.TransactionalBillingReporting
{
	public class TransactionalBillingTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			try
			{
				new TransactionalBillingStatementGeneratorBillingAPI(ServiceLogger).DoEverything();  // Daily - send to eHub
			}
			catch (EmailSendFailedException ex)
			{
				ServiceLogger.Log(LogType.Error, ex.Message);
			}
		}

		public const string Code = "CTB";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task description")]
		public const string Description = "Customs Transactional Billing (WiseCloud)";
	}
}
