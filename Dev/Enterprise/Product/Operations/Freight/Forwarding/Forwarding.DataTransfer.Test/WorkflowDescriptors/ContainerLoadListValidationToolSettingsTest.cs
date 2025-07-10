using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing;

internal class ContainerLoadListValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestRequestTypeJobType()
	{
		AssertEquals(ExternalRequestTypeJobTypes.Codes.CLH, new ContainerLoadListValidationToolSettings(new ContainerLoadListWorkflowDescriptor()).RequestTypeJobType);
	}
}
