using System.Windows.Forms;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	[TestedType(typeof(CreateInBondForConsolForm))]
	sealed class CreateInBondForConsolFormTest : ZFormBasherTest
	{
		public void TestAddButton_Click()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00000001";
			shipment1.JS_HouseBill = "HB0000001";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00000002";
			shipment2.JS_HouseBill = "HB0000002";
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00000003";
			shipment3.JS_HouseBill = "HB0000003";
			var wrapper = new CreateInBondForConsolWraper(consol);
			wrapper.NewMovementHeaders.AddNew();
			using (var form = new CreateInBondForConsolForm(wrapper))
			{
				form.Show();
				AssertEquals(wrapper.ShipmentsWithoutInBond.Count, 3);
				AssertEquals(wrapper.NewMovementHeaders.Count, 2);
				AssertEquals(wrapper.NewMovementHeaders[0].AllocatedShipmentsForMovement.Count, 0);
				AssertEquals(wrapper.NewMovementHeaders[1].AllocatedShipmentsForMovement.Count, 0);

				var shipmentsWithoutInBondGrid = form.FindSingleOrDefault<ZArchitecture.ZGrid>("ShipmentsForInBondGrid");
				var newMovementHeadersGrid = form.FindSingleOrDefault<ZArchitecture.ZGrid>("NewMovementHeadersGrid");
				var addButton = form.FindSingleOrDefault<ZButton>("AddButton");
				addButton.PerformClick();
				AssertEquals(wrapper.ShipmentsWithoutInBond.Count, 2);
				AssertEquals(wrapper.NewMovementHeaders[0].AllocatedShipmentsForMovement.Count, 1);
				AssertEquals(wrapper.NewMovementHeaders[0].AllocatedShipmentsForMovement[0].ShipmentNumber, "S00000001");
				AssertEquals(wrapper.NewMovementHeaders[1].AllocatedShipmentsForMovement.Count, 0);

				shipmentsWithoutInBondGrid.CurrentRowIndex = 1;
				newMovementHeadersGrid.CurrentRowIndex = 1;
				addButton.PerformClick();
				AssertEquals(wrapper.ShipmentsWithoutInBond.Count, 1);
				AssertEquals(wrapper.NewMovementHeaders[0].AllocatedShipmentsForMovement.Count, 1);
				AssertEquals(wrapper.NewMovementHeaders[1].AllocatedShipmentsForMovement.Count, 1);
				AssertEquals(wrapper.NewMovementHeaders[1].AllocatedShipmentsForMovement[0].ShipmentNumber, "S00000003");
			}
		}

		public void TestRemoveButton_Click()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00000001";
			shipment1.JS_HouseBill = "HB0000001";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00000002";
			shipment2.JS_HouseBill = "HB0000002";
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00000003";
			shipment3.JS_HouseBill = "HB0000003";
			var wrapper = new CreateInBondForConsolWraper(consol);
			using (var form = new CreateInBondForConsolForm(wrapper))
			{
				form.Show();
				AssertEquals(wrapper.ShipmentsWithoutInBond.Count, 3);
				AssertEquals(wrapper.NewMovementHeaders.Count, 1);
				AssertEquals(wrapper.NewMovementHeaders[0].AllocatedShipmentsForMovement.Count, 0);

				var addButton = form.FindSingleOrDefault<ZButton>("AddButton");
				var removeButton = form.FindSingleOrDefault<ZButton>("RemoveButton");
				addButton.PerformClick();
				addButton.PerformClick();
				addButton.PerformClick();
				AssertEquals(wrapper.ShipmentsWithoutInBond.Count, 0);
				AssertEquals(wrapper.NewMovementHeaders[0].AllocatedShipmentsForMovement.Count, 3);

				removeButton.PerformClick();
				AssertEquals(wrapper.ShipmentsWithoutInBond.Count, 1);
				AssertEquals(wrapper.NewMovementHeaders[0].AllocatedShipmentsForMovement.Count, 2);

				removeButton.PerformClick();
				AssertEquals(wrapper.ShipmentsWithoutInBond.Count, 2);
				AssertEquals(wrapper.NewMovementHeaders[0].AllocatedShipmentsForMovement.Count, 1);

				removeButton.PerformClick();
				AssertEquals(wrapper.ShipmentsWithoutInBond.Count, 3);
				AssertEquals(wrapper.NewMovementHeaders[0].AllocatedShipmentsForMovement.Count, 0);
			}
		}

		public void TestAllocatedShipmentsForMovementGroupBoxCaption()
		{
			var consol = Factory.New<ForwardingConsol>();
			var wrapper = new CreateInBondForConsolWraper(consol);
			wrapper.NewMovementHeaders.AddNew();
			wrapper.NewMovementHeaders.AddNew();
			using (var form = new CreateInBondForConsolForm(wrapper))
			{
				form.Show();
				var allocatedShipmentsForMovementGroupBox = form.FindSingleOrDefault<ZGroupBox>("AllocatedShipmentsForMovementGroupBox");
				var newMovementHeadersGrid = form.FindSingleOrDefault<ZArchitecture.ZGrid>("NewMovementHeadersGrid");
				AssertEquals(allocatedShipmentsForMovementGroupBox.Text, "Allocated Shipments for Movement (NOT YET SPECIFIED 1)");

				newMovementHeadersGrid.CurrentRowIndex = 1;
				AssertEquals(allocatedShipmentsForMovementGroupBox.Text, "Allocated Shipments for Movement (NOT YET SPECIFIED 2)");

				newMovementHeadersGrid.CurrentRowIndex = 2;
				AssertEquals(allocatedShipmentsForMovementGroupBox.Text, "Allocated Shipments for Movement (NOT YET SPECIFIED 3)");
			}
		}

		protected override Form GetFormToBashCore()
		{
			var consol = Factory.New<ForwardingConsol>();
			var wrapper = new CreateInBondForConsolWraper(consol);
			return new CreateInBondForConsolForm(wrapper);
		}
	}
}
