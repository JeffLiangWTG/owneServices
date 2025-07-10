using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing;

internal class OrderLineValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestRequestTypeJobType()
	{
		AssertEquals(ExternalRequestTypeJobTypes.Codes.ORL, new OrderLineValidationToolSettings(new OrderLineWorkflowDescriptor()).RequestTypeJobType);
	}
}
