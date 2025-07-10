using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing;

internal class ContainerLoadListLineValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestRequestTypeJobType()
	{
		AssertEquals(ExternalRequestTypeJobTypes.Codes.CLI, new ContainerLoadListLineValidationToolSettings(new ContainerLoadListLineWorkflowDescriptor()).RequestTypeJobType);
	}
}
