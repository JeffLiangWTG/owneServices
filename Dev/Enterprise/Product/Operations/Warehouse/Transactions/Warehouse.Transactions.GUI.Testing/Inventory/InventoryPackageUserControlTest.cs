using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.Warehouse.Transactions.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Warehouse.Transactions.GUI.Testing
{
	public class InventoryPackageUserControlTest : WhsGuiTestCaseWithFactory
	{
		#region TestGrid_ProperlyDisplay

		public void TestGrid_ProperlyDisplay()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var orderPackage1 = packageJob1.Packages.AddNew("PLT", "o11");
			var orderPackage2 = packageJob1.Packages.AddNew("PLT", "o12");

			Helper.CreatePickNew(order);
			var pickLine1 = orderLine.PickLines.AddNew();
			pickLine1.WZ_Units = 5m;
			pickLine1.WZ_WE_InventoryLine = receive.Lines[0].PK;
			var pickLine2 = orderLine.PickLines.AddNew();
			pickLine2.WZ_Units = 5m;
			pickLine2.WZ_WE_InventoryLine = receive.Lines[0].PK;

			var divot1 = orderPackage1.PackedItemDivots.AddNew();
			divot1.KI_ParentTableCode = WhsPickLineSchema.Constants.Prefix;
			divot1.KI_ParentID = pickLine1.PK;
			divot1.KI_PackedQty = pickLine1.WZ_Units;
			divot1.KI_ParentTableCode = pickLine1.TablePrefix;
			
			var divot2 = orderPackage2.PackedItemDivots.AddNew();
			divot2.KI_ParentID = pickLine2.PK;
			divot2.KI_PackedQty = pickLine2.WZ_Units;
			divot2.KI_ParentTableCode = pickLine2.TablePrefix;
			Factory.Save();

			using (var form = new TestInventoryForm(inventory.InDocketLine))
			{
				form.Show();

				var userControl = form.GetInventoryPackageUserControl();
				var grid = userControl.FindSingle<ZGrid>();
				AssertEquals("Precondition - should have 2 package info lines.", 2, grid.VisibleRowCount);

				grid.Sort = WhsInventoryPackageDetail.Schema.PackageID;

				for (int i = 0; i < grid.Columns.Count; i++)
				{
					var column = grid.Columns[i];
					if (column.ColumnStyle.HeaderText == "Package ID")
					{
						grid.CurrentCell = new DataGridCell(0, i);
						grid.BeginEdit(column.ColumnStyle, 0);
						AssertEquals("Package ID", grid.LastFocusedColumn.HeaderText);
						AssertEquals("o11", grid.LastFocusedColumn.TextBox.Text);

						grid.CurrentCell = new DataGridCell(1, i);
						grid.BeginEdit(column.ColumnStyle, 1);
						AssertEquals("Package ID", grid.LastFocusedColumn.HeaderText);
						AssertEquals("o12", grid.LastFocusedColumn.TextBox.Text);

						break;
					}
				}
			}
		}

		#endregion

		#region TestInventoryForm class

		internal class TestInventoryForm : InventoryForm
		{
			public TestInventoryForm(WhsDocketLine inventoryLine)
				: base(inventoryLine)
			{
			}

			public InventoryPackageUserControl GetInventoryPackageUserControl()
			{
				MainTabControl.SelectTab(base.PackageTabPage);
				return base.inventoryPackageUserControl;
			}
		}

		#endregion
	}
}
