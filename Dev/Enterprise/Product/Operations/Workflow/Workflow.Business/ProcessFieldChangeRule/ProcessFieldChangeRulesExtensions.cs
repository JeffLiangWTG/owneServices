using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public static class ProcessFieldChangeRulesExtensions
	{
		public static Dictionary<string, List<IReadOnlyProcessFieldChangeRule>> GetProcessFieldChangeRules(this BusinessObjectFactory factory, string workflowProviderType)
		{
			var ruleStore = ObjectFactory.Get<IProcessFieldChangeRuleStore>();
			return ruleStore.GetProcessFieldChangeRules(factory, workflowProviderType);
		}

		public static bool IsTableReferencedByFieldChangeRules(this BusinessObjectFactory factory, string tablePrefix)
		{
			if (tablePrefix == ProcessFieldChangeRuleSchema.Constants.Prefix ||
				tablePrefix == ProcessFieldChangeRuleFieldSchema.Constants.Prefix)
			{
				return false;
			}
			var ruleStore = ObjectFactory.Get<IProcessFieldChangeRuleStore>();
			return ruleStore.GetActiveRuleTables(factory).Contains(tablePrefix);
		}

		public static bool IsTableReferencedAsChild(this BusinessObjectFactory factory, string tablePrefix)
		{
			return false;
		}
	}
}
