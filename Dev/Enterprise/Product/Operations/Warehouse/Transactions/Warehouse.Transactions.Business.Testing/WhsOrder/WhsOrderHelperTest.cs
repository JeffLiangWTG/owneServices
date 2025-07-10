using System.Linq;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderHelperTest : WhsTestCaseWithFactory
	{
		public void TestOrderStatuses()
		{
			var status = new DocketStatus();
			status.AddRange(new WhsOrderStatus());
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Finalised));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Putaway));
			AssertContainsExactElementsInAnyOrder(status, WhsOrderHelper.OrderStatuses);
		}

		public void TestOrderStatuses_NoDuplicateCodes()
		{
			var orderStatuses = WhsOrderHelper.OrderStatuses.ToArray();
			var distinctStatusCodes = orderStatuses.Select(os => os.Code).Distinct();

			AssertEquals(distinctStatusCodes.Count(), orderStatuses.Length);
		}

		public void TestOrderStatuses_NoDuplicateDescriptions()
		{
			var orderStatuses = WhsOrderHelper.OrderStatuses.ToArray();
			var distinctStatusDescriptions = orderStatuses.Select(os => os.Description).Distinct();

			AssertEquals(distinctStatusDescriptions.Count(), orderStatuses.Length);
		}
	}
}
