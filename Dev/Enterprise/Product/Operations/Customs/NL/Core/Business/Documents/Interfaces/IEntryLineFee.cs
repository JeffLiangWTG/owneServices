using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public interface IEntryLineFee
{
	#region linefees
	ZString Description { get; }
	ZDecimal BaseAmount { get; }
	ZDecimal TariffApplied { get; }
	ZString MethodOfCalculation { get; }
	ZString UnitOfTariff { get; }
	ZString TariffCode { get; }
	ZDecimal TariffAmount { get; }
	#endregion
}
