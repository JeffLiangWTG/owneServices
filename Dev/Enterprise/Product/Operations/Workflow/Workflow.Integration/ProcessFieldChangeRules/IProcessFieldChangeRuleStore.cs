
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Integration
{
	public interface IProcessFieldChangeRuleStore
	{
		Dictionary<string, List<IReadOnlyProcessFieldChangeRule>> GetProcessFieldChangeRules(BusinessObjectFactory factory, string processType);
		IEnumerable<string> GetActiveRuleTables(BusinessObjectFactory factory);
		void ClearCache();
	}
}
