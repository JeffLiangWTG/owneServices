using System.Collections.Generic;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using WTG.ProductionRules.Core;

namespace Enterprise.ProductionRules.Integration
{
	public interface IScheduledRuleProcessor
	{
		ZGuid GetBranchToRunRulesAgainst(ReadOnlyBusinessObjectFactory factory, IProductionRuleSet ruleSet);
		IEnumerable<IInputFact> LoadInputFacts(ReadOnlyBusinessObjectFactory factory, IProductionRuleSet ruleSet, CancellationToken cancellationToken);
		void ProcessResults(BusinessObjectFactory factory, ProductionRulesEngineResult result, INotifications notifications, CancellationToken cancellationToken);
		string InformationMessageForNothingProcessed { get; }

		GuidRegistryItem ErrorContactGroupRegistryItem { get; }
	}
}
