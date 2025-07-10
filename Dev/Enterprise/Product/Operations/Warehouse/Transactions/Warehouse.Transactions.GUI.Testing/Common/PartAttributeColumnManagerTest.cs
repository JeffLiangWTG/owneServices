using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class PartAttributeColumnManagerTest : WhsTestCaseWithFactory
	{
		#region Constructor

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorThrowsExceptionIfGridNull()
		{
			PartAttributeColumnManager manager = new PartAttributeColumnManager(null, "", "", "", "", "", "");
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor2ThrowsExceptionIfGridNull()
		{
			PartAttributeColumnManager manager = new PartAttributeColumnManager(null, "", "", "", "", "", "", "", "", "", "");
		}

		#endregion

		#region Column Control

		#region TestSetColumnsForOneClient

		public void TestSetColumnsForOneClient() => TestSetColumnsForOneClient(withIsExpired: false);
		public void TestSetColumnsForOneClient_WithIsExpired() => TestSetColumnsForOneClient(withIsExpired: true);

		void TestSetColumnsForOneClient(bool withIsExpired)
		{
			var whs = Helper.CreateWarehouse("1");
			var org1 = Helper.CreateClient("TESTCO1");
			var org2 = Helper.CreateClient("TESTCO2");
			var org3 = Helper.CreateClient("TESTCO3");

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

			using (var form = new TestForm(Factory.New<WhsReceive>()))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var manager = new PartAttributeColumnManager(
					grid, WhsDocketLineSchema.WE_ExpiryDate.Name, WhsDocketLineSchema.WE_PackingDate.Name,
					WhsDocketLineSchema.WE_PartAttrib1.Name, WhsDocketLineSchema.WE_PartAttrib2.Name, WhsDocketLineSchema.WE_PartAttrib3.Name, WhsDocketLineSchema.WE_SerialNumber.Name);

				if (withIsExpired)
				{
					grid.Columns.AddTextColumn(WhsInventoryView.Schema.IsExpired, 65, false, false, false);
					manager = new PartAttributeColumnManager(
						grid, WhsDocketLineSchema.WE_ExpiryDate.Name, WhsDocketLineSchema.WE_PackingDate.Name,
						WhsDocketLineSchema.WE_PartAttrib1.Name, WhsDocketLineSchema.WE_PartAttrib2.Name, WhsDocketLineSchema.WE_PartAttrib3.Name, WhsDocketLineSchema.WE_SerialNumber.Name,
						WhsInventoryView.Schema.IsExpired);
				}

				manager.SetColumns(org1);
				AssertAttributeVisibility(grid, false, false, false, false, false, false, withIsExpired);

				manager.SetColumns(org2);
				AssertAttributeVisibility(grid, true, true, true, true, true, true, withIsExpired);
				AssertAttributeTitles(grid, "Batch #", "Vehicle #", "Colour");

				manager.SetColumns(org3);
				AssertAttributeVisibility(grid, false, true, false, true, false, false, withIsExpired);
				AssertAttributeTitles(grid, "", "Speed #", "");
			}
		}

		public void TestSetColumnsForOneClient_SerialNumberUsed()
		{
			TestSetColumnsForOneClient_SerialNumberCore(true);
		}

		public void TestSetColumnsForOneClient_SerialNumberNotUsed()
		{
			TestSetColumnsForOneClient_SerialNumberCore(false);
		}

		void TestSetColumnsForOneClient_SerialNumberCore(bool serialNumberUsed)
		{
			var org = Helper.CreateClient("TESTCO");
			org.MiscServ.OM_IMUseSerialNumber = serialNumberUsed;

			using (var form = new TestForm(Factory.New<WhsReceive>()))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var manager = new PartAttributeColumnManager(
					grid, WhsDocketLineSchema.WE_ExpiryDate.Name, WhsDocketLineSchema.WE_PackingDate.Name,
					WhsDocketLineSchema.WE_PartAttrib1.Name, WhsDocketLineSchema.WE_PartAttrib2.Name, WhsDocketLineSchema.WE_PartAttrib3.Name, WhsDocketLineSchema.WE_SerialNumber.Name);

				manager.SetColumns(org);
				AssertEquals("Serial Number visibility incorrect", serialNumberUsed, grid.Columns[WhsDocketLineSchema.WE_SerialNumber.Name].IsVisible);
			}
		}

		#endregion

		#region TestSetColumnsForManyClients

		public void TestSetColumnsForManyClients() => TestSetColumnsForManyClients(withIsExpired: false);
		public void TestSetColumnsForManyClients_WithIsExpired() => TestSetColumnsForManyClients(withIsExpired: true);

		void TestSetColumnsForManyClients(bool withIsExpired)
		{
			var org1 = Helper.CreateClient("TESTCO1");
			var org2 = Helper.CreateClient("TESTCO2");
			var org3 = Helper.CreateClient("TESTCO3");
			var org4 = Helper.CreateClient("TESTCO4");
			var org5 = Helper.CreateClient("TESTCO5");

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

			org4.MiscServ.OM_IMPartAttrib1Name = "Batch #";
			org4.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			org4.MiscServ.OM_IMPartAttrib2Name = "Vehicle #";
			org4.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			org4.MiscServ.OM_IMPartAttrib3Name = "Colour";
			org4.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;

			org5.MiscServ.OM_IMPartAttrib1Name = "ProdDate";
			org5.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			org5.MiscServ.OM_IMPartAttrib2Name = "PurDate";
			org5.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			org5.MiscServ.OM_IMPartAttrib3Name = "SellDate";
			org5.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;

			using (var form = new TestForm(Factory.New<WhsReceive>()))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var manager = new PartAttributeColumnManager(
					grid, WhsDocketLineSchema.WE_ExpiryDate.Name, WhsDocketLineSchema.WE_PackingDate.Name,
					WhsDocketLineSchema.WE_PartAttrib1.Name, WhsDocketLineSchema.WE_PartAttrib2.Name, WhsDocketLineSchema.WE_PartAttrib3.Name, WhsDocketLineSchema.WE_SerialNumber.Name);
				if (withIsExpired)
				{
					grid.Columns.AddTextColumn(WhsInventoryView.Schema.IsExpired, 65, false, false, false);
					manager = new PartAttributeColumnManager(
						grid, WhsDocketLineSchema.WE_ExpiryDate.Name, WhsDocketLineSchema.WE_PackingDate.Name,
						WhsDocketLineSchema.WE_PartAttrib1.Name, WhsDocketLineSchema.WE_PartAttrib2.Name, WhsDocketLineSchema.WE_PartAttrib3.Name, WhsDocketLineSchema.WE_SerialNumber.Name,
						WhsInventoryView.Schema.IsExpired);
				}

				var clients = new List<OrgHeader>();
				clients.Add(org1);
				manager.SetColumns(clients);
				AssertAttributeVisibility(grid, false, false, false, false, false, false, withIsExpired);

				clients.Add(org2);
				manager.SetColumns(clients);
				AssertAttributeVisibility(grid, true, true, true, true, true, true, withIsExpired);
				AssertAttributeTitles(grid, "Batch #", "Vehicle #", "Colour");

				clients.Add(org3);
				manager.SetColumns(clients);
				AssertAttributeVisibility(grid, true, true, true, true, true, true, withIsExpired);
				AssertAttributeTitles(grid, "Batch #", "Vehicle # / Speed #", "Colour");

				clients.Add(org4);
				manager.SetColumns(clients);
				AssertAttributeVisibility(grid, true, true, true, true, true, true, withIsExpired);
				AssertAttributeTitles(grid, "Batch #", "Vehicle # / Speed #", "Colour");

				clients.Add(org5);
				manager.SetColumns(clients);
				AssertAttributeVisibility(grid, true, true, true, true, true, true, withIsExpired);
				AssertAttributeTitles(grid, "Batch # / ProdDate", "Vehicle # / Speed # / PurDate", "Colour / SellDate");

				clients.Remove(org2);
				manager.SetColumns(clients);
				AssertAttributeVisibility(grid, false, true, true, true, true, false, withIsExpired);
				AssertAttributeTitles(grid, "Batch # / ProdDate", "Speed # / Vehicle # / PurDate", "Colour / SellDate");

				clients.Remove(org4);
				manager.SetColumns(clients);
				AssertAttributeVisibility(grid, false, true, true, true, true, false, withIsExpired);
				AssertAttributeTitles(grid, "ProdDate", "Speed # / PurDate", "SellDate");

				clients.Remove(org5);
				manager.SetColumns(clients);
				AssertAttributeVisibility(grid, false, true, false, true, false, false, withIsExpired);
				AssertAttributeTitles(grid, "", "Speed #", "");

				clients.Remove(org3);
				manager.SetColumns(clients);
				AssertAttributeVisibility(grid, false, false, false, false, false, false, withIsExpired);
			}
		}

		public void TestSetColumnsForManyClients_SerialNumber()
		{
			var org1 = Helper.CreateClient("TESTCO1");
			var org2 = Helper.CreateClient("TESTCO2");
			var org3 = Helper.CreateClient("TESTCO3");

			org2.MiscServ.OM_IMUseSerialNumber = true;

			using (var form = new TestForm(Factory.New<WhsReceive>()))
			{
				form.Show();

				var grid = form.UserControl.LinesGrid;
				var manager = new PartAttributeColumnManager(
					grid, WhsDocketLineSchema.WE_ExpiryDate.Name, WhsDocketLineSchema.WE_PackingDate.Name,
					WhsDocketLineSchema.WE_PartAttrib1.Name, WhsDocketLineSchema.WE_PartAttrib2.Name, WhsDocketLineSchema.WE_PartAttrib3.Name, WhsDocketLineSchema.WE_SerialNumber.Name);

				var clients = new List<OrgHeader>();
				clients.Add(org1);
				manager.SetColumns(clients);
				AssertEquals("Serial Number visibility incorrect", false, grid.Columns[WhsDocketLineSchema.WE_SerialNumber.Name].IsVisible);

				clients.Add(org2);
				manager.SetColumns(clients);
				AssertEquals("Serial Number visibility incorrect", true, grid.Columns[WhsDocketLineSchema.WE_SerialNumber.Name].IsVisible);

				clients.Add(org3);
				manager.SetColumns(clients);
				AssertEquals("Serial Number visibility incorrect", true, grid.Columns[WhsDocketLineSchema.WE_SerialNumber.Name].IsVisible);
			}
		}

		#endregion

		#region TestSetColumn

		public void TestSetColumn()
		{
			var docket = Factory.New<WhsReceive>();
			using (var form = new TestForm(docket))
			{
				form.Show();
				var grid = form.UserControl.LinesGrid;
				var name = WhsDocketLineSchema.WE_ExpiryDate.Name;
				var manager = new PartAttributeColumnManager(grid, name, "", "", "", "", "");

				manager.SetColumn(name, "ICanSeeYou", true);
				AssertEquals(true, grid.Columns[name].IsVisible);
				AssertEquals("ICanSeeYou", grid.Columns[name].ColumnStyle.HeaderText);

				manager.SetColumn(name, "Hidden", false);
				AssertEquals(false, grid.Columns[name].IsVisible);

				AssertNoExceptionThrown(() => manager.SetColumn("", "", false));
			}
		}

		#endregion

		#endregion

		#region Implementation

		void AssertAttributeVisibility(ZGrid grid, bool expiryIsVisible, bool packingIsVisible,
			bool part1IsVisible, bool part2IsVisible, bool part3IsVisible, bool isExpiredVisible, bool withIsExpired)
		{
			AssertEquals("Expiry visibility incorrect", expiryIsVisible, grid.Columns[WhsDocketLineSchema.WE_ExpiryDate.Name].IsVisible);
			AssertEquals("Packing visibility incorrect", packingIsVisible, grid.Columns[WhsDocketLineSchema.WE_PackingDate.Name].IsVisible);
			AssertEquals("PartAttrib1 visibility incorrect", part1IsVisible, grid.Columns[WhsDocketLineSchema.WE_PartAttrib1.Name].IsVisible);
			AssertEquals("PartAttrib2 visibility incorrect", part2IsVisible, grid.Columns[WhsDocketLineSchema.WE_PartAttrib2.Name].IsVisible);
			AssertEquals("PartAttrib3 visibility incorrect", part3IsVisible, grid.Columns[WhsDocketLineSchema.WE_PartAttrib3.Name].IsVisible);
			if (withIsExpired)
			{
				AssertEquals("Is Expired visibility incorrect", isExpiredVisible, grid.Columns[WhsInventoryView.Schema.IsExpired].IsVisible);
			}
		}

		void AssertAttributeTitles(ZGrid grid, string part1ColumnName, string part2ColumnName, string part3ColumnName)
		{
			if (!string.IsNullOrEmpty(part1ColumnName))
			{
				AssertEquals("PartAttrib1 title incorrect", part1ColumnName, grid.Columns[WhsDocketLineSchema.WE_PartAttrib1.Name].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part2ColumnName))
			{
				AssertEquals("PartAttrib2 title incorrect", part2ColumnName, grid.Columns[WhsDocketLineSchema.WE_PartAttrib2.Name].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part3ColumnName))
			{
				AssertEquals("PartAttrib3 title incorrect", part3ColumnName, grid.Columns[WhsDocketLineSchema.WE_PartAttrib3.Name].ColumnStyle.HeaderText);
			}
		}

		public void TestSetColumnsForOneClient_WithRCAColumns()
		{
			var whs = Helper.CreateWarehouse("1");
			var org1 = Helper.CreateClient("TESTCO1");
			var org2 = Helper.CreateClient("TESTCO2");
			var org3 = Helper.CreateClient("TESTCO3");

			org2.MiscServ.OM_IMPartAttrib1Name = "Batch #";
			org2.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			org2.MiscServ.OM_IMPartAttrib2Name = "Vehicle #";
			org2.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			org2.MiscServ.OM_IMPartAttrib3Name = "Colour";
			org2.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			org2.MiscServ.OM_IMUseSerialNumber = true;

			org3.MiscServ.OM_IMPartAttrib2Name = "Speed #";
			org3.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			org3.MiscServ.OM_IMUseSerialNumber = true;

			using (var form = new TestReleaseEntryForm(Factory.New<WhsPick>(), new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.userControl.TabControl.SelectedTab = form.userControl.PickLinesTabPage;

				var grid = form.userControl.PickLinesGrid;
				var manager = new PartAttributeColumnManager(
					grid,
					"InventoryLineForAvailableInventory+" + WhsDocketLineSchema.Constants.WE_ExpiryDate,
					"InventoryLineForAvailableInventory+" + WhsDocketLineSchema.Constants.WE_PackingDate,
					"InventoryLineForAvailableInventory+" + WhsDocketLineSchema.Constants.WE_PartAttrib1,
					"InventoryLineForAvailableInventory+" + WhsDocketLineSchema.Constants.WE_PartAttrib2,
					"InventoryLineForAvailableInventory+" + WhsDocketLineSchema.Constants.WE_PartAttrib3,
					"InventoryLineForAvailableInventory+" + WhsDocketLineSchema.Constants.WE_SerialNumber,
					WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib1.Name,
					WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib2.Name,
					WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib3.Name,
					WhsPickLineSchema.WZ_ReleaseCapturedSerialNumber.Name);
				manager.SetColumns(org1);
				AssertReleaseCapturedAttributeVisibility(grid, false, false, false, false);

				manager.SetColumns(org2);
				AssertReleaseCapturedAttributeVisibility(grid, true, true, true, true);
				AssertReleaseCapturedAttributeTitles(grid, "Batch #", "Vehicle #", "Colour");

				manager.SetColumns(org3);
				AssertReleaseCapturedAttributeVisibility(grid, false, true, false, true);
				AssertReleaseCapturedAttributeTitles(grid, "", "Speed #", "");
			}
		}

		void AssertReleaseCapturedAttributeVisibility(ZGrid grid, bool part1IsVisible, bool part2IsVisible, bool part3IsVisible, bool serialIsVisible)
		{
			AssertEquals("PartAttrib1 visibility incorrect", part1IsVisible, grid.Columns[WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib1.Name].IsVisible);
			AssertEquals("PartAttrib2 visibility incorrect", part2IsVisible, grid.Columns[WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib2.Name].IsVisible);
			AssertEquals("PartAttrib3 visibility incorrect", part3IsVisible, grid.Columns[WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib3.Name].IsVisible);
			AssertEquals("Serial visibility incorrect", serialIsVisible, grid.Columns[WhsPickLineSchema.WZ_ReleaseCapturedSerialNumber.Name].IsVisible);
		}

		void AssertReleaseCapturedAttributeTitles(ZGrid grid, string part1ColumnName, string part2ColumnName, string part3ColumnName)
		{
			var postfixForRCAColumns = ": Release Captured";
			if (!string.IsNullOrEmpty(part1ColumnName))
			{
				AssertEquals("Release Captured PartAttrib1 title incorrect", part1ColumnName + postfixForRCAColumns, grid.Columns[WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib1.Name].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part2ColumnName))
			{
				AssertEquals("Release Captured PartAttrib2 title incorrect", part2ColumnName + postfixForRCAColumns, grid.Columns[WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib2.Name].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part3ColumnName))
			{
				AssertEquals("Release Captured PartAttrib3 title incorrect", part3ColumnName + postfixForRCAColumns, grid.Columns[WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib3.Name].ColumnStyle.HeaderText);
			}

			AssertEquals("Release Captured Serial title incorrect", "Serial #" + postfixForRCAColumns, grid.Columns[WhsPickLineSchema.WZ_ReleaseCapturedSerialNumber.Name].ColumnStyle.HeaderText);
		}

		class TestForm : ZForm
		{
			public TestForm(WhsDocket docket) : base(docket) { }
			public DocketLinesGridUserControl UserControl;

			protected override void InitializeComponent()
			{
				this.UserControl = new ReceiveDocketLinesGridUserControl();
				this.Controls.Add(this.UserControl);
				this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsDocket";
			}
		}

		class TestReleaseEntryForm : ReleaseEntryForm
		{
			public TestReleaseEntryForm(WhsPick pick, NotificationSubscriberGuiHelper whsNotificationSubscriberGuiHelper)
				: base(pick, whsNotificationSubscriberGuiHelper)
			{
				FinaliseOrderTabControl.SelectedTab = FinaliseOrderTabControl.GetTabPage("OrderLinesTabPage");
				userControl = ReleaseLineGridUserControl;
			}

			public ReleaseLineGridUserControl userControl;
		}

		#endregion
	}
}
