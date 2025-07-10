using System.Windows.Forms;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	[TestedType(typeof(AreaEntryForm))]
	class AreaEntryFormBasherTest : ZFormBasherTest
	{
		#region TestPrinterDropEditVisibilityForWarehouseType

		public void TestPrinterDropEditVisibilityForWarehouseType_Product()
		{
			TestPrinterDropEditVisibilityForWarehouseType(WarehouseTypes.Codes.Product, expectedVisible: true);
		}

		public void TestPrinterDropEditVisibilityForWarehouseType_FreeTradeZone()
		{
			TestPrinterDropEditVisibilityForWarehouseType(WarehouseTypes.Codes.FreeTradeZone, expectedVisible: true);
		}

		public void TestPrinterDropEditVisibilityForWarehouseType_Transit()
		{
			TestPrinterDropEditVisibilityForWarehouseType(WarehouseTypes.Codes.Transit, expectedVisible: false);
		}

		public void TestPrinterDropEditVisibilityForWarehouseType_ContainerYard()
		{
			TestPrinterDropEditVisibilityForWarehouseType(WarehouseTypes.Codes.ContainerYard, expectedVisible: false);
		}

		void TestPrinterDropEditVisibilityForWarehouseType(string warehouseType, bool expectedVisible)
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = warehouseType;
			var area = warehouse.Areas[0];
			Factory.Save();

			using (var form = new AreaEntryForm(area))
			{
				form.Show();
				AssertEquals("Pick Pack Printer Drop Edit should be visible for Product Warehouse.", expectedVisible, form.PickPackPrinterGuidDropEdit.Visible);
			}
		}

		#endregion

		#region TestShowHideControlsBasedOnWarehouseType

		public void TestShowHideControlsBasedOnWarehouseType_NewArea()
		{
			var area = Factory.New<WhsArea>();
			using (var form = new AreaEntryForm(area))
			{
				form.Show();
				AssertEquals("TransitClientGuidFindBox should not be visible for area has noe warehouse.", false, form.TransitClientGuidFindBox.Visible);
				AssertEquals("PickPackPrinterGuidDropEdit should not be visible for area has no warehouse.", false, form.PickPackPrinterGuidDropEdit.Visible);
			}
		}

		public void TestShowHideControlsBasedOnWarehouseType_Product()
		{
			TestShowHideControlsBasedOnWarehouseTypeCore(WarehouseTypes.Codes.Product, visibleOfTransitClient: false, visibleOfPickPackPrinter: true);
		}

		public void TestShowHideControlsBasedOnWarehouseType_FreeTradeZone()
		{
			TestShowHideControlsBasedOnWarehouseTypeCore(WarehouseTypes.Codes.FreeTradeZone, visibleOfTransitClient: false, visibleOfPickPackPrinter: true);
		}

		public void TestShowHideControlsBasedOnWarehouseType_Transit()
		{
			TestShowHideControlsBasedOnWarehouseTypeCore(WarehouseTypes.Codes.Transit, visibleOfTransitClient: true, visibleOfPickPackPrinter: false, "Transit Warehouse Client");
		}

		public void TestShowHideControlsBasedOnWarehouseType_ContainerYard()
		{
			TestShowHideControlsBasedOnWarehouseTypeCore(WarehouseTypes.Codes.ContainerYard, visibleOfTransitClient: true, visibleOfPickPackPrinter: false, "Container Yard Client");
		}

		void TestShowHideControlsBasedOnWarehouseTypeCore(string warehouseType, bool visibleOfTransitClient, bool visibleOfPickPackPrinter, string transitClientCaption = "")
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = warehouseType;
			var area = warehouse.Areas[0];

			using (var form = new AreaEntryForm(area))
			{
				form.Show();
				AssertEquals("TransitClientGuidFindBox's Visible:", visibleOfTransitClient, form.TransitClientGuidFindBox.Visible);
				AssertEquals("PickPackPrinterGuidDropEdit's Visible:", visibleOfPickPackPrinter, form.PickPackPrinterGuidDropEdit.Visible);

				if (visibleOfTransitClient)
				{
					AssertEquals("TransitClientGuidFindBox' Caption", transitClientCaption, form.TransitClientGuidFindBox.CaptionResourceString.Caption);
				}
			}
		}

		#endregion

		#region TestHideClientAndDischargeIfNotTransitWarehouse

		public void TestHideClientAndDischargeIfNotTransitWarehouse()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var area = warehouse.Areas[0];
			Factory.Save();

			using (var form = new AreaEntryForm(area))
			{
				form.Show();
				AssertEquals("TransitClientGuidFindBox should be visible for Transit Warehouse.", true, form.TransitClientGuidFindBox.Visible);
			}

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;

			using (var form = new AreaEntryForm(area))
			{
				form.Show();
				AssertEquals("TransitClientGuidFindBox should not be visible for Product Warehouse.", false, form.TransitClientGuidFindBox.Visible);
			}
		}

		#endregion

		#region TestValidationOnFormLoad

		public void TestValidationOnFormLoad()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var client = helper.CreateClient();
			var warehouse = helper.CreateWarehouse("WHS", "A", 2, 2);
			var area = warehouse.Areas[0];
			var product = helper.CreateProduct("Guitars", client);
			product.OP_WeightUQ = "L";
			product.OP_CubicUQ = "CM";
			product.OP_Weight = 10m;
			product.OP_Cubic = 1m;

			iHelper.CreateStock(warehouse.PK, client.PK, product.PK, 10m);
			Factory.Save();

			helper.SetLocationMaxWeightAndVolume(warehouse.FindLocation("A-1-1"), 10m, "L", 1m, "CM");

			using (var form = new AreaEntryForm(area))
			{
				form.Show();

				AssertHasWarning(area.WA_CalcCurrentVolumeInfo, "Cannot calculate Volume because the following Product(s) have invalid units:\r\n\tProduct 'GUITARS' has an invalid Volume Unit 'CM'.");
				AssertHasWarning(area.WA_CalcCurrentWeightInfo, "Cannot calculate Weight because the following Product(s) have invalid units:\r\n\tProduct 'GUITARS' has an invalid Weight Unit 'L'.");
				AssertHasWarning(area.WA_CalcMaxVolumeInfo, "Cannot calculate Max Volume because the following Location(s) have invalid units:\r\n\tLocation 'A-1-1' has an invalid Volume Unit 'CM'.");
				AssertHasWarning(area.WA_CalcMaxWeightInfo, "Cannot calculate Max Weight because the following Location(s) have invalid units:\r\n\tLocation 'A-1-1' has an invalid Weight Unit 'L'.");
				AssertNotNull("DocumentUDF not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new AreaEntryForm(Factory.New<WhsArea>());
		}

		#endregion
	}
}
