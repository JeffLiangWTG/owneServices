using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.ProductionRules.Business
{
	class ProductionRuleWrapper : IProductionRule
	{
		public ProductionRuleWrapper(string context, string contextSubType, string name, string description, short priority, string ruleDefinition)
		{
			Context = Argument.NotNull(context, nameof(context));
			ContextSubType = Argument.NotNull(contextSubType, nameof(contextSubType));
			Definition = Argument.NotNull(ruleDefinition, nameof(ruleDefinition));
			Description = Argument.NotNull(description, nameof(description));
			Name = Argument.NotNull(name, nameof(name));
			Priority = priority;
		}

		#region IProductionRule Implementation

		public string Context { get; }

		public string ContextSubType { get; }

		public string Definition { get; }

		public string Description { get; }

		public string Name { get; }

		public short Priority { get; }

		#endregion
	}
}
