using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing;

internal class JobSupplierBookingLineValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestRequestTypeJobType()
	{
		AssertEquals(ExternalRequestTypeJobTypes.Codes.SBL, new JobSupplierBookingLineValidationToolSettings(new JobSupplierBookingLineWorkflowDescriptor()).RequestTypeJobType);
	}
}
