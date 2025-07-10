using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	/// <summary>
	/// o	Details of cargo imported in a craft including empty containers 
	///	o	Submitted by Carriers, freight-forwarders and brokers
	/// </summary>
	public interface IInwardCargoReport
	{
		ZString SenderReferenceNumber { get; }
		ZString TSWReferenceNumber { get; }
		ZBool IsSea { get; }
		ZBool IsCarrierCargoReport { get; }
		ZString CraftName { get; }
		ZString LloydsNo { get; }
		ZString VoyageNo { get; }
		ZString FlightNo { get; }
		ZDateTime ArrivalDate { get; }
		ZString PortOfArrival { get; }
		IOrganisationSimple Carrier { get; }
		ZBool UseInterfaceSequenceNumber { get; }
		IEnumerable<IICRConsignment> Consignments { get; }
		IDeclarant Declarant { get; }
		IAdditionalInformation AdditionalInformation { get; }
		ZString MPIAccountDetails { get; }
		IEnumerable<ITSWAttachment> SupportingDocuments { get; }
	}

	public interface IICRConsignment
	{
		ZBool WriteOffRequest { get; }
		ZBool IsLinkEmptyContainer { get; }
		ITranshipmentDetails TranshipmentDetails { get; }
		ZInt SequenceNumber { get; }
		ZDecimal ConsignmentValueInNZD { get; }
		IEnumerable<ZString> Permits { get; }
		ZBool MAFContainerDeclaration { get; }
		ZBool IsConsolidation { get; }
		IEnumerable<ZString> MAFContainerStatements { get; }
		IEnumerable<ZString> MPIApprovedSystemNumbers { get; }
		ZString MasterBill { get; }
		IPartyInformation Consignee { get; }
		IPartyInformation Consignor { get; }
		IOrganisation DeliverToParty { get; }
		ZString FreightPaymentMethod { get; }
		ZString PortOfOrigin { get; }
		ZString GoodsLocation { get; }
		ZString PortOfLoading { get; }
		IPartyInformation NotifyParty { get; }
		IEnumerable<IOrganisationSimple> DeliveryNotifyParties { get; }
		IEnumerable<ZString> NotifyPartyCodes { get; }
		IEnumerable<IOrganisation> ContainerPackingLocations { get; }
		IEnumerable<ZString> TranshipmentPorts { get; }
		ZString BillNumber { get; }
		ZString BillType { get; }
		IOrganisationSimple Deconsolidator { get; }
		ZBool HasContainers { get; }
		IEnumerable<ITransportEquipment> Containers { get; }
		ZString PortOfDischarge { get; }
		ZString HandlingInformation { get; }
		ZString MPIAccountDetails { get; }
		IEnumerable<IICRConsignmentItem> ConsignmentItems { get; }
		ZString IsGSTPrePaid { get; }
		ZString VendorIdentifier { get; }
		ZString ApprovedTransitionalFacilityCode { get; }
		IEnumerable<ITSWAttachment> SupportingDocuments { get; }
	}

	public interface IICRConsignmentItem
	{
		ZShort SequenceNumber { get; }
		ZBool IsEmptyContainer { get; }
		ZString GoodsDescription { get; }
		ZString IdentityNumber { get; }
		ZDecimal Value { get; }
		ZString Currency { get; }
		ZString IdentityType { get; }
		IEnumerable<IClassification> Classifications { get; }
		ZBool SendFlashpointTemp { get; }
		ZDecimal FlashpointTempInCelsius { get; }
		ITemperatureRequirements Temperatures { get; }
		ZDecimal GrossWeightInKg { get; }
		ZString GoodsOriginCountry { get; }
		ZInt PackageQty { get; }
		ZString PackageType { get; }
		ZString ContainerNumber { get; }
		ZString MPIApprovedSystemNumber { get; }
	}

	public interface IExtraAdditionalInformationParent
	{
		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		IEnumerable<(ZString Code, ZString Description, ZString TypeCode)> ExtraAdditionalInformations { get; }
	}
}
