using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class ShipmentItemSelectionDialogTest : TestCaseWithFactory
	{
		public void TestSendManifest_SelectedItemTextOnDialog()
		{
			var header = Factory.NewWithValidTestData<Trip>();
			var shipment1 = header.Shipments.AddNew();
			shipment1.B0_ReferenceID = "Shipment1";
			var shipment2 = header.Shipments.AddNew();
			shipment2.B0_ReferenceID = "Shipment2";
			var shipment3 = header.Shipments.AddNew();
			shipment3.B0_ReferenceID = "Shipment3";
			var shipment4 = header.Shipments.AddNew();
			shipment4.B0_ReferenceID = "Shipment4";
			using (var dlg = new ShipmentItemSelectionDialog(header, new[] { shipment1, shipment2, shipment3, shipment4 }))
			{
				SelectOnlyShipmentNodes(dlg, bizoPK => bizoPK == shipment1.PK);
				AssertEquals("1 Selected", "1 of 4 Shipment(s) selected.", dlg.SeletedItem);
				SelectOnlyShipmentNodes(dlg, bizoPK => (bizoPK == shipment2.PK || bizoPK == shipment1.PK));
				AssertEquals("2 Selected", "2 of 4 Shipment(s) selected.", dlg.SeletedItem);
			}
		}

		void SelectOnlyShipmentNodes(ShipmentItemSelectionDialog dlg, Func<ZGuid, bool> shouldCheck)
		{
			foreach (var node in dlg.ShipmentTreeViewInternal.Nodes[0].Nodes.OfType<ZBusinessObjectTreeNode>())
			{
				node.Checked = shouldCheck(node.BizO.PK);
			}
		}
	}
}
