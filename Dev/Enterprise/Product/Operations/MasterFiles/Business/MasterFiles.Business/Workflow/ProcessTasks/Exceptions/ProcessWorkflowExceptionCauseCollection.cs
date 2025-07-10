using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessWorkflowExceptionCauseCollection : BusinessObjectCollection<ProcessWorkflowExceptionCause>
	{
		public ProcessWorkflowExceptionCauseCollection(BusinessObjectFactory factory, ZQuery filter = null)
			: base(factory, filter)
		{
		}
	}
}
