using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	internal sealed class HyperlinkExtensionsTest : TestCaseWithFactory
	{
		public void TestForwardingConsolHyperlink()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001023";
			var link = (LogControllerLink)consol.Hyperlink();

			AssertEquals("C00001023", link.Text);
			AssertEquals(ControllerIDs.JobConsol, link.Controller);
			AssertEquals(consol.PK, link.PK);
		}

		public void TestForwardingShipmentHyperlink()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001023";
			var link = (LogControllerLink)shipment.Hyperlink();

			AssertEquals("S00001023", link.Text);
			AssertEquals(ControllerIDs.JobShipment, link.Controller);
			AssertEquals(shipment.PK, link.PK);
		}
	}
}
