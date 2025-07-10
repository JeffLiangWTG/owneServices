using System;
using System.Net;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost.Tests;

class AccountingControllerTest : TestCaseWithFactory
{
	public void TestGetControllerID_Shipment_Success()
	{
		var jobShipment = Factory.NewWithValidTestData<ForwardingShipment>();
		Factory.Save();

		var result = GetController().GetControllerID(jobShipment.PK.ToGuid(), JobShipmentSchema.Constants.Prefix)
			.GetResult();

		Assert(result.GetStatusCode() == (int)HttpStatusCode.OK);
		AssertNotNull(result.GetContent(HttpStatusCode.OK));
		AssertEquals("JobShipment", result.GetValue(HttpStatusCode.OK));
	}

	public void TestGetControllerID_Consol_Success()
	{
		var jobConsol = Factory.NewWithValidTestData<ForwardingConsol>();
		Factory.Save();

		var result = GetController().GetControllerID(jobConsol.PK.ToGuid(), JobConsolSchema.Constants.Prefix)
			.GetResult();

		Assert(result.GetStatusCode() == (int)HttpStatusCode.OK);
		AssertNotNull(result.GetContent(HttpStatusCode.OK));
		AssertEquals("JobConsol", result.GetValue(HttpStatusCode.OK));
	}

	public void TestGetControllerID_NoJob_Returned()
	{
		var invalidPK = Guid.NewGuid();

		AssertExceptionThrown<NullReferenceException>(() =>
		{
			var result = GetController().GetControllerID(invalidPK, JobShipmentSchema.Constants.Prefix)
				.GetResult();
		});
	}

	AccountingController GetController() => ControllerHelper.GetController<AccountingController>();
}
