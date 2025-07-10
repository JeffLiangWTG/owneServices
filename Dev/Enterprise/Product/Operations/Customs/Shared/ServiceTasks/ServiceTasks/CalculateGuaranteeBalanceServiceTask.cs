using System;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CalculateGuaranteeBalanceServiceTask.Code,
	CalculateGuaranteeBalanceServiceTask.FriendlyName,
	CalculateGuaranteeBalanceServiceTask.Category,
	typeof(CalculateGuaranteeBalanceServiceTask),
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	IsMandatory = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(CalculateGuaranteeBalanceServiceTask.Code,
	CusPermitLineTransactionSchema.Constants.TableName,
	new[] { CusPermitLineTransactionSchema.Constants.CPL_IsAggregated + "=" + CalculateGuaranteeBalanceServiceTask.IsNotAggregated,
		CusPermitLineTransactionSchema.Constants.CPL_TransactionStatus + "=" + CalculateGuaranteeBalanceServiceTask.TransactionStatus },
	"Guarantee Balance Calculation Permit Line")]

namespace Enterprise.Customs.ServiceTasks
{
	public class CalculateGuaranteeBalanceServiceTask : ServiceProviderImpl
	{
		internal const string Code = "BGC";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "service task")]
		public const string FriendlyName = "Guarantee Balance Calculation";
		internal const string IsNotAggregated = "N";
		internal const string TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
		internal const string ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		internal const string Category = "SYS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error messages for logging and reporting should be in English")]
		public override void RunTask(CancellationToken token)
		{
			var batchSize = GetBatchSize();

			var sqlText = $@"begin try
		declare @PermitHeadersWithWaitingTransactions table (cph uniqueidentifier )
		declare @ProcessedTransactions table (cpl uniqueidentifier, cph uniqueIdentifier, balanceAdjustment decimal(19, 5) )
	
		insert into @PermitHeadersWithWaitingTransactions
			Select top {batchSize} CPH_PK from dbo.CusPermitHeader WITH (UPDLOCK, READPAST)
				inner join (select CPL_CPH_PermitHeader, sum(CPL_TranValue) TotalTranValue from dbo.CusPermitLineTransaction
								where CPL_IsAggregated = 0 and (CPL_TransactionStatus = 'CON' or CPL_TransactionType = 'OBA') and CPL_TransactionCategory = 'CUM'
								group by CPL_CPH_PermitHeader) transactions on CPH_PK = transactions.CPL_CPH_PermitHeader
				where CPH_ApplicationCode = 'GUA' and CPH_Balance + TotalTranValue >= 0

		if exists (select 1 from @PermitHeadersWithWaitingTransactions) 
		begin			
			insert into @ProcessedTransactions
				select CPL_PK, CPL_CPH_PermitHeader, CPL_TranValue from dbo.CusPermitLineTransaction 
					inner join	@PermitHeadersWithWaitingTransactions WH
					on WH.cph = CPL_CPH_PermitHeader
					where CPL_IsAggregated = 0 and (CPL_TransactionStatus = 'CON' or CPL_TransactionType = 'OBA') and CPL_TransactionCategory = 'CUM'

			update dbo.CusPermitHeader
				set
					CPH_Balance = CPH_Balance + PTForHeader.aggregatedAdjustment,
					CPH_SystemLastEditTimeUtc = GETUTCDATE(),
					CPH_SystemLastEditUser = '{GlbStaff.CurrentUser?.GS_Code.ToString() ?? "~BP"}'
				from 
				(
					select cph, sum(balanceAdjustment) as aggregatedAdjustment
					from @ProcessedTransactions
					group by cph
				) PTForHeader
				where CusPermitHeader.CPH_PK = PTForHeader.cph

			update dbo.CusPermitLineTransaction
				set
					CPL_IsAggregated = 1,
					CPL_SystemLastEditTimeUtc = GETUTCDATE(),
					CPL_SystemLastEditUser = '{GlbStaff.CurrentUser?.GS_Code.ToString() ?? "~BP"}'
				from 
				(
					select cpl
					from @ProcessedTransactions
				) PTForLineTransaction
				where CusPermitLineTransaction.CPL_PK = PTForLineTransaction.cpl
		end

		select count(cph) from @PermitHeadersWithWaitingTransactions 
	end try
	begin catch
		throw
	end catch
";
			var recordsAffected = 1;
			while (recordsAffected > 0)
			{
				using (var manager = GetTransactionManager())
				{
					try
					{
						recordsAffected = (int)Db.Connection.ExecuteScalar(sqlText);
						ServiceLogger.Log(LogType.Information, $@"updated {recordsAffected} permits.");
						manager.CommitTransaction();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						if (ex is SqlException sqlException &&
							!(ZExceptionExtensions.IsInfrastructureDbError(sqlException) || sqlException.IsTimeoutExpired() || sqlException.IsLockTimeoutExpired()))
						{
							ErrorReporter.ReportOnce(ex.Message);
						}

						var errorMessage = string.Format(CultureInfo.InvariantCulture,
							"Environmental error occurred while run Service Task:[{0}]. Error message: {1}", Code, ex.Message);
						ServiceLogger.Log(LogType.Warning, errorMessage);

						manager.RollbackTransaction();
						break;
					}
				}

				token.ThrowIfCancellationRequested();
			}
		}

		int GetBatchSize()
		{
			return DataRegistry.Business.CustomsDataRegistry.Instance.NoOfGuaranteesBGCServiceProcessPerBatch.Value;
		}

		ITransactionManager GetTransactionManager()
		{
			return Db.Connection.BeginTransactionWithManager();
		}
	}
}
