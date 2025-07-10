using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsInventoryHeldCode))]
	sealed class WhsInventoryHeldCodeTestCase : WhsBusinessObjectTestCase
	{
		public void TestCanDelete()
		{
			var aaaCode = Helper.CreateInventoryHeldCode("AAA", "AAA");
			AssertEquals(true, aaaCode.CanDelete);

			aaaCode.WHC_IsSystem = true;
			AssertEquals(false, aaaCode.CanDelete);
			AssertEquals("Cannot delete System Hold Codes.", aaaCode.ReasonForNotAbleToDelete);
		}

		public void TesHumanReadableNameCore()
		{
			var aaaCode = Helper.CreateInventoryHeldCode("AAA", "Three As");
			AssertEquals("Inventory Hold Code AAA - Three As", aaaCode.HumanReadableName);

			AssertEquals("Inventory Hold Code", Factory.New<WhsInventoryHeldCode>());
		}

		public void TestIsDamaged()
		{
			var aaaCode = Helper.CreateInventoryHeldCode("AAA", "AAA");
			AssertEquals(false, aaaCode.IsDamaged);

			var helCode = Helper.CreateInventoryHeldCode(InventoryHoldCodes.Codes.Held, "Held");
			AssertEquals(false, helCode.IsDamaged);

			var damCode = Helper.CreateInventoryHeldCode(InventoryHoldCodes.Codes.Damaged, "Damaged");
			AssertEquals(true, damCode.IsDamaged);
		}

		public void TestIsOriginalHeldCodeUsed()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var org1PK = iHelper.CreateClient("C1");
			var org2PK = iHelper.CreateClient("C2");

			var code1 = Helper.CreateInventoryHeldCode("AAA", "AAA");
			var code2 = Helper.CreateInventoryHeldCode("BBB", "BBB for C1", org1PK);
			var code3 = Helper.CreateInventoryHeldCode("BBB2", "BBB for C2", org2PK);
			AssertEquals(false, code1.IsOriginalHeldCodeUsed());
			AssertEquals(false, code2.IsOriginalHeldCodeUsed());
			AssertEquals(false, code3.IsOriginalHeldCodeUsed());
			Factory.Save();

			var part1 = iHelper.CreateProduct(org1PK, "P1");
			var part2 = iHelper.CreateProduct(org2PK, "P2");
			var receive1PK = iHelper.CreateWhsReceive(org1PK, whs.PK, "1", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receive1PK, part1.PK, 10m, "A-1-1");
			var receive2PK = iHelper.CreateWhsReceive(org2PK, whs.PK, "2", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receive2PK, part2.PK, 10m, "A-1-1");
			Factory.Save();
			AssertEquals(false, code1.IsOriginalHeldCodeUsed());
			AssertEquals(false, code2.IsOriginalHeldCodeUsed());
			AssertEquals(false, code3.IsOriginalHeldCodeUsed());

			var receive3PK = iHelper.CreateWhsReceive(org1PK, whs.PK, "3", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receive3PK, part1.PK, 10m, "A-1-1", code1.WHC_Code);
			var receive4PK = iHelper.CreateWhsReceive(org1PK, whs.PK, "4", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receive4PK, part1.PK, 10m, "A-1-1", code2.WHC_Code);
			Factory.Save();
			AssertEquals(true, code1.IsOriginalHeldCodeUsed());
			AssertEquals(true, code2.IsOriginalHeldCodeUsed());
			AssertEquals(false, code3.IsOriginalHeldCodeUsed());
		}

		public void TestGetWHC_DescriptionMultilingual()
		{
			var inventoryHeldCode = Factory.NewWithValidTestData<WhsInventoryHeldCode>();

			using (var mockData = Res.UseMockData())
			{
				var key = inventoryHeldCode.WHC_DescriptionInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Damaged");
				mockData.Put(key, new ResourceStringData(key, "损坏的"));
				inventoryHeldCode.WHC_Description = "Damaged";
				AssertEquals("损坏的", inventoryHeldCode.WHC_DescriptionMultilingual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Helper.CreateInventoryHeldCode("WET", "WATER DAMAGE");
		}
	}
}
