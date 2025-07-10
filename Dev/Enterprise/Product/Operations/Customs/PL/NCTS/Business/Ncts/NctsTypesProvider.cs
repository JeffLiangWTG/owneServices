using System;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class NctsTypesProvider : INctsTypesProvider
{
	public Type NctsAdditionalInfoType => typeof(NctsAdditionalInfo);
	public Type NctsArrivalCargoDescType => typeof(NctsArrivalCargoDesc);
	public Type NctsDepartureCargoDescType => typeof(EU.NCTS.Business.NctsDepartureCargoDesc);
}
