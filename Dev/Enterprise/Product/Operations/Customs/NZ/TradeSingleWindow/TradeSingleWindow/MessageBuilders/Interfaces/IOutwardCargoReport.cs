
namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	using System.Collections.Generic;
	using CargoWise.Types;

	/// <summary>
	/// o	Clearance details of the cargo exported in a craft. 
	/// o	Submitted by Shipping Companies or their agent
	/// </summary>

	public interface IOutwardCargoReportHeader
	{
		IOrganisationSimple Consolidator { get; }

		ZBool IsSea { get; }
		ZBool IsConsolidation { get; }
		ZString TSWReferenceNumber { get; }
		ZString SenderReferenceNumber { get; }

		ZString MasterBillNumber { get; }
		ZString CraftName { get; }
		ZString LloydsNo { get; }
		ZString VoyageNo { get; }
		ZString FlightNo { get; }
		ZDateTime DepartureDate { get; }
		IEnumerable<ZString> RoutingCountryCodes { get; }
		IOrganisationSimple Carrier { get; }
		ZString PortOfDeparture { get; }
		IEnumerable<ZString> NotifyPartyCodes { get; }
		ZString NotifyPartyName { get; }
		ZString NotifyPartyEmail { get; }

		IEnumerable<IOCRConsignment> OCRLines { get; }
		IEnumerable<ITransportEquipment> Containers { get; }

		IAdditionalInformation AdditionalInformation { get; }
	}

	public interface IOCRConsignment
	{
		ZString CustomsClearanceNo { get; }
		IAssociatedTransportDocument BillNumber { get; }
	}
}
