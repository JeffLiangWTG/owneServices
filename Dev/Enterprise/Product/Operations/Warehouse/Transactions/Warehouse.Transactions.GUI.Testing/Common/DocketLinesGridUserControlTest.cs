using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	abstract class DocketLinesGridUserControlTest<TUserControl> : WhsGuiTestCaseWithFactory
			where TUserControl : DocketLinesGridUserControl
	{
		#region Constructor

		public void TestConstructor()
		{
			using (var userControl = GetNewDocketLinesGridUserControl())
			{
				AssertNotNull("Find Attributes MenuItem should be added", FindMenuItem(userControl.LinesGrid, "Find &Attributes"));

				var duplicateMenuItem = FindMenuItem(userControl.LinesGrid, "Du&plicate");
				if (ExpectDuplicateMenuItemAdded)
				{
					AssertNotNull("Duplicate MenuItem should be added", duplicateMenuItem);
				}
				else
				{
					AssertNull("Duplicate MenuItem should *not* be added", duplicateMenuItem);
				}

				TestConstructorCore(userControl);
			}
		}

		protected virtual bool ExpectDuplicateMenuItemAdded => true;

		protected virtual void TestConstructorCore(TUserControl userControl)
		{
		}

		#endregion

		#region Properties

		public void TestDocket()
		{
			var docket = GetNewDocket();
			using (var form = GetNewDocketLinesTestForm(docket))
			{
				form.Show();
				var userControl = form.UserControl;
				AssertEquals(docket, userControl.Docket);
			}
		}

		#endregion

		#region Binding

		public void TestBindingAttributes()
		{
			using (var userControl = GetNewDocketLinesGridUserControl())
			{
				AssertEquals(ControlBindTo, userControl.BindTo);
				userControl.BindTo = "LinesTest";
				AssertEquals("LinesTest", userControl.BindTo);
			}
		}

		protected virtual string ControlBindTo => "Lines";

		#endregion

		#region Column Control

		#region TestColumnsAreInitialisedCorrectly

		public void TestColumnsAreInitialisedCorrectly()
		{
			using (var form = GetNewDocketLinesTestForm(GetNewDocket()))
			{
				form.Show();

				var userControl = form.UserControl;
				AssertAttributeVisibility(userControl.LinesGrid, false, false, false, false, false, false);
			}
		}

		public void TestSerialNumberColumnIsInitialisedCorrectly()
		{
			using (var form = GetNewDocketLinesTestForm(GetNewDocket()))
			{
				form.Show();

				var userControl = form.UserControl;
				AssertEquals(false,
					userControl.LinesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == WhsDocketLineSchema.Constants.WE_SerialNumber).IsUnavailable);
			}
		}

		#endregion

		#region TestUnitAndPackQtyColumnsAreGroupedCorrectly

		#region TestUnitQuantityGrouping

		public void TestUnitQuantityGrouping()
		{
			AssertColumnGrouping("Unit Quantity", UnitsColumnName, WhsDocketLine.Schema.ProductUQ);
		}

		#endregion

		#region TestPackQuantityGrouping

		public void TestPackQuantityGrouping()
		{
			AssertColumnGrouping("Pack Quantity", PackQuantityColumnName, WhsDocketLine.Schema.WE_F3_NKPackType);
		}

		#endregion

		#region TestColumnGrouping

		void AssertColumnGrouping(string columnGrouping, string firstColumnName, string secondColumnName)
		{
			AssertGroupedColumnsAreGroupedCorrectly_ResetColumns(columnGrouping, firstColumnName, secondColumnName);
			AssertGroupedColumnsAreGroupedCorrectly_Seperate_ClosingAndOpenForm_ResetColumns(columnGrouping, firstColumnName, secondColumnName);
		}

		#endregion

		#region AssertGroupedColumnsAreGroupedCorrectly_ResetColumns

		void AssertGroupedColumnsAreGroupedCorrectly_ResetColumns(string columnGrouping, string firstColumnName, string secondColumnName)
		{
			using (var form = GetNewDocketLinesTestForm(GetNewDocket()))
			{
				var userControl = form.UserControl;
				form.Show();
				userControl.LinesGrid.ResetColumns();
				AssertColumnGrouping(userControl.LinesGrid, columnGrouping, firstColumnName, secondColumnName);
			}
		}

		#endregion

		#region AssertGroupedColumnsAreGroupedCorrectly_Seperate_ClosingAndOpenForm_ResetColumns

		void AssertGroupedColumnsAreGroupedCorrectly_Seperate_ClosingAndOpenForm_ResetColumns(string columnGrouping, string firstColumnName, string secondColumnName)
		{
			using (var form = GetNewDocketLinesTestForm(GetNewDocket()))
			{
				var userControl = form.UserControl;
				form.Show();
				userControl.LinesGrid.ResetColumns();
				SeperateColumns(userControl.LinesGrid, firstColumnName, secondColumnName);
			}

			using (var form = GetNewDocketLinesTestForm(GetNewDocket()))
			{
				var userControl = form.UserControl;
				form.Show();
				userControl.LinesGrid.ResetColumns();
				AssertColumnGrouping(userControl.LinesGrid, columnGrouping, firstColumnName, secondColumnName);
			}
		}

		#endregion

		void AssertColumnGrouping(ZGrid grid, string columnGrouping, string firstColumnName, string secondColumnName)
		{
			var indexOfFirstColumn = GetColumnIndexFromColumnName(grid.Columns, firstColumnName);
			var indexOfSecondColumn = GetColumnIndexFromColumnName(grid.Columns, secondColumnName);
			var columnIndexDifference = (indexOfSecondColumn - indexOfFirstColumn);
			AssertEquals($"Columns of the {columnGrouping} group are not adjacent to each other. The result is the difference in column index.", 1, columnIndexDifference);
		}

		#endregion

		int GetColumnIndexFromColumnName(ZGridColumns columns, string columnName)
		{
			var index = 0;
			foreach (var column in columns)
			{
				if (column.ColumnName == columnName)
				{
					break;
				}
				index++;
			}
			return index;
		}

		void SeperateColumns(ZGrid grid, string firstColumnName, string secondColumnName)
		{
			var firstColumn = new ZGridColumn();
			var secondColumn = new ZGridColumn();
			foreach (var column in grid.Columns)
			{
				if (column.ColumnName == firstColumnName)
				{
					firstColumn = column;
				}
				if (column.ColumnName == secondColumnName)
				{
					secondColumn = column;
				}
			}

			var maxIndex = grid.Columns.Count - 1;
			var minIndex = 0;
			grid.Columns.Move(firstColumn, maxIndex);
			grid.Columns.Move(secondColumn, minIndex);
		}

		protected virtual string UnitsColumnName
		{
			get { return WhsDocketLineSchema.Constants.WE_TransactionQuantity; }
		}

		protected virtual string PackQuantityColumnName
		{
			get { return WhsDocketLine.Schema.WE_PackQuantity; }
		}

		#endregion

		#region Events

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
			var label = org2.CustomLabels.AddNew();
			label.OT_OH = org2.PK;
			label.OT_FieldName = "Contract #";

			org3.MiscServ.OM_IMPartAttrib2Name = "Speed #";
			org3.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			org3.MiscServ.OM_IMUsePackingDate = true;

			var docket = GetNewDocket();
			docket.WD_OH_Client = org1.PK;

			using (var form = GetNewDocketLinesTestForm(docket))
			{
				form.Show();
				var userControl = form.UserControl;
				AssertAttributeVisibility(userControl.LinesGrid, false, false, false, false, false, false);
				docket.WD_OH_Client = org2.PK;
				AssertAttributeVisibility(userControl.LinesGrid, true, true, true, true, true, true);
				AssertAttributeTitles(userControl.LinesGrid, "Batch #", "Vehicle #", "Colour", "Contract #");
				docket.WD_OH_Client = org3.PK;
				AssertAttributeVisibility(userControl.LinesGrid, false, true, false, true, false, false);
				AssertAttributeTitles(userControl.LinesGrid, "", "Speed #", "", "");
			}
		}

		#endregion

		#region TestOnDocketStatusChanged

		public void TestOnDocketStatusChanged()
		{
			if (TestDocketStatusChanged)
			{
				var docket = GetNewDocket();
				using (var form = GetNewDocketLinesTestForm(docket))
				{
					form.Show();

					var userControl = form.UserControl;
					AssertEquals("Precondition: ", RemoveAction.RemoveAndDelete, userControl.LinesGrid.RemoveAction);
					docket.WD_FinalisedDate = ZDateTimeOffset.Now;
					AssertEquals(RemoveAction.NoRemovePossible, userControl.LinesGrid.RemoveAction);

					docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
					docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
					AssertEquals(RemoveAction.NoRemovePossible, userControl.LinesGrid.RemoveAction);

					docket.WD_DocketStatus = DocketStatus.Codes.Entered;
					AssertEquals(RemoveAction.RemoveAndDelete, userControl.LinesGrid.RemoveAction);
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool TestDocketStatusChanged
		{
			get { return true; }
		}

		#endregion

		#region TestOnDocketSubTypeChanged

		public void TestOnDocketSubTypeChanged()
		{
			if (TestDocketSubTypeChanged)
			{
				var whs1 = Helper.CreateWarehouse("1");
				var docket = GetNewDocket();
				docket.WD_WW_Whs = whs1.PK;
				AssertEquals(false, docket.IsCustomsTransaction);

				using (var form = GetNewDocketLinesTestForm(docket))
				{
					form.Show();
					var userControl = form.UserControl;
					AssertBondAttributesVisibility(userControl, false);
					AssertUSBondedColumnsVisibility(userControl, false);

					whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
					docket.WD_WW_Whs = ZGuid.Empty;
					docket.WD_WW_Whs = whs1.PK;
					AssertBondAttributesVisibility(userControl, false);
					AssertUSBondedColumnsVisibility(userControl, false);

					SetTransactionAsBonded(docket);
					docket.WD_WW_Whs = ZGuid.Empty;
					docket.WD_WW_Whs = whs1.PK;
					AssertBondAttributesVisibility(userControl, true);
					AssertUSBondedColumnsVisibility(userControl, true);

					whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
					docket.WD_WW_Whs = ZGuid.Empty;
					docket.WD_WW_Whs = whs1.PK;
					AssertBondAttributesVisibility(userControl, true);
					AssertUSBondedColumnsVisibility(userControl, false);
				}

				TestOnDocketSubTypeChangedCore();
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool TestDocketSubTypeChanged
		{
			get { return true; }
		}

		protected virtual void TestOnDocketSubTypeChangedCore()
		{
		}

		void SetTransactionAsBonded(WhsDocket docket)
		{
			Helper.EnableWarehouseForBond(docket.Warehouse, true);
			SetTransactionAsBondedCore(docket);
		}

		protected virtual void SetTransactionAsBondedCore(WhsDocket docket)
		{
		}

		#endregion

		#region TestFindAttributes

		public void TestFindAttributes()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			data.Line111.WI_PartAttrib1 = "PA1";
			data.Line111.InDocketLine.WE_PartAttrib1 = "PA1";
			data.Line111.WI_CustomAttrib_2 = "CA2";
			data.Line111.InDocketLine.WE_CustomAttrib2 = "CA2";
			Factory.Save();

			var docket = GetNewDocket();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;

			using (var form = GetNewDocketLinesTestForm(docket))
			{
				form.Show();

				var userControl = form.UserControl;
				userControl.LinesGrid.Select(0);
				userControl.FireFindAttributesMenuItemClickForTesting();

				var popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm;
				var module = popup.Module_ForTest;
				((BusinessObjectCollection)module.GridCollection).Load();

				for (var i = 0; i < module.GridCollection.Count; i++)
				{
					if (!((WhsInventoryView)module.GridCollection[i]).WI_PartAttrib1.IsEmpty)
					{
						((ZDisplayGrid)module.DisplayGrid).Select(i);
						AssertEquals("Precondition: Available To Pick Quantity for selected inventory is 100.", 100m, ((WhsInventoryView)module.GridCollection[i]).WI_AvailableToPickQuantity);
						break;
					}
				}

				var docketLine = (WhsDocketLine)userControl.LinesGrid.ListManager.GetCurrent();
				docketLine.WE_TransactionQuantity = 2;

				popup.ExposedOKButtonForTesting.PerformClick();

				AssertEquals("PA1", docketLine.WE_PartAttrib1);
				AssertEquals("CA2", docketLine.WE_CustomAttrib2);
				AssertEquals("Quantity should not get copied in 'Find Attribute' function.", 2m, docketLine.WE_TransactionQuantity);
				AssertNotNull(docket.NotificationManager.LastPopped);
				AssertEquals(userControl.TestNotify, docket.NotificationManager.LastPopped);

				var menuItem = userControl.LinesGrid.ContextMenu.MenuItems.FindByText("Find Attributes");

				userControl.LinesGrid.ListManager.EndCurrentEdit();
				userControl.LinesGrid.Select(userControl.LinesGrid.List.IndexOf(docketLine));

				userControl.LinesGrid.ContextMenu.ShowPopupMenu();
				Assert("Precondition: Enabled.", menuItem.Enabled);

				TestFindAttributes_Core(docketLine, menuItem, userControl.LinesGrid.ContextMenu);

				docket.WD_FinalisedDate = ZDateTimeOffset.Now;
				userControl.LinesGrid.ContextMenu.ShowPopupMenu();
				Assert("Should be disabled on finalised dockets.", !menuItem.Enabled);
			}
		}

		protected virtual void TestFindAttributes_Core(WhsDocketLine line, MenuItem menuItem, ContextMenu contextMenu)
		{
		}

		#endregion

		#endregion

		#region INotifications Members

		public void TestNotify()
		{
			var docket = GetNewDocket();
			using (var form = GetNewDocketLinesTestForm(docket))
			{
				var type = new TestINotificationType("Message", "NotZErrorMessageBox");
				var e = new TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				form.UserControl.Notify(e);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("TestMessageToDisplay"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestQueryUser()
		{
			var helperMock = new Mock<INotificationSubscriberQueryUser>();
			using (ObjectFactory.Substitute("WhsNotificationSubscriberGuiHelper", helperMock.Object))
			using (var userControl = new DocketLinesGridUserControl())
			{
				GUITestHelper.VerifyQueryUserMethodInNotificationSubscriberGuiHelperIsBeingCalled(userControl, helperMock);
			}
		}

		#endregion

		#region Implementation

		void AssertUSBondedColumnsVisibility(DocketLinesGridUserControl userControl, ZBool isVisible)
		{
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains(WhsDocketLineSchema.Constants.WE_PackageGroupId));
			AssertEquals(IsPerPackageQtyColumnVisible && isVisible, userControl.LinesGrid.Columns.Contains(WhsDocketLineSchema.Constants.WE_PerPackageQty));
		}

		protected virtual bool IsPerPackageQtyColumnVisible
		{
			get { return true; }
		}

		void AssertBondAttributesVisibility(DocketLinesGridUserControl userControl, ZBool isVisible)
		{
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsTariffLookup"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsTariffItem"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsTariffDesc"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_EntryKey"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_EntryLineNo"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_EntryDate"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_DeclarationReference"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_RN_NKCountryOfOrigin"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_CustomsQty"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_CustomsUnitOfQty"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_ValueForDuty"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_TILV"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_AddInfo"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_CustomsSecondQuantity"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_CustomsSecondUnitQty"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_Tariff"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_PrimaryPreference"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_CustomsThirdQuantity"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_CustomsThirdUnitQty"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+WB_OA_ManufacturerAddress"));
			AssertEquals(isVisible, userControl.LinesGrid.Columns.Contains("CustomsData+ManufacturerCode"));
		}

		protected void AssertAttributeVisibility(ZGrid grid, bool expiry, bool packing, bool part1, bool part2, bool part3, bool customAttrib1)
		{
			AssertEquals("Expiry visibility incorrect", expiry, grid.Columns[WhsDocketLineSchema.WE_ExpiryDate.Name].IsVisible);
			AssertEquals("Packing visibility incorrect", packing, grid.Columns[WhsDocketLineSchema.WE_PackingDate.Name].IsVisible);
			AssertEquals("PartAttrib1 visibility incorrect", part1, grid.Columns[WhsDocketLineSchema.WE_PartAttrib1.Name].IsVisible);
			AssertEquals("PartAttrib2 visibility incorrect", part2, grid.Columns[WhsDocketLineSchema.WE_PartAttrib2.Name].IsVisible);
			AssertEquals("PartAttrib3 visibility incorrect", part3, grid.Columns[WhsDocketLineSchema.WE_PartAttrib3.Name].IsVisible);
		}

		void AssertAttributeTitles(ZGrid grid, string part1, string part2, string part3, string customAttrib1)
		{
			if (!string.IsNullOrEmpty(part1))
			{
				AssertEquals("PartAttrib1 title incorrect", part1, grid.Columns[WhsDocketLineSchema.WE_PartAttrib1.Name].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part2))
			{
				AssertEquals("PartAttrib2 title incorrect", part2, grid.Columns[WhsDocketLineSchema.WE_PartAttrib2.Name].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part3))
			{
				AssertEquals("PartAttrib3 title incorrect", part3, grid.Columns[WhsDocketLineSchema.WE_PartAttrib3.Name].ColumnStyle.HeaderText);
			}
		}

		protected abstract WhsDocket GetNewDocket();
		protected abstract TUserControl GetNewDocketLinesGridUserControl();
		protected abstract DocketLinesTestForm GetNewDocketLinesTestForm(WhsDocket docket);

		public abstract class DocketLinesTestForm : ZForm
		{
			public DocketLinesTestForm(WhsDocket docket)
				: base(docket)
			{
			}

			public TUserControl UserControl;

			protected override void InitializeComponent()
			{
				this.UserControl = GetNewDocketLinesGridUserControl();
				this.Controls.Add(this.UserControl);
				this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsDocket";
			}

			protected abstract TUserControl GetNewDocketLinesGridUserControl();
		}

		#endregion
	}
}
