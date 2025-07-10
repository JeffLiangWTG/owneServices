using System;
using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.ServiceTasks
{
	abstract class MessagingService : Customs.ServiceTasks.CustomsServiceTask
	{
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

		protected Guid[] ValidBranches
		{
			get
			{
				if (branchesWithBrokerageID == null)
				{
					GetNZCompanyPerBranchWithBrokerageIDs();
				}

				return branchesWithBrokerageID;
			}
		}
		Guid[] branchesWithBrokerageID;

		protected Guid[] BranchesWithoutBrokerageID
		{
			get
			{
				if (branchesWithoutBrokerageID == null)
				{
					GetNZCompanyPerBranchWithBrokerageIDs();
				}

				return branchesWithoutBrokerageID;
			}
		}
		Guid[] branchesWithoutBrokerageID;

		void GetNZCompanyPerBranchWithBrokerageIDs()
		{
			var validBranches = new List<Guid>();
			var invalidBranches = new List<Guid>();

			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Enterprise.Core.Constants.CountryCodes.NewZealand))
			{
				if ((NZCustomsDataRegistry.Instance.NZBrokerageID.GetValueWithoutFallback(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty).Length > 4))
				{
					validBranches.Add(branch.PK.ToGuid());
				}
				else
				{
					invalidBranches.Add(branch.PK.ToGuid());
				}
			}

			branchesWithBrokerageID = validBranches.ToArray();
			branchesWithoutBrokerageID = invalidBranches.ToArray();
		}
	}
}
