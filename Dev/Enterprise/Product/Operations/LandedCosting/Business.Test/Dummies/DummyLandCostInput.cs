using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business.Testing
{
	public sealed class DummyLandCostInput : IDefaultLandedCostInput
	{
		public ZString ChargeDescriptionExposed;
		public ZString ChargeDescription => ChargeDescriptionExposed;

		public Money AmountToDistributeExposed;
		public Money AmountToDistribute => AmountToDistributeExposed;

		public ZGuid FKToChargeCodeExposed;
		public ZGuid FKToChargeCode => FKToChargeCodeExposed;

		public ZDecimal ExchangeRateExposed;
		public ZDecimal ExchangeRate => ExchangeRateExposed;

		public ZBool IsValidToImportExposed;
		public ZBool IsValidToImport => IsValidToImportExposed;
	}
}
