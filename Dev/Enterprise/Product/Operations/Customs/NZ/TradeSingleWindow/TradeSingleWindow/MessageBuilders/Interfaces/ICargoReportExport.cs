
namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	using System.Collections.Generic;
	using CargoWise.Types;

	/// <summary>
	/// o	Details of cargo (parties, commodities and locations)
	/// o	Submitted Carriers and freight-forwarders
	/// </summary>
	public interface ICargoReportExport
	{
		ZBool IsSea { get; }
		ZBool IsAir { get; }
		ZBool IsMail { get; }
		ZBool IsContainerised { get; }
		ZBool HasEmptyContainersOnly { get; }
		ZString SenderReferenceNumber { get; }
		ZString TSWReferenceNumber { get; }
		IOrganisationSimple Carrier { get; }
		IAdditionalInformation AdditionalInformation { get; }
		ZString CraftName { get; }
		ZString LloydsNo { get; }
		ZString VoyageNo { get; }
		ZString FlightNo { get; }
		ZDateTime DepartureDate { get; }
		ZBool UseInterfaceSequenceNumber { get; }
		IEnumerable<ICREConsignment> Consignments { get; }
		IDeclarant Declarant { get; }
		ZString PortOfDeparture { get; }
		IEnumerable<ITSWAttachment> SupportingDocuments { get; }
	}

	public interface ICREConsignment
	{
		ZBool WriteOffRequest { get; }
		ITranshipmentDetails TranshipmentDetails { get; }
		ZShort SequenceNumber { get; }
		ZString HandlingInfo { get; }
		ZDecimal ConsignmentValueInNZD { get; }
		IPartyInformation Consignee { get; }
		IEnumerable<ICREConsignmentItem> ConsignmentItems { get; }
		IPartyInformation Consignor { get; }
		IOrganisation DeliverToParty { get; }
		ZString FreightPaymentMethod { get; }
		ZString GoodsLocation { get; }
		ZString PortOfLoading { get; }
		IEnumerable<IPartyInformation> NotifyParties { get; }
		IEnumerable<IOrganisationSimple> DeliveryNotifyParties { get; }
		IEnumerable<ZString> NotifyPartyCodes { get; }
		IAssociatedTransportDocument BillNumber { get; }
		IOrganisationSimple Consolidator { get; }
		ZBool HasContainers { get; }
		IEnumerable<ITransportEquipment> Containers { get; }
		ZString PortOfDischarge { get; }
	}

	public interface ICREConsignmentItem
	{
		ZShort SequenceNumber { get; }
		ZBool IsEmptyContainer { get; }
		ZString GoodsDescription { get; }
		IEnumerable<ICommodity> Identifiers { get; }
		ZDecimal Value { get; }
		ZString Currency { get; }
		IEnumerable<IClassification> Classifications { get; }
		ZDecimal GrossWeightInKg { get; }
		ZString GoodsOriginCountry { get; }
		ZInt PackageQty { get; }
		ZString PackageType { get; }
		ZString ContainerNumber { get; }
		ZString UNDGHazardousGoodsCode { get; }
	}
}
