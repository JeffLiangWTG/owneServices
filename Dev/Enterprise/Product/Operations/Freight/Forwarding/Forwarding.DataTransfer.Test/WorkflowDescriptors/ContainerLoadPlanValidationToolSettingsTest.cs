using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing;

internal class ContainerLoadPlanValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestRequestTypeJobType()
	{
		AssertEquals(ExternalRequestTypeJobTypes.Codes.CLP, new ContainerLoadPlanValidationToolSettings(new ContainerLoadPlanWorkflowDescriptor()).RequestTypeJobType);
	}
}
