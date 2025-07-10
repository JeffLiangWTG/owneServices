using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public interface IWorkflowTriggerFieldChangeSource
	{
		IReadOnlyList<IWorkflowProvider> ParentWorkflowProviders { get; }
	}
}
