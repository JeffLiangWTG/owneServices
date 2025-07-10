using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class IPickFaceInfoTest : WhsTestCaseWithFactory
	{
		public void TestIPickFaceInfo()
		{
			var pickFaceInfo = GetPickFaceInfo();
			var iPickFaceInfoInstance = (IPickFaceInfo)pickFaceInfo;

			CombineAssertions(() =>
			{
				AssertEquals(nameof(IPickFaceInfo.WarehousePK), new ZGuid("00000000-0000-0000-0000-000000000001"),
					iPickFaceInfoInstance.WarehousePK);
				AssertEquals(nameof(IPickFaceInfo.PickToReplenishPK), new ZGuid("00000000-0000-0000-0000-000000000002"),
					iPickFaceInfoInstance.PickToReplenishPK);
				AssertEquals(nameof(IPickFaceInfo.ClientPK), new ZGuid("00000000-0000-0000-0000-000000000003"),
					iPickFaceInfoInstance.ClientPK);
				AssertEquals(nameof(IPickFaceInfo.ProductPK), new ZGuid("00000000-0000-0000-0000-000000000004"),
					iPickFaceInfoInstance.ProductPK);
				AssertEquals(nameof(IPickFaceInfo.TransferToLocationPK),
					new ZGuid("00000000-0000-0000-0000-000000000005"), iPickFaceInfoInstance.TransferToLocationPK);
				AssertEquals(nameof(IPickFaceInfo.ReplenishQuantity), 1m, iPickFaceInfoInstance.ReplenishQuantity);
				AssertEquals(nameof(IPickFaceInfo.ReplenishMultiple), 2m, iPickFaceInfoInstance.ReplenishMultiple);
				AssertEquals(nameof(IPickFaceInfo.IsDeadLocked), true, iPickFaceInfoInstance.IsDeadLocked);
				AssertEquals(nameof(IPickFaceInfo.IsDynamicTransfer), true, iPickFaceInfoInstance.IsDynamicTransfer);
				AssertEquals(nameof(IPickFaceInfo.OrderedExpiryDate), ZDate.BrettsBirthday.AddDays(10),
					iPickFaceInfoInstance.OrderedExpiryDate);
				AssertEquals(nameof(IPickFaceInfo.OrderedPackingDate), ZDate.BrettsBirthday.AddDays(-10),
					iPickFaceInfoInstance.OrderedPackingDate);
				AssertEquals(nameof(IPickFaceInfo.OrderedAttribute1), "red", iPickFaceInfoInstance.OrderedAttribute1);
				AssertEquals(nameof(IPickFaceInfo.OrderedAttribute2), "big", iPickFaceInfoInstance.OrderedAttribute2);
				AssertEquals(nameof(IPickFaceInfo.OrderedAttribute3), "new", iPickFaceInfoInstance.OrderedAttribute3);
				AssertEquals(nameof(IPickFaceInfo.OrderedSerialNumber), "serial",
					iPickFaceInfoInstance.OrderedSerialNumber);
			});
		}

		PickFaceInfo GetPickFaceInfo()
		{
			var warehousePK = new ZGuid("00000000-0000-0000-0000-000000000001");
			var pickToReplenishPK = new ZGuid("00000000-0000-0000-0000-000000000002");
			var clientPK = new ZGuid("00000000-0000-0000-0000-000000000003");
			var productPK = new ZGuid("00000000-0000-0000-0000-000000000004");
			var transferToLocationPK = new ZGuid("00000000-0000-0000-0000-000000000005");
			var replenishQuantity = 1m;
			var replenishMultiple = 2m;
			var isDeadLocked = ZBool.True;
			var isDynamicTransfer = ZBool.True;
			var orderedExpiryDate = ZDate.BrettsBirthday.AddDays(10);
			var orderedPackingDate = ZDate.BrettsBirthday.AddDays(-10);
			var orderedAttribute1 = "red";
			var orderedAttribute2 = "big";
			var orderedAttribute3 = "new";
			var orderedSerialNumber = "serial";
			var pickFaceInfo = new PickFaceInfo(warehousePK, pickToReplenishPK, clientPK, productPK,
				transferToLocationPK, replenishQuantity, replenishMultiple, isDeadLocked,
				isDynamicTransfer, orderedExpiryDate, orderedPackingDate, orderedAttribute1, orderedAttribute2,
				orderedAttribute3, orderedSerialNumber);

			return pickFaceInfo;
		}
	}
}
