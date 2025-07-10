using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessWorkflowExceptionResolutionDependentCollection))]
	public class ProcessWorkflowExceptionResolutionDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ProcessWorkflowExceptionResolutionDependentCollection(Factory.New<ProcessWorkflowExceptionType>());
		}
	}
}
