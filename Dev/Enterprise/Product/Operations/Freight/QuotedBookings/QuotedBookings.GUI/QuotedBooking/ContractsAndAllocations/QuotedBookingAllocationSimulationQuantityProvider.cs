using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	sealed class QuotedBookingAllocationSimulationQuantityProvider : IRatingContractSimulationQuantityProvider
	{
		public QuotedBookingAllocationSimulationQuantityProvider(QuotedBooking quotedBooking)
		{
			this.quotedBooking = quotedBooking;
		}

		readonly QuotedBooking quotedBooking;

		public ZDecimal GetTotalCNQuantityForAllocation()
		{
			return quotedBooking.QuotedBookingContainers
				.OfType<ForwardingContainer>()
				.Sum(container => container.JC_ContainerCount);
		}

		public ZDecimal GetTotalCNQuantityNotAllocatedToContract(IRatingContract ratingContract)
		{
			var containers = quotedBooking.QuotedBookingContainers.OfType<ForwardingContainer>().ToArray();
			var allocationRoutesToContractMapping = quotedBooking.Factory.GetCachedValue($"AllocationRoutesToContractMapping_{quotedBooking.PK}", () =>
			{
				return GetAllocationRoutesToContractMappingOfContainers(containers, quotedBooking.Factory);
			});

			return containers
				.Where(container => !AllocationLineBelongsToContract(container.JC_RCA_AllocationLine, ratingContract.PK, allocationRoutesToContractMapping))
				.Sum(container => container.JC_ContainerCount);
		}

		public ZDecimal GetTotalTEUQuantityForAllocation()
		{
			return quotedBooking.QuotedBookingContainers
				.OfType<ForwardingContainer>()
				.Sum(container => container.JC_ContainerCount * (container.Container)?.RC_TEU ?? 0);
		}

		public ZDecimal GetTotalTEUQuantityNotAllocatedToContract(IRatingContract ratingContract)
		{
			var containers = quotedBooking.QuotedBookingContainers.OfType<ForwardingContainer>().ToArray();
			var allocationRoutesToContractMapping = quotedBooking.Factory.GetCachedValue($"AllocationRoutesToContractMapping_{quotedBooking.PK}", () =>
			{
				return GetAllocationRoutesToContractMappingOfContainers(containers, quotedBooking.Factory);
			});
			return containers
				.Where(container => !AllocationLineBelongsToContract(container.JC_RCA_AllocationLine, ratingContract.PK, allocationRoutesToContractMapping))
				.Sum(container => container.JC_ContainerCount * (container.Container)?.RC_TEU ?? 0);
		}

		Dictionary<ZGuid, (ZGuid allocationLinePK, ZGuid contractPK)> GetAllocationRoutesToContractMappingOfContainers(IEnumerable<IForwardingContainer> containers, BusinessObjectFactory factory)
		{
			return factory.Load<IRatingContractAllocationLine>(new ZQuery(RatingContractAllocationLineSchema.PK, containers.Select(container => container.JC_RCA_AllocationLine).ToArray()))
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
