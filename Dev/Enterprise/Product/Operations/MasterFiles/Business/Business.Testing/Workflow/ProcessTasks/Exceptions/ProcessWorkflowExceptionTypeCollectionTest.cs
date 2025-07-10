using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessWorkflowExceptionTypeCollection))]
	public class ProcessWorkflowExceptionTypeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ProcessWorkflowExceptionTypeCollection(Factory);
		}
	}
}
