using System.Threading;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.ZA.ServiceTasks.ProcessReceiptService.Code,
	Enterprise.Customs.ZA.ServiceTasks.ProcessReceiptService.FriendlyName,
	Enterprise.Customs.ZA.ServiceTasks.MessagingService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.ZA.ServiceTasks.ProcessReceiptService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Seconds",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.ZA.ServiceTasks.ProcessReceiptService.Code,
	CusWHSOperatorTransactionSchema.Constants.TableName,
	new[] { CusWHSOperatorTransactionSchema.Constants.WOT_TransactionType + "=" + WarehouseOperatorTransactionTypeList.Codes.REC,
				 CusWHSOperatorTransactionSchema.Constants.WOT_Status + "=" + WarehouseOperatorTransactionStatusList.Codes.QUE },
	"ZA External Warehouse Receipt Processor"
	)]

namespace Enterprise.Customs.ZA.ServiceTasks
{
	public class ProcessReceiptService : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "ZRP";
		public const string FriendlyName = "ZA Receipt Processor";

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.SouthAfrica))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var logger = GetNewLogger();
					var processor = new ReceiptProcessor(branch.Factory, branch.Company, logger);
					logger.Log(LogType.Information, ZString.Format("Processing receipts for Company {0}", branch.Company.GC_Code));
					processor.ProcessReceipts();
				}
			}
		}

		protected LoggingInformation GetNewLogger()
		{
			LoggingInformation result = new LoggingInformation();
			result.OnLogInfoAdded += new LoggingInformation.LogInfoAdded(Logger_OnLogInfoAdded);
			return result;
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log.Trim());
		}
	}
}
