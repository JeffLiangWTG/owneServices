using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing;

sealed class  JobSupplierBookingValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestRequestTypeJobType()
	{
		AssertEquals(ExternalRequestTypeJobTypes.Codes.SBK, new JobSupplierBookingValidationToolSettings(new JobSupplierBookingWorkflowDescriptor()).RequestTypeJobType);
	}
}
