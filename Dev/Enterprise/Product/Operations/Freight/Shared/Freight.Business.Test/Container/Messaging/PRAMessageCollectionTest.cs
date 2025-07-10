using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(PRAMessageCollection))]
	sealed class PRAMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFilter()
		{
			var praMessage = Container.PRAMessages.AddNew();
			var comTracMessage = Container.ComTracMessages.AddNew();
			var collection = GetCollectionToTest();
			collection.Load();
			AssertCollectionContains(praMessage, collection);
		}

		CommonContainer Container
		{
			get { return container ?? (container = Factory.NewWithValidTestData<CommonContainer>()); }
		}
		CommonContainer container;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PRAMessageCollection(Container, Factory);
		}
	}
}
