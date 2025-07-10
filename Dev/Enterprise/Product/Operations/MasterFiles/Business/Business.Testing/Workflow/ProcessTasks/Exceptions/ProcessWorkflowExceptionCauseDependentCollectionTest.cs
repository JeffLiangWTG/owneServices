using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessWorkflowExceptionCauseDependentCollection))]
	public class ProcessWorkflowExceptionCauseDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ProcessWorkflowExceptionCauseDependentCollection(Factory.New<ProcessWorkflowExceptionType>());
		}
	}
}
