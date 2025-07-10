using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface ITriggerActionDiagnostics
	{
		ZString GetUnsavedFiredTriggersInformation(BusinessObjectFactory factory);
	}
}
