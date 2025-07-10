using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IProcessTaskTemplateLoader
	{
		IProcessTaskTemplate FindTemplateForScreenLayout(IWorkflowProviderCore host, bool ignoreCache = false);

		IProcessTaskTemplateMatches FindMatches(IWorkflowProviderCore workflowProvider, bool ignoreCache = false, bool includeUniversalTemplates = false, bool includeOnlyUniversalTemplates = false);
	}

	public interface IProcessTaskTemplateMatches
	{
		IList<IProcessTaskTemplate> Matches { get; }
	}

	public interface IProcessTaskTemplate : IIdentified
	{
		IFormCustomisationSettings FormCustomisationSettings { get; }
		ICustomColumnDefinition[] CustomColumnDefinitions { get; }
		ZString P0_CustomFieldFallback { get; }
		ZBlob P0_FormState { get; }
	}

	public interface ICustomColumnDefinition
	{
		ZGuid Identifier { get; }
		string Type { get; }
		string Name { get; }
		string NameLocalized { get; }
		int MaxLength { get; }
		int? Sequence { get; }
		bool IsDeleted { get; }
		bool IsRuleActive { get; }
		ZGuid? RuleDefinitionReference { get; }
		ICustomAddOnRule[] GetRules();
	}
}
