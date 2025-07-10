using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.ASQ,
	ServiceTaskApplicationCodeList.Descriptions.ASQ,
	TRMessageConstants.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.TR.ServiceTasks.SendTransactionIDQueryMessageService),
	MinimumPeriod = "1Minute",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Turkey,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute",
	ActiveByDefault = true
	)]

[assembly: HostedServiceQueueProvider(ServiceTaskApplicationCodeList.Codes.ASQ, "TR Auto-Send Query Message Queue", typeof(Enterprise.Customs.TR.ServiceTasks.SendTransactionIDQueryMessageServiceQueue))]
namespace Enterprise.Customs.TR.ServiceTasks
{
	public class SendTransactionIDQueryMessageService : MessagingService
	{
		[HostedServiceRequirement]
		public static string IsRequired() => GlbExternalPasswordHelper.CheckAnyStaffHasCertificate();

		protected override void RunTaskCore(CancellationToken token)
		{
			Logger.Log(LogType.Information, "TR Auto-Send Query Message Queue service task started.");

			try
			{
				foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Turkey))
				{
					token.ThrowIfCancellationRequested();
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						new QueryTransactionIDMessageSendHelper(Logger).SendQueryMessage(token);
					}
				}

				Logger.Log(LogType.Information, "TR Auto-Send Query Message Queue service task completed.");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger.Log(LogType.Error, FormattableString.Invariant($"ASQ service task ended abruptly.\r\nException: {ex.GetType()}\r\nException Message: {ex.Message}.\r\nStackTrace: {ex.StackTrace}"));
			}
		}
	}

	public class SendTransactionIDQueryMessageServiceQueue : IHostedServiceQueueProvider
	{
		QueueResult IHostedServiceQueueProvider.QueueResult
		{
			get
			{
				var result = QueueResult.Error;
				Db.Connection.ExecuteReader(QueueSizeQuery, reader => result = new QueueResult(reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1))));
				return result;
			}
		}

		const string QueueSizeQuery = @"SELECT COUNT(*), ISNULL(MAX(DATEDIFF(second, CPT_EarliestTimeOfNextAttemptUtc, GETUTCDATE())), 0) FROM dbo.CusPollingTransaction
			WHERE CPT_Status = 'OPN' AND CPT_ApplicationCode = 'TRC' AND CPT_Type IN ('TRO', 'TRE', 'TRN', 'T1N', 'DTE') AND CPT_EarliestTimeOfNextAttemptUtc <= GETUTCDATE()";
	}
}
