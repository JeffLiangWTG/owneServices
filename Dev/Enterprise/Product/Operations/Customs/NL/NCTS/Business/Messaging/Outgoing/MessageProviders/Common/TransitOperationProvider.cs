using System;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class TransitOperationProvider : ITransitOperation
{
	protected readonly NctsHeader header;
	readonly NctsCommonMovementHeader movementHeader;

	public TransitOperationProvider(NctsHeader nctsHeader)
	{
		header = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		movementHeader = Argument.NotNull(header.CommonMovementHeader, nameof(movementHeader));
	}

	public virtual string LRN => movementHeader.BM_PaperlessInbondNum;

	public string MRN => header.MovementReferenceNumber;

	public string DeclarationType => movementHeader.BM_InBondEntryType;

	public string AdditionalDeclarationType => movementHeader.BM_AdditionalDeclarationType;

	public string TIRCarnetNumber => movementHeader is NctsDepartureMovementHeader depMovementHeader ? depMovementHeader.TirCarnetNumber : null;

	public DateTime? PresentationDateAndTime => movementHeader.BM_ArrivalDate.IsValid ? movementHeader.BM_ArrivalDate.ToDateTime() : null;

	public int? Security => GetSecurity(movementHeader.BM_TypeOfSecurity);

	public bool ReducedDatasetIndicator => movementHeader.BM_ReducedDatasetIndicator;

	public string SpecificCircumstanceIndicator => movementHeader.BM_SpecificCircumstance;

	public string CommunicationLanguageAtDeparture => header.BH_CommunicationLanguage.IsEmpty ? string.Empty : header.BH_CommunicationLanguage.ToLower();

	public virtual bool BindingItinerary => header.CountriesOfRouting.Any();

	public DateTime? LimitDate => movementHeader.IsSimplifiedNctsProcedure && movementHeader.BM_ExportDate.IsValid ? movementHeader.BM_ExportDate.ToDateTime() : null;

	public DateTime ArrivalNotificationDateAndTime => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(!movementHeader.BM_ArrivalDate.IsValid ? ZDateTime.Now : movementHeader.BM_ArrivalDate, removeMillisecond: true);

	public bool IncidentFlag => header.BH_ExportFlag == EventFlagList.Codes.Yes;

	public bool SimplifiedProcedure => (movementHeader is NctsDepartureMovementHeader depMovementHeader && depMovementHeader.CusAuthorizationUsages.Count > 0) || (header.IsArrivalMovement && header.CusAuthorizationUsages.Count > 0);

	public bool AmendmentTypeFlag => false;

	static int? GetSecurity(ZString typeOfSecurity) => (string)typeOfSecurity switch
	{
		NctsTypeOfSecurityList.Codes.NON => 0,
		NctsTypeOfSecurityList.Codes.ENT => 1,
		NctsTypeOfSecurityList.Codes.EXI => 2,
		NctsTypeOfSecurityList.Codes.BTH => 3,
		_ => null,
	};
}
