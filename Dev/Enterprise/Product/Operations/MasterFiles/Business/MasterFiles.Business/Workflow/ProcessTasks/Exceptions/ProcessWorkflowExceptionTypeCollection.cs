using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessWorkflowExceptionTypeCollection : BusinessObjectCollection<ProcessWorkflowExceptionType>
	{
		public ProcessWorkflowExceptionTypeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
