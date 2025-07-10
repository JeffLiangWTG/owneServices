using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

class BrokeragePlugInTest : EU.GUI.Testing.BrokeragePlugInTest
{
	protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);

	protected override Type ExpectedTopLevelMenuType => typeof(EDIMenu);

	public override void TestClearHasChanges()
	{
		MakeThisDeclarationPackingRelevant(JobDeclaration);
		JobDeclaration.JE_MessageType = "IMP";
		JobDeclaration.JE_OverrideFreightDefaults = false;
		MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(JobDeclaration.MergeManager);
		PackLine packLine = Shipment.OuterPackLines.AddNew();
		packLine.JL_PackageCount = 10;
		Shipment.JS_HouseBill = "HBL1";
		Shipment.JS_INCO = "FOB";
		if (JobDeclaration.Forwarder != null)
		{
			JobDeclaration.Forwarder.OH_ScreeningStatus = "NOT";
		}
		base.Factory.Save();
		using (GetPlugInToTest() as BaseBrokeragePlugIn)
		{
			Assertion.AssertEquals(expected: false, JobDeclaration.HasChanges);
		}
	}
}
