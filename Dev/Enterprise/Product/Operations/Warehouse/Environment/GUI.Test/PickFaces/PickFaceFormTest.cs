using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Environment.GUI.PickFaces;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	[TestedType(typeof(PickFaceForm))]
	class PickFaceFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = (PickFaceFormForTest)GetFormToBashCore())
			{
				AssertEquals("Fixed Pick Face", form.FormCaption);
			}
		}

		public void TestProductButton()
		{
			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 1);
			var locations = data.Locations[data.Warehouses[0]];

			int replenishMin = 1;
			var pickFace = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[0], replenishMin++);
			Factory.Save();

			var pickFaceBizO = Factory.Load<WhsPickFace>(pickFace.PK);
			using (var form = GetNewPickFaceForm(pickFaceBizO))
			{
				form.Show();
				form.ProductButton.PerformClick();
				using (var productForm = (OrgSupplierPartForm)form.GetController().LastShownForm)
				{
					AssertNotNull(productForm);
					AssertEquals(true, productForm.Visible);
					AssertEquals(data.Parts[0].PK, ((BusinessObject)productForm.BusinessEntity).PK);
				}
			}
		}

		public void TestProductButton_NoProduct()
		{
			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 1);
			var locations = data.Locations[data.Warehouses[0]];
			Factory.Save();

			var pickFaceBizO = Factory.LoadTop1<WhsPickFace>(new ZQuery(WhsPickFaceSchema.WF_WL, locations[0].PK));
			using (var form = GetNewPickFaceForm(pickFaceBizO))
			{
				form.Show();
				form.ProductButton.PerformClick();
				AssertMultilineASCIIEquals("Expect a message to the user.", "Use the product module to assign products to this pick face.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOrgProductLocationDetailsAndVisibility()
		{
			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 1);
			var locations = data.Locations[data.Warehouses[0]];

			int replenishMin = 1;
			var pickFace = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[0], replenishMin++);
			Factory.Save();

			var pickFaceBizO = Factory.Load<WhsPickFace>(pickFace.PK);
			using (var form = GetNewPickFaceForm(pickFaceBizO))
			{
				form.Show();
				var orgFindBox = GUITestHelper.FindControl<ZGuidFindBox>(form.Controls, "OrgFindBox");
				AssertEquals(data.Clients[0].PK, orgFindBox.Guid);
				AssertEquals(true, orgFindBox.ReadOnly);

				var productFindBox = GUITestHelper.FindControl<ZGuidFindBox>(form.Controls, "ProductFindBox");
				AssertEquals(data.Parts[0].PK, productFindBox.Guid);
				AssertEquals(true, productFindBox.ReadOnly);

				var locationTextBox = GUITestHelper.FindControl<ZTextBox>(form.Controls, "LocationTextBox");
				AssertEquals(locations[0].WLV_LocationString, locationTextBox.Text);
				AssertEquals(true, locationTextBox.ReadOnly);
			}
		}

		public void TestReplenishmentDetailsAndVisibility()
		{
			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 1);
			var locations = data.Locations[data.Warehouses[0]];

			var replenishMin = 1m;
			var replenishMax = 2m;
			var replenishMultiple = 2m;
			var pickFace = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[0], replenishMin, replenishMax, replenishMultiple);
			Factory.Save();

			var pickFaceBizO = Factory.Load<WhsPickFace>(pickFace.PK);
			using (var form = GetNewPickFaceForm(pickFaceBizO))
			{
				form.Show();
				var replenishMinCalcEdit = GUITestHelper.FindControl<ZCalcEdit>(form.Controls, "ReplenishMinCalcEdit");
				AssertEquals(replenishMin, replenishMinCalcEdit.CalcValue);
				AssertEquals(false, replenishMinCalcEdit.ReadOnly);

				var replenishMaxCalcEdit = GUITestHelper.FindControl<ZCalcEdit>(form.Controls, "ReplenishMaxCalcEdit");
				AssertEquals(replenishMax, replenishMaxCalcEdit.CalcValue);
				AssertEquals(false, replenishMaxCalcEdit.ReadOnly);

				var replenishMultipleCalcEdit = GUITestHelper.FindControl<ZCalcEdit>(form.Controls, "ReplenishMultipleCalcEdit");
				AssertEquals(replenishMultiple, replenishMultipleCalcEdit.CalcValue);
				AssertEquals(false, replenishMultipleCalcEdit.ReadOnly);
			}
		}

		public void TestCommittedPicksAndTheirOrderInGrid()
		{
			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 1);
			var locations = data.Locations[data.Warehouses[0]];

			int replenishMin = 1;
			var pickFace = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[0], replenishMin++);
			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var receive1PK = transactionHelper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R1", Helper.Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive1PK, data.Parts[0].PK, 100m, locations[0].PK);
			transactionHelper.FinaliseDocket(receive1PK);
			Factory.Save();

			var order1 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order1");
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[0].PK, 30m);
			var pick1 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order1.PK);
			pick1.PickPriority = 0;
			var order2 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order2");
			transactionHelper.CreateWhsOrderLine(order2.PK, data.Parts[0].PK, 40m);
			var pick2 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order2.PK);
			pick2.PickPriority = 2;
			var order3 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order3");
			transactionHelper.CreateWhsOrderLine(order3.PK, data.Parts[0].PK, 30m);
			var pick3 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order3.PK);
			pick3.PickPriority = 3;
			Factory.Save();

			var pickFaceBizO = Factory.Load<WhsPickFace>(pickFace.PK);
			using (var form = GetNewPickFaceForm(pickFaceBizO))
			{
				form.Show();
				var grid = GUITestHelper.FindControl<ZGrid>(form.Controls, "CommittedPicksGrid");

				AssertEquals(grid.Name, "CommittedPicksGrid");
				AssertEquals("The first column name should be \"Pick No\"", grid.Columns[0].ColumnStyle.HeaderText, "Pick No");
				AssertEquals("The second column name should be \"Pick Priority\".", grid.Columns[1].ColumnStyle.HeaderText, "Pick Priority");
				AssertEquals("The third column name should be \"Quantity Committed\".", grid.Columns[2].ColumnStyle.HeaderText, "Quantity Committed");
				AssertEquals("The count of committed picks should be 3", 3, grid.List.Count);
				AssertEquals("The first row should be for pick 2 (priority 2)", ((WhsPickFaceCommittedStockView)grid.List[0]).WCP_WP, pick2.PK);
				AssertEquals("The second row should be for pick 3 (priority 3)", ((WhsPickFaceCommittedStockView)grid.List[1]).WCP_WP, pick3.PK);
				AssertEquals("The third row should be for pick 1 (priority 0)", ((WhsPickFaceCommittedStockView)grid.List[2]).WCP_WP, pick1.PK);
			}
		}

		public void TestAwaitingReplenishmentPicksAndTheirOrderInGrid()
		{
			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 2);
			var locations = data.Locations[data.Warehouses[0]];
			var normalLocation =
				Helper.CreateLocationType("NOR", "NormalLocation", false, 1, LocationClasses.Codes.NOR);
			locations[0].WLV_WLT_LocationType = normalLocation.PK;

			int replenishMin = 1;
			var pickFace = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[1], replenishMin++);
			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var receive1PK = transactionHelper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R1", Helper.Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive1PK, data.Parts[0].PK, 100m, locations[0].PK);
			transactionHelper.FinaliseDocket(receive1PK);
			Factory.Save();

			var transfer = transactionHelper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[0].PK, "T1", Helper.Notify);
			var transferLine = transactionHelper.CreateWhsTransferLine(transfer, data.Parts[0].PK, 100m, locations[0].PK, locations[1].PK);
			var transferLineBizO = Factory.GetBizOsForPK(transferLine.ToGuid());
			transferLineBizO[0].RunPreSaveValidation();
			Factory.Save();

			var order1 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order1");
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[0].PK, 30m);
			var pick1 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order1.PK);
			pick1.PickPriority = 0;
			var order2 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order2");
			transactionHelper.CreateWhsOrderLine(order2.PK, data.Parts[0].PK, 40m);
			var pick2 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order2.PK);
			pick2.PickPriority = 1;
			var order3 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order3");
			transactionHelper.CreateWhsOrderLine(order3.PK, data.Parts[0].PK, 30m);
			var pick3 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order3.PK);
			pick3.PickPriority = 2;
			Factory.Save();

			var pickFaceBizO = Factory.Load<WhsPickFace>(pickFace.PK);
			using (var form = GetNewPickFaceForm(pickFaceBizO))
			{
				form.Show();
				var grid = GUITestHelper.FindControl<ZGrid>(form.Controls, "AwaitingPicksGrid");

				AssertEquals(grid.Name, "AwaitingPicksGrid");
				AssertEquals("The first column name should be \"Pick No\"", grid.Columns[0].ColumnStyle.HeaderText, "Pick No");
				AssertEquals("The second column name should be \"Pick Priority\".", grid.Columns[1].ColumnStyle.HeaderText, "Pick Priority");
				AssertEquals("The third column name should be \"Quantity Required\".", grid.Columns[2].ColumnStyle.HeaderText, "Quantity Required");
				AssertEquals("The count of awaiting picks should be 3", 3, grid.List.Count);
				AssertEquals("The first row should be for pick 2 (priority 1)", ((WhsPickFaceAwaitingReplenishmentView)grid.List[0]).WWP_WP, pick2.PK);
				AssertEquals("The second row should be for pick 3 (priority 2)", ((WhsPickFaceAwaitingReplenishmentView)grid.List[1]).WWP_WP, pick3.PK);
				AssertEquals("The third row should be for pick 1 (priority 0)", ((WhsPickFaceAwaitingReplenishmentView)grid.List[2]).WWP_WP, pick1.PK);
			}
		}

		#region Implementation

		WhsTestHelperFunctionsEnv helper;

		WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		class PickFaceFormForTest : PickFaceForm
		{
			public PickFaceFormForTest(WhsPickFace pickFace)
				: base(pickFace)
			{
			}

			public new ZButton ProductButton => base.ProductButton;
			public ZController GetController() => ControllerForTest;
		}

		protected override Form GetFormToBashCore()
		{
			return new PickFaceFormForTest(Factory.New<WhsPickFace>());
		}

		PickFaceFormForTest GetNewPickFaceForm(WhsPickFace pickFace)
		{
			return new PickFaceFormForTest(pickFace);
		}

		#endregion
	}
}
