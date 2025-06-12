using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using CargoWise.eServices.Billing.DataAccess;
using Common.Logging;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;

namespace CargoWise.eServices.Billing.WcfService.Hangfire.BillingDatabase
{
	public class BillingDatabaseProcessor : BillingJob
	{
		[DisableConcurrentExecution(ProcessingBillingResourceName, 0)]
		[AutomaticRetry(Attempts = 50, DelaysInSeconds = new[] { 60 }, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete, OnlyOn = new[] { typeof(DistributedLockTimeoutException) })]
		public static void RecurringProcess(PerformContext context, CancellationToken token) => new BillingDatabaseProcessor(logger: GetLogger("ProcessBillingDatabase")).StartProcess(token, context);

		[DisableConcurrentExecution(ProcessingBillingResourceName, 0)]
		[AutomaticRetry(Attempts = 1000, DelaysInSeconds = new[] { 60 }, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete, OnlyOn = new[] { typeof(DistributedLockTimeoutException) })]
		[ProlongExpirationTime(Days = 90)]
		public static void RecurringProcessMonthlyAggregation(PerformContext context, CancellationToken token) => new BillingDatabaseProcessor(performMonthlyAggregation: true, logger: GetLogger("ProcessBillingDatabaseMonthlyAggregation")).StartProcess(token, context);

		[DisableConcurrentExecution(ProcessingBillingResourceName, 0)]
		[AutomaticRetry(Attempts = 100, DelaysInSeconds = new[] { 60 }, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete, OnlyOn = new[] { typeof(DistributedLockTimeoutException) })]
		[ProlongExpirationTime(Days = 2)]
		public static void RecurringProcessUpdateBillingCubePartial(PerformContext context, CancellationToken token) => new BillingDatabaseProcessor(updateBillingCubePartial: true, logger: GetLogger("ProcessBillingDatabaseUpdateBillingCubePartial")).StartProcess(token, context);

		public BillingDatabaseProcessor() : this(false, false, LogManager.GetLogger<BillingDatabaseProcessor>()) { }

		public BillingDatabaseProcessor(bool performMonthlyAggregation = false, bool updateBillingCubePartial = false, ILog logger = null)
		{
			PerformMonthlyAggregation = performMonthlyAggregation;
			UpdateBillingCubePartial = updateBillingCubePartial;
			Logger = logger;
		}

		public override void Processing(CancellationToken token, params object[] args)
		{
			try
			{
				var repo = CreateRepository(token);
				repo.UpdateChargeable(DateTimeProvider?.DateTimeNow.ToUniversalTime(), performMonthlyAggregation: PerformMonthlyAggregation, updateBillingCube: UpdateBillingCubePartial);
			}
			catch (SqlException e) when (e.Message.Contains("Operation cancelled by user"))
			{
				// To stop auto-requeueing the job when the user cancels the operation
				Logger.Error(e.Message);
			}
		}

		internal virtual IBillingRepository CreateRepository(CancellationToken token = default)
		{
			Func<SqlConnection> sqlConnectionFactory = () =>
			{
				var connectionStringBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["BillingContext"].ConnectionString)
				{
					Pooling = false
				};
				return new SqlConnection(connectionStringBuilder.ToString());
			};
			return new BillingRepository(sqlConnectionFactory, Logger, true, token);
		}
		public override ILog Logger { get; } = LogManager.GetLogger<BillingDatabaseProcessor>();
		public override string Name => nameof(BillingDatabaseProcessor);
		protected override bool ExitWhenClosed => true;
		public override string ItemName => "billing database";
		bool PerformMonthlyAggregation { get; }
		bool UpdateBillingCubePartial { get; }
		const string ProcessingBillingResourceName = nameof(BillingDatabaseProcessor) + ".ProcessingBillingDatabase";

		static ILog GetLogger(string name) => LogManager.GetLogger($"{typeof(BillingDatabaseProcessor).FullName}.{name}");
	}
}
