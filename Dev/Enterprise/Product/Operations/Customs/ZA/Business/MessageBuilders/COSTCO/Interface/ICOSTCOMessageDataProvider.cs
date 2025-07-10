using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public interface ICOSTCOMessageDataProvider : IEDIFACTMessageAttachee
		, IBGM_BeginningOfMessage
		, IDTM_DocumentDateTime
		, IDTM_ActualArrivalDate
		, IDTM_EstimatedDateOfDeparture
		, IDTM_DateTimeFullyUnloadedLoaded
		, IFTX_ExcessIndicator
		, IFTX_ImportExportTranshipmentIndicator
		, IRFF_DocumentToBeAmended
		, ITDT_TransportInformation
		, IRFF_ManifestType
		, IRFF_PrincipalCarrierConveyageNumber
		, ILOC_PlacePortOfDischarge
		, ILOC_PlacePortOfLoading
		, INAD_MessageSender
		, INAD_OutturnProviderCode
		, ICNT_ControlTotal
	{
		IEnumerable<ICOSTCOContainerInformation> Containers { get; }
		IEnumerable<ICOSTCOLineLevelInformation> Bills { get; }
		ZBool IsDOR { get; }
		ZBool IsBBB { get; }
		ZBool IsVOR { get; }
		ZBool IsAOR { get; }
		ZBool IsEOR { get; }
		ZBool IsALD { get; }
		ZBool IsAir { get; }
		ZBool IsImport { get; }
		ZBool IsExport { get; }
	}

	public interface IBGM_BeginningOfMessage
	{
		ZString OutturnManifestType { get; }
	}

	public interface IDTM_DocumentDateTime
	{
		ZDateTime DocumentIssueDateTime { get; }
	}

	public interface IDTM_ActualArrivalDate
	{
		ZDateTime ActualArrivalDateTime { get; }
	}

	public interface IDTM_EstimatedDateOfDeparture
	{
		ZDateTime EstimatedDateOfDeparture { get; }
	}

	public interface IDTM_DateTimeFullyUnloadedLoaded
	{
		ZDateTime DateTimeFullyUnloadedLoaded { get; }
	}

	public interface IFTX_ExcessIndicator
	{
		ZString ExcessIndicator { get; }
	}

	public interface IFTX_ImportExportTranshipmentIndicator
	{
		ZString ImportExportTranshipmentIndicator { get; }
	}

	public interface IRFF_DocumentToBeAmended
	{
		ZString DocumentToBeAmended { get; }
	}

	public interface ITDT_TransportInformation
	{
		ZString VoyageFlightNumber { get; }
		ZString TransportCode { get; }
		ZString CarrierCode { get; }
		ZString CallSign { get; }
	}

	public interface IRFF_ManifestType
	{
		ZString ManifestType { get; }
	}

	public interface IRFF_PrincipalCarrierConveyageNumber
	{
		ZString PrincipalCarrierConveyageNumber { get; }
	}

	public interface ILOC_PlacePortOfDischarge
	{
		ZString PlaceOfDicharge { get; }
		ZString TerminalDepotCode { get; }
	}

	public interface ILOC_PlacePortOfLoading
	{
		ZString PlaceOfLoading { get; }
		ZString TerminalBerth { get; }
	}

	public interface INAD_MessageSender
	{
		ZString MessageSender { get; }
	}

	public interface INAD_OutturnProviderCode
	{
		ZString OutturnProviderCode { get; }
	}

	public interface ICNT_ControlTotal
	{
		ZInt TotalNumberOfPackages { get; }
	}
}
