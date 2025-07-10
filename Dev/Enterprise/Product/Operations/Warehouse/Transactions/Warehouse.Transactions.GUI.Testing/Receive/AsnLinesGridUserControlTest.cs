using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class AsnLinesGridUserControlTest : WhsGuiTestCaseWithFactory
	{
		#region Test Cases

		#region TestSerialNumberColumnInitialised

		public void TestSerialNumberColumnInitialised()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new TestForm(receive))
			{
				form.Show();

				AssertEquals(false,
					form.UserControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == WhsAsnLineSchema.Constants.WN_SerialNumber).IsUnavailable);
			}
		}

		#endregion

		#region TestDocket

		public void TestDocket()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new TestForm(receive))
			{
				form.Show();
				AssertEquals(typeof(WhsReceive), form.UserControl.Receive.GetType());
				AssertEquals(form.UserControl.Receive, receive);
			}
		}

		#endregion

		#region TestSerialNumberUserControl

		public void TestSerialNumberUserControl_SchemaRedesignChanges_Enable()
		{
			TestSerialNumberUserControl(enableSchemaRedesignChanges: true);
		}

		public void TestSerialNumberUserControl_SchemaRedesignChanges_Disable()
		{
			TestSerialNumberUserControl(enableSchemaRedesignChanges: false);
		}

		void TestSerialNumberUserControl(bool enableSchemaRedesignChanges)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1, true, false);
			helper.CreateAsnLine(receive, data.Part1, 1);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSchemaRedesignChanges))
			using (var form = new TestForm(receive))
			{
				form.Show();

				AssertEquals(enableSchemaRedesignChanges, form.UserControl.SerialNumberControl.Visible);
			}
		}

		#endregion

		#region Events

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

			org3.MiscServ.OM_IMPartAttrib2Name = "Lot #";
			org3.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			org3.MiscServ.OM_IMUsePackingDate = true;

			var receive = Helper.CreateWhsReceive(org1, whs);

			using (var form = new TestForm(receive))
			{
				form.Show();

				var userControl = form.UserControl;
				AssertAttributeVisibility(userControl.Grid, expiry: false, packing: false, part1: false, part2: false, part3: false);

				receive.WD_OH_Client = org2.PK;
				AssertAttributeVisibility(userControl.Grid, expiry: true, packing: true, part1: true, part2: true, part3: true);
				AssertAttributeTitles(userControl.Grid, "Batch #", "Vehicle #", "Colour");

				receive.WD_OH_Client = org3.PK;
				AssertAttributeVisibility(userControl.Grid, expiry: false, packing: true, part1: false, part2: true, part3: false);
				AssertAttributeTitles(userControl.Grid, "", "Lot #", "");
			}
		}

		#endregion

		#endregion

		#region Implementation

		void AssertAttributeVisibility(ZGrid grid, bool expiry, bool packing, bool part1, bool part2, bool part3)
		{
			AssertEquals("Expiry visibility incorrect", expiry, grid.Columns[WhsAsnLineSchema.Constants.WN_ExpiryDate].IsVisible);
			AssertEquals("Packing visibility incorrect", packing, grid.Columns[WhsAsnLineSchema.Constants.WN_PackingDate].IsVisible);
			AssertEquals("Part1 visibility incorrect", part1, grid.Columns[WhsAsnLineSchema.Constants.WN_PartAttrib1].IsVisible);
			AssertEquals("Part2 visibility incorrect", part2, grid.Columns[WhsAsnLineSchema.Constants.WN_PartAttrib2].IsVisible);
			AssertEquals("Part3 visibility incorrect", part3, grid.Columns[WhsAsnLineSchema.Constants.WN_PartAttrib3].IsVisible);
		}

		void AssertAttributeTitles(ZGrid grid, string part1, string part2, string part3)
		{
			if (!string.IsNullOrEmpty(part1))
			{
				AssertEquals("Part1 title incorrect", part1, grid.Columns[WhsAsnLineSchema.Constants.WN_PartAttrib1].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part2))
			{
				AssertEquals("Part2 title incorrect", part2, grid.Columns[WhsAsnLineSchema.Constants.WN_PartAttrib2].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part3))
			{
				AssertEquals("Part3 title incorrect", part3, grid.Columns[WhsAsnLineSchema.Constants.WN_PartAttrib3].ColumnStyle.HeaderText);
			}
		}

		class TestForm : ZForm
		{
			public TestForm(WhsDocket docket)
				: base(docket)
			{
				this.docket = docket;
			}
			public AsnLinesGridUserControl UserControl;

			protected override void InitializeComponent()
			{
				this.UserControl = new AsnLinesGridUserControl();
				this.UserControl.BindTo = "AsnLines";
				this.UserControl.DataSourceAssemblyName = "";
				this.UserControl.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsAsnLine";

				this.Controls.Add(this.UserControl);
				this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsDocket";
			}
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in TestForm")]
			readonly WhsDocket docket;
		}

		#endregion
	}
}
