using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public static class WhsPickTestExtensions
	{
		public static string AutoAllocateItemsWithMock(this WhsPick whsPick, bool shouldAddFetchHints = true)
		{
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(whsPick.Factory, whsPick.PK);
				return whsPick.AutoAllocateItems(shouldAddFetchHints: shouldAddFetchHints);
			}
		}

		public static void AutoAllocateItemsWithMock(this WhsPick whsPick, ZDateTimeOffset dateToFilterInventory, bool shouldAddFetchHints = true)
		{
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				whsPick.AutoAllocateItems(dateToFilterInventory, shouldAddFetchHints: shouldAddFetchHints);
			}
		}

		public static void PickOrdersWithAllocationMock(this WhsPick whsPick, WhsPickableDocket unattachedOrder, bool saveFactory = true)
		{
			whsPick.PickOrdersWithAllocationMock(new[] { unattachedOrder }, saveFactory);
		}

		public static bool PickOrdersWithAllocationMock(this WhsPick whsPick)
		{
			bool result;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				result = whsPick.PickOrders();
			}
			return result;
		}

		public static bool PickOrdersWithAllocationMock(this WhsPick whsPick, WhsPickableDocket[] unattachedOrders, bool saveFactory = true)
		{
			bool result;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				result = whsPick.PickOrders(unattachedOrders, saveFactory);
			}
			return result;
		}
	}
}
