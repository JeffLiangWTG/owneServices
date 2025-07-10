using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Will need this when developing the module and rest of functionality")]
	public class ProcessWorkflowExceptionCollection : BusinessObjectCollection<ProcessWorkflowException>
	{
		public ProcessWorkflowExceptionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
