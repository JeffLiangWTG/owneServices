using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyCollection))]
	sealed class GlbCompanyCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbCompanyCollection>
	{
		protected override GlbCompanyCollection GetCollectionToTest()
		{
			return new GlbCompanyCollection(Factory);
		}

		public void TestAddWithItemsInDB()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbCompanyCollection collection = new GlbCompanyCollection(factory, new ZQuery());
			collection.AdditionalFilter = ZQuery.NoResultQuery;
			AssertEquals("Collection should be empty", 0, collection.Count);

			GlbCompany company = factory.Load<GlbCompany>(TestCaseHelper.GetFirstPKFromTable(GlbCompany.Schema.TableName));
			collection.AdditionalFilter = new ZQuery(GlbCompanySchema.PK, company.PK);
			AssertEquals("Collection should have one element", 1, collection.Count);
			AssertEquals("Collection should return added deparment", company, collection[0]);
		}
	}
}
