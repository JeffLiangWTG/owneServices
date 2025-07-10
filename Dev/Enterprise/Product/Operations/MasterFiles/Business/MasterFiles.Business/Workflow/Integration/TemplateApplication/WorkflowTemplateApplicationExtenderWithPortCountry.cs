using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class WorkflowTemplateApplicationExtenderWithPortCountry<TWorkflowProvider> : WorkflowTemplateApplicationExtender<TWorkflowProvider>, IPortCountryCode
		where TWorkflowProvider : BusinessObject, IWorkflowProvider
	{
		ZString IPortCountryCode.DestinationCountry(IWorkflowProvider workflowProvider)
		{
			return DestinationCountry((TWorkflowProvider)workflowProvider);
		}
		ZString IPortCountryCode.OriginCountry(IWorkflowProvider workflowProvider)
		{
			return OriginCountry((TWorkflowProvider)workflowProvider);
		}

		protected abstract ZString DestinationCountry(TWorkflowProvider workflowProvider);
		protected abstract ZString OriginCountry(TWorkflowProvider workflowProvider);
	}
}
