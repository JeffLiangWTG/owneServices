using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessWorkflowExceptionResolutionCollection : BusinessObjectCollection<ProcessWorkflowExceptionResolution>
	{
		public ProcessWorkflowExceptionResolutionCollection(BusinessObjectFactory factory, ZQuery filter = null)
			: base(factory, filter)
		{
		}
	}
}
