using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	public interface IAutoRatingChargeInfo
	{
		ZGuid PK { get; }
		AccChargeCode ChargeCode { get; }
		ZBool IsApportioned { get; }
		ICurrency CostCurrency { get; }
		ICurrency SellCurrency { get; }
		ZDecimal CostAmount { get; }
		ZDecimal AgentDeclaredCostAmount { get; }
		ZDecimal SellAmount { get; }
		ZDecimal LocalCostAmount { get; }
		ZDecimal LocalSellAmount { get; }
		ZString CostReference { get; }
		ZString SellReference { get; }
		ZString CostRatingBehavior { get; }
		ZString SellRatingBehavior { get; }
		ZGuid CostAccountPK { get; }
		GlbCompany Company { get; }

		IAutoRatingChargeInfo ParentConsolCost { get; }
		RateAttributeSet RateAttributes { get; }

		bool CanReautorate(CostSell costOrSell, params ZString[] operationalJobCodes);
	}
}