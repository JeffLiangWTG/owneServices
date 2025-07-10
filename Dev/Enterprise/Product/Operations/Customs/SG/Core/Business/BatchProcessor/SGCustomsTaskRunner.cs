using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGCustomsTaskRunner
	{
		public const string SGCustomsServiceCode = "SGC";
		public const string SGCustomsServiceName = "Singapore Customs TradeXchange/TradeNet";

		public SGCustomsTaskRunner(ILogger serviceLogger)
		{
			if (serviceLogger == null)
			{
				throw new ArgumentNullException(nameof(serviceLogger));
			}

			this.serviceLogger = serviceLogger;
		}

		public void Run(CancellationToken token)
		{
			ResetToTestSystemIfNeeded();
			RunTasksForEachCompany(token);
		}

		public static void ResetLastSentEmailWithBrokerErrorsRegistryItem()
		{
			SGCustomsDataRegistry.Instance.LastSentEmailWithBrokerErrors.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, DateTime.MinValue);
		}

		void RunTasksForEachCompany(CancellationToken token)
		{
			var useDirectWebServices = SGCustomsDataRegistry.Instance.UseMhxDirectWebServicesInsteadOfScripting.Value;
			foreach (var branchPK in GetSingleBranchPerRelevantSgCompanies())
			{
				using (DisposableEnvironment.ForBranch(branchPK))
				{
					if (useDirectWebServices)
					{
						// Soap
						GetNewBatchProcessHookedUpToLogger<BatchSGCInterchangeRetriever40WebServices>().ExecuteBatch(token);
						GetNewBatchProcessHookedUpToLogger<BatchSGCMessageProcessor>().ExecuteBatch(token);
						GetNewBatchProcessHookedUpToLogger<BatchSGCInterchangeSender40WebServices>().ExecuteBatch(token);
					}
					else
					{
						// Scripting
						GetNewBatchProcessHookedUpToLogger<BatchSGCInterchangeRetriever40Script>().ExecuteBatch(token);
						GetNewBatchProcessHookedUpToLogger<BatchSGCMessageProcessor>().ExecuteBatch(token);
						GetNewBatchProcessHookedUpToLogger<BatchSGCInterchangeSender40Script>().ExecuteBatch(token);
					}
				}
			}
		}

		Guid[] GetSingleBranchPerRelevantSgCompanies()
		{
			var factory = new BusinessObjectFactory();

			var result = new List<Guid>();

			foreach (GlbCompany company in GlbCompany.GetActiveCompanies(Enterprise.Core.Constants.CountryCodes.Singapore, factory))
			{
				var query1 = new ZQuery(GlbExternalPasswordSchema.GP_GC, company.PK);
				query1.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.SG4);
				query1.AddToFilter(GlbExternalPasswordSchema.GP_UserID, SQLComparisonOperator.NotEqual, "");
				query1.AddToFilter(GlbExternalPasswordSchema.GP_CurrentPassword, SQLComparisonOperator.NotEqual, "");
				query1.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, Core.Constants.PasswordOK);

				if (factory.LoadTop1<GlbExternalPassword>(query1) != null)
				{
					result.Add(company.FirstActiveBranch.PK.ToGuid());
				}
			}

			return result.ToArray();
		}

		T GetNewBatchProcessHookedUpToLogger<T>() where T : BatchProcess, new()
		{
			var result = new T() { Logger = new LoggingInformation() };

			result.Logger.OnLogInfoAdded +=
				new LoggingInformation.LogInfoAdded((log, logType) =>
				{
					serviceLogger.Log(logType, log);
				});

			return result;
		}

		readonly ILogger serviceLogger;

		void ResetToTestSystemIfNeeded()
		{
			var isSendingToProduction = !SGCustomsDataRegistry.Instance.SendTestMessages.Value;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			if (isSendingToProduction && registrationKey.DatabaseType != DatabaseTypes.Codes.Production)
			{
				SGCustomsDataRegistry.Instance.SendTestMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}
	}
}
