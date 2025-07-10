using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(WhsInventoryWrapperCollection))]
	sealed class WhsInventoryWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WhsInventoryWrapperCollection>
	{
		WhsDataTestHelper Helper
		{
			get { return helper ?? (helper = new WhsDataTestHelper(Factory)); }
		}
		WhsDataTestHelper helper;

		protected override WhsInventoryWrapperCollection GetCollectionToTest()
		{
			return new WhsInventoryWrapperCollection(Header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(Receive.PK, Helper.Part.PK, "PACKAGE1", 100m, 1000m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory = whsReceiveLine.Inventory;
			return new WhsInventoryWrapper(whsInventory, Header);
		}

		Warehouse.Integration.IWhsReceive Receive
		{
			get { return receive ?? (receive = Helper.GetNewWhsReceive(WhsWarehouse.PK, Helper.Importer.PK)); }
		}
		Warehouse.Integration.IWhsReceive receive;

		Warehouse.Integration.IWhsWarehouse WhsWarehouse
		{
			get { return whsWarehouse ?? (whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10")); }
		}
		Warehouse.Integration.IWhsWarehouse whsWarehouse;

		DeclarationInventorySelectionHeader Header
		{
			get { return header ?? (header = new DeclarationInventorySelectionHeader(Declaration)); }
		}
		DeclarationInventorySelectionHeader header;

		BaseJobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<BaseJobDeclaration>()); }
		}
		BaseJobDeclaration declaration;
	}
}
