using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class ConsolAllocationSimulationQuantityProvider : IRatingContractSimulationQuantityProvider
	{
		public ConsolAllocationSimulationQuantityProvider(ForwardingConsol consol)
		{
			Argument.NotNull(consol, nameof(consol));
			this.consol = consol;
		}

		readonly ForwardingConsol consol;

		public ZDecimal GetTotalCNQuantityForAllocation()
		{
			return consol.Containers
				.OfType<ForwardingContainer>()
				.Sum(container => container.JC_ContainerCount);
		}

		public ZDecimal GetTotalCNQuantityNotAllocatedToContract(IRatingContract ratingContract)
		{
			var containers = consol.Containers.OfType<ForwardingContainer>().ToArray();

			var allocationRoutesToContractMapping = consol.Factory.GetCachedValue($"AllocationRoutesToContractMapping_{consol.PK}", () =>
			{
				return GetAllocationRoutesToContractMappingOfContainers(containers, consol.Factory);
			});

			return containers
				.Where(container => !AllocationLineBelongsToContract(container.JC_RCA_AllocationLine, ratingContract.PK, allocationRoutesToContractMapping))
				.Sum(container => container.JC_ContainerCount);
		}

		public ZDecimal GetTotalTEUQuantityForAllocation()
		{
			return consol.Containers
				.OfType<ForwardingContainer>()
				.Sum(container => container.JC_ContainerCount * (container.Container)?.RC_TEU ?? 0);
		}

		public ZDecimal GetTotalTEUQuantityNotAllocatedToContract(IRatingContract ratingContract)
		{
			var containers = consol.Containers.OfType<ForwardingContainer>().ToArray();
			var allocationRoutesToContractMapping = consol.Factory.GetCachedValue($"AllocationRoutesToContractMapping_{consol.PK}", () =>
			{
				return GetAllocationRoutesToContractMappingOfContainers(containers, consol.Factory);
			});

			return containers
				.Where(container => !AllocationLineBelongsToContract(container.JC_RCA_AllocationLine, ratingContract.PK, allocationRoutesToContractMapping))
				.Sum(container => container.JC_ContainerCount * (container.Container)?.RC_TEU ?? 0);
		}

		Dictionary<ZGuid, (ZGuid allocationLinePK, ZGuid contractPK)> GetAllocationRoutesToContractMappingOfContainers(IEnumerable<IForwardingContainer> containers, BusinessObjectFactory factory)
		{
			return factory.Load<RatingContractAllocationLine>(new ZQuery(RatingContractAllocationLineSchema.PK, containers.Select(container => container.JC_RCA_AllocationLine).ToArray()))
				.Distinct()
				.Select(allocationLine => (allocationLine.PK, allocationLine.RCA_RCT_RatingContract))
				.ToDictionary(allocationLineTuple => allocationLineTuple.PK);
		}

		bool AllocationLineBelongsToContract(ZGuid allocationLinePK, ZGuid contractPK, Dictionary<ZGuid, (ZGuid allocationLinePK, ZGuid contractPK)> allocationLineToContractMapping)
		{
			if (allocationLineToContractMapping.TryGetValue(allocationLinePK, out var allocationLineToContractPair))
			{
				return contractPK == allocationLineToContractPair.contractPK;
			}

			return false;
		}
	}
}
