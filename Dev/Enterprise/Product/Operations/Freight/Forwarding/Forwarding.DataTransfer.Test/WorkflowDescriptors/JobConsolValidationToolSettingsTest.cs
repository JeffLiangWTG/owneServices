using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing;

internal class JobConsolValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestRequestTypeJobType()
	{
		AssertEquals(ExternalRequestTypeJobTypes.Codes.CON, new JobConsolValidationToolSettings(new JobConsolWorkflowDescriptor()).RequestTypeJobType);
	}
}
