using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Messaging.CUSCAR
{
	public interface ICusCarHeader : Enterprise.Messaging.Business.IEDIMessageCollectionProvider
	{
		/// <summary>
		///  COH, COM, BBK, etc
		/// </summary>
		ManifestDocumentType ManifestDocumentType { get; }
		ZString MRNForAmendOrDelete { get; }
		ZDateTime ManifestDate { get; }  // Today?
		ZDateTime DepartureDate { get; }
		ZDateTime ArrivalDate { get; }
		ZString CW1Reference { get; }
		ZString CARN { get; }
		IEnumerable<ICusCarParty> GetParties(string billIssuer);
		IEnumerable<ICusCarPerson> GetPeople(string billIssuer);
		ZString ConveyanceNumberOrTransportName { get; }  // voyage
		ZString AgentType { get; }
		ZString TransportMode { get; }
		ZString CarrierCode { get; }
		ZString VesselID { get; }
		ZString TransportNationality { get; }
		ZString PortOfLoading { get; }
		ZString PortOfDischarge { get; }
		ZString PortOfLoadingCountry { get; }
		ZString ImportExportNature { get; }
		ZString PortOfDischargeCountry { get; }
		ZString ContainerMode { get; }
		IEnumerable<ICusCarContainer> GetContainersByBillIssuer(ZString billIssuer);
		ZDecimal GetGrossMassInKilosByBillIssuer(ZString billIssuer);
		ZString ManifestNumber { get; }
		ZDateTime ManifestRegistrationDate { get; }
		IEnumerable<ICusCarLine> GetLinesByBillIssuer(ZString billIssuer);
		ZString CustomsOffice { get; }
		ZString PlaceOfExit { get; }
		/// <summary>
		/// 23 = Import, 24 = transit, etc
		/// </summary>
		ZString ManifestTypeOrBolNature { get; }
		ZDateTime DateAtCustomsOffice { get; }
		IEnumerable<ICusCarContainer> HeaderContainers { get; }
		ZString MasterBol { get; }
		ZDateTime EstimatedTimeOfLoading { get; }
		ZString CARNForAmendOrDelete { get; }
		ZString CustomsCodeForContainerMode { get; }
		ZString CustomsCodeForImportExportNature { get; }
		IEnumerable<ICusTransport> Transports { get; }
	}

	public enum ManifestDocumentType
	{
		None, COM, COH, BBB, BBK, ECL, FFM, FWB, HAB, RMA, RFM, AQM, ALM, ALH, AND, ANT
	}
}
