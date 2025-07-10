using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressSourceAndJobNumberModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateParentType()
		{
			var filter = new AddressSourceAndJobNumberModuleFilter("TEST");
			filter.JobNumber = "";
			filter.AddressSourceCode = "";
			AssertNoErrors(filter.AddressSourceCodeInfo);

			filter.AddressSourceCode = "INVALIDPARENT";
			AssertHasErrors(filter.AddressSourceCodeInfo);

			filter.JobNumber = "123";
			filter.AddressSourceCode = "";
			AssertHasError(filter.AddressSourceCodeInfo, "Can't filter job number without address source");

			filter.AddressSourceCode = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			AssertNoErrors(filter.AddressSourceCodeInfo);
		}
	}
}
