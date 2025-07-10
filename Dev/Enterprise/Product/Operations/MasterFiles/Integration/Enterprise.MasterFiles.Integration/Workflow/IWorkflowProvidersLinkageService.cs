using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IWorkflowProvidersLinkageService
	{
		void WorkflowProvidersLinked(IWorkflowProviderCore sourceWorkflowProvider, IWorkflowProviderCore connectedWorkflowProvider, BusinessObjectFactory factory);
		void WorkflowLinkedTemplatesApplied(IWorkflowProviderCore sourceWorkflowProvider, IWorkflowProviderCore connectedWorkflowProvider, BusinessObjectFactory factory, IEnumerable<IProcessTaskTemplate> templatesToApply);
		void WorkflowProvidersUnLinked(IWorkflowProviderCore sourceWorkflowProvider, IWorkflowProviderCore disconnectedWorkflowProvider, BusinessObjectFactory factory);
	}
}
