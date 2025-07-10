using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.ServiceTasks;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	QueueProcessingServiceTask.Code,
	QueueProcessingServiceTask.Description,
	"ACC",
	typeof(QueueProcessingServiceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "15minutes",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true)
]

[assembly: HostedServiceQueueProvider(
	QueueProcessingServiceTask.Code,
	QueueProcessingServiceTask.Description,
	typeof(QueueProcessingServiceTask.QueueProvider))
]

namespace Enterprise.MasterFiles.Business.ServiceTasks
{
	public class QueueProcessingServiceTask : ServiceProviderImpl
	{
		public const string Code = "AQP";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Accounting Queue Processor";

		public override void RunTask(CancellationToken token)
		{
			var queuesToProcess = new[]
			{
				new {  Description = (NoResString)"Processing Queue for Accounting Balances (recognized, unrecognized and balance)", StoredProcedureName = "AggregateAccOrgBalanceChangesIntoAccOrgBalance" }
			};

			foreach (var queueToProcess in queuesToProcess)
			{
				token.ThrowIfCancellationRequested();
				ServiceLogger.Log(LogType.Information, "Started: " + queueToProcess.Description);

				var stopwatch = new Stopwatch();
				try
				{
					stopwatch.Start();
					using (var transactionManager = Db.Connection.BeginTransactionWithManager())
#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
					using (var cmd = Db.Connection.Command(queueToProcess.StoredProcedureName))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
						cmd.ExecuteNonQuery();
						transactionManager.CommitTransaction();
					}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods

					stopwatch.Stop();
					var seconds = stopwatch.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture);

					ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Completed: {0} - {1} seconds", queueToProcess.Description, seconds));
				}
				catch (SqlException e)
				{
					if (e.Message.Contains((NoResString)"Arithmetic overflow error"))
					{
						ServiceLogger.Log(LogType.Error, "A Transaction with too high a balance was processed, please find and reverse any transactions with a transaction or outstanding balance close to 922,337,203,685,477");
					}
					else
					{
						throw;
					}
				}
			}
		}

		public class QueueProvider : IHostedServiceQueueProvider
		{
			QueueResult IHostedServiceQueueProvider.QueueResult
			{
				get
				{
					var result = QueueResult.Error;
					Db.Connection.ExecuteReader("SELECT COUNT(1), ISNULL(MAX(DATEDIFF(second, Y2_SystemLastEditTimeUtc, GETUTCDATE())), 0) FROM dbo.AccOrgBalanceChanges", reader =>
					{
						result = new QueueResult(reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1)));
					});
					return result;
				}
			}
		}
	}
}
