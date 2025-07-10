using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface ICommonBillOfLading : IBaseBillOfLading
	{
		ZString BillActionCode { get; }
		ZString AmendmentCode { get; }
		ZString IssuerCode { get; }
		ZString BillOfLadingSequenceNumber { get; }
		ZString ForeignPort { get; }
		ZDecimal ManifestQuantity { get; }
		ZString ManifestUnits { get; }
		ZDecimal Weight { get; }
		ZString WeightUnit { get; }
		ZString BillOfLadingStatusIndicator { get; }
		ZBool IsMasterInbond { get; }
		ZString HouseBillNumber { get; }
		ZString FIRMS { get; }
		ZDecimal Volume { get; }
		ZString VolumeUnit { get; }
		ZString PlaceOfReceiptByCarrier { get; }
		ZString SpaceCharterBLReference { get; }
		ZString SecondNotifyParty1 { get; }
		ZString SecondNotifyParty2 { get; }
		ZString LastForeignPortBeforeDepartingForTheUS { get; }
		ZString ModeOfTransportationFromThePlacePriorToLoading { get; }
		ZString MethodOfPaymentForTransportation { get; }
		ZString ContractualPossessionForeignPort { get; }

		IEnumerable<IShipmentReferenceDetail> ShipmentReferenceDetails(ActionCode actionCode);
	}

	public interface IACEBillOfLading : ICommonBillOfLading
	{
		IEnumerable<IEntity> Entities(ActionCode actionCode);
		IEnumerable<IACEContainer> Containers { get; }

		IMovemenDetails MovemenDetails { get; }
	}
}
