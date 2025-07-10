using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentProcessTask))]
	public class HVLVConsignmentProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.NewWithValidTestData<HVLVConsignment>().WorkflowItems.AddNew();
	}
}
