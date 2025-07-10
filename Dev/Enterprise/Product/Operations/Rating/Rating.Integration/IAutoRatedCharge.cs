using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Rating.Integration
{
	public interface IAutoRatedCharge
	{
		IAccChargeCode ChargeCode { get; }
		ZGuid ProviderPK { get; }
		ZGuid DebtorOverridePK { get; set; }
		ZString InvoiceLineDescription { get; set; }
		CostSell CostSell { get; }
		ZString ChargeUnit { get; }
		string UnitFactor { get; }
		ZString Description { get; set; }
		CalculatorType CalculatorType { get; }
		void Multiply(ZDecimal multiplyRatio);
	}
}
