using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Tracking.Business
{
	public interface IShipmentDeclaration : ICancellable
	{
		ShipmentDeclarationSchema ShipmentDeclarationSchema { get; }

		ZGuid PersistentBizOPK { get; }

		ZString Number { get; }
		ZString HouseBill { get; }
		ZString MasterBill { get; }

		ZGuid ConsignorPK { get; }
		ZString ConsignorName { get; }
		ZString ConsignorFullAddress { get; }
		ZString ConsignorAddress { get; }
		ZString ConsignorCity { get; }
		ZString ConsignorState { get; }
		ZString ConsignorPostCode { get; }

		ZGuid ConsigneePK { get; }
		ZString ConsigneeName { get; }
		ZString ConsigneeFullAddress { get; }
		ZString ConsigneeAddress { get; }
		ZString ConsigneeCity { get; }
		ZString ConsigneeState { get; }
		ZString ConsigneePostCode { get; }

		ZString OriginPortCode { get; }
		ZString DestinationPortCode { get; }

		ZString CurrentLoadPort { get; }
		ZString CurrentDischargePort { get; }
		ZString MainLoadPort { get; }
		ZString MainDischargePort { get; }

		ZDateTime ETA { get; }
		ZDateTime ETDWithSuppression { get; }
		ZDateTime ETAWithSuppression { get; }

		ZString MainVessel { get; }
		ZString MainVoyageWithSuppression { get; }
		ZString CurrentVessel { get; }
		ZString CurrentVoyageWithSuppression { get; }

		ZString BookingReference { get; }
		ZString OwnerReference { get; }
		ZString TransportMode { get; }

		ZString PacksWithUnits { get; }
		ZString VolumeWithUnits { get; }
		ZString WeightWithUnits { get; }

		ZDecimal GoodsValue { get; }
		ZString GoodsValueCurrency { get; }
		ZString GoodsDescription { get; }

		ZDateTime EstimatedPickupDate { get; }
		ZDateTime PickupDateRequiredBy { get; }
		ZDateTime EstimatedDeliveryDate { get; }
		ZDateTime DeliveryDateRequiredBy { get; }
		ZDateTime DeliveryDate { get; }
		ZDateTime ActualPickupDate { get; }

		ZString ServiceLevelCode { get; }
		ZString Charges { get; }

		ZDateTime ReceivedDate { get; }
		ZString ReceivedBy { get; }
		ZInt PiecesReceived { get; }

		ZBool BookedOnline { get; }

		ZString Top3Containers { get; }
		ZString OrderReference { get; }

		ZGuid SendingForwarderPK { get; }
		ZGuid ReceivingForwarderPK { get; }

		ZString ShipmentType { get; }

		TrackingMilestoneCollection Milestones { get; }

		ShipmentDeclarationLookups ShipmentDeclarationLookups { get; }

		ZDecimal LoadingMeters { get; }

		ZString ContainerMode { get; }

		ZString ChargesApply { get; }
		ICodeDescriptionPairList ChargesApply_List { get; }
		ZString ReleaseType { get; }
		ICodeDescriptionPairList ReleaseType_List { get; }
		ZString OnBoard { get; }
		ICodeDescriptionPairList OnBoard_List { get; }

		ZString AdditionalTerms { get; }
		ZString InspectionTypeCode { get; }
		ZString PaymentTerm { get; }
		ICodeDescriptionPairList PaymentTerm_List { get; }

		ZString DeliveryAgentFullName { get; }
		ZString PickupAgentFullName { get; }

		ZString DeclarationCountry { get; }

		ZDecimal TEUCount { get; }

		ZString Top3JobNotes { get; }

		ZDateTime FirstLegLoadETD { get; }
		ZDateTime FirstLegLoadATD { get; }
		ZDateTime LastLegDischargeETA { get; }
		ZDateTime LastLegDischargeATA { get; }
	}
}
