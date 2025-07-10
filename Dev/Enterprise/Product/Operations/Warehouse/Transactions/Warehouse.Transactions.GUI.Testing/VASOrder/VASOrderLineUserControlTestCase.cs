using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI.VAS_Order.Testing
{
	class VASOrderLineUserControlTestCase : WhsGuiTestCaseWithFactory
	{
		#region TestSerialNumberColumnInitialised

		public void TestSerialNumberColumnInitialised()
		{
			var whs = Helper.CreateWarehouse("1");
			var org1 = Helper.CreateClient("TESTCO1", "TESTCO1");
			var vasOrder = Helper.CreateWhsVASOrder(whs.Areas[0], org1);

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var userControl = GUITestHelper.FindControl<VASOrderLineGridUserControl>(form.Controls, "VASOrderLineGridUserControl");
				var linesGrid = GUITestHelper.FindControl<ZGrid>(userControl.Controls, "VASOrderLineGrid");
				AssertEquals(false,
					linesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == WhsVASOrderLineSchema.Constants.WVL_SerialNumber).IsUnavailable);
			}
		}

		#endregion

		#region TestClientChanged

		public void TestClientChanged()
		{
			var whs = Helper.CreateWarehouse("1");

			var org1 = Helper.CreateClient("TESTCO1", "TESTCO1");
			var org2 = Helper.CreateClient("TESTCO2", "TESTCO2");
			var org3 = Helper.CreateClient("TESTCO3", "TESTCO3");

			org2.MiscServ.OM_IMPartAttrib1Name = "Batch #";
			org2.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			org2.MiscServ.OM_IMPartAttrib2Name = "Vehicle #";
			org2.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			org2.MiscServ.OM_IMPartAttrib3Name = "Colour";
			org2.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			org2.MiscServ.OM_IMUseExpiryDate = true;
			org2.MiscServ.OM_IMUsePackingDate = true;

			org3.MiscServ.OM_IMPartAttrib2Name = "Speed #";
			org3.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			org3.MiscServ.OM_IMUsePackingDate = true;

			var vasOrder = Helper.CreateWhsVASOrder(whs.Areas[0], org1);

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var userControl = GUITestHelper.FindControl<VASOrderLineGridUserControl>(form.Controls, "VASOrderLineGridUserControl");
				var linesGrid = GUITestHelper.FindControl<ZGrid>(userControl.Controls, "VASOrderLineGrid");

				AssertAttributeVisibility(linesGrid, expiry: false, packing: false, part1: false, part2: false, part3: false);

				vasOrder.WVO_OH_Client = org2.PK;
				AssertAttributeVisibility(linesGrid, expiry: true, packing: true, part1: true, part2: true, part3: true);
				AssertAttributeTitles(linesGrid, "Batch #", "Vehicle #", "Colour");

				vasOrder.WVO_OH_Client = org3.PK;
				AssertAttributeVisibility(linesGrid, expiry: false, packing: true, part1: false, part2: true, part3: false);
				AssertAttributeTitles(linesGrid, "Part Attrib. 1", "Speed #", "Part Attrib. 3");
			}
		}

		void AssertAttributeVisibility(ZGrid grid, bool expiry, bool packing, bool part1, bool part2, bool part3)
		{
			AssertEquals("Expiry visibility incorrect", expiry, grid.Columns[WhsVASOrderLineSchema.WVL_ExpiryDate.Name].IsVisible);
			AssertEquals("Packing visibility incorrect", packing, grid.Columns[WhsVASOrderLineSchema.WVL_PackingDate.Name].IsVisible);
			AssertEquals("PartAttrib1 visibility incorrect", part1, grid.Columns[WhsVASOrderLineSchema.WVL_PartAttrib1.Name].IsVisible);
			AssertEquals("PartAttrib2 visibility incorrect", part2, grid.Columns[WhsVASOrderLineSchema.WVL_PartAttrib2.Name].IsVisible);
			AssertEquals("PartAttrib3 visibility incorrect", part3, grid.Columns[WhsVASOrderLineSchema.WVL_PartAttrib3.Name].IsVisible);
		}

		void AssertAttributeTitles(ZGrid grid, string part1, string part2, string part3)
		{
			AssertEquals("PartAttrib1 title incorrect", part1, grid.Columns[WhsVASOrderLineSchema.WVL_PartAttrib1.Name].ColumnStyle.HeaderText);
			AssertEquals("PartAttrib2 title incorrect", part2, grid.Columns[WhsVASOrderLineSchema.WVL_PartAttrib2.Name].ColumnStyle.HeaderText);
			AssertEquals("PartAttrib3 title incorrect", part3, grid.Columns[WhsVASOrderLineSchema.WVL_PartAttrib3.Name].ColumnStyle.HeaderText);
		}

		#endregion

		#region TestFetchForView

		public void TestFetchForView()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var today = ZDate.Today;

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);

			for (int j = 0; j < 10; j++)
			{
				var part = Helper.CreateProduct("PR" + j, data.Org1);
				var line = Helper.CreateWhsVASOrderLine(vasOrder, part, 1m);

				line.WVL_PackingDate = today;
				line.WVL_ExpiryDate = today.AddDays(30);
				line.WVL_PartAttrib1 = "PA-1";
				line.WVL_PartAttrib2 = "PA-2";
				line.WVL_PartAttrib3 = "PA-3";
			}

			Factory.Save();

			var dbHits = new Dictionary<string, int>();
			dbHits.Add(OrgAddressSchema.Constants.TableName, 0);
			dbHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			dbHits.Add(OrgMiscServSchema.Constants.TableName, 1);
			dbHits.Add(WhsVASOrderSchema.Constants.TableName, 1);
			dbHits.Add(WhsVASOrderLineSchema.Constants.TableName, 1);
			dbHits.Add(OrgSupplierPartSchema.Constants.TableName, 2);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			using (var control = new VASOrderLineGridUserControl())
			using (var form = new ZForm(newFactory.Load<WhsVASOrder>(vasOrder.PK)))
			{
				form.Controls.Add(control);
				var linesGrid = GUITestHelper.FindControl<ZGrid>(control.Controls, "VASOrderLineGrid");
				linesGrid.SetAllColumnsVisible(true);

				form.Show();

				// scroll across the grid, shouldnt be needed now but adding just in case
				int horizontalScrollPosition = 0;
				linesGrid.HorizontalScrollToOffset(0);
				while (horizontalScrollPosition < linesGrid.HorizontalScrollBarMaximum)
				{
					int offset = Math.Min(linesGrid.HorizontalScrollBarMaximum - horizontalScrollPosition, linesGrid.ClientRectangle.Width);
					horizontalScrollPosition += offset;
					linesGrid.HorizontalScrollToOffset(horizontalScrollPosition);
					Application.DoEvents();
				}

				AssertDbHits(dbHits, newFactory);
			}
		}

		#endregion
	}
}
