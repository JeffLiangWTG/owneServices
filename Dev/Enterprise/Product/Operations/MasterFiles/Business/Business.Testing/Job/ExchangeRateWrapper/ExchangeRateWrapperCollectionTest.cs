using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExchangeRateWrapperCollection))]
	public class TestExchangeRateWrapperCollection : NonPersistentBusinessObjectCollectionTestCase<ExchangeRateWrapperCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExchangeRateWrapper(Factory);
		}

		protected override ExchangeRateWrapperCollection GetCollectionToTest()
		{
			return new ExchangeRateWrapperCollection(Factory);
		}

		public void TestAddingNewWrapperToCollection()
		{
			ExchangeRateWrapper addedWrapper = WrapperCollection.AddNew();
			AssertEquals("There should be one wrapper in the collection", 1, WrapperCollection.Count);
			addedWrapper.SellRate = new ZDecimal(2.34);
			AssertEquals("The wrapper in the collection should be affected", new ZDecimal(2.34),
					WrapperCollection[0].SellRate);
		}

		#region Implmentation

		protected ExchangeRateWrapperCollection WrapperCollection;
		protected override void SetUp()
		{
			base.SetUp();
			WrapperCollection = new ExchangeRateWrapperCollection(Factory);
		}

		#endregion
	}
}
