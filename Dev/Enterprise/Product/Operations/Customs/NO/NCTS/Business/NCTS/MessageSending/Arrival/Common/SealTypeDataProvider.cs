using CargoWise.Customs.NO.MessageContracts.NCTS;

namespace Enterprise.Customs.NO.NCTS.Business.Arrival;

sealed class SealTypeDataProvider(string identifier) : SealType05DataProviderAbstractClass
{
	public override string Identifier => identifier;
}
