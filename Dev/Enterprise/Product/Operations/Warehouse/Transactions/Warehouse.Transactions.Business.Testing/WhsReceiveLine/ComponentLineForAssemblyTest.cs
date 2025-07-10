using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(ComponentLineForAssembly))]
	class ComponentLineForAssemblyTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var inventoryLine = Factory.New<WhsReceiveLine>();
			var componentLine = new ComponentLineForAssembly(inventoryLine, 2m);
			AssertEquals(nameof(ComponentLineForAssembly.InventoryLine), inventoryLine, componentLine.InventoryLine);
			AssertEquals(nameof(ComponentLineForAssembly.UnitsToPick), 2m, componentLine.UnitsToPick);
		}

		public void TestConstructor_InvalidArgs()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ComponentLineForAssembly(null, 2m));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComponentLineForAssembly(Factory.New<WhsReceiveLine>(), 2m);
		}
	}
}
