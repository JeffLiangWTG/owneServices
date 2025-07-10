using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	public class AllocationConsumptionLogger
	{
		public AllocationConsumptionLogger(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			ConsumptionCaptureRequired = true;
		}

		readonly ForwardingConsol consol;

		bool ConsumptionCaptureRequired { get; set; }

		Dictionary<RatingContractAllocationLine, decimal> PersistedAllocationRouteConsumptionMap => persistedAllocationRouteConsumptionMap
			??= new Dictionary<RatingContractAllocationLine, decimal>();

		Dictionary<RatingContractAllocationLine, decimal> persistedAllocationRouteConsumptionMap;

		bool AllocationConsumptionLoggingIsEnabled()
		{
			return FreightConfigurationRegistry.Instance.EnableAllocatedEventGenerationOnConsolidations.Value
				&& ContractsPermissions.IsAllocationsVisible();
		}

		public void TryCaptureInitialPersistedConsumption()
		{
			if (!AllocationConsumptionLoggingIsEnabled())
			{
				return;
			}

			if (ConsumptionCaptureRequired)
			{
				CapturePersistedConsumption();
				ConsumptionCaptureRequired = false;
			}

			ShouldTryLogAllocationConsumptionChanges = true;
		}

		public void TryLogAllocationConsumptionChanges()
		{
			if (!AllocationConsumptionLoggingIsEnabled())
			{
				return;
			}

			if (!ShouldTryLogAllocationConsumptionChanges)
			{
				return;
			}

			var finalAllocationConsumptionMap = new Dictionary<RatingContractAllocationLine, decimal>();

			foreach (var container in consol.Containers.OfType<ForwardingContainer>())
			{
				var allocationRoute = consol.Factory.Load<RatingContractAllocationLine>(container.JC_RCA_AllocationLine);
				if (allocationRoute == null)
				{
					continue;
				}

				if (finalAllocationConsumptionMap.TryGetValue(allocationRoute, out var consumption))
				{
					finalAllocationConsumptionMap[allocationRoute] = consumption + GetFinalConsumptionOfContainer(container, allocationRoute);
				}
				else
				{
					finalAllocationConsumptionMap[allocationRoute] = GetFinalConsumptionOfContainer(container, allocationRoute);
				}
			}

			var initialAllocationRoutes = PersistedAllocationRouteConsumptionMap.Keys.ToList();
			var finalAllocationRoutes = finalAllocationConsumptionMap.Keys.ToList();

			var removedAllocationRoutes = initialAllocationRoutes.Except(finalAllocationRoutes).ToList();
			var addedAllocationRoutes = finalAllocationRoutes.Except(initialAllocationRoutes).ToList();
			var amendedAllocationRoutes = initialAllocationRoutes.Intersect(finalAllocationRoutes).ToList();

			foreach (var allocationRoute in removedAllocationRoutes)
			{
				consol.Logs.AddNew(Events.Allocated, GetEventReferenceParameters(allocationRoute, 0, GetAllocationRouteUnitForLog(allocationRoute), AllocationStatusParameterCodes.Deleted));
			}

			foreach (var allocationRoute in addedAllocationRoutes)
			{
				var allocatedQuantity = finalAllocationConsumptionMap[allocationRoute];
				consol.Logs.AddNew(Events.Allocated, GetEventReferenceParameters(allocationRoute, allocatedQuantity, GetAllocationRouteUnitForLog(allocationRoute), AllocationStatusParameterCodes.New));
			}

			foreach (var allocationRoute in amendedAllocationRoutes)
			{
				var initialQuantity = PersistedAllocationRouteConsumptionMap[allocationRoute];
				var allocatedQuantity = finalAllocationConsumptionMap[allocationRoute];

				if (initialQuantity != allocatedQuantity)
				{
					consol.Logs.AddNew(Events.Allocated, GetEventReferenceParameters(allocationRoute, allocatedQuantity, GetAllocationRouteUnitForLog(allocationRoute), AllocationStatusParameterCodes.Amended));
				}
			}

			MarkAllocationConsumptionCaptured();
		}

		void CapturePersistedConsumption()
		{
			var containersInDatabase = consol.Containers.OfType<ForwardingContainer>().Where(container => container.IsInDatabase);
			foreach (var container in containersInDatabase)
			{
				var allocationRoutePK = (ZGuid)container.JC_RCA_AllocationLineInfo.OriginalValue;
				if (container.Factory.Load<RatingContractAllocationLine>(allocationRoutePK) is RatingContractAllocationLine allocationRoute)
				{
					if (PersistedAllocationRouteConsumptionMap.TryGetValue(allocationRoute, out var count))
					{
						PersistedAllocationRouteConsumptionMap[allocationRoute] = count + GetPersistedConsumptionOfContainer(container, allocationRoute);
					}
					else
					{
						PersistedAllocationRouteConsumptionMap[allocationRoute] = GetPersistedConsumptionOfContainer(container, allocationRoute);
					}
				}
			}
		}

		void MarkAllocationConsumptionCaptured()
		{
			ConsumptionCaptureRequired = true;
			PersistedAllocationRouteConsumptionMap.Clear();
		}

		decimal GetPersistedConsumptionOfContainer(ForwardingContainer container, RatingContractAllocationLine allocationRoute)
		{
			var originalContainerCount = (ZShort)container.JC_ContainerCountInfo.OriginalValue;

			decimal GetTEUCount()
			{
				var originalRefContainerPK = (ZGuid)container.JC_RCInfo.OriginalValue;
				var refContainer = container.Factory.Load<RefContainer>(originalRefContainerPK);
				return (refContainer?.RC_TEU ?? 0) * originalContainerCount;
			}

			return allocationRoute.RCA_AllocatedUQ.ToString() switch
			{
				Core.Constants.AllocationQuantityUnits.TwentyFootUnits => GetTEUCount(),
				Core.Constants.AllocationQuantityUnits.Containers => originalContainerCount,
				_ => 0
			};
		}

		decimal GetFinalConsumptionOfContainer(ForwardingContainer container, RatingContractAllocationLine allocationRoute)
		{
			return allocationRoute.RCA_AllocatedUQ.ToString() switch
			{
				Core.Constants.AllocationQuantityUnits.TwentyFootUnits => (container.RefContainer?.RC_TEU ?? 0) * container.JC_ContainerCount,
				Core.Constants.AllocationQuantityUnits.Containers => container.JC_ContainerCount,
				_ => 0
			};
		}

		bool ShouldTryLogAllocationConsumptionChanges { get; set; }

		KeyValuePair<string, string>[] GetEventReferenceParameters(RatingContractAllocationLine allocationRoute, decimal allocatedQuantity, string unit, string changeCode)
		{
			return new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Status, changeCode),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Quantity, allocatedQuantity.ToString()),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, unit),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.ReferenceNumber, allocationRoute.RCA_AllocationLineID),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.ContractNumber, allocationRoute.Contract.RCT_ContractNumber),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Organization, allocationRoute.Contract.ServiceProvider.OH_Code)
			};
		}

		string GetAllocationRouteUnitForLog(RatingContractAllocationLine allocationRoute)
		{
			return allocationRoute.RCA_AllocatedUQ.ToString() switch
			{
				Core.Constants.AllocationQuantityUnits.TwentyFootUnits => "TEU",
				Core.Constants.AllocationQuantityUnits.Containers => "CNT",
				_ => ""
			};
		}
	}

	static class AllocationStatusParameterCodes
	{
		public const string New = "NEW";
		public const string Amended = "AMD";
		public const string Deleted = "DEL";
	}
}
