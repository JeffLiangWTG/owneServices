using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class ProfitShareRedistributionRuleCollection : DependentBusinessObjectCollection<ProfitShareRedistributionRule, ProfitShareRedistribution>
	{
		public ProfitShareRedistributionRuleCollection(ProfitShareRedistribution parent)
			: base(parent)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return ProfitShareRedistributionRuleSchema.PRR_PSR_ProfitShareRedistribution; }
		}
	}
}
