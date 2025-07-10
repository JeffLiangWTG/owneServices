using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ForwardingConsolContainerSplitterTest : TestCaseWithFactory
	{
		public void TestNoContainer()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			ForwardingConsolContainerSplitter splitter = new ForwardingConsolContainerSplitter();
			splitter.Split(null);

			AssertEquals("Please select container", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDoNotAskUserIfContainerHasNoShipmentsPacked()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			AssertEquals("Prerequisite", 1, consol.Containers.Count);

			ForwardingConsolContainerSplitter splitter = new ForwardingConsolContainerSplitter();
			splitter.Split(multiContainer);

			Assert("Should not show message if there's no shipments", UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals(3, consol.Containers.Count);
		}

		public void TestGetSplitMethodFromUserMessage()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 2;

			AssertEquals("Prerequisite", 1, consol.Containers.Count);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ForwardingConsolContainerSplitter splitter = new ForwardingConsolContainerSplitter();
			splitter.Split(multiContainer);

			AssertEquals("These containers have shipment/s packed into them, would you like to pack the shipment/s to each of these containers?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(3, consol.Containers.Count);
		}

		public void TestPackAllInFirstContainer()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 10;

			AssertEquals("Prerequisite", 1, consol.Containers.Count);
			AssertEquals("Prerequisite", 1, shipment.OuterPackLines.Count);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			ForwardingConsolContainerSplitter splitter = new ForwardingConsolContainerSplitter();
			splitter.Split(multiContainer);

			AssertEquals(3, consol.Containers.Count);
			AssertCollectionContains(multiContainer, consol.Containers);
			AssertEquals(3, shipment.OuterPackLines.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment.OuterPackLines[0] }, consol.Containers[0].PackLines);
			AssertContainsExactElementsInAnyOrder(new[] { shipment.OuterPackLines[1] }, consol.Containers[1].PackLines);
			AssertContainsExactElementsInAnyOrder(new[] { shipment.OuterPackLines[2] }, consol.Containers[2].PackLines);
			AssertArrayEqualsByElements(new ZInt[] { 10, 0, 0 }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
		}

		public void TestPackEvenly()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 10;

			AssertEquals("Prerequisite", 1, consol.Containers.Count);
			AssertEquals("Prerequisite", 1, shipment.OuterPackLines.Count);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ForwardingConsolContainerSplitter splitter = new ForwardingConsolContainerSplitter();
			splitter.Split(multiContainer);

			AssertEquals(3, consol.Containers.Count);
			AssertCollectionNotContains(multiContainer, consol.Containers);
			AssertEquals(3, shipment.OuterPackLines.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment.OuterPackLines[0] }, consol.Containers[0].PackLines);
			AssertContainsExactElementsInAnyOrder(new[] { shipment.OuterPackLines[1] }, consol.Containers[1].PackLines);
			AssertContainsExactElementsInAnyOrder(new[] { shipment.OuterPackLines[2] }, consol.Containers[2].PackLines);
			AssertEquals(4, shipment.OuterPackLines[0].JL_PackageCount);
			AssertEquals(3, shipment.OuterPackLines[1].JL_PackageCount);
			AssertEquals(3, shipment.OuterPackLines[2].JL_PackageCount);
		}

		public void TestUserCancelled()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 10;

			AssertEquals("Prerequisite", 1, consol.Containers.Count);
			AssertEquals("Prerequisite", 1, shipment.OuterPackLines.Count);

			ForwardingPackLine packLine = shipment.OuterPackLines[0];

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			ForwardingConsolContainerSplitter splitter = new ForwardingConsolContainerSplitter();
			splitter.Split(multiContainer);

			AssertContainsExactElementsInAnyOrder(new[] { multiContainer }, consol.Containers);
			AssertContainsExactElementsInAnyOrder(new[] { packLine }, shipment.OuterPackLines);
		}
	}
}
