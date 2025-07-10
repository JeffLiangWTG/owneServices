using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;

[assembly: HostedService(
	code: ServiceTaskCodeList.Codes.CusPollingTransactionProcessor,
	description: ServiceTaskCodeList.Descriptions.CusPollingTransactionProcessor,
	category: ApplicationCode.PLCustoms,
	typeof(CusPollingTransactionProcessingService),
	AllowsMultipleInstances = false,
	RequiresCompanyInCountry = CountryCodes.Poland,
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute",
	ActiveByDefault = true)]

namespace Enterprise.Customs.PL.ServiceTasks;

public class CusPollingTransactionProcessingService : CustomsServiceTask
{
	protected override void RunTaskCore(CancellationToken cancellationToken)
	{
		var totalMessagesSent = 0;
		var cusPollingTransactionProcessor = new CusPollingTransactionProcessor(Logger, cancellationToken);
		foreach (var branch in GlbCompany.GetActiveCompanies(CountryCodes.Poland).SelectMany(x => x.Branches))
		{
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				BatchProcess();
			}
		}

		LogResult();
		return;

		void BatchProcess()
		{
			var factory = new BusinessObjectFactory { NameForDebugging = ($"{typeof(CusPollingTransactionProcessingService).FullName}.{nameof(BatchProcess)}") };
			cancellationToken.ThrowIfCancellationRequested();
			var processingResult = cusPollingTransactionProcessor.ProcessForBranch(factory);
			if (processingResult.HasChanges && TrySaveChanges(factory))
			{
				totalMessagesSent += processingResult.MessagesSent;
			}
		}

		bool TrySaveChanges(BusinessObjectFactory factory)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				factory.Save();
				return true;
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				return false;
			}
		}

		void LogResult()
		{
			if (totalMessagesSent > 0)
			{
				ServiceLogger.Log(LogType.Information, FormattableString.Invariant($"PL {totalMessagesSent} message(s) has been created successfully."));
			}
		}
	}

	[HostedServiceRequirement]
	public static string IsRequired() => PLServiceTaskHelper.CheckCertificate();
}
