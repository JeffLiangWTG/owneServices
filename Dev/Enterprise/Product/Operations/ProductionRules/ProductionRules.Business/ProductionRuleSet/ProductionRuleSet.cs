using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ProductionRules.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProductionRules.Business
{
	public class ProductionRuleSet : AutoProductionRuleSet, IProductionRuleSet
	{
		public ProductionRuleSet(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ActiveBusinessObjectCollection<ProductionRule> Rules => new ActiveBusinessObjectCollection<ProductionRule>(Factory, new ZQuery(ProductionRuleSchema.PRL_PRS_RuleSet, PK));

		public IWhsWarehouse Warehouse => Factory.Load<IWhsWarehouse>(PRS_WW_Warehouse);
	}
}
