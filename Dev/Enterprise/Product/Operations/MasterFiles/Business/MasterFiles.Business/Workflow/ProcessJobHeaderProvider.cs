using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class ProcessJobHeaderProvider
	{
		public static IProcessJobHeader GetForParent(IWorkflowProviderCore workflowProvider, BusinessObjectFactory factory, bool addDefaultProcessHeaderIfNone = true, bool checkTemplates = true)
		{
			return ObjectFactory.Get<IProcessJobHeaderProvider>().GetForParent(workflowProvider, factory, addDefaultProcessHeaderIfNone, checkTemplates);
		}

		public static IProcessJobHeader GetForParentWithoutCreation(IWorkflowProviderCore workflowProvider, BusinessObjectFactory factory)
		{
			return ObjectFactory.Get<IProcessJobHeaderProvider>().GetForParentWithoutCreation(workflowProvider, factory);
		}

		public static IProcessJobHeader GetForTemplate(IProcessTaskTemplate template, IWorkflowProviderCore parent)
		{
			return ObjectFactory.Get<IProcessJobHeaderProvider>().GetForTemplate(template, parent);
		}

		public static bool SupportsPAVE(string workflowType, BusinessObjectFactory factory)
		{
			return ObjectFactory.Get<IProcessJobHeaderProvider>().SupportsPAVE(workflowType, factory);
		}

		public static IProcessHeader GetDefaultWorkflowForTask(ProcessTask task)
		{
			return ObjectFactory.Get<IProcessJobHeaderProvider>().GetDefaultWorkflowForTask(task);
		}

		public static IProcessHeaderCollection GetWorkflowsForParent(IWorkflowProviderCore workflowProvider, BusinessObjectFactory factory)
		{
			return ObjectFactory.Get<IProcessJobHeaderProvider>().GetWorkflowsForParent(workflowProvider, factory);
		}

		public static IProcessHeaderCollection GetWorkflowsForProcessTaskCollection(ProcessTaskCollection processTaskCollection, BusinessObjectFactory factory)
		{
			return ObjectFactory.Get<IProcessJobHeaderProvider>().GetWorkflowsForProcessTaskCollection(processTaskCollection, factory);
		}

		public static bool BufferManagementEnabledForWorkflowProvider(IWorkflowProviderCore workflowProvider, BusinessObjectFactory factory)
		{
			return ObjectFactory.Get<IProcessJobHeaderProvider>().BufferManagementEnabledForWorkflowProvider(workflowProvider, factory);
		}
	}
}
