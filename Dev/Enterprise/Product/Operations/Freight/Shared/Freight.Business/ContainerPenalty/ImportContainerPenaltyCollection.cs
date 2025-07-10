using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class ImportContainerPenaltyCollection : ContainerPenaltyCollection
	{
		public ImportContainerPenaltyCollection(CommonContainer container, string processType) : base(container, processType)
		{
		}

		public override RefUNLOCO PenaltyPort
		{
			get { return container.ContainerParent?.DischargePort; }
		}

		public ContainerPenalty FindOrCreateArrivalCTOStoragePenalty(bool createIfNotExists, bool notCreateIfNotMatch = true, CommonShipment shipment = null)
		{
			var found = FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, shipment);
			if (found != null)
			{
				return found;
			}

			if (createIfNotExists)
			{
				if (notCreateIfNotMatch && !IsMatchStorage(ContainerDetentionDirection.Import, ContainerPenaltyCreditorType.Codes.CTO, shipment))
				{
					return null;
				}

				return FindOrCreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, timeUnit: ContainerPenaltyTimeUnit.Codes.Days, shipment: shipment);
			}

			return null;
		}

		public ContainerPenalty FindOrCreateArrivalCarrierStoragePenalty(bool createIfNotExists, bool notCreateIfNotMatch = true, CommonShipment shipment = null)
		{
			var found = FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, shipment);
			if (found != null)
			{
				return found;
			}

			if (createIfNotExists)
			{
				if (notCreateIfNotMatch && !IsMatchStorage(ContainerDetentionDirection.Import, ContainerPenaltyCreditorType.Codes.Carrier, shipment))
				{
					return null;
				}

				if (FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, shipment) != null)
				{
					return null;
				}

				return FindOrCreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, timeUnit: ContainerPenaltyTimeUnit.Codes.Days, shipment: shipment);
			}

			return null;
		}

		public ContainerPenalty FindOrCreateArrivalCarrierDetentionPenalty(bool createIfNotExists, bool notCreateIfNotMatch = true, bool returnNullIfShouldNotDefault = false, CommonShipment shipment = null) => FindOrCreateCarrierDetentionPenalty(createIfNotExists, notCreateIfNotMatch, returnNullIfShouldNotDefault, shipment);

		public ContainerPenalty FindOrCreateArrivalMDDPenalty(bool createIfNotExists, bool notCreateIfNotMatch = true, CommonShipment shipment = null) => FindOrCreateMDDPenalty(createIfNotExists, notCreateIfNotMatch, shipment);
	}
}
