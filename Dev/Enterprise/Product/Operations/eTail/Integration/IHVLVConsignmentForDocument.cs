using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.Integration
{
	[FlattenPropertiesFromInheritanceForMacroEvaluation(nameof(IHVLVConsignment))]
	public interface IHVLVConsignmentForDocument : IHVLVConsignment
	{
		ZString ACASReportGoodsDescription { get; }
		ZString ChargeableForDisplay { get; }
		ZBool ConsigneeIsOrganisation { get; }
		ZString ConsolDestination { get; }
		ZDateTime ConsolETA { get; }
		ZDateTime ConsolETD { get; }
		ZString ConsolOrigin { get; }
		ZString ConsolMasterBill { get; }
		ZString ConsolVoyageFlight { get; }
		ZString ConsolVessel { get; }
		ZString DestinationCountry { get; }
		ZString ExportCustomsClearanceStatusDescription { get; }
		ZString FirstLineWeightUnit { get; }
		ZBool HasFailedPreScreeningStatus { get; }
		ZBool HasUnknownPrescreeningStatus { get; }
		ZString HumanReadableNameWithoutId { get; }
		ZString HVC_Calc_DeliveryCartageZone { get; }
		ZString ImportCustomsClearanceStatusDescription { get; }
		ZBool IsExport { get; }
		ZBool IsImport { get; }
		ZBool IsReturn { get; }
		ZBool IsSurplusAtDestination { get; }
		ZString LastMileCarrierDepotID { get; }
		ZString LastMileCarrierServiceCode { get; }
		ZString LastMileCarrierServiceLevelDescription { get; }
		ZString LocalTransportCompanyLabel { get; }
		ZBool NeedACASAmendment { get; }
		ZString OriginCountry { get; }
		ZBool ShipperIsOrganisation { get; }
		ZString ShipmentTransportMode { get; }
		ZString ShipmentPackingMode { get; }
		ZBool RequiresACAS { get; }
		ZString ReleaseStatusDescription { get; }
		ZString PreScreeningWarningDetails { get; }
		ZString PreScreeningErrorDetails { get; }
		ZString PaymentTermDisplay { get; }
		ZBool TransportModeIsSea { get; }
		ZBool TransportModeIsAir { get; }
		ZDecimal TotalLineCustomsValues { get; }
		ZDecimal TotalLineNetWeight { get; }
		ZDecimal TotalLineGrossWeight { get; }
		ZShort TotalItemLines { get; }
		ZBool ShowImport { get; }
		ZBool ShowExport { get; }

		IHVLVBookingHeader BookingHeader { get; }
		IOrgAddress ConsigneeAddress { get; }
		IRefCountry ConsigneeCountryCode { get; }
		IOrgAddress DestinationDepot { get; }
		IRefCurrency GoodsValueCurrency { get; }
		IOrgHeader LastMileCarrier { get; }
		IOrgHeader LastMileCarrierBookingAgent { get; }
		IOrgAddress ShipperAddress { get; }
		IRefCountry ShipperCountryCode { get; }
		IHVLVConsignmentCollection ConsignmentsBelongToSameConsignee { get; }
		IHVLVConsignmentCollection ConsignmentsBelongToSameConsigneeExcludingParent { get; }
		IHVLVConsignmentCollection FormerConsignments { get; }
		IHVLVConsignmentCollection ReturnConsignments { get; }

		IProcessTaskCollection WorkflowItems { get; }
		IEnumerable<IHVLVItemForDocument> ActiveItems { get; }
		Forwarding.IForwardingShipment ManagingShipment { get; }
		Forwarding.IForwardingShipment ManifestedOnShipment { get; }
		Customs.IBaseJobDeclaration ImportDeclaration { get; }
		Customs.IBaseJobDeclaration ExportDeclaration { get; }
		Customs.ICusEntryNumAdditionalReferenceCollection CustomsReferenceNumbers { get; }

		Logs Logs { get; }
		Notes Notes { get; }
	}
}
