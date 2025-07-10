using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI.WhsPicksAwaitingReplenishment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing.PicksAwaitingReplenishment
{
	[TestedType(typeof(WhsPicksAwaitingReplenishmentForm))]
	class WhsPicksAwaitingReplenishmentFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = (WhsPicksAwaitingReplenishmentForm)GetFormToBashCore())
			{
				AssertEquals("Fixed Pick Faces Awaiting Replenishment", form.FormCaption);
			}
		}

		public void TestClientProductGrid()
		{
			var data = new PickFaceViewTestData(Factory, clients: 2, locationsPerWarehouse: 3);
			var locations = data.Locations[data.Warehouses[0]];
			var normalLocationType =
				Helper.CreateLocationType("NOR", "NormalLocation", false, 1, LocationClasses.Codes.NOR);
			locations[0].WLV_WLT_LocationType = normalLocationType.PK;

			int replenishMin = 1;
			var pickFace1 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[1], replenishMin++);
			var pickFace2 = Helper.CreateProductPickFace(data.Parts[1], data.Clients[1], locations[2], replenishMin++);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R0", Helper.Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Parts[0], 100m, locations[0]);
			receive1.FinaliseDocket();
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Clients[1].PK, data.Warehouses[0].PK, "R1", Helper.Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Parts[1], 50m, locations[2]);
			receive2.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[0].PK, "T0", Helper.Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Parts[0], 100m, locations[0].PK, locations[1].PK);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "O0");
			Helper.CreateWhsOrderLine(order1, data.Parts[0], 100m);
			var pick1 = Helper.CreatePickNew(false, false, order1);
			var order2 = Helper.CreateWhsOrder(data.Clients[1].PK, data.Warehouses[0].PK, data.Clients[1].PK, "O1");
			Helper.CreateWhsOrderLine(order2, data.Parts[1], 50m);
			var pick2 = Helper.CreatePickNew(false, false, order2);
			Factory.Save();

			using (var form = GetNewWhsPicksAwaitingReplenishmentForm(pick1))
			{
				form.Show();
				var grid = GUITestHelper.FindControl<ZGrid>(form.Controls, "ClientProductGrid");
				AssertEquals(grid.Name, "ClientProductGrid");
				AssertEquals("The first column name should be \"Client\"", grid.Columns[0].ColumnStyle.HeaderText, "Client");
				AssertEquals("The second column name should be \"Product\".", grid.Columns[1].ColumnStyle.HeaderText, "Product");
				AssertEquals("The count of client/product combinations that are awaiting replenishment should be 1", 1, grid.List.Count);
				AssertEquals("The first row should be for the first client/product combination", ((PickFaceProductsAwaitingReplenishment)grid.List[0]).ClientCode, data.Clients[0].OH_Code);
				AssertEquals("The first row should be for the first client/product combination", ((PickFaceProductsAwaitingReplenishment)grid.List[0]).ProductCode, data.Parts[0].OP_PartNum);
			}
		}

		public void TestClientProductGrid_FetchHints()
		{
			var data = new PickFaceViewTestData(Factory, clients: 10, locationsPerWarehouse: 11);
			var locations = data.Locations[data.Warehouses[0]];
			var normalLocationType =
				Helper.CreateLocationType("NOR", "NormalLocation", false, 10, LocationClasses.Codes.NOR);
			locations[10].WLV_WLT_LocationType = normalLocationType.PK;

			int replenishMin = 1;
			WhsPick pick = null;

			for (int i = 0; i < 10; i++)
			{
				Helper.CreateProductPickFace(data.Parts[i], data.Clients[i], locations[i], replenishMin++);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Clients[i].PK, data.Warehouses[0].PK, $"R{i}", Helper.Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Parts[i], 100m, locations[10]);
				receive.FinaliseDocket();
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Clients[i].PK, data.Warehouses[0].PK, $"T{i}", Helper.Notify);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Parts[i], 100m, locations[10].PK, locations[i].PK);
				transferLine.RunPreSaveValidation();
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Clients[i].PK, data.Warehouses[0].PK, data.Clients[i].PK, $"O{i}");
				Helper.CreateWhsOrderLine(order, data.Parts[i], 100m);
				Helper.CreateWhsOrderLine(order, data.Parts[0], 100m);
				Helper.CreateWhsOrderLine(order, data.Parts[1], 100m);
				Helper.CreateWhsOrderLine(order, data.Parts[2], 100m);
				pick = Helper.CreatePickNew(false, false, order);
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickFaceAwaitingReplenishmentViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				using (var form = GetNewWhsPicksAwaitingReplenishmentForm(newFactory.Load<WhsPick>(pick.PK)))
				{
					form.Show();
					Application.DoEvents();
				}
			}
		}

		public void TestPicksAwaitingReplenishmentGrid()
		{
			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 2);
			var locations = data.Locations[data.Warehouses[0]];
			var normalLocationType =
				Helper.CreateLocationType("NOR", "NormalLocation", false, 1, LocationClasses.Codes.NOR);
			locations[0].WLV_WLT_LocationType = normalLocationType.PK;

			var pickFace = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[1], 1m, 2m, 2m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R0", Helper.Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Parts[0], 100m, locations[0]);
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[0].PK, "T0", Helper.Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Parts[0], 100m, locations[0].PK, locations[1].PK);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "O0");
			Helper.CreateWhsOrderLine(order1, data.Parts[0], 20m);
			var pick1 = Helper.CreatePickNew(false, false, order1);
			pick1.PickPriority = 3;
			var order2 = Helper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "O1");
			Helper.CreateWhsOrderLine(order2, data.Parts[0], 30m);
			var pick2 = Helper.CreatePickNew(false, false, order2);
			pick2.PickPriority = 2;
			var order3 = Helper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "O2");
			Helper.CreateWhsOrderLine(order3, data.Parts[0], 50m);
			var pick3 = Helper.CreatePickNew(false, false, order3);
			pick3.PickPriority = 1;
			Factory.Save();

			using (var form = GetNewWhsPicksAwaitingReplenishmentForm(pick1))
			{
				form.Show();
				var clientProductGrid = GUITestHelper.FindControl<ZGrid>(form.Controls, "ClientProductGrid");
				clientProductGrid.Select(0);

				var awaitingReplenishmentGrid = GUITestHelper.FindControl<ZGrid>(form.Controls, "PicksAwaitingGrid");
				AssertEquals(awaitingReplenishmentGrid.Name, "PicksAwaitingGrid");
				AssertEquals("The first column name should be \"Pick No\"", awaitingReplenishmentGrid.Columns[0].ColumnStyle.HeaderText, "Pick No");
				AssertEquals("The second column name should be \"Pick Priority\".", awaitingReplenishmentGrid.Columns[1].ColumnStyle.HeaderText, "Pick Priority");
				AssertEquals("The third column name should be \"Quantity Required\".", awaitingReplenishmentGrid.Columns[2].ColumnStyle.HeaderText, "Quantity Required");

				AssertEquals("The count of picks awaiting replenishment should be 3", 3, awaitingReplenishmentGrid.List.Count);
				AssertEquals("The first row should be for the third pick", ((WhsPickFaceAwaitingReplenishmentView)awaitingReplenishmentGrid.List[0]).WWP_WP, pick3.PK);
				AssertEquals("The second row should be for the second pick", ((WhsPickFaceAwaitingReplenishmentView)awaitingReplenishmentGrid.List[1]).WWP_WP, pick2.PK);
				AssertEquals("The third row should be for the first pick", ((WhsPickFaceAwaitingReplenishmentView)awaitingReplenishmentGrid.List[2]).WWP_WP, pick1.PK);

				var args = new ColourDecidingEventArgs(awaitingReplenishmentGrid.List[2]);
				form.PicksAwaitingGrid_ColourDecidingForTest(null, args);
				AssertEquals(Color.SkyBlue, args.Colour);
			}
		}

		#region Implementation

		WhsPicksAwaitingReplenishmentFormForTest GetNewWhsPicksAwaitingReplenishmentForm(WhsPick pick)
		{
			return new WhsPicksAwaitingReplenishmentFormForTest(pick);
		}

		class WhsPicksAwaitingReplenishmentFormForTest : WhsPicksAwaitingReplenishmentForm
		{
			public WhsPicksAwaitingReplenishmentFormForTest(WhsPick pick) : base(pick)
			{
			}
		}

		protected override Form GetFormToBashCore()
		{
			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 2);
			var locations = data.Locations[data.Warehouses[0]];
			var normalLocationType =
				Helper.CreateLocationType("NOR", "NormalLocation", false, 1, LocationClasses.Codes.NOR);
			locations[0].WLV_WLT_LocationType = normalLocationType.PK;

			var pickFace = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[1], 1m, 2m, 2m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R0", Helper.Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Parts[0], 100m, locations[0]);
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[0].PK, "T0", Helper.Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Parts[0], 100m, locations[0].PK, locations[1].PK);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "O0");
			Helper.CreateWhsOrderLine(order1, data.Parts[0], 20m);
			var pick1 = Helper.CreatePickNew(false, false, order1);
			Factory.Save();
			return new WhsPicksAwaitingReplenishmentForm(pick1);
		}

		WhsTestHelperFunctions helper;

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		#endregion
	}
}
