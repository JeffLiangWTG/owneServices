using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxRateCollectionForRegistry))]
	sealed class AccTaxRateCollectionForRegistryTest : AccTaxRateCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccTaxRateCollectionForRegistry(Factory);
		}

		public void TestRelationshipFilterIsEmpty()
		{
			AccTaxRateCollectionForRegistry testCollection = (AccTaxRateCollectionForRegistry)GetCollectionToTest();
			Assert(testCollection.RelationshipFilterInternal.IsEmpty);
		}
	}
}
