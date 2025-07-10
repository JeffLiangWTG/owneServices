using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC013CTransitOperationProvider : IExtendedTransitOperation
{
	public CC013CTransitOperationProvider(NctsDepartureMovementHeader movementHeader, MessageSendingObject messageSendingObject)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}

	readonly NctsDepartureMovementHeader movementHeader;
	readonly MessageSendingObject messageSendingObject;

	public string MRN => messageSendingObject.MovementReferenceNumber.IsEmpty ? null : messageSendingObject.MovementReferenceNumber;

	public NCTSIndicator AmendmentTypeFlag => messageSendingObject.AmendmentType == AmendmentTypeList.Codes._0DeclarationAmendment ? NCTSIndicator.NO : NCTSIndicator.YES;

	public string LRN => string.IsNullOrEmpty(MRN) ? EDIMessage.PL_NCTS_LRN_PlaceHolder : null;

	public string DeclarationType => movementHeader.BM_InBondEntryType;

	public string AdditionalDeclarationType => movementHeader.BM_AdditionalDeclarationType;

	public string TIRCarnetNumber => null;

	public DateTime? PresentationOfTheGoodsDateAndTime => movementHeader.BM_PresentationDateTime.IsValid
		? DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(movementHeader.BM_PresentationDateTime.ToZDateTime(), removeMillisecond: true) : null;

	public string Security => CachedValueHelper.GetValue(ref security, () => GetSecurity());
	CachedValue<string> security;

	public NCTSIndicator ReducedDatasetIndicator => NCTSIndicator.NO;

	public string SpecificCircumstanceIndicator => movementHeader.BM_SpecificCircumstance;

	public string CommunicationLanguageAtDeparture => null;

	public NCTSIndicator BindingItinerary => NCTSIndicator.NO;

	public DateTime? LimitDate => CachedValueHelper.GetValue(ref limitDate, movementHeader.GetExportDate);
	CachedValue<DateTime?> limitDate;

	string GetSecurity()
	{
		string result = null;
		switch (movementHeader.BM_TypeOfSecurity)
		{
			case NctsTypeOfSecurityList.Codes.NON:
				result = ExportSecurityTypeList.Codes.NotUsed;
				break;
			case NctsTypeOfSecurityList.Codes.ENT:
				result = PL.Business.Declaration.ExportSecurityTypeList.Codes.ENS;
				break;
			case NctsTypeOfSecurityList.Codes.EXI:
				result = ExportSecurityTypeList.Codes.EXS;
				break;
			case NctsTypeOfSecurityList.Codes.BTH:
				result = PL.Business.Declaration.ExportSecurityTypeList.Codes.EnsAndExs;
				break;
		}
		return result;
	}
}
