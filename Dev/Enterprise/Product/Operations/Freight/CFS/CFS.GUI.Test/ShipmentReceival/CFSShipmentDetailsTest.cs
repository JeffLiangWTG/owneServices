using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class CFSShipmentDetailsTest : BaseFreightTest
	{
		public void TestCoLoadCheckBoxChanged()
		{
			using (ShipmentReceivalForm shipmentForm = new ShipmentReceivalForm(Shipment))
			{
				shipmentForm.Show();
				Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
				AssertEquals("Consignor Label should be Sending Forwarder", "Sending Forwarder", ((ZDocAddressControl)GetControl(shipmentForm.ShipmentDetails, "ConsignorDocumentaryDocAddressControl")).Text);
				AssertEquals("Consignee Label should be Receiving Forwarder", "Receiving Forwarder", ((ZDocAddressControl)GetControl(shipmentForm.ShipmentDetails, "ConsigneeDocumentaryDocAddressControl")).Text);

				Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
				AssertEquals("Consignor Label should be Consignor", "Consignor", ((ZDocAddressControl)GetControl(shipmentForm.ShipmentDetails, "ConsignorDocumentaryDocAddressControl")).Text);
				AssertEquals("Consignee Label should be Consignee", "Consignee", ((ZDocAddressControl)GetControl(shipmentForm.ShipmentDetails, "ConsigneeDocumentaryDocAddressControl")).Text);
			}
		}

		public void TestBlindCoLoadCheckBoxChanged()
		{
			using (ShipmentReceivalForm shipmentForm = new ShipmentReceivalForm(Shipment))
			{
				shipmentForm.Show();
				Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BlindCoLoadMaster;
				AssertEquals("Consignor Label should be Sending Forwarder", "Sending Forwarder", ((ZDocAddressControl)GetControl(shipmentForm.ShipmentDetails, "ConsignorDocumentaryDocAddressControl")).Text);
				AssertEquals("Consignee Label should be Receiving Forwarder", "Receiving Forwarder", ((ZDocAddressControl)GetControl(shipmentForm.ShipmentDetails, "ConsigneeDocumentaryDocAddressControl")).Text);

				Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
				AssertEquals("Consignor Label should be Consignor", "Consignor", ((ZDocAddressControl)GetControl(shipmentForm.ShipmentDetails, "ConsignorDocumentaryDocAddressControl")).Text);
				AssertEquals("Consignee Label should be Consignee", "Consignee", ((ZDocAddressControl)GetControl(shipmentForm.ShipmentDetails, "ConsigneeDocumentaryDocAddressControl")).Text);
			}
		}

		public void TestControlVisibility()
		{
			bool initialValue = Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value;
			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (ShipmentReceivalForm form = new ShipmentReceivalForm(Shipment))
			{
				ShipmentDetails detailsControl = form.ShipmentDetails;
				form.Show();

				detailsControl.ShipmentServicesTabControl.SelectedTab = (ZTabPage)GetControl(detailsControl, "ShipmentTabPage");
				Assert("Control should be visible", detailsControl.WarehouseDropEdit.Visible);
				Assert("Control should be visible", GetControl(detailsControl, "LocationTextBox").Visible);
				Assert("Control should not be visible", !GetControl(detailsControl, "WarehouseTextBox").Visible);
			}

			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (ShipmentReceivalForm form = new ShipmentReceivalForm(Shipment))
			{
				ShipmentDetails detailsControl = form.ShipmentDetails;
				form.Show();

				detailsControl.ShipmentServicesTabControl.SelectedTab = (ZTabPage)GetControl(detailsControl, "ShipmentTabPage");
				Assert("Control should not be visible", !detailsControl.WarehouseDropEdit.Visible);
				Assert("Control should not be visible", !GetControl(detailsControl, "LocationTextBox").Visible);
				Assert("Control should be visible", GetControl(detailsControl, "WarehouseTextBox").Visible);
			}

			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, initialValue);
		}

		public void TestColumnsStyles()
		{
			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (ShipmentDetails control = new ShipmentDetails())
			{
				Assert("Column LocationWhsGuid should be in grid", CheckColumnIsInGrid((ZGrid)GetControl(control, "LocationsGrid"), "LocationWhsGuid"));
				Assert("Column LocationString should be in grid", CheckColumnIsInGrid((ZGrid)GetControl(control, "LocationsGrid"), "LocationString"));
				Assert("Column JQ_WarehouseLocation should not be in grid", !CheckColumnIsInGrid((ZGrid)GetControl(control, "LocationsGrid"), PackLocation.Schema.JQ_WarehouseLocation));
			}

			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (ShipmentDetails control = new ShipmentDetails())
			{
				Assert("Column LocationWhsGuid should not be in grid", !CheckColumnIsInGrid((ZGrid)GetControl(control, "LocationsGrid"), "LocationWhsGuid"));
				Assert("Column LocationString should not be in grid", !CheckColumnIsInGrid((ZGrid)GetControl(control, "LocationsGrid"), "LocationString"));
				Assert("Column JQ_WarehouseLocation should not be in grid", CheckColumnIsInGrid((ZGrid)GetControl(control, "LocationsGrid"), PackLocation.Schema.JQ_WarehouseLocation));
			}
		}

		public void TestMasterChanged()
		{
			var oldMaster = Factory.New<CFSShipment>();
			var subShipment = Factory.New<CFSShipment>();
			subShipment.JS_JS_ColoadMasterShipment = oldMaster.PK;

			var newMaster = Factory.New<CFSShipment>();
			Factory.Save();

			using (var form = new ShipmentReceivalForm(subShipment))
			{
				form.Show();

				var helper = new Mock<IShipmentVsConsolGUIMessageHelper>();

				using (ShipmentVsConsolGUIMessageHelper.OverrideHelperInstance(helper.Object))
				{
					helper.Setup(m => m.OnShipmentMasterChanged(CFSShipmentVsConsolMessageHelper.Instance, subShipment, It.IsAny<CommonConsol>(), It.Is<MasterChangedEventArgs>(x => x.OldMasterPK == oldMaster.PK && x.NewMasterPK == newMaster.PK)))
						.Callback(() => Assert(true));
					subShipment.JS_JS_ColoadMasterShipment = newMaster.PK;
				}
			}
		}

		public void TestAddtionalTabVisibility()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			using (ShipmentReceivalForm form = new ShipmentReceivalForm(Shipment))
			{
				ShipmentDetails detailsControl = form.ShipmentDetails;
				form.Show();

				Assert("Reference Numbers Tab Page should not be visible", !((ZTabPage)GetControl(detailsControl, "NumbersTabPage")).TabVisible);
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);

			using (ShipmentReceivalForm form = new ShipmentReceivalForm(Shipment))
			{
				ShipmentDetails detailsControl = form.ShipmentDetails;
				form.Show();

				Assert("Reference Numbers Tab Page should be visible", ((ZTabPage)GetControl(detailsControl, "NumbersTabPage")).TabVisible);
			}
		}

		#region Implementation

		Control GetControl(ShipmentDetails control, string name)
		{
			return (Control)typeof(ShipmentDetails).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(control);
		}

		CFSShipment fShipment;
		CFSShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					SetupLocalBranchAsLocalCartage();
					fShipment = (CFSShipment)GetImportShipment(typeof(CFSShipment));
					Shipment.Consols.AddNew();
					Shipment.Consols[0].JK_OA_UnpackDepotAddress = LocalDepot.MainAddress.PK;
					Factory.Save();
				}
				return fShipment;
			}
		}

		bool CheckColumnIsInGrid(ZGrid grid, string columnName)
		{
			bool result = false;
			foreach (ZGridColumnInfo info in grid.ColumnStyles)//.ToArray())
			{
				if (info.ColumnName == columnName)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
