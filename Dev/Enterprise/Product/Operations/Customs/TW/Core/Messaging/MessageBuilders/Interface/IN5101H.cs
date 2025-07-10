using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IN5101HDeclaration
	{
		ZString FunctionalReferenceID { get; }

		ZString FunctionCode { get; }

		ZString StatusCode { get; }

		ITransportMeans BorderTransportMeans { get; }

		ZString CarrierId { get; }

		IEnumerable<IN5101HConsignment> Consignments { get; }

		ZString DeconsolidatorId { get; }

		IN5101HGoodsShipment GoodsShipment { get; }

		ZDate UnloadingLocationArrivalDateTime { get; }
	}

	public interface IN5101HConsignment
	{
		ZDecimal BoardedQuantity { get; }

		ZDecimal TotalPackageQuantity { get; }

		ZString EscortMark { get; }

		ZDecimal TotalGrossMassMeasure { get; }

		ZString TypeCode { get; }

		ZString AssociatedTransportDocumentId { get; }

		IPartyDetails Consignee { get; }

		IN5101HConsignmentItem ConsignmentItem { get; }

		IPartyDetails Consignor { get; }

		ZString GoodsLocationId { get; }

		ILocation LoadingLocation { get; }

		IEnumerable<IPartyDetails> NotifyParties { get; }

		ITransportContractDocument TransportContractDocument { get; }

		IEnumerable<ITransportEquipment> TransportEquipments { get; }

		ZString UnloadingLocationId { get; }
	}

	public interface IN5101HConsignmentItem
	{
		ZString Split { get; }

		ZDecimal TotalPackageQuantity { get; }

		IEnumerable<IAdditionalInformation> AdditionalInformations { get; }

		IN5101HCommodity Commodity { get; }

		IN5101HGoodsMeasure GoodsMeasure { get; }

		IPackaging Packaging { get; }

		ZString UCRId { get; }
	}

	public interface IN5101HCommodity
	{
		ZString CargoDescription { get; }

		IEnumerable<IClassification> Classifications { get; }
	}

	public interface IN5101HGoodsMeasure
	{
		ZDecimal GrossVolumeMeasure { get; }

		ZString VolumeUnitCode { get; }
	}

	public interface IN5101HGoodsShipment
	{
		IN5101HConsignment Consignment { get; }

		ZString EntryOfficeId { get; }
	}
}
