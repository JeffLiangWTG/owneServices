using CargoWise.Customs.NO.MessageContracts.NCTS;

namespace Enterprise.Customs.NO.NCTS.Business.Arrival;

sealed class EconomicOperatorTypeDataProvider(string identificationNumber) : EconomicOperatorType03DataProviderAbstractClass
{
	public override string IdentificationNumber { get; } = identificationNumber;
}
