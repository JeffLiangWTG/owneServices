using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ForwardingContainersPackingPlugInTest : ContainersPackingPlugInTest
	{
		public void TestMergeContextMenuItem()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			using (var form = CreateForm(consol))
			{
				form.Show();
				form.SelectContainersTab();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var grid = form.UnAllocatedPackLinesGrid;
				grid.SelectAllElements();

				grid.ContextMenu.MenuItems[4].PerformClick();

				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertCollectionContains(packLine1, shipment.OuterPackLines);
				AssertCollectionNotContains(packLine2, shipment.OuterPackLines);
			}
		}

		[RequiresSTA]
		public void TestMergeAllContextMenuItem()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			var packLine1 = shipment1.OuterPackLines.AddNew();
			var packLine2 = shipment1.OuterPackLines.AddNew();

			var shipment2 = consol.Shipments.AddNew();
			var packLine3 = shipment2.OuterPackLines.AddNew();
			var packLine4 = shipment2.OuterPackLines.AddNew();

			var shipment3 = consol.Shipments.AddNew();
			var packLine5 = shipment3.OuterPackLines.AddNew();
			var packLine6 = shipment3.OuterPackLines.AddNew();
			packLine6.JL_Description = "Test";

			using (var form = CreateForm(consol))
			{
				form.Show();
				form.SelectContainersTab();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UnAllocatedPackLinesGrid.ContextMenu.MenuItems[5].PerformClick();

				AssertEquals(1, shipment1.OuterPackLines.Count);
				AssertCollectionContains(packLine1, shipment1.OuterPackLines);
				AssertCollectionNotContains(packLine2, shipment1.OuterPackLines);

				AssertEquals(1, shipment2.OuterPackLines.Count);
				AssertCollectionContains(packLine3, shipment2.OuterPackLines);
				AssertCollectionNotContains(packLine4, shipment2.OuterPackLines);

				AssertEquals(2, shipment3.OuterPackLines.Count);
				AssertCollectionContains(packLine5, shipment3.OuterPackLines);
				AssertCollectionContains(packLine6, shipment3.OuterPackLines);
			}
		}

		[RequiresSTA]
		public void TestSplitContextMenuItem()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 200m;
			shipment.JS_ActualVolume = 100m;
			shipment.JS_OuterPacks = 3;

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 2;

			using (HelperForm form = CreateForm(consol))
			{
				form.Show();
				form.SelectContainersTab();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ContainersModuleButtonGrid.InnerGrid.ContextMenu.MenuItems[0].PerformClick();

				AssertEquals(2, consol.Containers.Count);
				AssertCollectionNotContains(multiContainer, consol.Containers);
			}
		}

		protected override HelperForm CreateForm(CommonConsol consol)
		{
			return new HelperForm(consol, ControllerIDs.JobConsolContainersPacking);
		}

		protected override CommonConsol CreateConsol()
		{
			return Factory.New<ForwardingConsol>();
		}
	}
}
