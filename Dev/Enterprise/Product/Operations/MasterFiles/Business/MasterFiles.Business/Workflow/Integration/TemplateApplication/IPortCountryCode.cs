using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IPortCountryCode
	{
		ZString OriginCountry(IWorkflowProvider workflowProvider);
		ZString DestinationCountry(IWorkflowProvider workflowProvider);
	}
}
