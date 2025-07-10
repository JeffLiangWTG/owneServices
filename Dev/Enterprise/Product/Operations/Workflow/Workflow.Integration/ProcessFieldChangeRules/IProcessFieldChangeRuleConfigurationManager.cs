using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Integration
{
	public interface IProcessFieldChangeRuleConfigurationManager
	{
		CodeDescriptionPairList GetFieldAndTableNames(ZString workflowType);
		CodeDescriptionPairList GetFields(ZString workflowType);
		CodeDescriptionPairList GetTables(ZString workflowType);
	}
}
