using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccWithholdingCollection))]
	sealed class AccWithholdingCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccWithholdingCollection(Factory, GlbCompany.CurrentCompany);
		}

		public void TestConstructors()
		{
			AccWithholding wHT1 = Factory.NewWithValidTestData<AccWithholding>();
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			wHT1.AW_GC = newCompany.PK;

			AccWithholding wHT2 = Factory.NewWithValidTestData<AccWithholding>();
			wHT2.AW_GC = GlbCompany.CurrentCompany.PK;

			AccWithholdingCollection collection1 = new AccWithholdingCollection(Factory, newCompany);
			collection1.Load();

			AssertCollectionContains(wHT1, collection1);
			AssertCollectionNotContains(wHT2, collection1);

			AccWithholdingCollection collection2 = new AccWithholdingCollection(Factory);
			collection2.Load();

			AssertCollectionContains(wHT2, collection2);
			AssertCollectionNotContains(wHT1, collection2);
		}

		public void TestEmptyCollectionWhenCompanyIsNull()
		{
			AccWithholding wHT1 = Factory.NewWithValidTestData<AccWithholding>();
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			wHT1.AW_GC = newCompany.PK;

			AccWithholding wHT2 = Factory.NewWithValidTestData<AccWithholding>();
			wHT2.AW_GC = GlbCompany.CurrentCompany.PK;

			AccWithholdingCollection collection1 = new AccWithholdingCollection(Factory, null);
			collection1.Load();

			AssertCollectionNotContains(wHT1, collection1);
			AssertCollectionContains(wHT2, collection1);
		}
	}
}
