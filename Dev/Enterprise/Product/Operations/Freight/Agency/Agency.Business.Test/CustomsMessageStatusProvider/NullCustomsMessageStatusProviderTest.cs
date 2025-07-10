using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business
{
	internal class NullCustomsMessageStatusProviderTest : TestCaseWithFactory
	{
		public void TestEmpty()
		{
			AssertEquals(false, Provider.ShouldShow(Bill));
			AssertEquals("", Provider.GetCustomsStatus(Bill));
			AssertEquals("", Provider.GetMessageStatus(Bill));
			AssertEquals("", Provider.GetUserFriendlyStatusMessage(Bill));
		}

		#region Implementation
		NullCustomsMessageStatusProvider Provider
		{
			get
			{
				return provider ?? (provider = new NullCustomsMessageStatusProvider());
			}
		}

		NullCustomsMessageStatusProvider provider;
		BillOfLading Bill
		{
			get
			{
				return bill ?? (bill = Factory.New<BillOfLading>());
			}
		}

		BillOfLading bill;
		#endregion
	}
}
