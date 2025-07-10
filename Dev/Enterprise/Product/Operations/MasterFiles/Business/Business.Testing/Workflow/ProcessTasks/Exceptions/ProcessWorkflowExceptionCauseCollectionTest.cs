using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessWorkflowExceptionCauseCollection))]
	public class ProcessWorkflowExceptionCauseCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ProcessWorkflowExceptionCauseCollection(Factory);
		}
	}
}
