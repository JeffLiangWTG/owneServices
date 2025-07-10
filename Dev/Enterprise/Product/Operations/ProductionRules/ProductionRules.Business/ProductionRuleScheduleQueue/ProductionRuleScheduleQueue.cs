using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ProductionRules.Business
{
	public class ProductionRuleScheduleQueue : AutoProductionRuleScheduleQueue
	{
		public ProductionRuleScheduleQueue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Rule))]
		public override ZGuid PRQ_PRL_Rule
		{
			get => base.PRQ_PRL_Rule;
			set => base.PRQ_PRL_Rule = value;
		}

		public ProductionRule Rule => Factory.Load<ProductionRule>(PRQ_PRL_Rule);
	}
}
