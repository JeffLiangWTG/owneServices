using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business;

public sealed class NonPersistentTemplateValidationResult : AutoNonPersistentTemplateValidationResult
{
	public NonPersistentTemplateValidationResult(BusinessObjectFactory factory, ZString actionSource, ZString actionSourceDescription) : base(factory, actionSource, actionSourceDescription)
	{
	}

	public override ZInt TotalMatchedRules => RuleResults.Count;

	public override ZInt FailedRulesNumber => FailedRuleResults.Count();

	public override ZBool Passed => TotalMatchedRules.IsEmpty || FailedRulesNumber.IsEmpty;

	public NonPersistentRuleValidationResultCollection RuleResults => ruleResults ??= new NonPersistentRuleValidationResultCollection(Factory);
	NonPersistentRuleValidationResultCollection ruleResults;

	public IEnumerable<NonPersistentRuleValidationResult> FailedRuleResults => RuleResults.Where(x => !x.Passed);
}
