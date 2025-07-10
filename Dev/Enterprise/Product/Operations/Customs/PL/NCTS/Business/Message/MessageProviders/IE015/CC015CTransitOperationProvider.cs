using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC015CTransitOperationProvider(NctsDepartureMovementHeader movementHeader) : ITransitOperation
{
	readonly NctsDepartureMovementHeader movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));

	public string LRN => EDIMessage.PL_NCTS_LRN_PlaceHolder;

	public string DeclarationType => movementHeader.BM_InBondEntryType;

	public string AdditionalDeclarationType => movementHeader.BM_AdditionalDeclarationType;

	public string TIRCarnetNumber => movementHeader.TirCarnetNumber;

	public string Security => CachedValueHelper.GetValue(ref security, () => GetSecurity());
	CachedValue<string> security;

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

	public NCTSIndicator ReducedDatasetIndicator => CachedValueHelper.GetValue(ref reducedDatasetIndicator, () =>
		movementHeader.BM_ReducedDatasetIndicator
			? NCTSIndicator.YES
			: NCTSIndicator.NO);
	CachedValue<NCTSIndicator> reducedDatasetIndicator;

	public string SpecificCircumstanceIndicator => movementHeader.BM_SpecificCircumstance;

	public string CommunicationLanguageAtDeparture => null;

	public NCTSIndicator BindingItinerary => NCTSIndicator.NO;

	public DateTime? PresentationOfTheGoodsDateAndTime => movementHeader.BM_PresentationDateTime.IsValid
		? DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(movementHeader.BM_PresentationDateTime.ToZDateTime(), removeMillisecond: true) : null;

	public DateTime? LimitDate => CachedValueHelper.GetValue(ref limitDate, movementHeader.GetExportDate);
	CachedValue<DateTime?> limitDate;
}
