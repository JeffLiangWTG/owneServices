using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IWorkflowProviderCore
	{
		ZGuid PK { get; }
		ZString WorkflowType { get; }
		IColumnValueRanker GetTemplateSelectionCriteria();
	}
}
