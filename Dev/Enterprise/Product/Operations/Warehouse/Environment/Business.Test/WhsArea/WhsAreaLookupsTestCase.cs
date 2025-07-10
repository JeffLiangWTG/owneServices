using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsAreaLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		#region TestWarehouses

		public void TestWarehouses()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS");
			var transitWarehouse = Helper.CreateWarehouse("TRA");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var area = Factory.New<WhsArea>();
			AssertNotNull(area.Lookups.Warehouses);
			AssertEquals(typeof(WhsWarehouseCollectionWithSecurityCheck), area.Lookups.Warehouses.GetType());
			AssertEquals("Collection should not be loaded", 0, area.Lookups.Warehouses.Count);

			area.Lookups.Warehouses.Load();
			AssertContainsExactElementsInAnyOrder("Warehouses Lookups on Areas should include Transit Warehouses.", new[] { productWarehouse, transitWarehouse }, area.Lookups.Warehouses);
		}

		#endregion

		#region TestAreaTypes

		public void TestAreaTypes()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var area = Helper.CreateArea(warehouse, "Area1");

			var expectedAreaTypes = new List<string> { "BON", "DDA", "DPF", "EXC", "FRE", "IPR", "VAT" };

			var areaTypes = area.Lookups.AreaTypes;
			AssertNotNull(areaTypes);
			AssertType<AreaTypes>(areaTypes);

			AssertContainsExactElementsInAnyOrder(expectedAreaTypes, areaTypes.Cast<ICodeDescription>().Select(cd => cd.Code));
		}

		#endregion

		#region TestPrinters

		public void TestPrinters()
		{
			var area = Factory.New<WhsArea>();
			AssertEquals("Printer Lookups should have the correct Type.", ObjectFactory.GetType<IStmPrintQueueCollection>(), area.Lookups.Printers.GetType());
		}

		#endregion

		#region TestWeightUnitTypes

		public void TestWeightUnitTypes()
		{
			var area = Factory.New<WhsArea>();
			AssertEquals(true, area.Lookups.WeightUnitTypes.ContainsCode(Constants.Weight.Pounds));
		}

		#endregion

		#region TestVolumeUnitTypes

		public void TestVolumeUnitTypes()
		{
			var area = Factory.New<WhsArea>();
			AssertEquals(true, area.Lookups.VolumeUnitTypes.ContainsCode(Constants.Volume.Litre));
		}

		#endregion
	}
}
