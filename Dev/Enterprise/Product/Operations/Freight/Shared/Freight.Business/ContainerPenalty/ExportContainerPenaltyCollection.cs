using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class ExportContainerPenaltyCollection : ContainerPenaltyCollection
	{
		public ExportContainerPenaltyCollection(CommonContainer container, string processType) : base(container, processType)
		{
		}

		public override RefUNLOCO PenaltyPort
		{
			get { return container.ContainerParent?.LoadPort; }
		}

		public ContainerPenalty FindOrCreateDepartureCarrierStoragePenalty(bool createIfNotExists, bool notCreateIfNotMatch = true, CommonShipment shipment = null)
		{
			var found = FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, shipment);
			if (found != null)
			{
				return found;
			}

			if (createIfNotExists)
			{
				if (notCreateIfNotMatch && !IsMatchStorage(ContainerDetentionDirection.Export, ContainerPenaltyCreditorType.Codes.Carrier, shipment)
					|| FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, shipment) != null)
				{
					return null;
				}

				return FindOrCreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, timeUnit: ContainerPenaltyTimeUnit.Codes.Days, shipment: shipment);
			}

			return null;
		}

		public ContainerPenalty FindOrCreateDepartureCTOStoragePenalty(bool createIfNotExists, bool notCreateIfNotMatch = true, CommonShipment shipment = null)
		{
			var found = FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, shipment);
			if (found != null)
			{
				return found;
			}

			if (createIfNotExists)
			{
				if (notCreateIfNotMatch && !IsMatchStorage(ContainerDetentionDirection.Export, ContainerPenaltyCreditorType.Codes.CTO, shipment))
				{
					return null;
				}

				return FindOrCreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, timeUnit: ContainerPenaltyTimeUnit.Codes.Days, shipment: shipment);
			}

			return null;
		}

		public ContainerPenalty FindOrCreateDepartureCarrierDetentionPenalty(bool createIfNotExists, bool notCreateIfNotMatch = true, bool returnNullIfShouldNotDefault = false, CommonShipment shipment = null) => FindOrCreateCarrierDetentionPenalty(createIfNotExists, notCreateIfNotMatch, returnNullIfShouldNotDefault, shipment);

		public ContainerPenalty FindOrCreateDepartureMDDPenalty(bool createIfNotExists, bool notCreateIfNotMatch = true, CommonShipment shipment = null) => FindOrCreateMDDPenalty(createIfNotExists, notCreateIfNotMatch, shipment);
	}
}
