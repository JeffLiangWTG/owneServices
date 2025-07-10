using System.Collections.Generic;

namespace Enterprise.MasterFiles.Integration
{
	public interface IWorkflowTriggerEventSource
	{
		IReadOnlyList<IWorkflowProviderCore> ParentWorkflowProviders { get; }
		IGlbCompany JobHeaderCompany { get; }
	}
}
