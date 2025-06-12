using System.Linq;
using System.Threading;
using CargoWise.eServices.Billing.DataAccess;
using Common.Logging;
using Newtonsoft.Json.Linq;

namespace CargoWise.eServices.Billing.WcfService.Hangfire.BillingDatabase
{
	public class BillingDatabaseSPExecution : BillingJob
	{
		public override void Processing(CancellationToken token, params object[] args)
		{
			var param = (args.First() as JObject)?.ToObject<SPExecutionParam>();
			Logger.Info($"{param}");
			CreateRepository().SPExecution(param);
		}

		internal virtual IBillingRepository CreateRepository() => new BillingRepository(Logger, true);
		public override ILog Logger => LogManager.GetLogger<BillingDatabaseSPExecution>();
		public override string Name => nameof(BillingDatabaseSPExecution);
		public override string ItemName => "billing database store procedure";
		protected override bool ExitWhenClosed => true;
	}
}
