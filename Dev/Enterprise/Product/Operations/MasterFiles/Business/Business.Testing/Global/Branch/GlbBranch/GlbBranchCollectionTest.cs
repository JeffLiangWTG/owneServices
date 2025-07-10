using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchCollection))]
	class GlbBranchCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbBranchCollection(Factory);
		}

		public void TestAddWithInDBElements()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbBranchCollection collection = new GlbBranchCollection(factory, new ZQuery());
			AssertEquals("Collection should be empty", 0, collection.Count);

			GlbBranch branch = factory.Load<GlbBranch>(TestCaseHelper.GetFirstPKFromTable(GlbBranch.Schema.TableName));
			collection.Add(branch);
			AssertEquals("Collection should have 1 element", 1, collection.Count);
			AssertEquals("Collection should return only Added Branch", branch, collection[0]);
		}
	}
}
