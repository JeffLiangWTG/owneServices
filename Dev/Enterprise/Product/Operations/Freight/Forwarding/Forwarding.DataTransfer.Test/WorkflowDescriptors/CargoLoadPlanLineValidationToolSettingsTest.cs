using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing;

internal class CargoLoadPlanLineValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestRequestTypeJobType()
	{
		AssertEquals(ExternalRequestTypeJobTypes.Codes.CPL, new CargoLoadPlanLineValidationToolSettings(new CargoLoadPlanLineWorkflowDescriptor()).RequestTypeJobType);
	}
}
