using CargoWise.Types;

namespace Enterprise.ProductionRules.Integration
{
	public interface IProductionRuleSet
	{
		ZGuid PK { get; }
		ZString PRS_Context { get; }
		ZString PRS_ContextSubType { get; }
		ZGuid PRS_WW_Warehouse { get; }
		ZGuid PRS_GC_Company { get; }
	}
}
