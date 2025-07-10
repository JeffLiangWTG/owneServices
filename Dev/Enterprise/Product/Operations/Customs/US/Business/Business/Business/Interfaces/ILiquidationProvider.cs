using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	interface ILiquidationProvider
	{
		JobDeclaration Declaration { get; }

		void SetAnticipatedLiquidatedDuty(ZDecimal dutyAmount);
		void SetAnticipatedLiquidationDate(ZDateTime dateTime);
	}
}
