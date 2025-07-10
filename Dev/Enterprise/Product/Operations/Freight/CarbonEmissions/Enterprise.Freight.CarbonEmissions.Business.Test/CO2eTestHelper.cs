using System;
using CargoWise.Application;
using CargoWise.EventReference;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing;

public static class CO2eTestHelper
{
	public static string CO2eStaleWarning = "Greenhouse gas emissions recalculation is required because the input data has changed.";

	public static void AssertSTUEvent(IStmALogParent bizo, string reason, string oldValue = CO2eStatusList.Codes.Current, string newValue = CO2eStatusList.Codes.NotCurrent)
	{
		var stu = bizo.Logs.MostRecentLogByEventTime(AutoEvents.StatusUpdated);
		NUnit.Framework.Assertion.AssertEquals("TYP parameter", "CO2e Status", stu.Parameters[Constants.EventReferenceParameters.Codes.Type]);
		NUnit.Framework.Assertion.AssertEquals("RES parameter", $"Input value(s) have changed: {reason}", stu.Parameters[Constants.EventReferenceParameters.Codes.Reason]);
		NUnit.Framework.Assertion.AssertEquals("NEW parameter", newValue, stu.Parameters[Constants.EventReferenceParameters.Codes.New]);
		NUnit.Framework.Assertion.AssertEquals("OLD parameter", oldValue, stu.Parameters[Constants.EventReferenceParameters.Codes.Old]);
	}

	public static IDisposable MockCO2eFeatureControl(bool enabled = true)
	{
		var mock = new Mock<ICO2eFeatureControlHelper>();
		mock.Setup(m => m.Enabled).Returns(enabled);
		return ObjectFactory.Substitute(mock.Object);
	}
}
