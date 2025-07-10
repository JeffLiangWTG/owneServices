using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickLinesExtensionsTest : TestCaseWithFactory
	{
		#region TestGetOriginallyPickedPickLines_MustNotPassInNull

		public void TestGetOriginallyPickedPickLines_MustNotPassInNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				((IEnumerable<WhsPickLine>)null).GetOriginallyPickedPickLines());
		}

		#endregion

		#region TestGetOriginallyPickedPickLines

		public void TestGetOriginallyPickedPickLines()
		{
			var originalInventoryPK = ZGuid.NewZGuid();
			var transferLine = Factory.New<WhsTransferLine>();
			var pickLine1 = Factory.New<WhsPickLine>();
			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine1.WZ_WE_InventoryLine = transferLine.PK;
			pickLine1.WZ_WE_OriginalPickedInventoryLine = originalInventoryPK;
			pickLine2.WZ_WE_TransactionLine = transferLine.PK;
			pickLine2.WZ_WE_InventoryLine = originalInventoryPK;

			var collection = new[] { pickLine1, pickLine2 }.GetOriginallyPickedPickLines().ToArray();
			AssertContainsExactElementsInAnyOrder(
				new[] { new { X = pickLine2, Y = pickLine1 }, new { X = pickLine2, Y = pickLine2 } },
				collection.Select(o => new { X = o.PickLineForPickingDetails, Y = o.PickLineOnOrder }));
		}

		#endregion
	}
}
