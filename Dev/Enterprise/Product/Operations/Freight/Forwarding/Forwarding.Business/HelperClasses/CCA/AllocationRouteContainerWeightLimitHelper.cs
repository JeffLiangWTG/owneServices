using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class AllocationRouteContainerWeightLimitHelper
	{
		bool ShouldCheckForApprovalEventCancellations { get; set; }

		readonly ForwardingConsol consol;

		bool IsContainerWeightLimitEnabled => ContractsPermissions.IsAllocationsVisible() &&
			FreightConfigurationRegistry.Instance.EnableContainerWeightLimitSupportOnAllocationRoutes.Value;

		public AllocationRouteContainerWeightLimitHelper(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			ShouldCheckForApprovalEventCancellations = false;
		}

		public void SetRequireApprovalEventCancellationCheck()
		{
			ShouldCheckForApprovalEventCancellations = true;
		}

		public bool CheckContainerWeightLimit(IRatingContractAllocationLine allocationRoute)
		{
			if (allocationRoute == null || allocationRoute.RCA_ContainerWeightLimit == 0 || !IsContainerWeightLimitEnabled)
			{
				return true;
			}

			if (IsOverrideEventAlreadyLogged(allocationRoute))
			{
				return true;
			}

			return IsContainerWeightLimitValid(allocationRoute);
		}

		public void CheckAndPromptOverrideOnContainerWeightLimit(ZGuid allocationRoute)
		{
			if (allocationRoute.IsEmpty || !IsContainerWeightLimitEnabled)
			{
				return;
			}

			var route = consol.Factory.Load<IRatingContractAllocationLine>(allocationRoute);

			if (route == null)
			{
				return;
			}

			if (IsOverrideEventAlreadyLogged(route) || IsContainerWeightLimitValid(route))
			{
				return;
			}

			if (TryApproveOverride(route))
			{
				LogOverrideEvent(route);
			}
		}

		public void TryCancelOldOverrideEvents()
		{
			if (!ShouldCheckForApprovalEventCancellations || !IsContainerWeightLimitEnabled)
			{
				return;
			}

			var logs = FindAllLogsOnConsol();
			var existingAllocations = new List<IRatingContractAllocationLine>();

			if (consol.AllocationLine != null)
			{
				existingAllocations = [consol.AllocationLine];
			}
			else
			{
				existingAllocations = consol.Containers.OfType<ForwardingContainer>()
					.Select(c => c.AllocationLine).Distinct().Except((IRatingContractAllocationLine)null).ToList();
			}

			if (existingAllocations.Count == 0)
			{
				logs.ForEach(log => log.Cancel());
			}
			else
			{
				foreach (var log in logs)
				{
					if (!existingAllocations.Any(allocation => DoesLogExactlyMatchAllocation(log, allocation)))
					{
						log.Cancel();
					}
				}
			}
		}

		bool IsOverrideEventAlreadyLogged(IRatingContractAllocationLine allocationRoute)
		{
			return FindAllLogsOnConsol().Any(log => DoesLogExactlyMatchAllocation(log, allocationRoute));
		}

		void LogOverrideEvent(IRatingContractAllocationLine allocationRoute)
		{
			KeyValuePair<string, string>[] eventPairs = [
				new(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Organization, allocationRoute.Contract.ServiceProvider.OH_Code),
				new(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ContractNumber, allocationRoute.Contract.RCT_ContractNumber),
				new(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, allocationRoute.RCA_AllocationLineID),
			];
			consol.Logs.AddNew(Events.ApprovedAllocationWithContainerWeightLimitExceeded, eventPairs);
		}

		bool TryApproveOverride(IRatingContractAllocationLine allocationRoute)
		{
			var dialogsProvider = consol.Factory.GetValue<IAllocationContainerWeightLimitDialogsProvider>();

			if (dialogsProvider == null)
			{
				return false;
			}

			var message = CCAValidationMessageProvider.GrossContainerWeightExceedsAllocationLimit(allocationRoute);

			if (Env.Security.ApproveAllocationExceedContainerWeightLimit.IsAllowed)
			{
				return dialogsProvider.ConfirmContainerWeightLimitOverride(
					message + System.Environment.NewLine + Res.GetString(
						"a2da1303-73aa-4b2b-a1fa-eba02842bf5d",
						"Do you wish to approve this allocation?"));
			}

			return dialogsProvider.ConfirmContainerWeightLimitOverride(message + System.Environment.NewLine + Res.GetString(
					"577eb81a-afaa-4a40-bc54-7a9ef63591cd",
					"A user with the relevant security access right is needed to approve this allocation.")
					+ System.Environment.NewLine + Res.GetString(
						"90ac9164-cdfc-4030-a6ff-826203c29259",
						"Do you wish to have this user enter their credentials?"
					)) &&
				dialogsProvider.AllowContainerWeightLimitOverrideBySecurityRole(Res.GetString(
					"962dc264-26c5-4433-a798-d18368abfea6",
					"To complete this operation, a user with higher security rights must login. Please enter username and password details below."));
		}

		static bool DoesLogExactlyMatchAllocation(IStmALog log, IRatingContractAllocationLine allocationRoute)
		{
			var logParameters = StmALog.GetParametersFromReference(log.SL_Reference);

			return logParameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes
					.ReferenceNumber) == allocationRoute.RCA_AllocationLineID &&
				logParameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes
					.ContractNumber) == allocationRoute.Contract.RCT_ContractNumber &&
				logParameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes
					.Organization) == allocationRoute.Contract.ServiceProvider.OH_Code;
		}

		StmALog[] FindAllLogsOnConsol()
		{
			var logQuery = new ZQuery(StmALogSchema.SL_IsCancelled, SQLComparisonOperator.Equal, false);
			logQuery.AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.ApprovedAllocationWithContainerWeightLimitExceededCode));
			var logs = consol.Logs.Find(logQuery);
			return logs;
		}

		static decimal ConvertContainerGrossWeightToLimitUnits(IRatingContractAllocationLine allocationRoute, ForwardingContainer container)
		{
			return Constants.Weight.Convert(container.JC_GrossWeight, container.JC_GrossWeightUQ, allocationRoute.RCA_ContainerWeightLimitUQ);
		}

		bool IsContainerWeightLimitValid(IRatingContractAllocationLine allocationRoute)
		{
			var limit = allocationRoute.RCA_ContainerWeightLimit;

			if (limit == 0)
			{
				return true;
			}

			var containers = consol.Containers.Where(c => c.JC_RCA_AllocationLine == allocationRoute.PK)
				.OfType<ForwardingContainer>().ToArray();

			if (containers.Length == 0)
			{
				return true;
			}

			if (allocationRoute.RCA_ContainerWeightLimitType == ContainerWeightLimitType.AbsolutePerTEU)
			{
				return containers.Max(c => ConvertContainerGrossWeightToLimitUnits(allocationRoute, c) / c.JC_Calc_TEUCount) <= limit;
			}

			if (allocationRoute.RCA_ContainerWeightLimitType == ContainerWeightLimitType.AveragePerTEU)
			{
				var containersByType = containers
					.GroupBy(c => c.JC_RC);

				foreach (var containerType in containersByType)
				{
					var totalGrossWeight = containerType.Sum(c => ConvertContainerGrossWeightToLimitUnits(allocationRoute, c));
					var totalTEUs = containerType.Sum(c => c.JC_Calc_TEUCount);
					if (totalTEUs == 0)
					{
						continue;
					}

					var avgPerTEU = totalGrossWeight / totalTEUs;

					return avgPerTEU <= limit;
				}
			}

			return true;
		}
	}
}
