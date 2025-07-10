using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class DocketLineEnumerableExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestToProductAndQuantities

		public void TestToProductAndQuantities()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			var line1 = receive.Lines.AddNew();
			var line2 = receive.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line2.WE_OP = data.Part2.PK;
			line1.WE_TransactionQuantity = 10m;
			line2.WE_TransactionQuantity = 8m;

			AssertContainsExactElementsInAnyOrder(new[] { (data.Part1.PK, 10m), (data.Part2.PK, 8m) },
				new[] { line2, line1 }.ToProductAndQuantities().Select(o => (o.ProductPK, (decimal)o.Quantity)));
		}

		public void TestToProductAndQuantities_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				((IEnumerable<WhsDocketLine>)null).ToProductAndQuantities());
		}

		#endregion
	}
}
